//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Input;
using Sce.Atf.VectorMath;

using LevelEditorCore;
using LevelEditorCore.VectorMath;

namespace LevelEditor.Commands
{
    /// <summary>
    /// Commands to move pivot points to the min/center/max of the bounding box 
    /// in X, Y, Z or all coordinates</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(ICommandClient))]
    [Export(typeof(PivotCommands))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class PivotCommands : ICommandClient, IInitializable
    {
        /// <summary>
        /// Importing constructor receiving registered MEF components</summary>
        /// <param name="commandService"></param>
        /// <param name="contextRegistry"></param>
        [ImportingConstructor]
        public PivotCommands(ICommandService commandService, IContextRegistry contextRegistry)
        {
            m_commandService = commandService;
            m_contextRegistry = contextRegistry;

            // define functions for moving pivots
            //m_functions = new MovePivot[Enum.GetValues(typeof(Command)).Length];
            m_functions = new Func<Vec3F, AABB, Vec3F>[Enum.GetValues(typeof(Command)).Length];
            
            m_functions[(int)Command.XMin] = (Vec3F pivot, AABB aabb) =>
                {
                    return new Vec3F(aabb.Min.X, pivot.Y, pivot.Z);
                };
                
            m_functions[(int)Command.XCenter]=(Vec3F pivot, AABB aabb) =>
                {
                    return new Vec3F(aabb.Center.X, pivot.Y, pivot.Z);
                };

            m_functions[(int)Command.XMax]=(Vec3F pivot, AABB aabb) =>
                {
                    return new Vec3F(aabb.Max.X, pivot.Y, pivot.Z);
                };

            m_functions[(int)Command.YMin]=(Vec3F pivot, AABB aabb) =>
                {
                    return new Vec3F(pivot.X, aabb.Min.Y, pivot.Z);
                };

            m_functions[(int)Command.YCenter]=(Vec3F pivot, AABB aabb) =>
                {
                    return new Vec3F(pivot.X, aabb.Center.Y, pivot.Z); 
                };

            m_functions[(int)Command.YMax]=(Vec3F pivot, AABB aabb) =>
                {
                    return new Vec3F(pivot.X, aabb.Max.Y, pivot.Z);
                };

            m_functions[(int)Command.ZMin]=(Vec3F pivot, AABB aabb) =>
                {
                    return new Vec3F(pivot.X, pivot.Y, aabb.Min.Z);
                };
            m_functions[(int)Command.ZCenter]=(Vec3F pivot, AABB aabb) =>
                {
                    return new Vec3F(pivot.X, pivot.Y, aabb.Center.Z);
                };
            m_functions[(int)Command.ZMax]=(Vec3F pivot, AABB aabb) =>
                {
                    return new Vec3F(pivot.X, pivot.Y, aabb.Max.Z); ;
                };
            m_functions[(int)Command.AllCenter] = (Vec3F pivot, AABB aabb) =>
            {
                return aabb.Center;
            };
        }

        #region IInitializable Members

        /// <summary>
        /// Register commands in the Initialize method</summary>
        void IInitializable.Initialize()
        {
            m_commandService.RegisterCommand(
                Command.XMin,
                StandardMenu.Modify,
                CommandGroup.PivotCommands,
                "Move Pivot/X/Min",
                "Move pivot to left edge of bounding box",
                Keys.D1,
                null,
                CommandVisibility.Menu,
                this);
            m_commandService.RegisterCommand(
                Command.XCenter,
                StandardMenu.Modify,
                CommandGroup.PivotCommands,
                "Move Pivot/X/Center",
                "Move pivot X to center of bounding box",
                Keys.D2,
                null,
                CommandVisibility.Menu,
                this);
            m_commandService.RegisterCommand(
                Command.XMax,
                StandardMenu.Modify,
                CommandGroup.PivotCommands,
                "Move Pivot/X/Max",
                "Move pivot to right edge of bounding box",
                Keys.D3,
                null,
                CommandVisibility.Menu,
                this);

            m_commandService.RegisterCommand(
                Command.YMin,
                StandardMenu.Modify,
                CommandGroup.PivotCommands,
                "Move Pivot/Y/Min",
                "Move pivot to top edge of bounding box",
                Keys.D4,
                null,
                CommandVisibility.Menu,
                this);
            m_commandService.RegisterCommand(
                Command.YCenter,
                StandardMenu.Modify,
                CommandGroup.PivotCommands,
                "Move Pivot/Y/Center",
                "Move pivot Y to center of bounding box",
                Keys.D5,
                null,
                CommandVisibility.Menu,
                this);
            m_commandService.RegisterCommand(
                Command.YMax,
                StandardMenu.Modify,
                CommandGroup.PivotCommands,
                "Move Pivot/Y/Max",
                "Move pivot to bottom edge of bounding box",
                Keys.D6,
                null,
                CommandVisibility.Menu,
                this);

            m_commandService.RegisterCommand(
                Command.ZMin,
                StandardMenu.Modify,
                CommandGroup.PivotCommands,
                "Move Pivot/Z/Min",
                "Move pivot to front edge of bounding box",
                Keys.D7,
                null,
                CommandVisibility.Menu,
                this);
            m_commandService.RegisterCommand(
                Command.ZCenter,
                StandardMenu.Modify,
                CommandGroup.PivotCommands,
                "Move Pivot/Z/Center",
                "Move pivot Z to center of bounding box",
                Keys.D8,
                null,
                CommandVisibility.Menu,
                this);
            m_commandService.RegisterCommand(
                Command.ZMax,
                StandardMenu.Modify,
                CommandGroup.PivotCommands,
                "Move Pivot/Z/Max",
                "Move pivot to back edge of bounding box",
                Keys.D9,
                null,
                CommandVisibility.Menu,
                this);

            m_commandService.RegisterCommand(
                Command.AllCenter,
                StandardMenu.Modify,
                CommandGroup.PivotCommands,
                "Move Pivot/All/Center",
                "Move pivot to center of bounding box",
                Keys.D0,
                null,
                CommandVisibility.Menu,
                this);
            
        }

        #endregion

        #region ICommandClient Members

        /// <summary>
        /// Can the client do the command?</summary>
        /// <param name="commandTag">Command</param>
        /// <returns>True, iff client can do the command</returns>
        public bool CanDoCommand(object commandTag)
        {
            if( commandTag is Command )
            {
                foreach (ITransformable xformable in Transformables)
                    return true;
            }
            return false;                
        }

        /// <summary>
        /// Does the command</summary>
        /// <param name="commandTag">Command to be done</param>
        public void DoCommand(object commandTag)
        {
            if (commandTag is Command)
            {
                Command cmd = (Command)commandTag;
                var gamecontext = m_contextRegistry.GetActiveContext<IGameContext>();
                var transactionContext = (ITransactionContext)gamecontext;
                transactionContext.DoTransaction(() =>                    
                    {
                        foreach (ITransformable xformable in Transformables)
                        {                                                        
                            xformable.Pivot = 
                                m_functions[(int)cmd](xformable.Pivot, xformable.As<IBoundable>().LocalBoundingBox);                            
                        }
                    }, "Move Pivot".Localize());

            }            
        }
      
        private IEnumerable<ITransformable> Transformables
        {
            get
            {
                var gamecontext = m_contextRegistry.GetActiveContext<IGameContext>();
                var selectinContext = (ISelectionContext)gamecontext;
                return selectinContext != null ?
                   selectinContext.GetSelection<ITransformable>() : EmptyArray<ITransformable>.Instance;                
            }
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <remarks>This is used e.g. to set the check next to a menu command or 
        /// to show a toolbar button as pressed</remarks>
        /// <param name="commandTag">Command</param>
        /// <param name="commandState">Command info to update</param>
        public void UpdateCommand(object commandTag, CommandState commandState)
        {
            
        }

        #endregion

        /// <summary>
        /// Command enumeration, used as command tags to distinguish between commands</summary>
        private enum Command
        {
            XMin,
            XCenter,
            XMax,
            YMin,
            YCenter,
            YMax,
            ZMin,
            ZCenter,
            ZMax,
            AllCenter
        }

        private enum CommandGroup
        {
            PivotCommands
        }
        private Func<Vec3F,AABB,Vec3F>[] m_functions;

        private delegate Vec3F MovePivot(Vec3F pivot, AABB aabb);
        private readonly ICommandService m_commandService;
        private readonly IContextRegistry m_contextRegistry;
    }
}
