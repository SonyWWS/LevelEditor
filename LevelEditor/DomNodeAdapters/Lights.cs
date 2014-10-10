//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.


using System.Drawing;


using Sce.Atf.VectorMath;

using LevelEditorCore;

namespace LevelEditor.DomNodeAdapters
{
    public class DirLight : GameObject
    {
        protected override void OnNodeSet()
        {
            base.OnNodeSet();
            DomNodeUtil.SetVector(DomNode, Schema.gameObjectType.scaleAttribute, new Vec3F(0.4f, 0.4f, 0.4f)); 
            UpdateTransform();
            TransformationType = TransformationTypes.Translation;
        }

        /// <summary>
        /// Gets/Sets ambient color.</summary>
        public Color Ambient
        {
            get
            {
                int color = GetAttribute<int>(Schema.DirLight.ambientAttribute);
                return Color.FromArgb(color);
            }
            set
            {
                int color = value.ToArgb();
                SetAttribute(Schema.DirLight.ambientAttribute, color);
            }
        }

        /// <summary>
        /// Gets/Sets diffuse color</summary>
        public Color Diffuse
        {
            get
            {
                int color = GetAttribute<int>(Schema.DirLight.diffuseAttribute);
                return Color.FromArgb(color);
            }
            set
            {
                int color = value.ToArgb();
                SetAttribute(Schema.DirLight.diffuseAttribute, color);
            }
        }

        /// <summary>
        /// Gets/Sets specular color.</summary>
        public Color Specular
        {
            get
            {
                int color = GetAttribute<int>(Schema.DirLight.specularAttribute);
                return Color.FromArgb(color);
            }
            set
            {
                int color = value.ToArgb();
                SetAttribute(Schema.DirLight.specularAttribute, color);
            }

        }

        /// <summary>
        /// Gets/Sets direction that the light is pointing to.</summary>
        public Vec3F Direction
        {
            get { return DomNodeUtil.GetVector(DomNode, Schema.DirLight.directionAttribute); }
            set 
            {
                Vec3F dir = value;
                dir.Normalize();
                DomNodeUtil.SetVector(DomNode, Schema.DirLight.directionAttribute, dir);
            }
        }      
    }

    public class PointLight : GameObject
    {
        protected override void OnNodeSet()
        {
            base.OnNodeSet();
            DomNodeUtil.SetVector(DomNode, Schema.gameObjectType.scaleAttribute, new Vec3F(0.4f, 0.4f, 0.4f)); 
            UpdateTransform();
            TransformationType = TransformationTypes.Translation;            
        }
    }

    public class BoxLight : GameObject
    {
        protected override void OnNodeSet()
        {
            base.OnNodeSet();                        
            TransformationType = TransformationTypes.Translation | TransformationTypes.Scale;
        }
    }
}
