//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using LevelEditorCore.VectorMath;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;
using Sce.Atf.VectorMath;

using LevelEditorCore;

using ViewTypes = Sce.Atf.Rendering.ViewTypes;

namespace RenderingInterop
{
    public class NativeDesignControl : DesignViewControl
    {

        public NativeDesignControl(DesignView designView) :
            base(designView)
        {
            ClientSize = new Size(10, 10);
            if (!GameEngine.IsInError)
            {
                swapChainId = GameEngine.GetObjectTypeId("SwapChain");
                SurfaceId = GameEngine.CreateObject(swapChainId, this.Handle, IntPtr.Size);
                SizePropId = GameEngine.GetObjectPropertyId(swapChainId, "Size");
                GameEngine.SetObjectProperty(swapChainId, SurfaceId, SizePropId, ClientSize);
                BkgColorPropId = GameEngine.GetObjectPropertyId(swapChainId, "BkgColor");
                GameEngine.SetObjectProperty(swapChainId, SurfaceId, BkgColorPropId, BackColor);
                
            }
            if (s_marqueePen == null)
            {
                s_marqueePen = new Pen(Color.FromArgb(30, 30, 30), 2);
                s_marqueePen.DashPattern = new float[] { 3, 3 };
            }

            m_renderState = new RenderState();
            m_renderState.RenderFlag = GlobalRenderFlags.Solid | GlobalRenderFlags.Textured | GlobalRenderFlags.Lit | GlobalRenderFlags.Shadows;
            m_renderState.WireFrameColor = Color.DarkBlue;
            m_renderState.SelectionColor = Color.FromArgb(66, 255, 161);
            BackColor = SystemColors.ControlDark;
            m_renderState.Changed += (sender, e) => Invalidate();
        }

        public ulong SurfaceId
        {
            get;
            private set;
        }

        public RenderState RenderState
        {
            get { return m_renderState; }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            Size sz = ClientSize;
            if (!GameEngine.IsInError && SurfaceId != 0 && sz.Width > 0 && sz.Height > 0)
            {
                GameEngine.SetObjectProperty(swapChainId, SurfaceId, SizePropId, sz);
                Camera.Aspect = (float)sz.Width / (float)sz.Height;
            }            
        }
        protected override void Dispose(bool disposing)
        {
            GameEngine.DestroyObject(swapChainId, SurfaceId);
            SurfaceId = 0;
            if (disposing)
            {
                m_renderState.Dispose();
            }
            base.Dispose(disposing);
        }
        
        protected override IList<object> Pick(MouseEventArgs e)
        {           
            bool multiSelect = DragOverThreshold;
            List<object> paths = new List<object>();

            HitRecord[] hits;


            if(multiSelect)
            {// frustum pick                
                RectangleF rect = MakeRect(FirstMousePoint, CurrentMousePoint);
                hits = GameEngine.FrustumPick(SurfaceId, Camera.ViewMatrix, Camera.ProjectionMatrix, rect);
            }
            else
            {// ray pick
                Ray3F rayW = GetWorldRay(CurrentMousePoint);
                hits = GameEngine.RayPick(Camera.ViewMatrix, Camera.ProjectionMatrix, rayW, false);
            }

            // create unique list of hits
            HashSet<ulong> instanceSet = new HashSet<ulong>();
            List<HitRecord> uniqueHits = new List<HitRecord>();
            // build 'path' objects for each hit record.
            foreach (HitRecord hit in hits)
            {
                bool added = instanceSet.Add(hit.instanceId);
                if (added) uniqueHits.Add(hit);
            }

            HitRecord firstHit = new HitRecord();
            

            // build 'path' objects for each hit record.
            foreach (HitRecord hit in uniqueHits)
            {
                NativeObjectAdapter nobj = GameEngine.GetAdapterFromId(hit.instanceId);
                DomNode dom = nobj.DomNode;
                object hitPath = Util.AdaptDomPath(dom);
                object obj = DesignView.PickFilter.Filter(hitPath, e);
                if (obj != null)
                {
                    if (paths.Count == 0)
                    {
                        firstHit = hit;
                    }
                    var newPath = obj as AdaptablePath<object> ?? Util.AdaptDomPath((DomNode)obj);
                    paths.Add(newPath);
                }
            }


            if (multiSelect == false && paths.Count > 0)                
            {
                var path = paths[0];
                ISelectionContext selection = DesignView.Context.As<ISelectionContext>();
                ILinear linear = path.As<ILinear>();
                if (linear != null
                    && Control.ModifierKeys == System.Windows.Forms.Keys.Shift
                    && selection.SelectionContains(path))
                {
                    ITransactionContext trans = DesignView.Context.As<ITransactionContext>();
                    trans.DoTransaction(
                        delegate
                        {
                            linear.InsertPoint(firstHit.index, firstHit.hitPt.X, firstHit.hitPt.Y, firstHit.hitPt.Z);
                        }, "insert control point".Localize()
                        );
                }
            }
            return paths;
        }
        
        private IGame TargetGame()
        {            
            var selection = DesignView.Context.As<ISelectionContext>();
            DomNode node = selection.GetLastSelected<DomNode>();
                      
            IReference<IGame> gameref = Adapters.As<IReference<IGame>>(node);
            if (gameref != null && gameref.Target != null)
                return gameref.Target;  
                      
            if(node != null)
                return node.GetRoot().As<IGame>(); 
            
            return DesignView.Context.As<IGame>();
            
        }

        private readonly List<DomNode> m_ghosts = new List<DomNode>();
        
        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            base.OnDragEnter(drgevent);
            IGame dragDropTarget = TargetGame();

            if (dragDropTarget.RootGameObjectFolder.IsLocked)
            {
                drgevent.Effect = DragDropEffects.None;                
                return;
            }

            IGameObjectFolder rootFolder = dragDropTarget.RootGameObjectFolder;            
            m_ghosts.Clear();

            ResourceConverterService resourceConverter = Globals.MEFContainer.GetExportedValue<ResourceConverterService>();


            IEnumerable<object> nodes = Util.ConvertData(drgevent.Data, false);
                                   
            foreach (object iterNode in nodes)
            {
                DomNode node = iterNode as DomNode;
                if (node == null)
                {                 
                    if (resourceConverter != null)
                    {
                        IGameObject resGob = resourceConverter.Convert(iterNode as IResource);
                        node = resGob.As<DomNode>();                    
                    }                    
                }
                
                IGameObject gob = node.As<IGameObject>();
                if (gob == null || node.GetRoot().Is<IGame>())
                    continue;
                
                node.InitializeExtensions();
                m_ghosts.Add(node);
                rootFolder.GameObjects.Add(gob);                                             
            }

            drgevent.Effect = m_ghosts.Count > 0 ? DragDropEffects.Move | DragDropEffects.Link
            : DragDropEffects.None;
        }
        
        protected override void OnDragOver(DragEventArgs drgevent)
        {        
            base.OnDragOver(drgevent);
            if (DesignView.Context == null
                || m_ghosts.Count == 0) return;

            
            Point clientPoint = PointToClient(new Point(drgevent.X, drgevent.Y));
            Ray3F rayw = GetWorldRay(clientPoint);

            bool shiftPressed = Control.ModifierKeys == Keys.Shift;

            DomNode hitnode = null;
            HitRecord? hit = null;
            if(shiftPressed)
            {
                Matrix4F v = Camera.ViewMatrix;
                Matrix4F p = Camera.ProjectionMatrix;
                HitRecord[] hits = GameEngine.RayPick(v, p, rayw, false);
                foreach (HitRecord ht in hits)
                {
                    hitnode = GameEngine.GetAdapterFromId(ht.instanceId).Cast<DomNode>();

                    bool skip = false;
                    // ignore ghosts
                    foreach (DomNode node in m_ghosts)
                    {
                        if (hitnode == node || hitnode.IsDescendantOf(node))
                        {
                            skip = true;
                            break;
                        }
                    }
                    if (skip) continue;                                        
                    hit = ht;
                    break;
                }
            }
            
            ISnapFilter snapFilter = Globals.MEFContainer.GetExportedValue<ISnapFilter>();
            bool snap = (shiftPressed && hit.HasValue);
            foreach (DomNode ghost in m_ghosts)
            {
                HitRecord? hr = (snap && ( snapFilter == null || snapFilter.CanSnapTo(ghost,hitnode))) ? hit : null;
                ProjectGhost(ghost, rayw, hr);
            }

            GameLoop.Update();
            GameLoop.Render();            
        }
        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            base.OnDragDrop(drgevent);
            if (DesignView.Context == null) return;

            if (m_ghosts.Count > 0)
            {
                IGame dragDropTarget = TargetGame();
                IGameObjectFolder rootFolder = dragDropTarget.RootGameObjectFolder;

                foreach (DomNode ghost in m_ghosts)
                {
                    rootFolder.GameObjects.Remove(ghost.As<IGameObject>());
                }                                
                ApplicationUtil.Insert(
                    dragDropTarget,
                    rootFolder.Cast<DomNode>(),
                    m_ghosts,
                    "Drag and Drop",
                    null);

                m_ghosts.Clear();                
                DesignView.InvalidateViews();
            }
        }

        protected override void OnDragLeave(EventArgs e)
        {
            base.OnDragLeave(e);
            if (DesignView.Context == null) return;

            if (m_ghosts.Count > 0)
            {
                IGame dragDropTarget = TargetGame();
                IGameObjectFolder rootFolder = dragDropTarget.RootGameObjectFolder;
                foreach (DomNode ghost in m_ghosts)
                {
                    // reset translation.
                    ITransformable xformnode = ghost.As<ITransformable>();
                    xformnode.Translation = new Vec3F(0, 0, 0);

                    rootFolder.GameObjects.Remove(ghost.As<IGameObject>());
                }      
                
                m_ghosts.Clear();                
                DesignView.InvalidateViews();
            }

        }
        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                if (DesignView.Context == null || GameEngine.IsInError
                    || GameLoop == null)
                {
                    e.Graphics.Clear(DesignView.BackColor);
                    if (GameEngine.IsInError)
                        e.Graphics.DrawString(GameEngine.CriticalError, Font, Brushes.Red, 1, 1);
                    return;
                }
                GameLoop.Update();
                Render();                
            }
            catch(Exception ex)
            {
                e.Graphics.DrawString(ex.Message, Font, Brushes.Red, 1, 1);
            }            
        }
      
        // render the scene.
        public override void Render()
        {
            bool skipRender =
                SurfaceId == 0
                || GameEngine.IsInError
                || Width == 0
                || Height == 0
                || DesignView.Context == null;

            if (skipRender)
                return;

            m_clk.Start();
            GameEngine.SetObjectProperty(swapChainId, SurfaceId, BkgColorPropId, DesignView.BackColor);
            GameEngine.SetRenderState(RenderState);
            GameEngine.Begin(SurfaceId, Camera.ViewMatrix, Camera.ProjectionMatrix);

            IGame game = DesignView.Context.As<IGame>();
            GridRenderer gridRender = game.Grid.Cast<GridRenderer>();
            gridRender.Render(Camera);
          
            GameEngine.RenderGame();

            bool renderSelected = RenderState.DisplayBound == DisplayFlagModes.Selection
                || RenderState.DisplayCaption == DisplayFlagModes.Selection
                || RenderState.DisplayPivot == DisplayFlagModes.Selection;

            if (renderSelected)
            {
                var selection = DesignView.Context.As<ISelectionContext>().Selection;
                IEnumerable<DomNode> rootDomNodes = DomNode.GetRoots(selection.AsIEnumerable<DomNode>());
                RenderProperties(rootDomNodes,
                    RenderState.DisplayCaption == DisplayFlagModes.Selection,
                    RenderState.DisplayBound == DisplayFlagModes.Selection,
                    RenderState.DisplayPivot == DisplayFlagModes.Selection);
            }

            RenderProperties(Items,
                   RenderState.DisplayCaption == DisplayFlagModes.Always,
                   RenderState.DisplayBound == DisplayFlagModes.Always,
                   RenderState.DisplayPivot == DisplayFlagModes.Always);

            GameEngine.SetRendererFlag(BasicRendererFlags.Foreground | BasicRendererFlags.Lit);            
            if (DesignView.Manipulator != null)
                DesignView.Manipulator.Render(this);
                                  
            string str = string.Format("View Type: {0}   time per frame-render call: {1:0.00} ms", ViewType, m_clk.Milliseconds);
            GameEngine.DrawText2D(str, Util3D.CaptionFont, 1,1, Color.White);          
            GameEngine.End();

            if (IsPicking)
            {// todo: use Directx to draw marque.                
                using (Graphics g = CreateGraphics())
                {
                    Rectangle rect = MakeRect(FirstMousePoint, CurrentMousePoint);
                    if (rect.Width > 0 && rect.Height > 0)
                    {
                        g.DrawRectangle(s_marqueePen, rect);
                    }
                }
            }
            
        }
        
        private void RenderProperties(IEnumerable<object> objects, bool renderCaption, bool renderBound, bool renderPivot)
        {                      
            if (renderCaption || renderBound)
            {
                Util3D.RenderFlag = BasicRendererFlags.WireFrame;
                Matrix4F vp = Camera.ViewMatrix * Camera.ProjectionMatrix;
                foreach (object obj in objects)
                {
                    IBoundable bnode = obj.As<IBoundable>();
                    if (bnode == null || bnode.BoundingBox.IsEmpty || obj.Is<IGameObjectFolder>()) continue;

                    INameable nnode = obj.As<INameable>();
                    ITransformable trans = obj.As<ITransformable>();

                    if (renderBound)
                    {
                        Util3D.DrawAABB(bnode.BoundingBox);
                    }
                    if (renderCaption && nnode != null)
                    {
                        Vec3F topCenter = bnode.BoundingBox.Center;
                        topCenter.Y = bnode.BoundingBox.Max.Y;
                        Point pt = Project(vp, topCenter);
                        GameEngine.DrawText2D(nnode.Name, Util3D.CaptionFont, pt.X, pt.Y, Color.White);
                    }
                }
            }

            if (renderPivot)
            {
                Util3D.RenderFlag = BasicRendererFlags.WireFrame | BasicRendererFlags.DisableDepthTest;

                // create few temp matrics to
                Matrix4F toWorld = new Matrix4F();
                Matrix4F PV = new Matrix4F();
                Matrix4F sc = new Matrix4F();
                Matrix4F bl = new Matrix4F();
                Matrix4F recXform = new Matrix4F();
                foreach (object obj in objects)
                {
                    ITransformable trans = obj.As<ITransformable>();
                    IBoundable bnode = obj.As<IBoundable>();
                    if (trans == null || bnode == null || bnode.BoundingBox.IsEmpty || obj.Is<IGameObjectFolder>()) continue;

                    Path<DomNode> path = new Path<DomNode>(trans.Cast<DomNode>().GetPath());
                    toWorld.Set(Vec3F.ZeroVector);
                    TransformUtils.CalcPathTransform(toWorld, path, path.Count - 1);

                    // Offset by pivot
                    PV.Set(trans.Pivot);
                    toWorld.Mul(PV, toWorld);
                    Vec3F pos = toWorld.Translation;

                    const float pivotDiameter = 16; // in pixels
                    float s = Util.CalcAxisScale(Camera, pos, pivotDiameter, Height);                    
                    sc.Scale(s);
                    Util.CreateBillboard(bl, pos, Camera.WorldEye, Camera.Up, Camera.LookAt);
                    recXform = sc * bl;
                    Util3D.DrawPivot(recXform, Color.Yellow);
                }
            }
        }
       
        private IEnumerable<DomNode> Items
        {
            get
            {
                IGameDocumentRegistry gameDocumentRegistry = Globals.MEFContainer.GetExportedValue<IGameDocumentRegistry>();
                DomNode folderNode = gameDocumentRegistry.MasterDocument.RootGameObjectFolder.Cast<DomNode>();
                foreach (DomNode childNode in folderNode.Subtree)
                {
                    yield return childNode;
                }

                foreach (IGameDocument subDoc in gameDocumentRegistry.SubDocuments)
                {
                    folderNode = subDoc.RootGameObjectFolder.Cast<DomNode>();
                    foreach (DomNode childNode in folderNode.Subtree)
                    {
                        yield return childNode;
                    }
                }
            }
        }
        
        private Rectangle MakeRect(Point p1, Point p2)
        {
            int minx = Math.Min(p1.X, p2.X);
            int miny = Math.Min(p1.Y, p2.Y);
            int maxx = Math.Max(p1.X, p2.X);
            int maxy = Math.Max(p1.Y, p2.Y);
            int w = maxx - minx;
            int h = maxy - miny;
            return new Rectangle(minx, miny, w, h);
        }
        
        /// <summary>
        /// Projects the ghost</summary>
        private void ProjectGhost(DomNode ghost, 
            Ray3F rayw,
            HitRecord? hit)
        {

            ITransformable xformnode = ghost.Cast<ITransformable>();
            IBoundable bnode  = ghost.As<IBoundable>();
            AABB box = bnode.BoundingBox;
            
            Vec3F pt;
            if (hit.HasValue && hit.Value.hasNormal)
            {
                Vec3F rad = box.Radius;
                Vec3F norm = hit.Value.normal;
                Vec3F absNorm = Vec3F.Abs(norm);
                Vec3F offset = Vec3F.ZeroVector;
                
                if (absNorm.X > absNorm.Y)
                {
                    if (absNorm.X > absNorm.Z)
                        offset.X = norm.X > 0 ? rad.X : -rad.X;
                    else
                        offset.Z = norm.Z > 0 ? rad.Z : -rad.Z;                        
                }
                else
                {
                    if (absNorm.Y > absNorm.Z)
                        offset.Y = norm.Y > 0 ? rad.Y : -rad.Y;
                    else
                        offset.Z = norm.Z > 0 ? rad.Z : -rad.Z;                        
                        
                }                
                Vec3F localCenter = box.Center - xformnode.Translation;
                pt = hit.Value.hitPt + (offset - localCenter);
            }
            else
            {
                float offset = 6.0f * box.Radius.Length;
                pt = rayw.Origin + offset * rayw.Direction;
            }
                                          
            if (ViewType == ViewTypes.Front)
                pt.Z = 0.0f;
            else if (ViewType == ViewTypes.Top)
                pt.Y = 0.0f;
            else if (ViewType == ViewTypes.Left)
                pt.X = 0.0f;           
            xformnode.Translation = pt;

        }
        
        private static Pen s_marqueePen;
        private Clock m_clk = new Clock();        
        private RenderState m_renderState;
        private readonly uint swapChainId;
        private readonly uint SizePropId;
        private readonly uint BkgColorPropId;
    }
}
