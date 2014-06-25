//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows.Forms;

namespace LevelEditorCore
{
    public class ManipulatorInfo
    {
        public ManipulatorInfo(string name, string description, string image, Keys key)
        {
            Name = name;
            Description = description;
            Image = image;
            ShortCut = key;
        }

        public readonly string Name;
        public readonly string Description;
        public readonly string Image;
        public readonly Keys ShortCut;
    }
}
