//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.


using LevelEditorCore;
namespace LevelEditor.DomNodeAdapters
{
    public class Billboard  : GameObject
    {
        protected override void OnNodeSet()
        {
            base.OnNodeSet();
            TransformationType = TransformationTypes.Translation | TransformationTypes.Scale;
        }     
    }
}
