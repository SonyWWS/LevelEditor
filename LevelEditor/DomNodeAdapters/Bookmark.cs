//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf.VectorMath;
using Sce.Atf.Dom;
using Sce.Atf.Adaptation;

using ViewTypes = Sce.Atf.Rendering.ViewTypes;
using Camera = Sce.Atf.Rendering.Camera;

using LevelEditorCore;

namespace LevelEditor.DomNodeAdapters
{
    /// <summary>
    /// A named camera view that the user can save and recall from the Bookmark Lister.
    /// The Bookmarks are persisted in the document file because they're stored in the
    /// Dom Repository.
    /// </summary>
    public class Bookmark : DomNodeAdapter, INameable
    {
        protected override void OnNodeSet()
        {
            base.OnNodeSet();
            m_bookmarks = GetChildList<Bookmark>(Schema.bookmarkType.bookmarkChild);

            DomNode domCam = this.DomNode.GetChild(Schema.bookmarkType.cameraChild);
            if (domCam == null)
            {
                domCam = new DomNode(Schema.cameraType.Type);
                this.DomNode.SetChild(Schema.bookmarkType.cameraChild, domCam);
            }
        }


        /// <summary>
        /// set: Sets the internal DomObject representation of a camera to a copy of the given camera.
        /// get: Returns a new Camera object representing the private DomObject version of a camera.
        /// </summary>
        public Camera Camera
        {
            get
            {                                                
                DomNode domCam = this.DomNode.GetChild(Schema.bookmarkType.cameraChild);
                                                  
                // get the state from the DomObject representation of the camera.
                ViewTypes viewType = (ViewTypes)domCam.GetAttribute(Schema.cameraType.viewTypeAttribute);                
                Vec3F eye = DomNodeUtil.GetVector(domCam, Schema.cameraType.eyeAttribute);
                Vec3F lookAtPoint = DomNodeUtil.GetVector(domCam, Schema.cameraType.lookAtPointAttribute);
                Vec3F upVector = DomNodeUtil.GetVector(domCam, Schema.cameraType.upVectorAttribute);
                float yFov = (float)domCam.GetAttribute(Schema.cameraType.yFovAttribute);
                float nearZ = (float)domCam.GetAttribute(Schema.cameraType.nearZAttribute);
                float farZ = (float)domCam.GetAttribute(Schema.cameraType.farZAttribute);
                float focusRadius = (float)domCam.GetAttribute(Schema.cameraType.focusRadiusAttribute);

                // create the camera and initialize it and return it.
                Camera newCamera = new Camera();
                newCamera.SetState(viewType, eye, lookAtPoint, upVector, yFov, nearZ, farZ, focusRadius);
                return newCamera;
            }
            set
            {
                // get the state of the given camera
                ViewTypes viewType;
                Vec3F eye;
                Vec3F lookAtPoint;
                Vec3F upVector;
                float yFov;
                float nearZ;
                float farZ;
                float focusRadius;
                value.GetState(out viewType, out eye, out lookAtPoint, out upVector, out yFov,
                    out nearZ, out farZ, out focusRadius);
                               
                DomNode domCam = this.DomNode.GetChild(Schema.bookmarkType.cameraChild);

                domCam.SetAttribute(Schema.cameraType.viewTypeAttribute, (int)viewType);
                DomNodeUtil.SetVector(domCam,Schema.cameraType.eyeAttribute, eye);
                DomNodeUtil.SetVector(domCam,Schema.cameraType.lookAtPointAttribute, lookAtPoint);
                DomNodeUtil.SetVector(domCam, Schema.cameraType.upVectorAttribute, upVector);
                domCam.SetAttribute(Schema.cameraType.yFovAttribute, yFov);
                domCam.SetAttribute(Schema.cameraType.nearZAttribute, nearZ);
                domCam.SetAttribute(Schema.cameraType.farZAttribute, farZ);
                domCam.SetAttribute(Schema.cameraType.focusRadiusAttribute, focusRadius);

            }
        }
        
        /// <summary>
        /// Gets the list of sub Bookmarks</summary>
        public IList<Bookmark> Bookmarks
        {
            get { return m_bookmarks; }
        }

        #region INameable Members
        /// <summary>
        /// Gets and sets the state name</summary>
        public string Name
        {
            get { return (string)DomNode.GetAttribute(Schema.bookmarkType.nameAttribute); }
            set { DomNode.SetAttribute(Schema.bookmarkType.nameAttribute, value); }
        }

        public Bookmark CreateNew()
        {
            DomNode domNode = new DomNode(Schema.bookmarkType.Type);
            return DomNode.As<Bookmark>();           
        }
        private IList<Bookmark> m_bookmarks;

        #endregion
    }
}
