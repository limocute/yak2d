using System.Collections.Generic;
using Veldrid;
using Yak2D.Internal;

namespace Yak2D.Graphics
{
    public class ViewportManager : IViewportManager
    {
        private readonly IIdGenerator _idGenerator;
        private readonly IViewportFactory _viewportFactory;

        private ISimpleCollection<IViewportModel> _viewportCollection;

        private IViewportModel _activeViewport;

        public int ViewportCount { get { return _viewportCollection.Count; } }

        private List<ulong> _viewportsForDestruction;

        public ViewportManager(IViewportFactory viewportFactory,
                                IIdGenerator IdGenerator,
                                ISimpleCollectionFactory collectionFactory)
        {
            _idGenerator = IdGenerator;
            _viewportFactory = viewportFactory;

            _viewportCollection = collectionFactory.Create<IViewportModel>(48);

            _viewportsForDestruction = new List<ulong>();
        }

        public IViewportModel RetrieveViewportModel(ulong key) => _viewportCollection.Retrieve(key);

        public void DestroyViewport(ulong viewport)
        {
            _viewportsForDestruction.Add(viewport);
        }

        public void DestroyAllViewports()
        {
            var ids = _viewportCollection.ReturnAllIds();

            ids.ForEach(id =>
            {
                _viewportsForDestruction.Add(id);
            });
        }

        public void ProcessPendingDestruction()
        {
            _viewportsForDestruction.ForEach(id =>
            {
                _viewportCollection.Remove(id);
            });

            _viewportsForDestruction.Clear();
        }

        public IViewport CreateViewport(uint minx, uint miny, uint width, uint height)
        {
            var id = _idGenerator.New();

            var model = _viewportFactory.CreateViewport(minx, miny, width, height);

            var userReference = new ViewportReference(id);

            return _viewportCollection.Add(id, model) ? userReference : null;
        }

        public void SetActiveViewport(ulong id)
        {
            _activeViewport = RetrieveViewport(id);
        }

        private IViewportModel RetrieveViewport(ulong key)
        {
            var viewport = _viewportCollection.Retrieve(key);

            if (viewport == null)
            {
                return null;
            }

            return viewport;
        }

        public void ClearActiveViewport()
        {
            _activeViewport = null;
        }

        public void ConfigureViewportForActiveFramebuffer(CommandList cl)
        {
            var viewport = ReturnActiveViewport();

            if (viewport == null)
            {
                cl.SetFullViewports();
            }
            else
            {
                cl.SetViewport(0, (Viewport)viewport);
            }
        }

        private Viewport? ReturnActiveViewport()
        {
            if (_activeViewport == null)
            {
                return null;
            }
            else
            {
                return _activeViewport.Viewport;
            }
        }

        public void Shutdown()
        {
            DestroyAllViewports();
        }

        public void ReInitialise()
        {
            //Same as shutdown, not needed...
            DestroyAllViewports();
        }
    }
}