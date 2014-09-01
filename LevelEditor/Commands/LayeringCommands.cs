//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

using LevelEditorCore;

using LayerLister = LevelEditorCore.LayerLister;

namespace LevelEditor.Commands
{
    /// <summary>
    /// Component to add "Add Layer" command to app. Command is accessible only
    /// by right click (context menu).</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(IContextMenuCommandProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class LayeringCommands : ICommandClient, IContextMenuCommandProvider, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="contextRegistry">Context registry</param>
        [ImportingConstructor]
        public LayeringCommands(
            ICommandService commandService,
            IContextRegistry contextRegistry,
            LayerLister layerLister)
        {
            m_commandService = commandService;
            m_contextRegistry = contextRegistry;
            m_layerLister = layerLister;
        }

        private enum CommandTag
        {
            AddLayerFolder,
        }

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            m_commandService.RegisterCommand(
                new CommandInfo(
                    CommandTag.AddLayerFolder,
                    null,
                    null,
                    "Add Layer".Localize(),
                    "Creates a new layer folder".Localize()),
                this);
        }

        #endregion

        #region ICommandClient Members

        bool ICommandClient.CanDoCommand(object commandTag)
        {
            return
                CommandTag.AddLayerFolder.Equals(commandTag)
                && m_targetRef != null
                && m_targetRef.Target != null
                && (m_targetRef.Target.Is<ILayer>() || m_targetRef.Target.Is<ILayeringContext>());
        }

        void ICommandClient.DoCommand(object commandTag)
        {
            if (CommandTag.AddLayerFolder.Equals(commandTag))
            {
                ILayer newLayer = new DomNode(Schema.layerType.Type).As<ILayer>();
                newLayer.Name = "New Layer".Localize();

                IList<ILayer> layerList = null;
                object target = m_targetRef.Target;
                if (target != null)
                {
                    ILayer parentLayer = Adapters.As<ILayer>(target);
                    if (parentLayer != null)
                        layerList = parentLayer.Layers;
                    else
                    {
                        LayeringContext layeringContext = Adapters.As<LayeringContext>(target);
                        if (layeringContext != null)
                            layerList = layeringContext.Layers;
                    }
                }

                if (layerList != null)
                {
                    ILayeringContext layeringContext = m_contextRegistry.GetMostRecentContext<ILayeringContext>();
                    ITransactionContext transactionContext = Adapters.As<ITransactionContext>(layeringContext);
                    TransactionContexts.DoTransaction(
                        transactionContext,
                        delegate
                        {
                            layerList.Add(newLayer);
                        },
                        "Add Layer".Localize());
                }
            }
        }

        void ICommandClient.UpdateCommand(object commandTag, CommandState commandState)
        {
        }

        #endregion

        #region IContextMenuCommandProvider Members

        /// <summary>
        /// Gets tags for context menu (right click) commands</summary>
        /// <param name="context">Context containing target object</param>
        /// <param name="target">Right clicked object, or null if none</param>
        IEnumerable<object> IContextMenuCommandProvider.GetCommands(object context, object target)
        {
            m_targetRef = null;

            if (Adapters.Is<LayeringContext>(context) && m_layerLister.TreeControl.Focused)
            {
                m_targetRef = new WeakReference(target);
                yield return CommandTag.AddLayerFolder;
            }
        }

        #endregion

        private LayerLister m_layerLister;
        private ICommandService m_commandService;
        private IContextRegistry m_contextRegistry;
        private WeakReference m_targetRef;
    }
}
