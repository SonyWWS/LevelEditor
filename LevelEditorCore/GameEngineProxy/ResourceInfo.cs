//Copyright © 2015 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace LevelEditorCore
{    
    /// <summary>
    ///  Supported resourece (Asset) types.</summary>
    public static class ResourceTypes
    {        
        public const string Model    = "Model"; // static or skinned 3d model.
        public const string Texture  = "Texture";
    }
    
    /// <summary>
    /// Contains information related to supported resources (Assets)</summary>
    public class ResourcesInfos
    {
        /// <summary>
        /// Construct new instance from given engineInfoStr
        /// </summary>
        /// <param name="engineInfo">a string contians engine information</param>
        public ResourcesInfos(string engineInfoStr)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(engineInfoStr);
            XmlElement Reseselm = (XmlElement)doc.DocumentElement.SelectSingleNode("SupportedResources");
            var resDecrList = new List<ResourceInfo>();

            if (Reseselm != null)
            {
                foreach (XmlElement resElm in Reseselm)
                {
                    if (resElm.LocalName != "ResourceDescriptor")
                        continue;
                    string type = resElm.GetAttribute("Type");
                    string dscr = resElm.GetAttribute("Description");
                    string exts = resElm.GetAttribute("Ext");
                    var res = new ResourceInfo(type, dscr, exts);
                    foreach (string ext in res.FileExts)
                        m_allExtensions.Add(ext);                    
                    m_resInfoMap.Add(res.Type, res);
                    resDecrList.Add(res);
                }
            }
            Resources = resDecrList;

        }

        /// <summary>
        /// Tests if the given extension is recognized by
        /// the game engine as a valid game asset</summary>
        /// <param name="ext">file extension to test</param>
        /// <returns>true if the ext is supported, otherwise false</returns>
        public bool IsSupported(string ext)
        {
            return m_allExtensions.Contains(ext);
        }

        /// <summary>
        /// Gets ResourceDescriptor for given type of resources.</summary>
        /// <param name="resType">Resource type</param>
        /// <returns>ResourceDescriptor or null if none found.</returns>
        public ResourceInfo GetByType(string resType)
        {
            ResourceInfo res;
            m_resInfoMap.TryGetValue(resType, out res);
            return res;
        }

        /// <summary>
        /// Gets list of supported resources (assets).</summary>
        public IEnumerable<ResourceInfo> Resources
        {
            get;
            private set;
        }


        private HashSet<string> m_allExtensions = 
            new HashSet<string>();

        private readonly Dictionary<string, ResourceInfo>
            m_resInfoMap = new Dictionary<string, ResourceInfo>();

    }

    /// <summary>
    /// Contains information about
    /// each supported resource (asset) type.</summary>
    public class ResourceInfo
    {
        /// <summary>
        /// Construct new instance from the given parameters.</summary>
        /// <param name="type">type of the resource</param>
        /// <param name="dscr">A short description</param>
        /// <param name="exts">A comma separated file extentions</param>
        public ResourceInfo(string type, string dscr, string exts)
        {
            if (string.IsNullOrWhiteSpace(type)
                || string.IsNullOrWhiteSpace(dscr)
                || string.IsNullOrWhiteSpace(exts))
                throw new ArgumentNullException();

            Type = type;
            Description = dscr;

            exts = exts.ToLower();
            char[] sep = { ',' };

            string[] extArray = exts.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            foreach (string ext in extArray)
                m_extensions.Add(ext);
            FileExts = extArray;

            // compose filter pattern from file exts and
            // description.
            if (extArray.Length > 1)
            {                
                var filterStr = new StringBuilder();
                var patternStr = new StringBuilder();
                filterStr.AppendFormat("{0} (", Description);
                for (int i = 0; i < extArray.Length - 1; i++)
                {
                    filterStr.AppendFormat("*{0}, ", extArray[i]);
                    patternStr.AppendFormat("*{0};", extArray[i]);                    
                }
                patternStr.AppendFormat("*{0}", extArray[extArray.Length - 1]);
                filterStr.AppendFormat("*{0})|{1}", extArray[extArray.Length - 1], patternStr);
                Filter = filterStr.ToString();
            }
            else 
            {
                Filter = string.Format("{0} (*{1})|*{1}", Description, extArray[0]);
            }
        }


        /// <summary>
        /// Gets filter pattern can be 
        /// used for file dialog boxes.</summary>
        public string Filter
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets resource type.
        /// Example model, texture, material, etc.</summary>
        public readonly string Type;
             
        /// <summary>
        /// Resource description</summary>
        public readonly string Description;

        /// <summary>
        /// Gets file extension for this resource type</summary>
        public IEnumerable<string> FileExts
        {
            get;
            private set;
        }


        /// <summary>
        /// Tests if the given extension is recognized by
        /// the game engine as a valid game asset
        /// for this particular resource type</summary>
        /// <param name="ext">file extension to test</param>
        /// <returns>true if the ext is supported, otherwise false</returns>
        public bool IsSupported(string ext)
        {
            return m_extensions.Contains(ext);
        }


        // file extensions for this resource type
        private HashSet<string> m_extensions =
            new HashSet<string>();
        
    }
}
