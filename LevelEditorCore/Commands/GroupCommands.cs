//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;
using Sce.Atf.VectorMath;

using LevelEditorCore.VectorMath;

namespace LevelEditorCore.Commands
{
    /// <summary>
    /// Group and Ungroup game objects commands</summary>
    /// <remarks>    
    /// Group: Creates a new GameObjectGroup, moves all the selected GameObjects
    /// to the newly created group.    
    /// Ungroup: Selected GameObjects that are in a group are moved
    /// to the root GameObjectFolder of their level.
    /// </remarks>
    [Export(typeof(GroupCommands))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class GroupCommands : ICommandClient, IInitializable
    {
        [ImportingConstructor]
        public GroupCommands(ICommandService commandService, IContextRegistry contextRegistry)
        {
            m_commandService = commandService;
            m_contextRegistry = contextRegistry;            
        }

        /// <summary>
        /// Tests if the gameobjects can be grouped.
        /// The gameobjects can be grouped if there are more than one 
        /// and they belong to the same level.</summary>        
        public bool CanGroup(IEnumerable<IGameObject> gobs)
        {
            // can group if there are more than one gob and they all belong to same level.
            DomNode root1 = null;
            int gobcount = 0;
            bool sameLevel = true;
            foreach (IGameObject gob in gobs)
            {
                DomNode root = gob.As<DomNode>().GetRoot();
                if (!root.Is<IGame>())
                {
                    sameLevel = false;
                    break;
                }
                if (root1 == null)
                    root1 = root;

                if (root1 != root)
                {
                    sameLevel = false;
                    break;
                }
                gobcount++;
            }

            return sameLevel && gobcount > 1;
        }

        /// <summary>
        /// Tests if the Gameobjects can be Ungrouped.        
        /// </summary>        
        public bool CanUngroup(IEnumerable<IGameObject> gobs)
        {
            // can ungroup.
            foreach (IGameObject gob in gobs)
            {
                if (gob.Is<IGameObjectGroup>())
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Groups the specified GameObjects</summary>
        /// <param name="gobs">GameObjects to be grouped</param>
        /// <remarks>Creates a new GameObjectGroup and moves all 
        /// the GameObjects into it.</remarks>
        public IGameObjectGroup Group(IEnumerable<IGameObject> gobs)
        {
            // extra check.
            if (!CanGroup(gobs)) return null;

            IGame game = null;
            AABB groupBox = new AABB();
            List<IGameObject> gameObjects = new List<IGameObject>();
            foreach (IGameObject gameObject in gobs)
            {
                if (game == null)
                {
                    game = gameObject.As<DomNode>().GetRoot().As<IGame>();
                }

                gameObjects.Add(gameObject);

                IBoundable boundable = gameObject.As<IBoundable>();
                groupBox.Extend(boundable.BoundingBox);                
            }

            IGameObjectGroup group = game.CreateGameObjectGroup();
            DomNode node = group.As<DomNode>();
            node.InitializeExtensions();
            ITransformable transformable = node.As<ITransformable>();
            transformable.Translation = groupBox.Center;
            
            Matrix4F invWorld = new Matrix4F();
            invWorld.Invert(transformable.Transform);

            game.RootGameObjectFolder.GameObjects.Add(group);

            foreach (IGameObject gameObject in gameObjects)
            {
                ITransformable xformable = gameObject.As<ITransformable>();
                Matrix4F world = ComputeWorldTransform(xformable);
                SetTransform(xformable, world);
                group.GameObjects.Add(gameObject);
                Vec3F trans = world.Translation;
                invWorld.Transform(ref trans);
                xformable.Translation = trans;
            }

            return group;            
        }

        /// <summary>
        /// Ungroups the specified gameobjects.</summary>        
        public IEnumerable<IGameObject> Ungroup(IEnumerable<IGameObject> gobs)
        {
            if (!CanUngroup(gobs))
                return EmptyArray<IGameObject>.Instance;

            List<IGameObject> ungrouplist = new List<IGameObject>();
            List<IGameObject> goblist = new List<IGameObject>(gobs);
            foreach (IGameObject gameObject in goblist)
            {
                // Ungroup selected groups
                IGameObjectGroup group = gameObject.As<IGameObjectGroup>();
                if (group == null) continue;

                List<IGameObject> childList = new List<IGameObject>(group.GameObjects);
                IGame game = group.As<DomNode>().GetRoot().As<IGame>();
                // Move children to the root game object folder
                
                foreach (IGameObject child in childList)
                {
                    ITransformable xformChild = child.As<ITransformable>();
                    Matrix4F world = ComputeWorldTransform(xformChild);             
                    game.RootGameObjectFolder.GameObjects.Add(child);
                    SetTransform(xformChild, world);                    
                    ungrouplist.Add(child);
                }
                // Remove group
                group.Cast<DomNode>().RemoveFromParent();
            }
            return ungrouplist;            
        }

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            // Register commands
            m_commandService.RegisterCommand(StandardCommand.EditGroup, StandardMenu.Edit, StandardCommandGroup.EditGroup,
                "Group".Localize(), "Group".Localize(), Keys.Control | Keys.G, null, CommandVisibility.All, this);
            m_commandService.RegisterCommand(StandardCommand.EditUngroup, StandardMenu.Edit, StandardCommandGroup.EditGroup,
                "Ungroup".Localize(), "Ungroup".Localize(), Keys.Control | Keys.U, null, CommandVisibility.All, this);

            if (m_scriptingService != null)
                m_scriptingService.SetVariable("groupCommands", this);

            m_contextRegistry.ActiveContextChanged += ContextRegistry_ActiveContextChanged;
        }

        #endregion

        #region ICommandClient Members

        /// <summary>
        /// Returns true iff the specified command can be performed</summary>
        /// <param name="commandTag">Command</param>
        /// <returns>True iff the specified command can be performed</returns>
        bool ICommandClient.CanDoCommand(object commandTag)
        {
            switch ((StandardCommand)commandTag)
            {
                case StandardCommand.EditGroup:
                    return m_canGroup;
                case StandardCommand.EditUngroup:
                    return m_canUngroup;
            }
            return false;

        }

        /// <summary>
        /// Does the specified command</summary>
        /// <param name="commandTag">Command</param>
        void ICommandClient.DoCommand(object commandTag)
        {
            ITransactionContext transactionContext = m_selectionContext.As<ITransactionContext>();
            if (transactionContext == null) return;

            switch ((StandardCommand)commandTag)
            {
                case StandardCommand.EditGroup:
                    transactionContext.DoTransaction(delegate
                    {
                        IGameObjectGroup group = Group(SelectedGobs);
                        m_selectionContext.Set(Util.AdaptDomPath(group.As<DomNode>()));
                    }, "Group".Localize());                    
                    break;
                case StandardCommand.EditUngroup:
                    transactionContext.DoTransaction(delegate
                    {
                        IEnumerable<IGameObject> gobs = Ungroup(SelectedGobs);
                        List<object> newselection = new List<object>();
                        foreach(var gob in gobs)
                        {
                            newselection.Add(Util.AdaptDomPath(gob.As<DomNode>()));
                        }
                        m_selectionContext.SetRange(newselection);

                    }, "Ungroup".Localize());                    
                    break;
            }
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="state">Command state to update</param>
        void ICommandClient.UpdateCommand(object commandTag, Sce.Atf.Applications.CommandState state)
        {
        }

        #endregion

        /// <summary>
        /// Gets the current GameContext's selection of GameObjects</summary>
        private IEnumerable<IGameObject> SelectedGobs
        {
            get
            {
                IEnumerable<DomNode> rootDomNodes = m_selectionContext != null ?
                DomNode.GetRoots(m_selectionContext.GetSelection<DomNode>()) : EmptyArray<DomNode>.Instance;
                return rootDomNodes.AsIEnumerable<IGameObject>();
            }
        }
       
        private void ContextRegistry_ActiveContextChanged(object sender, System.EventArgs e)
        {
            if (m_selectionContext != null)
            {
                m_selectionContext.SelectionChanged -= SelectionChanged;                
            }

            object context = m_contextRegistry.GetActiveContext<IGameContext>();
            m_selectionContext = (ISelectionContext)context;

            if (m_selectionContext != null)
            {
                m_selectionContext.SelectionChanged += SelectionChanged;             
            }

            m_canGroup = CanGroup(SelectedGobs);
            m_canUngroup = CanUngroup(SelectedGobs);
        }

        private void SelectionChanged(object sender, System.EventArgs e)
        {
            m_canGroup = CanGroup(SelectedGobs);
            m_canUngroup = CanUngroup(SelectedGobs);
        }

        private Matrix4F ComputeWorldTransform(ITransformable xform)
        {
            Matrix4F world = new Matrix4F();
            DomNode node = xform.As<DomNode>();
            foreach (DomNode n in node.Lineage)
            {
                ITransformable xformNode = n.As<ITransformable>();
                if (xformNode != null)
                {
                    world.Mul(world, xformNode.Transform);
                }
            }
            return world;            
        }

        private void SetTransform(ITransformable xform, Matrix4F mtrx)
        {
            xform.Translation = mtrx.Translation;
            xform.Scale = mtrx.GetScale();
            Vec3F rot = new Vec3F();
            mtrx.GetEulerAngles(out rot.X, out rot.Y, out rot.Z);
            xform.Rotation = rot;
        }

        private bool m_canGroup;
        private bool m_canUngroup;
        private ISelectionContext m_selectionContext;
        private readonly ICommandService m_commandService;
        private readonly IContextRegistry m_contextRegistry;

        [Import(AllowDefault = true)]
        private ScriptingService m_scriptingService = null;
    }
}
