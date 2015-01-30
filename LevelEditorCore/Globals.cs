//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition.Hosting;

using Sce.Atf;

namespace LevelEditorCore
{
    /// <summary>
    /// Global properties and constants</summary>
    /// <remarks>Generally we try to avoid globals. Most constants should eventually migrate 
    /// to a config file to make it possible to modify them without requiring a binary release.</remarks>
    public static class Globals
    {
        /// <summary>
        /// Gets MEF container.
        /// </summary>
        public static CompositionContainer MEFContainer
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the resource root absolute Uri used to resolve relative resource Uris</summary>
        public static Uri ResourceRoot
        {
            get { return s_resourceRoot; }
            set { s_resourceRoot = value; }
        }

        /// <summary>
        /// Gets the Resource Service</summary>
        public static IResourceService ResourceService { get; private set; }

        /// <summary>
        /// Initializes global static references to MEF components</summary>
        /// <param name="container">Initialized MEF composition container</param>
        public static void InitializeComponents(CompositionContainer container)
        {
            ResourceService = container.GetExportedValue<IResourceService>();
            MEFContainer = container;
        }

        public static bool AutomationMode = false;
        public static string AutomationName = null;

        private static Uri s_resourceRoot;
    }
}
