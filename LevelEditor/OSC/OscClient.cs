//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing;
using Sce.Atf;
using Sce.Atf.Dom;
using Sce.Atf.Applications;
using Sce.Atf.Adaptation;

using LevelEditorCore;
using LevelEditor.DomNodeAdapters;

using RenderingInterop;

namespace LevelEditor.OSC
{
    /// <summary>
    /// A simple component used for exposing a few objects to Open Sound Control (OSC).
    /// OSC allows a tablet computer, like the Sony Xperia Tablet Z, to be used as a
    /// multi-touch input device. This class was designed to work with the TouchOSC
    /// app on the Android OS. Lemur is a more powerful app that also supports OSC,
    /// and that runs on the iPad. A sample TouchOSC layout that works with this code
    /// is TouchOSC_layout.touchosc, in this directory.</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(IOscService))]
    [Export(typeof(OscService))]
    [Export(typeof(OscClient))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class OscClient : OscService, IInitializable
    {
        /// <summary>
        /// Gets root domnode of master document.</summary>
        public DomNode MasterNode
        {
            get { return m_designView.Context.As<DomNode>(); }
        }

        /// <summary>
        /// Gets active directional light if it exists or null</summary>
        public DirLight ActiveDirLight
        {
            get
            {
                DirLight dirlight = null;
                if (MasterNode != null)
                {
                    foreach (DomNode node in MasterNode.Subtree)
                    {
                        dirlight = node.As<DirLight>();
                        if (dirlight != null) break;
                    }
                }
                return dirlight;
            }
        }

        protected override object SelectedToCommon(object selected)
        {
            return selected.As<DomNode>();
        }

        /// <summary>
        /// Raises the MessageReceived event</summary>
        /// <param name="args">Event args containing the OSC address and data. Set the Handled
        /// property to true to avoid further processing.</param>
        protected override void OnMessageRecieved(OscMessageReceivedArgs args)
        {
            switch (args.Address)
            {
                case SolidAddress:
                    SetRenderFlags(GlobalRenderFlags.Solid, args.Data);
                    return;
                case WireAddress:
                    SetRenderFlags(GlobalRenderFlags.WireFrame, args.Data);
                    return;
                case SolidAndWireAddress:
                    SetRenderFlags(GlobalRenderFlags.Solid | GlobalRenderFlags.WireFrame, args.Data);
                    return;
                case TexturesAddress:
                    SetRenderFlags(GlobalRenderFlags.Textured, args.Data);
                    return;
                case LightingAddress:
                    SetRenderFlags(GlobalRenderFlags.Lit, args.Data);
                    return;
                case BackfacesAddress:
                    SetRenderFlags(GlobalRenderFlags.RenderBackFace, args.Data);
                    return;
                case ShadowsAddress:
                    SetRenderFlags(GlobalRenderFlags.Shadows, args.Data);
                    return;
                case NormalsAddress:
                    SetRenderFlags(GlobalRenderFlags.RenderNormals, args.Data);
                    return;
                case GlobalRecordA:
                    if (IsOscDataTrue(args.Data))
                        RecordGlobalLighting(0);
                    return;
                case GlobalRecallA:
                    if (IsOscDataTrue(args.Data))
                        RecallGlobalLighting(0);
                    return;
                case GlobalRecordB:
                    if (IsOscDataTrue(args.Data))
                        RecordGlobalLighting(1);
                    return;
                case GlobalRecallB:
                    if (IsOscDataTrue(args.Data))
                        RecallGlobalLighting(1);
                    return;
                case GlobalRecordC:
                    if (IsOscDataTrue(args.Data))
                        RecordGlobalLighting(2);
                    return;
                case GlobalRecallC:
                    if (IsOscDataTrue(args.Data))
                        RecallGlobalLighting(2);
                    return;
                case GlobalRecordD:
                    if (IsOscDataTrue(args.Data))
                        RecordGlobalLighting(3);
                    return;
                case GlobalRecallD:
                    if (IsOscDataTrue(args.Data))
                        RecallGlobalLighting(3);
                    return;
                case GlobalRecordE:
                    if (IsOscDataTrue(args.Data))
                        RecordGlobalLighting(4);
                    return;
                case GlobalRecallE:
                    if (IsOscDataTrue(args.Data))
                        RecallGlobalLighting(4);
                    return;
                case RecordA:
                    if (IsOscDataTrue(args.Data))
                        RecordSelectedLight(0);
                    return;
                case RecallA:
                    if (IsOscDataTrue(args.Data))
                        RecallSelectedLight(0);
                    return;
                case RecordB:
                    if (IsOscDataTrue(args.Data))
                        RecordSelectedLight(1);
                    return;
                case RecallB:
                    if (IsOscDataTrue(args.Data))
                        RecallSelectedLight(1);
                    return;
            }
            if (args.Address.Contains("GlobalAmbient"))
            {
                SetGlobalAmbientComponent(ActiveDirLight, args.Address, args.Data);
                return;
            }
            if (args.Address.Contains("GlobalSpecular"))
            {
                SetGlobalSpecularComponent(ActiveDirLight, args.Address, args.Data);
                return;
            }
            if (args.Address.Contains("GlobalDiffuse"))
            {
                SetGlobalDiffuseComponent(ActiveDirLight, args.Address, args.Data);
                return;
            }
            base.OnMessageRecieved(args);
        }

        private class LightingInfo
        {
            public LightingInfo(DirLight light)
            {
                m_ambient = light.Ambient;
                m_specular = light.Specular;
                m_diffuse = light.Diffuse;
            }

            public static LightingInfo TryCreate(DomNode domNode)
            {
                AttributeInfo diffuseInfo = domNode.Type.GetAttributeInfo("diffuse");
                AttributeInfo ambientInfo = domNode.Type.GetAttributeInfo("ambient");
                AttributeInfo specularInfo = domNode.Type.GetAttributeInfo("specular");
                if (diffuseInfo != null &&
                    ambientInfo != null &&
                    specularInfo != null)
                {
                    return new LightingInfo(
                        Color.FromArgb((int) domNode.GetAttribute(ambientInfo)),
                        Color.FromArgb((int) domNode.GetAttribute(specularInfo)),
                        Color.FromArgb((int) domNode.GetAttribute(diffuseInfo)));
                }
                return null;
            }

            public void Recall(DirLight light)
            {
                light.Ambient = m_ambient;
                light.Specular = m_specular;
                light.Diffuse = m_diffuse;
            }

            public void Recall(DomNode domNode)
            {
                AttributeInfo diffuseInfo = domNode.Type.GetAttributeInfo("diffuse");
                AttributeInfo ambientInfo = domNode.Type.GetAttributeInfo("ambient");
                AttributeInfo specularInfo = domNode.Type.GetAttributeInfo("specular");
                if (diffuseInfo != null &&
                    ambientInfo != null &&
                    specularInfo != null)
                {
                    domNode.SetAttribute(ambientInfo, m_ambient.ToArgb());
                    domNode.SetAttribute(specularInfo, m_specular.ToArgb());
                    domNode.SetAttribute(diffuseInfo, m_diffuse.ToArgb());
                }
            }

            private LightingInfo(Color ambient, Color specular, Color diffuse)
            {
                m_ambient = ambient;
                m_specular = specular;
                m_diffuse = diffuse;
            }

            private readonly Color m_ambient;
            private readonly Color m_specular;
            private readonly Color m_diffuse;
        }

        private void RecallGlobalLighting(int i)
        {
            if (m_globalLightInfos[i] != null)
            {
                DirLight light = ActiveDirLight;
                if (light != null)
                {
                    //We don't want to set m_settingGlobalLighting to false because the sliders
                    // on OSC display need to be updated in response to the recall button being pressed
                    m_domNode.As<ITransactionContext>().DoTransaction(
                        () => m_globalLightInfos[i].Recall(light),
                        "OSC Input".Localize("The name of a command"));
                }
            }
        }

        private void RecordGlobalLighting(int i)
        {
            DirLight light = ActiveDirLight;
            if (light != null)
                m_globalLightInfos[i] = new LightingInfo(light);
        }

        private void RecallSelectedLight(int i)
        {
            if (m_selectedLightInfos[i] != null)
            {
                object lastSelected = SelectionContext.LastSelected;
                var domNode = lastSelected.As<DomNode>();
                if (domNode == null)
                {
                    var path = lastSelected as AdaptablePath<object>;
                    if (path != null)
                        domNode = path.Last.As<DomNode>();
                }
                if (domNode != null)
                {
                    m_domNode.As<ITransactionContext>().DoTransaction(
                        () => m_selectedLightInfos[i].Recall(domNode),
                        "OSC Input".Localize("The name of a command"));
                    m_designView.ActiveView.Invalidate();
                }
            }
        }

        private void RecordSelectedLight(int i)
        {
            object lastSelected = SelectionContext.LastSelected;
            var domNode = lastSelected.As<DomNode>();
            if (domNode == null)
            {
                var path = lastSelected as AdaptablePath<object>;
                if (path != null)
                    domNode = path.Last.As<DomNode>();
            }
            if (domNode != null)
            {
                m_selectedLightInfos[i] = LightingInfo.TryCreate(domNode);
            }
        }

        private void SetGlobalAmbientComponent(DirLight activeDirLight, string address, object data)
        {
            if (activeDirLight == null)
                return;
            Color origColor = activeDirLight.Ambient;
            m_settingGlobalLighting = true;
            m_domNode.As<ITransactionContext>().DoTransaction(() =>
                activeDirLight.Ambient = ReplaceComponent(origColor, address, data),
                "OSC Input".Localize("The name of a command"));
            m_settingGlobalLighting = false;
            m_designView.ActiveView.Invalidate();
        }

        private void SetGlobalSpecularComponent(DirLight activeDirLight, string address, object data)
        {
            if (activeDirLight == null)
                return;
            Color origColor = activeDirLight.Specular;
            m_settingGlobalLighting = true;
            m_domNode.As<ITransactionContext>().DoTransaction(() =>
                activeDirLight.Specular = ReplaceComponent(origColor, address, data),
                "OSC Input".Localize("The name of a command"));
            m_settingGlobalLighting = false;
            m_designView.ActiveView.Invalidate();
        }

        private void SetGlobalDiffuseComponent(DirLight activeDirLight, string address, object data)
        {
            if (activeDirLight == null)
                return;
            Color origColor = activeDirLight.Diffuse;
            m_settingGlobalLighting = true;
            m_domNode.As<ITransactionContext>().DoTransaction(() =>
                activeDirLight.Diffuse = ReplaceComponent(origColor, address, data),
                "OSC Input".Localize("The name of a command"));
            m_settingGlobalLighting = false;
            m_designView.ActiveView.Invalidate();
        }

        private Color ReplaceComponent(Color origColor, string address, object data)
        {
            int newComponent;
            if (!GetOscParam(data, out newComponent))
                return origColor;
            int r = origColor.R;
            int g = origColor.G;
            int b = origColor.B;
            if (address.Contains("Red"))
                r = newComponent;
            else if (address.Contains("Green"))
                g = newComponent;
            else if (address.Contains("Blue"))
                b = newComponent;
            return Color.FromArgb(r, g, b);
        }

        private void DomNodeOnAttributeChanged(object sender, AttributeEventArgs attributeEventArgs)
        {
            if (attributeEventArgs.DomNode.Type == Schema.DirLight.Type)
            {
                SendGlobalLightingData();
            }
        }

        private void RenderStateChanged(object sender, EventArgs eventArgs)
        {
            var data = new[]
            {
                new Tuple<string, object>(SolidAddress, GetRenderFlags(GlobalRenderFlags.Solid)), 
                new Tuple<string, object>(WireAddress, GetRenderFlags(GlobalRenderFlags.WireFrame)), 
                new Tuple<string, object>(SolidAndWireAddress, GetRenderFlags(GlobalRenderFlags.Solid | GlobalRenderFlags.WireFrame)), 
                new Tuple<string, object>(TexturesAddress, GetRenderFlags(GlobalRenderFlags.Textured)), 
                new Tuple<string, object>(LightingAddress, GetRenderFlags(GlobalRenderFlags.Lit)), 
                new Tuple<string, object>(BackfacesAddress, GetRenderFlags(GlobalRenderFlags.RenderBackFace)), 
                new Tuple<string, object>(ShadowsAddress, GetRenderFlags(GlobalRenderFlags.Shadows)), 
                new Tuple<string, object>(NormalsAddress, GetRenderFlags(GlobalRenderFlags.RenderNormals)), 
            };
            Send(data);
        }

        private float GetRenderFlags(GlobalRenderFlags flags)
        {
            return (((NativeDesignControl) m_designView.ActiveView).RenderState.RenderFlag & flags) == flags
                ? 1.0f
                : 0.0f;
        }

        private bool GetOscParam(object oscData, out float param1)
        {
            param1 = default(float);
            var oscParams = oscData as IEnumerable<object>;
            if (oscParams == null)
                return false;
            var iterator = oscParams.GetEnumerator();
            if (iterator.MoveNext())
            {
                param1 = (float)iterator.Current;
                return true;
            }
            return false;
        }

        private bool GetOscParam(object oscData, out int param1)
        {
            param1 = default(int);
            var oscParams = oscData as IEnumerable<object>;
            if (oscParams == null)
                return false;
            var iterator = oscParams.GetEnumerator();
            if (iterator.MoveNext())
            {
                param1 = (int)(float)iterator.Current;
                return true;
            }
            return false;
        }

        private bool IsOscDataTrue(object oscData)
        {
            var oscParams = oscData as IEnumerable<object>;
            if (oscParams == null)
                return false;
            var iterator = oscParams.GetEnumerator();
            if (iterator.MoveNext() &&
                iterator.Current is float)
            {
                return (float) iterator.Current == 1.0f;
            }
            return false;
        }

        private void SetRenderFlags(GlobalRenderFlags flags, object oscData)
        {
            float param1;
            if (!GetOscParam(oscData, out param1))
                return;
            var designControl = (NativeDesignControl)m_designView.ActiveView;
            if (param1 == 0.0f)
                designControl.RenderState.RenderFlag &= ~flags;
            else
                designControl.RenderState.RenderFlag |= flags;
            designControl.Invalidate();
        }

        void IInitializable.Initialize()
        {
            //m_globalFog = new GlobalFog();
            m_designView.ContextChanged += DesignView_ContextChanged;
            ((NativeDesignControl)m_designView.ActiveView).RenderState.Changed += RenderStateChanged;

            base.Initialize();

            AddDiffuseLightingOscAddresses(Schema.PointLight.Type.GetTag<PropertyDescriptorCollection>());
            AddDiffuseLightingOscAddresses(Schema.BoxLight.Type.GetTag<PropertyDescriptorCollection>());
        }

        private void AddDiffuseLightingOscAddresses(PropertyDescriptorCollection descriptorCollection)
        {
            if (descriptorCollection != null)
            {
                foreach (System.ComponentModel.PropertyDescriptor descriptor in descriptorCollection)
                {
                    // Look for property descriptors for attributes on this type of DomNode.
                    var attrDesc = descriptor as AttributePropertyDescriptor;
                    if (attrDesc != null)
                    {
                        if (attrDesc.DisplayName == "Diffuse")
                        {
                            AddPropertyAddress(new ByteAttributePropertyDescriptor(attrDesc, 2), RedAddress);
                            AddPropertyAddress(new ByteAttributePropertyDescriptor(attrDesc, 1), GreenAddress);
                            AddPropertyAddress(new ByteAttributePropertyDescriptor(attrDesc, 0), BlueAddress);
                        }
                    }
                }
            }
           
        }

        // Gets or sets a particular byte of a 32-bit integer on a DomNode's attribute.
        // This is useful for getting and setting a particular color channel of an RGB int.
        private class ByteAttributePropertyDescriptor : AttributePropertyDescriptor
        {
            // 'byteIndex' is 0 for the lowest byte (green), 1 for next lowest byte (blue),
            //  and 2 for the third byte (red).
            public ByteAttributePropertyDescriptor(AttributePropertyDescriptor original, byte byteIndex)
                : base(original.Name, original.AttributeInfo, original.Category, original.Description, original.IsReadOnly)
            {
                m_bitShift = byteIndex * 8;
            }
            
            // Gets the color as a 32-bit integer and extracts the correct byte.
            public override object GetValue(object component)
            {
                var rgb = (int)base.GetValue(component);
                return (rgb >> m_bitShift) & 0xff;
            }

            // 'value' should be an int, in the range [0,255]. 'component' must be a DomNode.
            public override void SetValue(object component, object value)
            {
                var domNode = component.Cast<DomNode>();
                int rgb = ((int)domNode.GetAttribute(AttributeInfo));
                uint mask = (uint)((0xffffffff) ^ (0xff << m_bitShift));
                rgb = (int)(((uint)rgb & mask)) | ((int)value << m_bitShift);
                base.SetValue(component, rgb);
            }

            private readonly int m_bitShift; // # of bits to shift to get color component
        }

        private void DesignView_ContextChanged(object sender, EventArgs e)
        {
            if (m_domNode != null)
                m_domNode.AttributeChanged -= DomNodeOnAttributeChanged;

            m_domNode = m_designView.Context.As<DomNode>();

            if (m_domNode != null)
                m_domNode.AttributeChanged += DomNodeOnAttributeChanged;

            SendGlobalLightingData();
            RenderStateChanged(null, EventArgs.Empty);
        }

        private void SendGlobalLightingData()
        {
            if (m_settingGlobalLighting)
                return;

            DirLight dirLight = ActiveDirLight;
            if (dirLight == null)
                return;
            var data = new[]
            {
                new Tuple<string, object>(GlobalAmbientRed, (float)dirLight.Ambient.R), 
                new Tuple<string, object>(GlobalAmbientBlue, (float)dirLight.Ambient.B), 
                new Tuple<string, object>(GlobalAmbientGreen, (float)dirLight.Ambient.G), 
                new Tuple<string, object>(GlobalSpecularRed, (float)dirLight.Specular.R), 
                new Tuple<string, object>(GlobalSpecularBlue, (float)dirLight.Specular.B), 
                new Tuple<string, object>(GlobalSpecularGreen, (float)dirLight.Specular.G), 
                new Tuple<string, object>(GlobalDiffuseRed, (float)dirLight.Diffuse.R), 
                new Tuple<string, object>(GlobalDiffuseBlue, (float)dirLight.Diffuse.B), 
                new Tuple<string, object>(GlobalDiffuseGreen, (float)dirLight.Diffuse.G), 
            };
            Send(data);
        }

        private const string SolidAddress = "/1/solid";
        private const string WireAddress = "/1/wire";
        private const string SolidAndWireAddress = "/1/solid+wire";
        private const string TexturesAddress = "/1/textures";
        private const string LightingAddress = "/1/lighting";
        private const string BackfacesAddress = "/1/backfaces";
        private const string ShadowsAddress = "/1/shadows";
        private const string NormalsAddress = "/1/normals";

        private const string RedAddress = "/1/red";
        private const string GreenAddress = "/1/green";
        private const string BlueAddress = "/1/blue";
        private const string RecallA = "/1/RecallA";
        private const string RecordA = "/1/RecordA";
        private const string RecallB = "/1/RecallB";
        private const string RecordB = "/1/RecordB";

        private const string GlobalAmbientRed = "/1/GlobalAmbientRed";
        private const string GlobalAmbientBlue = "/1/GlobalAmbientBlue";
        private const string GlobalAmbientGreen = "/1/GlobalAmbientGreen";

        private const string GlobalDiffuseRed = "/1/GlobalDiffuseRed";
        private const string GlobalDiffuseBlue = "/1/GlobalDiffuseBlue";
        private const string GlobalDiffuseGreen = "/1/GlobalDiffuseGreen";

        private const string GlobalSpecularRed = "/1/GlobalSpecularRed";
        private const string GlobalSpecularBlue = "/1/GlobalSpecularBlue";
        private const string GlobalSpecularGreen = "/1/GlobalSpecularGreen";

        private const string GlobalRecallA = "/1/GlobalRecallA";
        private const string GlobalRecordA = "/1/GlobalRecordA";
        private const string GlobalRecallB = "/1/GlobalRecallB";
        private const string GlobalRecordB = "/1/GlobalRecordB";
        private const string GlobalRecallC = "/1/GlobalRecallC";
        private const string GlobalRecordC = "/1/GlobalRecordC";
        private const string GlobalRecallD = "/1/GlobalRecallD";
        private const string GlobalRecordD = "/1/GlobalRecordD";
        private const string GlobalRecallE = "/1/GlobalRecallE";
        private const string GlobalRecordE = "/1/GlobalRecordE";

        [Import]
        private IDesignView m_designView;

        private bool m_settingGlobalLighting;
        private readonly LightingInfo[] m_globalLightInfos = new LightingInfo[5]; //A,B,C,D,E
        private readonly LightingInfo[] m_selectedLightInfos = new LightingInfo[2]; //A,B

        private DomNode m_domNode;
    }
}
