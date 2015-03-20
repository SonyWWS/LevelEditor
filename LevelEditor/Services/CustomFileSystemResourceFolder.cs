//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

using Sce.Atf;

using LevelEditorCore;

namespace LevelEditor
{
    public class CustomFileSystemResourceFolder : IFileSystemResourceFolder, IEquatable<CustomFileSystemResourceFolder>
        , IComparable<CustomFileSystemResourceFolder>
    {
        /// <summary>
        /// Compares this to other.</summary>        
        public int CompareTo(CustomFileSystemResourceFolder other)
        {
            return FullPath.CompareTo(other.FullPath);
        }

        /// <summary>
        /// Tests for equality</summary>
        /// <param name="other">Other path</param>
        /// <returns>True iff this path is equivalent to other</returns>
        public bool Equals(CustomFileSystemResourceFolder other)
        {
            if (other == null) return false;
            return FullPath == other.FullPath;
        }

        /// <summary>
        /// Tests object for equality</summary>
        /// <param name="obj">Other object</param>
        /// <returns>True iff this path is equivalent to other object</returns>
        public override bool Equals(Object obj)
        {
            if (obj == null) return false;

            CustomFileSystemResourceFolder other = obj as CustomFileSystemResourceFolder;
            if (other == null) return false;
            return Equals(other);
        }



        /// <summary>
        /// Tests  for equality</summary>
        /// <param name="a">First path</param>
        /// <param name="b">Second path</param>
        /// <returns>True iff paths are equivalent</returns>
        public static bool operator ==(CustomFileSystemResourceFolder a, CustomFileSystemResourceFolder b)
        {
            if ((object)a == null || ((object)b) == null)
                return Object.Equals(a, b);
            return a.Equals(b);
        }

        /// <summary>
        /// Tests paths for inequality</summary>
        /// <param name="a">First path</param>
        /// <param name="b">Second path</param>
        /// <returns>True iff paths are not equivalent</returns>
        public static bool operator !=(CustomFileSystemResourceFolder a, CustomFileSystemResourceFolder b)
        {
            if (a == null || b == null)
                return !Object.Equals(a, b);

            return !(a.Equals(b));
        }



        /// <summary>
        /// Obtains hash code</summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            return FullPath.GetHashCode();            
        }


        /// <summary>
        /// Constructor</summary>
        /// <param name="path">Absolute path of the directory</param>
        public CustomFileSystemResourceFolder(string path)
            : this(path, null)
        {
        }

        private CustomFileSystemResourceFolder(string path, IResourceFolder parent)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException("path");

            m_path = path;
            m_parent = parent;
            m_name = PathUtil.GetLastElement(path);
        }


        #region IResourceFolder Members

        /// <summary>
        /// Gets a list of subfolders, if any. If there are no subfolders an empty list is returned.
        /// This IList will be read-only.</summary>
        public IList<IResourceFolder> Folders
        {
            get
            {
                var folders = new List<IResourceFolder>();               
                try 
                { 
                    var directories = Directory.GetDirectories(m_path);
                    const FileAttributes systemOrHidden = FileAttributes.System | FileAttributes.Hidden;
                    foreach (string directory in directories)
                    {
                        DirectoryInfo dirInfo = new DirectoryInfo(directory);
                        if ((dirInfo.Attributes & systemOrHidden) != 0)
                            continue;
                        folders.Add(new CustomFileSystemResourceFolder(directory, this));
                    }
                }
                catch { }
                return new ReadOnlyCollection<IResourceFolder>(folders);
            }
        }

        /// <summary>
        /// Gets a list of Resource URIs contained in this folder. If none, an empty list is returned.
        /// This IList will be read-only.</summary>
        public IList<Uri> ResourceUris
        {
            get
            {                
                var uris = new List<Uri>();
                try 
                {
                    var systemOrHidden = FileAttributes.System | FileAttributes.Hidden;                    
                    var gameEngine = Globals.MEFContainer.GetExportedValue<IGameEngineProxy>();
                    var resInfos = gameEngine != null ? gameEngine.Info.ResourceInfos : null; 

                    var files = Directory.GetFiles(m_path);
                    foreach (string file in files)
                    {                        
                        FileInfo finfo = new FileInfo(file);                        
                        if ( (finfo.Attributes & systemOrHidden) != 0
                            || finfo.Name.StartsWith("~"))
                            continue;
                        string ext = finfo.Extension.ToLower();
                        if (resInfos == null || resInfos.IsSupported(ext))
                            uris.Add(new Uri(file));                        
                    }
                }
                catch { }                           
                return new ReadOnlyCollection<Uri>(uris);
            }
        }

        /// <summary>
        /// Gets the parent folder. Returns null if the current folder is the root of a folder tree</summary>
        public IResourceFolder Parent
        {
            get { return m_parent; }
        }

        /// <summary>
        /// Gets whether the Name property is read-only. If true, setting the Name may not have any
        /// effect.</summary>
        public bool ReadOnlyName
        {
            get { return true; }
        }

        /// <summary>
        /// Gets or sets the display name of the folder. The initial value is the directory name. Setting
        /// the name does not modify the corresponding directory name.</summary>
        public virtual string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        /// <summary>
        /// Not implemented. Returns null.</summary>
        /// <returns>Null</returns>
        public virtual IResourceFolder CreateFolder()
        {
            return null;
        }

        #endregion

        /// <summary>
        /// Gets the absolute path of the directory that this IResourceFolder maps to</summary>
        public string FullPath
        {
            get { return m_path; }
        }

        private string m_name;
        private readonly string m_path;
        private readonly IResourceFolder m_parent;        
    }
}
