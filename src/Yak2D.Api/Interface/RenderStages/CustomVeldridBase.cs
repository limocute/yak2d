using Veldrid;
using Veldrid.Sdl2;
using Veldrid.Utilities;

namespace Yak2D
{
    /// <summary>
    /// Override this base class to enable the creation of a custom rendering stage
    /// with exposure to all key components (device, factory, window, commandlist)
    /// The render method has access to an optional 4 input textures and single render target
    /// The update has raw access to the latest veldrid input snapshot object
    /// </summary>
    public abstract class CustomVeldridBase
    {
        /// <summary>
        /// Called by the framework when custom stage is created. Create all required resources
        /// </summary>
        /// <param name="device">Current veldrid graphics device object</param>
        /// <param name="window">Current SDL2 window</param>
        /// <param name="factory">Current veldrid resource factory, with auto-dispose</param>
        public abstract void Initialise(GraphicsDevice device,
                                        Sdl2Window window,
                                        DisposeCollectorResourceFactory factory);

        /// <summary>
        /// Called by framework when part of render queue generated by user
        /// </summary>
        /// <param name="cl">Current in-use veldrid command list. Add commands to the render queue</param>
        /// <param name="device">Current veldrid graphics device</param>
        /// <param name="texture0">Input Texture</param>
        /// <param name="texture1">Input Texture<</param>
        /// <param name="texture2">Input Texture<</param>
        /// <param name="texture3">Input Texture<</param>
        /// <param name="framebufferTarget">Output RenderTarget (veldrid framebuffer)</param>
        public abstract void Render(CommandList cl,
                                    GraphicsDevice device,
                                    ResourceSet texture0,
                                    ResourceSet texture1,
                                    ResourceSet texture2,
                                    ResourceSet texture3,
                                    Framebuffer framebufferTarget);
        /// <summary>
        /// Called by framework once per update loop
        /// </summary>
        /// <param name="timeStepSeconds">Time in seconds since the last update loop</param>
        /// <param name="inputSnapshot">Raw veldrid input snapshot object</param>
        public abstract void Update(float timeStepSeconds,
                                    InputSnapshot inputSnapshot);

        /// <summary>
        /// Dispose of any resources that will not be processed by the veldrid resource collector factory
        /// </summary>
        public abstract void DisposeOfResources();
    }
}