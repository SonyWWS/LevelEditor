//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Threading;
using System.IO;

using Sce.Atf;
using Sce.Atf.Dom;
using Sce.Atf.Applications;
using Sce.Atf.Adaptation;
using Sce.Atf.VectorMath;

using LevelEditorCore;


using Camera = Sce.Atf.Rendering.Camera;

namespace RenderingInterop
{

    /// <summary>
    /// Thumbnail resolver for 3d models resources</summary>    
    [Export(typeof(IThumbnailResolver))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ThumbnailResolver : IThumbnailResolver, IInitializable
    {

        public ThumbnailResolver()
        {
            m_syncContext = SynchronizationContext.Current;
            if (m_syncContext == null)
            {
                throw new Exception("The instance of this class can only be created on a thread"
                    + "that has WindowsFormsSynchronizationContext, ie GUI thread");
            }
            m_game = null;

            
        }
        #region IInitializable Members

        public void Initialize()
        {
           
           
        }

        #endregion


        #region IThumbnailResolver Members

        Image IThumbnailResolver.Resolve(Uri resourceUri)
        {
                        
            string assetPath = resourceUri.LocalPath;          
            string thumbnailPath = GetThumbnailPath(assetPath);

            Image img = null;
            // regenerate thumbnail if it doesn't exit or it older than access.
            if (File.Exists(thumbnailPath) == false 
                || File.GetLastWriteTime(assetPath) > File.GetLastWriteTime(thumbnailPath))
            {
                if (m_game == null) Init();
                m_syncContext.Send(delegate
                {                    
                    GenThumbnail(resourceUri, thumbnailPath);                    
                }, null);                
            }

            if (File.Exists(thumbnailPath) && File.GetLastWriteTime(thumbnailPath) > File.GetLastWriteTime(assetPath))
            {
                using (var strm = File.OpenRead(thumbnailPath))
                {
                    img = new Bitmap(strm);
                }
            }
            
            return img;
        }

        #endregion

        private string GetThumbnailPath(string assetPath)
        {
            string file = "~" + Path.GetFileName(assetPath);
            string path = Path.GetDirectoryName(assetPath);
            file = Path.ChangeExtension(file, "png");
            return path + "/" + file;
        }

        private void GenThumbnail(Uri resourceUri, string thumbnailPath)
        {             
            NativeObjectAdapter gameLevel = GameEngine.GetGameLevel();
            try
            {
                IResource resource = m_resourceService.Load(resourceUri);
                IGameObject gob = m_resourceConverterService.Convert(resource);
                if (gob == null) return;
                
                m_game.RootGameObjectFolder.GameObjects.Add(gob);

                GameEngine.SetRenderState(m_renderState);
                GameEngine.SetGameLevel(m_game.Cast<NativeObjectAdapter>());

                m_gameEngine.WaitForPendingResources();
                FrameTime fr = new FrameTime(0, 0);
                m_gameEngine.Update(fr, UpdateType.Paused);

                IBoundable boundable = gob.Cast<IBoundable>();
                Sphere3F sphere = boundable.BoundingBox.ToSphere();
                
                if (Math.Abs(sphere.Radius) <= float.Epsilon)
                    sphere.Radius = 1.0f;

                m_cam.SetPerspective(
               (float)Math.PI / 4,
               1.0f,
               sphere.Radius * 0.01f,
               sphere.Radius * 4.0f);


                Vec3F camPos = sphere.Center + new Vec3F(sphere.Radius, sphere.Radius, sphere.Radius) * 1.5f;
                m_cam.Set(camPos, sphere.Center, new Vec3F(0, 1, 0));

                GameEngine.Begin(m_renderSurface.InstanceId, m_cam.ViewMatrix, m_cam.ProjectionMatrix);
                GameEngine.RenderGame();
                GameEngine.End();
                GameEngine.SaveRenderSurfaceToFile(m_renderSurface.InstanceId, thumbnailPath);                
                m_game.RootGameObjectFolder.GameObjects.Remove(gob);

                m_resourceService.Unload(resourceUri);
            }
            finally
            {
                GameEngine.SetGameLevel(gameLevel);
            }
             
             
        }

        private void Init()
        {
            NativeObjectAdapter curLevel = GameEngine.GetGameLevel();
            try
            {
                // create new document by creating a Dom node of the root type defined by the schema                 
                DomNode rootNode = new DomNode(m_schemaLoader.GameType, m_schemaLoader.GameRootElement);
                INameable nameable = rootNode.Cast<INameable>();
                nameable.Name = "ThumbnailGenerator";

                NativeObjectAdapter gameLevel = rootNode.Cast<NativeObjectAdapter>();
                GameEngine.CreateObject(gameLevel);
                GameEngine.SetGameLevel(gameLevel);
                gameLevel.UpdateNativeOjbect();
                NativeGameWorldAdapter gworld = rootNode.Cast<NativeGameWorldAdapter>();

                m_game = rootNode.Cast<IGame>();
                IGameObjectFolder rootFolder = m_game.RootGameObjectFolder;                
                m_renderSurface = new TextureRenderSurface(96, 96);
                m_renderState = new RenderState();
                m_renderState.RenderFlag = GlobalRenderFlags.Solid | GlobalRenderFlags.Textured | GlobalRenderFlags.Lit | GlobalRenderFlags.Shadows;


            }
            finally
            {
                GameEngine.SetGameLevel(curLevel);
            }


            m_mainWindow.Closed += delegate
            {
                GameEngine.DestroyObject(m_game.Cast<NativeObjectAdapter>());
                m_renderSurface.Dispose();
                m_renderState.Dispose();
            };

        }
        
        private SynchronizationContext m_syncContext;

        [Import(AllowDefault = false)]
        private ISchemaLoader m_schemaLoader = null;

        [Import(AllowDefault = false)]
        private IMainWindow m_mainWindow = null;

        [Import(AllowDefault = false)]
        private IResourceService m_resourceService = null;

        [Import(AllowDefault = false)]
        private ResourceConverterService m_resourceConverterService = null;

        [Import(AllowDefault = false)]
        private IGameEngineProxy m_gameEngine;

        private IGame m_game;        
        private Camera m_cam = new Camera();
        private TextureRenderSurface m_renderSurface;
        private RenderState m_renderState;
    }
}
