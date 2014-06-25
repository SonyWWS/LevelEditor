using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using Sce.Atf.Dom;

namespace DomGen
{
    // strings found in the .xsd file and those added as tags to the DOM objects
    public static class SchemaStrings
    {
        public const string LegeNativeType = "LeGe.NativeType";
        public const string LeGeNativeProperty = "LeGe.NativeProperty";
        public const string LeGeNativeElement = "LeGe.NativeElement";
        public const string NativeName = "nativeName";
        public const string NativeType = "nativeType";        
        public const string Name = "name";
        public const string Access = "access";
        public const string Set = "set";
        public const string Get = "get";
    }

    // A native list is an attribute of a class that has Add/Remove functionality.
    public class NativeListInfo
    {
        public NativeListInfo(XmlElement elm)
        {
            string name = elm.GetAttribute(SchemaStrings.NativeName);
            string type = elm.GetAttribute(SchemaStrings.NativeType);
            string access = elm.GetAttribute(SchemaStrings.Access);
            bool canSet = true; 
            bool canGet = true;
            if (null != access)
            {
                canSet = access.Contains(SchemaStrings.Set);
                canGet = access.Contains(SchemaStrings.Get);
            }
            m_name = name;
            m_type = type;
            m_access = access;
            m_setable = canSet;
            m_getable = canGet;
        }
        private string m_name;
        public string NativeName
        {
            get { return m_name; }
        }
        private string m_type;
        public string NativeType
        {
            get { return m_type; }
        }
        private string m_access;
        public string Access
        {
            get { return m_access; }
        }
        private bool m_setable;
        public bool Setable
        {
            get { return m_setable; }
        }
        private bool m_getable;
        public bool Getable
        {
            get { return m_getable; }
        }
    }

    // A native property is an attribute of a class that has get/set functions.
    public class NativePropertyInfo
    {
        public NativePropertyInfo(XmlElement elm)
        {
            string nativeName = elm.GetAttribute(SchemaStrings.NativeName);
            string nativeType = elm.GetAttribute(SchemaStrings.NativeType);
            string access = elm.GetAttribute(SchemaStrings.Access);
            bool canSet = access.Contains(SchemaStrings.Set);
            bool canGet = access.Contains(SchemaStrings.Get);
            m_name = nativeName;
            m_type = nativeType;
            m_access = access;
            m_setable = canSet;
            m_getable = canGet;

        }
        private string m_name;
        public string NativeName
        {
            get { return m_name; }
        }
        private string m_type;
        public string NativeType
        {
            get { return m_type; }
        }
        private string m_access;
        public string Access
        {
            get { return m_access; }
        }
        private bool m_setable;
        public bool Setable
        {
            get { return m_setable; }
        }
        private bool m_getable;
        public bool Getable
        {
            get { return m_getable; }
        }

    }

    // a native class in a class that can be instantiated by the native code.
    // The class info stores the name of the class as well as inforamtion 
    // about all the native properties and lists.
    public class NativeClassInfo
    {
        public NativeClassInfo(XmlElement element, bool abstractType)
        {
            m_name = element.GetAttribute(SchemaStrings.NativeName);
            m_abstract = abstractType;
            m_properties = new List<NativePropertyInfo>();
            m_lists = new List<NativeListInfo>();
        }

        private string m_name;
        public string NativeName
        {
            get { return m_name;  }
            set { }
        }
        private bool m_abstract;
        public bool Abstract
        {
            get { return m_abstract; }
            set { }
        }


        private List<NativePropertyInfo> m_properties;
        public List<NativePropertyInfo> Properties
        {
            get { return m_properties; }
            set { }
        }

        private List<NativeListInfo> m_lists;
        public List<NativeListInfo> Lists
        {
            get { return m_lists; }
            set { }
        }

    }

    // the native schema info contains all the inforation about 
    // classes, properties and list supported by the native code.
    public class NativeSchemaInfo
    {
        public NativeSchemaInfo(XmlSchemaTypeLoader typeLoader)
        {
            m_nativeClasses = new List<NativeClassInfo>();

            // parse schema & add our Annotations
            foreach (DomNodeType domType in typeLoader.GetNodeTypes())
            {
                IEnumerable<XmlNode> annotations = domType.GetTagLocal<IEnumerable<XmlNode>>();
                if (annotations == null)
                    continue;
                                
                NativeClassInfo classInfo = null;
                foreach (XmlNode annot in annotations)
                {
                    XmlElement elm = annot as XmlElement;
                    if (elm.LocalName == SchemaStrings.LegeNativeType)
                    {
                        classInfo = new NativeClassInfo(elm, domType.IsAbstract);
                        m_nativeClasses.Add(classInfo);
                        break;
                    }
                }

                if (classInfo == null) continue;

                foreach (XmlNode annot in annotations)
                {
                    XmlElement elm = annot as XmlElement;
                    if (elm.LocalName == SchemaStrings.LeGeNativeProperty)
                    {
                        NativePropertyInfo info = new NativePropertyInfo(elm);
                        classInfo.Properties.Add(info);
                    }
                    else if (elm.LocalName == SchemaStrings.LeGeNativeElement)
                    {
                        NativeListInfo info = new NativeListInfo(elm);
                        classInfo.Lists.Add(info);
                    }
                }
            }
        }

        private List<NativeClassInfo> m_nativeClasses;
        public List<NativeClassInfo> NativeClasses
        {
            get { return m_nativeClasses; }
        }

    }
}