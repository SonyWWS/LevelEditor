//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Dom;

namespace RenderingInterop
{
    /// <summary>
    /// Similar to Dom AttributeInfo.
    /// Holds information about native property that only exist in 
    /// native side.
    /// An arrary of this class will be set as a tag on DomNodeType.
    /// </summary>
    class NativeAttributeInfo
    {

        public NativeAttributeInfo(DomNodeType type, string name, uint typeId, uint propId)
        {
            DefiningType = type;
            Name = name;
            TypeId = typeId;
            PropertyId = propId;
        }


        public readonly DomNodeType DefiningType;
        public readonly string Name;
        public readonly uint TypeId;
        public readonly uint PropertyId;
    }
}
