using System;
using System.Drawing;
using System.IO;
using System.Numerics;
using SixLabors.ImageSharp.PixelFormats;
using Yak2D.Internal;
using Yak2D.Utility;

namespace Yak2D.Surface
{
    public class Surfaces : ISurfaces
    {
        private ISystemComponents _systemComponents;
        private IStartupPropertiesCache _startUpPropertiesCache;
        private IGpuSurfaceManager _surfaceManager;

        public int TotalUserSurfaceCount { get { return _surfaceManager.TotalUserSurfaceCount; } }
        public int UserRenderTargetCount { get { return _surfaceManager.UserRenderTargetCount; } }
        public int UserTextureCount { get { return _surfaceManager.UserTextureCount; } }

        public Surfaces(IStartupPropertiesCache startUpPropertiesCache,
                        IGpuSurfaceManager surfaceManager,
                        ISystemComponents systemComponents)
        {
            _systemComponents = systemComponents;
            _startUpPropertiesCache = startUpPropertiesCache;
            _surfaceManager = surfaceManager;
        }

        public Size GetSurfaceDimensions(ITexture texture)
        {
            if (texture == null)
            {
                throw new Yak2DException("Surfaces -> GetSurfaceDimensions(), texture cannot be null", new ArgumentNullException("texture"));
            }

            return _surfaceManager.GetSurfaceDimensions(texture.Id);
        }

        public Size GetSurfaceDimensions(ulong surface)
        {
            return _surfaceManager.GetSurfaceDimensions(surface);
        }

        public ITexture LoadTexture(string path,
                                    AssetSourceEnum assetType,
                                    ImageFormat imageFormat = ImageFormat.PNG,
                                    SamplerType samplerType = SamplerType.Anisotropic,
                                    bool generateMipMaps = true)
        {
            switch (assetType)
            {
                case AssetSourceEnum.File:
                    return _surfaceManager.CreateTextureFromFile(path, imageFormat, samplerType, generateMipMaps);
                case AssetSourceEnum.Embedded:
                    return _surfaceManager.CreateTextureFromEmbeddedResourceInUserApplication(path, imageFormat, samplerType, generateMipMaps);
            }

            return null;
        }

        public ITexture LoadTexture(Stream stream, SamplerType samplerType = SamplerType.Anisotropic, bool generateMipMaps = true)
        {
            return _surfaceManager.GenerateTextureFromStream(stream, false, false, samplerType, generateMipMaps);
        }

        public TextureData LoadTextureColourData(string path, AssetSourceEnum assetType, ImageFormat imageFormat)
        {
            switch (assetType)
            {
                case AssetSourceEnum.File:
                    return _surfaceManager.LoadTextureColourDataFromFile(path, imageFormat);
                case AssetSourceEnum.Embedded:
                    return _surfaceManager.LoadTextureColourDataFromEmbeddedResourceInUserApplication(path, imageFormat);
            }

            return default(TextureData);
        }

        public IRenderTarget ReturnMainWindowRenderTarget()
        {
            return new RenderTargetReference(_surfaceManager.MainSwapChainFrameBufferKey);
        }

        public IRenderTarget CreateRenderTarget(uint width,
                                                uint height,
                                                bool autoClearColourAndDepthEachFrame = true,
                                                SamplerType samplerType = SamplerType.Anisotropic,
                                                uint numberOfMipMapLevels = 1)

        {
            if (width == 0 || height == 0)
            {
                throw new Yak2DException("Surfaces -> CreateRenderTarget(), dimensions cannot be zero");
            }

            if(numberOfMipMapLevels == 0)
            {
                numberOfMipMapLevels = 1;
            }

            return _surfaceManager.CreateRenderSurface(
                false,
                width,
                height,
                TexturePixelFormatConverter.ConvertYakToVeldrid(_systemComponents.SwapChainFramebufferPixelFormat),
                true,
                autoClearColourAndDepthEachFrame,
                autoClearColourAndDepthEachFrame,
                samplerType,
                numberOfMipMapLevels
            );
        }

        public void DestroySurface(ITexture surface)
        {
            if (surface == null)
            {
                throw new Yak2DException("Surfaces -> DestroySurface(), surface cannot be null", new ArgumentNullException("surface"));
            }

            _surfaceManager.DestroySurface(surface.Id);
        }

        public void DestroySurface(ulong surface)
        {
            _surfaceManager.DestroySurface(surface);
        }

        public void DestroyAllUserRenderTargets() => _surfaceManager.DestroyAllUserRenderTargets();
        public void DestroyAllUserTextures() => _surfaceManager.DestroyAllUserTextures();
        public void DestoryAllUserSurfaces() => _surfaceManager.DestroyAllUserSurfaces();

        public ITexture CreateFloat32FromData(uint textureWidth, uint textureHeight, float[] pixels, SamplerType samplerType = SamplerType.Anisotropic)
        {
            if (pixels == null)
            {
                throw new Yak2DException("Surfaces -> CreateFloat32FromData(), pixel data cannot be null", new ArgumentNullException("pixels"));
            }

            if (textureWidth == 0 | textureHeight == 0)
            {
                throw new Yak2DException("Surfaces -> CreateFloat32FromData(), texture dimensions cannot be zero");
            }

            if (textureWidth * textureHeight != pixels.Length)
            {
                throw new Yak2DException("Surfaces -> CreateFloat32FromData(), pixel data array size does not match dimensions");
            }

            return _surfaceManager.CreateFloat32TextureFromPixelData(textureWidth, textureHeight, pixels, samplerType);
        }

        public ITexture CreateRgbaFromData(uint textureWidth,
                                           uint textureHeight,
                                           Vector4[] pixels,
                                           SamplerType samplerType = SamplerType.Anisotropic,
                                           bool generateMipMaps = true)
        {
            if (pixels == null)
            {
                throw new Yak2DException("Surfaces -> CreateRgbaFromData(), pixel data cannot be null", new ArgumentNullException("pixels"));
            }

            if (textureWidth == 0 | textureHeight == 0)
            {
                throw new Yak2DException("Surfaces -> CreateRgbaFromData(), texture dimensions cannot be zero");
            }

            if (textureWidth * textureHeight != pixels.Length)
            {
                throw new Yak2DException("Surfaces -> CreateRgbaFromData(), pixel data array size does not match dimensions");
            }

            var pixelcount = textureWidth * textureHeight;
            var rgba = new Rgba32[pixelcount];
            for (var p = 0; p < pixelcount; p++)
            {
                var value = pixels[p];
                rgba[p] = new Rgba32(
                            Clamper.Clamp(value.X, 0.0f, 1.0f),
                            Clamper.Clamp(value.Y, 0.0f, 1.0f),
                            Clamper.Clamp(value.Z, 0.0f, 1.0f),
                            Clamper.Clamp(value.W, 0.0f, 1.0f)
                        );
            }
            return _surfaceManager.CreateRgbaTextureFromPixelData(textureWidth, textureHeight, rgba, samplerType, generateMipMaps, false);
        }

        public void SetMainWindowRenderTargetAutoClearDepth(bool autoClearDepth)
        {
            _surfaceManager.AutoClearMainWindowDepth = autoClearDepth;
        }

        public void SetMainWindowRenderTargetAutoClearColour(bool autoClearColour)
        {
            _surfaceManager.AutoClearMainWindowColour = autoClearColour;
        }
    }
}