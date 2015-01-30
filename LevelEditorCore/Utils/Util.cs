//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;

using Sce.Atf;
using Sce.Atf.VectorMath;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.Rendering;

using Vector3 = Sce.Atf.VectorMath.Vec3F;

namespace LevelEditorCore
{
    /// <summary>
    /// LevelEditor utility methods</summary>
    /// <remarks>These methods did not naturally fit into an existing class or service
    /// and were therefore added here to avoid duplicated code. They should be reviewed
    /// from time to time and moved into a more fitting class or service if possible.
    /// General, application-independent and frequently used methods should be considered
    /// for promotion into ATF.</remarks>
    public static class Util
    {

        /// <summary>
        /// Gets the lineage of this object, starting with the object, and ending with
        /// the root object.</summary>
        public static IEnumerable<DomNode> Lineage(object obj)
        {
            Path<object> path = obj as Path<object>;
            if (path != null)
            {
                for (int index = path.Count - 1; index >= 0; index--)
                {
                    DomNode node = path[index] as DomNode;
                    if (node != null)
                        yield return node;
                }
            }
            else
            {
                DomNode node = obj as DomNode;
                foreach (DomNode childNode in node.Lineage)
                {
                    yield return childNode;
                }
            }
        }

        /// <summary>
        /// Gets the path of a DomNode and converts it to an AdaptablePath<object></summary>
        /// <param name="domNode">DomNode</param>
        /// <returns>Object path to the specified DomNode</returns>
        public static AdaptablePath<object> AdaptDomPath(DomNode domNode)
        {
            List<object> path = new List<object>();
            while (domNode != null)
            {
                foreach (DomNode pnode in domNode.Lineage)
                    path.Add(pnode);

                domNode = null;
                IGame game = Adapters.As<IGame>(path.Last());
                if (game != null && game.Parent != null)
                {
                    DomNode gameRef = game.Parent.Cast<DomNode>();                    
                    path[path.Count - 1] = gameRef;
                    domNode = gameRef.Parent;                    
                }
            }
            path.Reverse();            
            AdaptablePath<object> adaptablePath = new AdaptablePath<object>(path);
            return adaptablePath;
        }


       

        /// <summary>
        /// Conversion function for drag-drop operations</summary>
        /// <param name="sourceObject">source object</param>        
        /// <returns>IEnumerable of objects </returns>
        public static IEnumerable<object> ConvertData(object sourceObject, bool copydataobject)
        {
            IResourceService resourceService = Globals.ResourceService;
            IEnumerable<object> objectlist = sourceObject as IEnumerable<object>;
            IDataObject dataObject = sourceObject as IDataObject;
            DomNode domNode = Adapters.As<DomNode>(sourceObject);
            if (domNode != null)
            {
                yield return domNode;
            }
            else if (objectlist != null)
            {
                foreach (object obj in objectlist)
                    yield return obj;
            }
            else if (dataObject != null)
            {                               
                DomNode[] domNodes = dataObject.GetData(typeof(DomNode[])) as DomNode[];
                if (domNodes != null)
                {
                    DomNode[] copies = copydataobject ? DomNode.Copy(domNodes) : domNodes;
                    foreach (DomNode node in copies)
                        yield return node;
                }

                object[] objects = dataObject.GetData(typeof(object[])) as object[];
                if (objects != null)
                {
                    foreach (object node in objects)
                        yield return node;                    
                }

                string[] files = dataObject.GetData(DataFormats.FileDrop) as string[];
                if (files != null && resourceService != null)
                {
                    foreach (string file in files)
                    {
                        IResource resource = resourceService.Load(new Uri(file));
                        if (resource != null)
                            yield return resource; 
                    }
                }
            }            
        }

        /// <summary>
        /// Gets the palette image for the specified type</summary>
        /// <remarks>If there is no image for the specified type, it iterates
        /// through the base class hierarchy to find one</remarks>
        /// <param name="nodeType">DomNodeType</param>
        /// <param name="imageList">ImageList to use for finding the image</param>
        /// <returns>Index of the image if found, -1 if unsuccessful</returns>
        public static int GetTypeImageIndex(DomNodeType nodeType, ImageList imageList)
        {
            int imageIndex = -1;
            if (nodeType != null)
            {
                NodeTypePaletteItem paletteItem = nodeType.GetTag<NodeTypePaletteItem>();
                if (paletteItem != null)
                    imageIndex = imageList.Images.IndexOfKey(paletteItem.ImageName);
            }            
            return imageIndex;
        }


        /// <summary>
        /// Creates Billboard matrix from the given parameters.</summary>        
        public static void CreateBillboard(Matrix4F matrix, Vec3F objectPos, Vec3F camPos, Vec3F camUp, Vector3 camLook)
        {

            Vector3 look = camPos - objectPos;
            float len = look.LengthSquared;
            if (len < 0.0001f)
            {
                look = -camLook;
            }
            else
            {
                look.Normalize();
            }

            Vector3 right = Vec3F.Cross(camUp, look);
            right.Normalize();
            Vector3 up = Vec3F.Cross(look, right);

            matrix.M11 = right.X;
            matrix.M12 = right.Y;
            matrix.M13 = right.Z;
            matrix.M14 = 0f;
            matrix.M21 = up.X;
            matrix.M22 = up.Y;
            matrix.M23 = up.Z;
            matrix.M24 = 0f;
            matrix.M31 = look.X;
            matrix.M32 = look.Y;
            matrix.M33 = look.Z;
            matrix.M34 = 0f;
            matrix.M41 = objectPos.X;
            matrix.M42 = objectPos.Y;
            matrix.M43 = objectPos.Z;
            matrix.M44 = 1f;            
        }


        /// <summary>
        /// Creates Billboard matrix from the given parameters.</summary>        
        public static Matrix4F CreateBillboard(Vec3F objectPos, Vec3F camPos, Vec3F camUp, Vector3 camLook)
        {
            Matrix4F matrix = new Matrix4F();
            CreateBillboard(matrix, objectPos, camPos, camUp, camLook);
            return matrix;
        }


        /// <summary>
        /// Calculates required scale such that when it applied to an 
        /// object. The object maintain a constant size in pixel regardless of 
        /// its distance from camera.
        /// Used for computing size of 3d manipulators.</summary>
        /// <param name="camera"></param>
        /// <param name="objectPosW"></param>
        /// <param name="sizeInPixels"></param>
        /// <param name="viewHeight"></param>        
        public static float CalcAxisScale(Camera camera,
            Vec3F objectPosW,
            float sizeInPixels,
            float viewHeight)
        {
            float worldHeight;
            // World height on origin's z value
            if (camera.Frustum.IsOrtho)
            {
                worldHeight = (camera.Frustum.Top - camera.Frustum.Bottom);
            }
            else
            {
                Matrix4F view = camera.ViewMatrix;
                Vec3F objPosV;
                view.Transform(objectPosW, out objPosV);
                worldHeight = 2.0f * Math.Abs(objPosV.Z) * (float)Math.Tan(camera.Frustum.FovY / 2.0f);
            }
            return sizeInPixels * (worldHeight / viewHeight);
        }
        
        public static void ZoomOnSphere(Camera cam, Sphere3F sphere)
        {
            float nearZ = cam.PerspectiveNearZ;
            // todo refactor cam.ZoomOnSphere() 
            cam.ZoomOnSphere(sphere);
            cam.PerspectiveNearZ = nearZ;
        }

        public static string GetFilePath(string filter,string initialDir, bool isNew)
        {
            string filePath = null;

            if (Globals.AutomationMode)
            {
                filePath = Globals.AutomationName;
            }
            else
            {
                IWin32Window owner = Globals.MEFContainer.GetExportedValue<IWin32Window>();
                if (isNew)
                {
                    using (SaveFileDialog dlg = new SaveFileDialog())
                    {
                        dlg.Filter = filter;
                        dlg.OverwritePrompt = true;
                        dlg.InitialDirectory = initialDir;
                        if (dlg.ShowDialog(owner) == DialogResult.OK)
                            filePath = dlg.FileName;
                    }
                }
                else
                {
                    using (OpenFileDialog dlg = new OpenFileDialog())
                    {
                        dlg.Filter = filter;
                        dlg.CheckFileExists = true;
                        dlg.InitialDirectory = initialDir;
                        if (dlg.ShowDialog(owner) == DialogResult.OK)
                            filePath = dlg.FileName;
                    }
                }
            }
            return filePath;            
        }


        /// <summary>
        /// Find all the DomNodes of the type that is equal or derived 
        /// from the given type.
        /// if exact is true then only find DomNode that have exact type.        
        /// </summary>
        /// <param name="type">DomNodeType</param>
        /// <param name="exact">if true then only consider exact match,
        /// otherwise match any type that is equal or derived from the given type</param>
        /// <returns></returns>
        public static IEnumerable<DomNode> FindAll(DomNodeType type, bool exact = false)
        {
            if (type != null)
            {
                IGameDocumentRegistry gameDocumentRegistry = Globals.MEFContainer.GetExportedValue<IGameDocumentRegistry>();
                if (gameDocumentRegistry != null)
                {
                    foreach (IGameDocument doc in gameDocumentRegistry.Documents)
                    {
                        DomNode folderNode = doc.RootGameObjectFolder.Cast<DomNode>();
                        foreach (DomNode childNode in folderNode.Subtree)
                        {
                            if ((exact && childNode.Type == type)
                                || (!exact && type.IsAssignableFrom(childNode.Type)))
                                yield return childNode;
                        }
                    }
                }
            }
        }
    }
}
