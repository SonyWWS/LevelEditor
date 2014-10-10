//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;
using Sce.Atf.VectorMath;

using LevelEditorCore;

using Camera = Sce.Atf.Rendering.Camera;
using ViewTypes = Sce.Atf.Rendering.ViewTypes;
using AxisSystemType = Sce.Atf.Rendering.Dom.AxisSystemType;


namespace RenderingInterop
{
    using HitRegion = TranslatorControl.HitRegion;

    [Export(typeof(IManipulator))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TranslateManipulator : Manipulator
    {
        public TranslateManipulator()
        {
            ManipulatorInfo = new ManipulatorInfo("Move".Localize(),
                                         "Activate move manipulator".Localize(),
                                         LevelEditorCore.Resources.TranslateImage,
                                         Keys.M);
            m_translatorControl = new TranslatorControl();
        }

        #region Implementation of IManipulator



        public override bool Pick(ViewControl vc, Point scrPt)
        {
            m_hitRegion = HitRegion.None;
            if (base.Pick(vc, scrPt) == false)
                return false;

            Camera camera = vc.Camera;
            float s;
            Util.CalcAxisLengths(camera, HitMatrix.Translation, out s);
            Matrix4F view = camera.ViewMatrix;
            Matrix4F vp = view  * camera.ProjectionMatrix;
            Matrix4F wvp = HitMatrix * vp;
            
            Ray3F rayL = vc.GetRay(scrPt,wvp);

            m_hitRegion = m_translatorControl.Pick(HitMatrix, view, rayL, HitRayV, s);
            
            bool picked = m_hitRegion != HitRegion.None;                      
            return picked;
        }

        public override void Render(ViewControl vc)
        {                                                
            Matrix4F normWorld = GetManipulatorMatrix();
            if (normWorld == null) return;
            float s;
            Util.CalcAxisLengths(vc.Camera, normWorld.Translation, out s);
            m_translatorControl.Render(normWorld, s);        
        }

        public override void OnBeginDrag()
        {
            if (m_hitRegion == HitRegion.None)
                return;

            m_cancelDrag = false;
            Clear(); // cached values.

            var selectionContext = DesignView.Context.As<ISelectionContext>();
            var selection = selectionContext.Selection;
            var transactionContext = DesignView.Context.As<ITransactionContext>();

            IEnumerable<DomNode> rootDomNodes = DomNode.GetRoots(selection.AsIEnumerable<DomNode>());
            m_duplicating = Control.ModifierKeys == m_duplicateKey;

            if (m_duplicating)
            { 
                List<DomNode> originals = new List<DomNode>();
                foreach (DomNode node in rootDomNodes)
                {
                    ITransformable transformable = node.As<ITransformable>();
                    if (!CanManipulate(transformable)) continue;
                    
                    originals.Add(node);                    
                }
                if (originals.Count > 0)
                {
                    DomNode[] copies = DomNode.Copy(originals);

                    transactionContext.Begin("Copy And Move".Localize());

                    List<object> newSelection = new List<object>();
                    // re-parent copy
                    for (int i = 0; i < copies.Length; i++)
                    {
                        DomNode copy = copies[i];
                        DomNode original = originals[i];

                        ChildInfo chInfo = original.ChildInfo;
                        if (chInfo.IsList)
                            original.Parent.GetChildList(chInfo).Add(copy);
                        else
                            original.Parent.SetChild(chInfo, copy);

                        newSelection.Add(Util.AdaptDomPath(copy));
                        copy.InitializeExtensions();
                    }

                    selectionContext.SetRange(newSelection);
                    NodeList.AddRange(copies.AsIEnumerable<ITransformable>());
                }
                
            }
            else
            {
                foreach (DomNode node in rootDomNodes)
                {
                    ITransformable transformable = node.As<ITransformable>();
                    if (!CanManipulate(transformable)) continue;                    
                    NodeList.Add(transformable);
                }

                if (NodeList.Count > 0)
                    transactionContext.Begin("Move".Localize());                
            }

            m_originalValues = new Vec3F[NodeList.Count];
            m_originalRotations = new Vec3F[NodeList.Count];            
            for(int k = 0; k < NodeList.Count; k++)
            {
                ITransformable node = NodeList[k];
                IManipulatorNotify notifier = node.As<IManipulatorNotify>();
                if (notifier != null) notifier.OnBeginDrag();
                m_originalValues[k] = node.Translation;
                m_originalRotations[k] = node.Rotation;
            }            
        }


        public override void OnDragging(ViewControl vc, Point scrPt)
        {
            if (m_cancelDrag || m_hitRegion == HitRegion.None || NodeList.Count == 0)
                return;

            bool hitAxis = m_hitRegion == HitRegion.XAxis
                || m_hitRegion == HitRegion.YAxis
                || m_hitRegion == HitRegion.ZAxis;
            
            Matrix4F proj = vc.Camera.ProjectionMatrix;
                      
            // create ray in view space.            
            Ray3F rayV = vc.GetRay(scrPt, proj);
            Vec3F translate = m_translatorControl.OnDragging(rayV);

            ISnapSettings snapSettings = (ISnapSettings)DesignView;            
            bool snapToGeom = Control.ModifierKeys == m_snapGeometryKey;

            if (snapToGeom)
            {
                Matrix4F view = vc.Camera.ViewMatrix;
                Matrix4F vp = view * proj;
                // create ray in world space.
                Ray3F rayW = vc.GetRay(scrPt, vp);

                Vec3F manipPos = HitMatrix.Translation;
                Vec3F manipMove;
                if (hitAxis)
                {
                    //Make rayw to point toward moving axis and starting 
                    // from manipulator’s world position.
                    rayW.Direction = Vec3F.Normalize(translate);
                    rayW.Origin = manipPos;                    
                    manipMove = Vec3F.ZeroVector;
                    m_cancelDrag = true; //stop further snap-to's   
                }
                else
                {
                    manipMove = rayW.ProjectPoint(manipPos) - manipPos;                                       
                }

                for (int i = 0; i < NodeList.Count; i++)
                {
                    ITransformable node = NodeList[i];
                    Vec3F snapOffset = TransformUtils.CalcSnapFromOffset(node, snapSettings.SnapFrom);
                    Path<DomNode> path = new Path<DomNode>(Adapters.Cast<DomNode>(node).GetPath());
                    Matrix4F parentLocalToWorld = TransformUtils.CalcPathTransform(path, path.Count - 2);
                    Vec3F orgPosW;
                    parentLocalToWorld.Transform(m_originalValues[i], out orgPosW);

                    Matrix4F parentWorldToLocal = new Matrix4F();
                    parentWorldToLocal.Invert(parentLocalToWorld);

                    rayW.MoveToIncludePoint(orgPosW + snapOffset + manipMove);
                    
                    HitRecord[] hits = GameEngine.RayPick(view, proj, rayW, true);
                    bool cansnap = false;
                    HitRecord target = new HitRecord();
                    if (hits.Length > 0)
                    {
                        // find hit record.
                        foreach (var hit in hits)
                        {
                            if (m_snapFilter.CanSnapTo(node, GameEngine.GetAdapterFromId(hit.instanceId)))
                            {
                                target = hit;
                                cansnap = true;
                                break;
                            }
                        }
                    }

                    if (cansnap)
                    {
                        Vec3F pos;
                        if (target.hasNearestVert && snapSettings.SnapVertex)
                        {
                            pos = target.nearestVertex;
                        }
                        else
                        {
                            pos = target.hitPt;
                        }

                        pos -= snapOffset;
                        parentWorldToLocal.Transform(ref pos);
                        Vec3F diff = pos - node.Transform.Translation;
                        node.Translation += diff;
                        bool rotateOnSnap = snapSettings.RotateOnSnap
                                           && target.hasNormal
                                           && (node.TransformationType & TransformationTypes.Rotation) != 0;
                        if (rotateOnSnap)
                        {
                            Vec3F localSurfaceNormal;
                            parentWorldToLocal.TransformNormal(target.normal, out localSurfaceNormal);
                            node.Rotation = TransformUtils.RotateToVector(
                                 m_originalRotations[i],
                                 localSurfaceNormal,
                                 AxisSystemType.YIsUp);
                        }
                    }
                }                
            }           
            else
            {
                IGrid grid = DesignView.Context.Cast<IGame>().Grid;
                bool snapToGrid = Control.ModifierKeys == m_snapGridKey
                                 && grid.Visible
                                 && vc.Camera.ViewType == ViewTypes.Perspective;
                float gridHeight = grid.Height;
                // translate.
                for (int i = 0; i < NodeList.Count; i++)
                {
                    ITransformable node = NodeList[i];
                    Path<DomNode> path = new Path<DomNode>(Adapters.Cast<DomNode>(node).GetPath());
                    Matrix4F parentLocalToWorld = TransformUtils.CalcPathTransform(path, path.Count - 2);
                    Matrix4F parentWorldToLocal = new Matrix4F();
                    parentWorldToLocal.Invert(parentLocalToWorld);
                    Vec3F localTranslation;
                    parentWorldToLocal.TransformVector(translate, out localTranslation);
                    Vec3F trans = m_originalValues[i] + localTranslation;
                   
                    if(snapToGrid)
                    {                    
                        if(grid.Snap)
                            trans = grid.SnapPoint(trans);
                        else
                            trans.Y = gridHeight;                    
                    }

                    node.Translation = trans;
                }                
            }
        }

        public override void OnEndDrag(ViewControl vc, Point scrPt)
        {
            if (m_hitRegion != HitRegion.None && NodeList.Count > 0)
            {
                for (int k = 0; k < NodeList.Count; k++)
                {                    
                    IManipulatorNotify notifier = NodeList[k].As<IManipulatorNotify>();
                    if (notifier != null) notifier.OnEndDrag();
                }        

                var transactionContext = DesignView.Context.As<ITransactionContext>();
                try
                {
                    if (transactionContext.InTransaction)
                        transactionContext.End();
                }
                catch (InvalidTransactionException ex)
                {
                    if (transactionContext.InTransaction)
                        transactionContext.Cancel();
                    if (ex.ReportError)
                        Outputs.WriteLine(OutputMessageType.Error, ex.Message);
                }
            }
            m_hitRegion = HitRegion.None;
            m_cancelDrag = false;
            Clear();
        }

        #endregion
          
        
        private bool CanManipulate(ITransformable node)
        {
            bool result = node != null
                  && (node.TransformationType & TransformationTypes.Translation) != 0
                  && node.Cast<IVisible>().Visible
                  && node.Cast<ILockable>().IsLocked == false;
            return result;
        }

        protected override Matrix4F GetManipulatorMatrix()
        {
            ITransformable node = GetManipulatorNode(TransformationTypes.Translation);
            if (node == null ) return null;

            ISnapSettings snapSettings = (ISnapSettings)DesignView;            
            Path<DomNode> path = new Path<DomNode>(node.Cast<DomNode>().GetPath());
            Matrix4F localToWorld = TransformUtils.CalcPathTransform(path, path.Count - 1);
            
            Matrix4F toworld = new Matrix4F();
            if (snapSettings.ManipulateLocalAxis)
            {
                toworld.Set(localToWorld);
                toworld.Normalize(toworld);                
            }
            else
            {
                toworld.Translation = localToWorld.Translation;                
            }

            Vec3F offset = TransformUtils.CalcSnapFromOffset(node, snapSettings.SnapFrom);
          
            // Offset by pivot
            Matrix4F P = new Matrix4F();
            P.Translation = offset;
            toworld.Mul(toworld,P);
                        
            return toworld;
        }

        /// <summary>
        /// Clear local cache</summary>
        private void Clear()
        {            
            NodeList.Clear();
            m_originalValues = null;
            m_originalRotations = null;
        }

        [Import(AllowDefault=false)]
        private ISnapFilter m_snapFilter;
        private TranslatorControl m_translatorControl;
        private HitRegion m_hitRegion = HitRegion.None;        
        private bool m_cancelDrag;
        private bool m_duplicating;        
        private Vec3F[] m_originalValues;
        private Vec3F[] m_originalRotations;                
        private Keys m_snapGridKey = Keys.Control;
        private Keys m_snapGeometryKey = Keys.Shift;
        private Keys m_duplicateKey = Keys.Control | Keys.Shift;              
    }    
}
