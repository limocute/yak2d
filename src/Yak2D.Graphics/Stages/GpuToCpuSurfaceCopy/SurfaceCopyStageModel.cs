﻿using System;
using System.Runtime.InteropServices;
using Veldrid;
using Yak2D.Internal;

namespace Yak2D.Graphics
{
    public class SurfaceCopyStageModel : ISurfaceCopyStageModel
    {
        public ulong StagingTextureId { get; private set; }
        public void SendToRenderStage(IRenderStageVisitor visitor, CommandList cl, RenderCommandQueueItem command) => visitor.DispatchToRenderStage(this, cl, command);
        public void CacheInstanceInVisitor(IRenderStageVisitor visitor) => visitor.CacheStageModel(this);

        private IFrameworkMessenger _frameworkMessenger;
        private ISystemComponents _systemComponents;
        private IGpuSurfaceManager _surfaceManager;
        private PixelFormat _pixelFormat;

        private uint _stagingTextureWidth;
        private uint _stagingTextureHeight;
        private Action<TextureData> _callback;
        private byte[] _data;

        public SurfaceCopyStageModel(IFrameworkMessenger frameworkMessenger,
                                     ISystemComponents systemComponents,
                                     IGpuSurfaceManager surfaceManager,
                                     uint stagingTextureWidth,
                                     uint stagingTextureHeight,
                                     PixelFormat pixelFormat,
                                     Action<TextureData> callback)
        {
            _frameworkMessenger = frameworkMessenger;
            _systemComponents = systemComponents;
            _surfaceManager = surfaceManager;
            _pixelFormat = pixelFormat;

            SetCallback(callback);

            CreateStagingTextureAndDataArray(stagingTextureWidth, stagingTextureHeight);
        }

        public void CreateStagingTextureAndDataArray(uint stagingTextureWidth, uint stagingTextureHeight)
        {
            _stagingTextureWidth = stagingTextureWidth;
            _stagingTextureHeight = stagingTextureHeight;

            if (StagingTextureId != 0UL)
            {
                _surfaceManager.DestroySurface(StagingTextureId);
            }

            StagingTextureId = _surfaceManager.CreateGpuCpuStagingSurface(_stagingTextureWidth,
                                                                           _stagingTextureHeight,
                                                                           _pixelFormat).Id;

            _data = new byte[4 * _stagingTextureWidth * _stagingTextureHeight];
        }

        public void SetCallback(Action<TextureData> callback)
        {
            if (callback != null)
            {
                _callback = callback;
            }
        }

        public void Update(float seconds) { }

        public void DestroyResources()
        {
            _surfaceManager?.DestroySurface(StagingTextureId);
        }

        public void CopyDataFromStagingTextureAndPassToUser()
        {
            var stagingTexture = _surfaceManager.RetrieveSurface(StagingTextureId);

            //Create a Device abstraction pass through for this MAP?
            MappedResourceView<byte> mapRead = _systemComponents.Device.RawVeldridDevice.Map<byte>(stagingTexture.Texture, MapMode.Read);

            var dataRowStride = mapRead.MappedResource.RowPitch;

            var requirePaddedArray = dataRowStride != 4 * _stagingTextureWidth;

            byte[] dataWithPadding = null;

            var dataByteSize = 4 * (int)_stagingTextureWidth * (int)_stagingTextureHeight;

            if(requirePaddedArray)
            {
                dataByteSize = (int)dataRowStride * (int)_stagingTextureHeight;
                dataWithPadding = new byte[dataByteSize];
            }

            Marshal.Copy(mapRead.MappedResource.Data, requirePaddedArray ? dataWithPadding : _data, 0, dataByteSize);

            _systemComponents.Device.RawVeldridDevice.Unmap(stagingTexture.Texture);

            if (requirePaddedArray)
            {
                var unpaddedDataRowStride = 4 * _stagingTextureWidth;
                for (var y = 0; y < _stagingTextureHeight; y++)
                {
                    for (var x = 0; x < unpaddedDataRowStride; x++)
                    {
                        var indexSource = (y * dataRowStride) + x;
                        var indexTarget = (y * unpaddedDataRowStride) + x;
                        _data[indexTarget] = dataWithPadding[indexSource];
                    }
                }
            }

            Span<byte> byteSpan = _data;

            var numberPixels = _stagingTextureWidth * _stagingTextureHeight;

            var userData = new TextureData
            {
                Width = _stagingTextureWidth,
                Height = _stagingTextureHeight,
                Pixels = new System.Numerics.Vector4[numberPixels]
            };

            if (_pixelFormat == PixelFormat.R32_Float)
            {
                var floatSpan = MemoryMarshal.Cast<byte, float>(byteSpan);
                for (var s = 0; s < numberPixels; s++)
                {
                    var t = s;
                    var backend = _systemComponents.Device.BackendType;
                    if (backend == GraphicsApi.OpenGL || backend == GraphicsApi.OpenGLES)
                    {
                        //Need to switch y coordinate
                        var y = s / (int)_stagingTextureWidth;
                        var x = s % (int)_stagingTextureWidth;

                        y = (int)_stagingTextureHeight - 1 - y;

                        t = (y * (int)_stagingTextureWidth) + x;
                    }
                    
                    //We only use the .X / .R component when returning single float data
                    userData.Pixels[t].X = floatSpan[s];
                }
            }
            else
            {
                //For the time being this will always be:
                //PixelFormat.R8_G8_B8_A8_UNorm

                for (var s = 0; s < numberPixels; s++)
                {
                    var t = s;
                    var backend = _systemComponents.Device.BackendType;
                    if (backend == GraphicsApi.OpenGL || backend == GraphicsApi.OpenGLES)
                    {
                        //Need to switch y coordinate
                        var y = s / (int)_stagingTextureWidth;
                        var x = s % (int)_stagingTextureWidth;

                        y = (int)_stagingTextureHeight - 1 - y;

                        t = (y * (int)_stagingTextureWidth) + x;
                    }

                    //Every 4 bytes will represent the 4 components of the colour
                    var r = t * 4;
                    var g = r + 1;
                    var b = g + 1;
                    var a = b + 1;

                    //Each component of colour is a byte that we need to convert into a 0 - 255 int and 
                    //then get the normalised 0 to 1 value out of it
                    userData.Pixels[s].X = ((float)((int)byteSpan[r])) / 255.0f;
                    userData.Pixels[s].Y = ((float)((int)byteSpan[g])) / 255.0f;
                    userData.Pixels[s].Z = ((float)((int)byteSpan[b])) / 255.0f;
                    userData.Pixels[s].W = ((float)((int)byteSpan[a])) / 255.0f;
                }
            }

            _callback.Invoke(userData);
        }
    }
}