using System.Collections.Generic;
using Veldrid;

namespace Yak2D.Graphics
{
    public interface IRenderCommandQueue
    {
        int CommandQueueSize { get; }
        int CallbackQueueSize { get; }

        void Add(RenderCommandType type,
                    ulong Stage,
                    ulong Surface,
                    ulong Camera,
                    ulong Texture0,
                    ulong Texture1,
                    ulong SpareId0,
                    ulong SpareId1,
                    RgbaFloat Colour);
        void Reset();
        IEnumerable<RenderCommandQueueItem> FlushCommands();
        IEnumerable<ulong> FlushCallbackStageIds();
    }
}