//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;
using System.Runtime.InteropServices;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

using LevelEditorCore;

using Resources = Sce.Atf.Resources;

namespace LevelEditor
{
    /// <summary>
    /// Main application class</summary>
    public class LevelEditorApplication
    {               
        /// <summary>
		/// The main entry point for the application</summary>
        [STAThread]
		static void  Main()
        {

            double start = LevelEditorCore.Timing.GetHiResCurrentTime();
#if DEBUG
            AllocConsole();
#endif

            // It's important to call these before starting the app; otherwise theming and bitmaps
            //  may not render correctly.
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.DoEvents(); // see http://www.codeproject.com/buglist/EnableVisualStylesBug.asp?df=100&forumid=25268&exp=0&select=984714

            // Set up localization support early on, so that user-readable strings will be localized
            //  during the initialization phase below. Use XML files that are embedded resources.
            Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.CurrentCulture;            
            Localizer.SetStringLocalizer(new EmbeddedResourceStringLocalizer());

#if !DEBUG
            SplashForm.ShowForm(typeof(LevelEditorApplication), "LevelEditor.Resources.SplashImg.png");
#endif
          
            // Register the embedded image resources so that they will be available for all users of ResourceUtil,
            //  such as the PaletteService.
            ResourceUtil.Register(typeof(Resources));

            // enable metadata driven property editing
            DomNodeType.BaseOfAllTypes.AddAdapterCreator(new AdapterCreator<CustomTypeDescriptorNodeAdapter>());

            // Add selected ATF components.
            TypeCatalog AtfCatalog = new TypeCatalog(       
                typeof(SettingsService),                // persistent settings and user preferences dialog               
                typeof(Outputs),                        // service that provides static methods for writing to IOutputWriter objects.
                typeof(OutputService),                  // rich text box for displaying error and warning messages. Implements IOutputWriter.                
                typeof(CommandService),                 // menus and toolbars
                typeof(ControlHostService),             // docking control host                        
                typeof(AtfUsageLogger),                 // logs computer info to an ATF server
                typeof(CrashLogger),                    // logs unhandled exceptions to an ATF server
                typeof(PythonService),                  // scripting service for automated tests
                typeof(ScriptConsole),
                typeof(AtfScriptVariables),
                typeof(AutomationService),
                typeof(FileDialogService),              // standard Windows file dialogs
                typeof(DocumentRegistry),               // central document registry with change notification            
                typeof(RecentDocumentCommands),         // standard recent document commands in File menu
                typeof(AutoDocumentService),            // opens documents from last session, or creates a new document, on startup            
                typeof(StandardFileExitCommand),        // standard File exit menu command            
                typeof(StandardViewCommands),           // standard View commands: frame selection, frame all
                typeof(MainWindowTitleService),         // tracks document changes and updates main form title            
                typeof(ContextRegistry),                // central context registry with change notification
                typeof(StandardEditCommands),           // standard Edit menu commands for copy/paste
                typeof(StandardEditHistoryCommands),    // standard Edit menu commands for undo/redo
                typeof(StandardSelectionCommands),      // standard Edit menu selection commands
                typeof(StandardLockCommands),           // standard Edit menu lock/unlock commands            
                typeof(PaletteService),                 // global palette, for drag/drop instancing            
                typeof(PropertyEditingCommands),        // commands for PropertyEditor and GridPropertyEditor             
                typeof(PropertyEditor),
                typeof(GridPropertyEditor),                
                typeof(WindowLayoutService),            // multiple window layout support
                typeof(WindowLayoutServiceCommands),    // window layout commands
                typeof(HistoryLister),                  // visual undo/redo
                typeof(SkinService),
                typeof(ResourceService)
                );

            TypeCatalog LECoreCatalog = new TypeCatalog(                                
                typeof(LevelEditorCore.DesignViewSettings),
                typeof(LevelEditorCore.ResourceLister),
                typeof(LevelEditorCore.ThumbnailService),
                typeof(LevelEditorCore.ResourceMetadataEditor),
                typeof(LevelEditorCore.LayerLister),
                typeof(LevelEditorCore.ResourceConverterService),
                
                typeof(LevelEditorCore.Commands.PickFilterCommands),
                typeof(LevelEditorCore.Commands.DesignViewCommands),
                typeof(LevelEditorCore.Commands.ManipulatorCommands),
                typeof(LevelEditorCore.Commands.ShowCommands),
                typeof(LevelEditorCore.Commands.GroupCommands),
                typeof(LevelEditorCore.Commands.CameraCommands),
                typeof(LevelEditorCore.MayaStyleCameraController),
                typeof(LevelEditorCore.ArcBallCameraController),
                typeof(LevelEditorCore.WalkCameraController),
                typeof(LevelEditorCore.FlyCameraController)
                );

            TypeCatalog thisAssemCatalog = new TypeCatalog(
                typeof(LevelEditor.GameLoopService),                
                typeof(LevelEditor.GameEditor),
                typeof(LevelEditor.BookmarkLister),                                
                typeof(LevelEditor.GameDocumentRegistry),                
                typeof(LevelEditor.SchemaLoader),
                typeof(LevelEditor.PrototypingService),
                typeof(LevelEditor.PrefabService),
                typeof(LevelEditor.GameProjectLister),
                typeof(LevelEditor.ResourceMetadataService),
                typeof(LevelEditor.ResourceConverter),               
                typeof(LevelEditor.Terrain.TerrainEditor),
                typeof(LevelEditor.Terrain.TerrainManipulator),                 
                typeof(LevelEditor.SnapFilter),
                typeof(LevelEditor.PickFilters.LocatorPickFilter),
                typeof(LevelEditor.PickFilters.BasicShapePickFilter),
                typeof(LevelEditor.PickFilters.NoCubePickFilter),                
                typeof(LevelEditor.Commands.PaletteCommands),
                typeof(LevelEditor.Commands.LevelEditorFileCommands),
                typeof(LevelEditor.Commands.HelpAboutCommand),
                typeof(LevelEditor.Commands.LevelEditorCommands),
                typeof(LevelEditor.Commands.LayeringCommands),
                typeof(LevelEditor.Commands.PivotCommands)
                // To use Open Sound Control (OSC), enable these three components:
                //,
                //typeof(LevelEditor.OSC.OscClient),
                //typeof(OscCommands),                    // Provides a GUI for configuring OSC support and to diagnose problems.
                //typeof(OscCommandReceiver)              // Executes this app's commands in response to receiving matching OSC messages.
                                                        // Needs to come after all the other ICommandClients in the catalog.
                );

            TypeCatalog renderingInteropCatalog = new TypeCatalog(
                typeof(RenderingInterop.GameEngine),
                typeof(RenderingInterop.NativeGameEditor),
                typeof(RenderingInterop.ThumbnailResolver),
                typeof(RenderingInterop.RenderCommands),
                typeof(RenderingInterop.AssetResolver),
                typeof(RenderingInterop.NativeDesignView),
                typeof(RenderingInterop.ResourcePreview),
                typeof(RenderingInterop.TranslateManipulator),  
                typeof(RenderingInterop.ExtensionManipulator),
                typeof(RenderingInterop.ScaleManipulator),
                typeof(RenderingInterop.RotateManipulator),
                typeof(RenderingInterop.TranslatePivotManipulator),
                typeof(RenderingInterop.TextureThumbnailResolver)
                );

            
            List<ComposablePartCatalog> catalogs = new List<ComposablePartCatalog>();
            catalogs.Add(AtfCatalog);            
            catalogs.Add(LECoreCatalog);
            catalogs.Add(renderingInteropCatalog);
            catalogs.Add(thisAssemCatalog);
            
                        
            // temp solution, look for statemachine plugin by name.
            string pluginDir = Application.StartupPath;
            string stmPlg = pluginDir + "\\StateMachinePlugin.dll";
            if(File.Exists(stmPlg))
            {
                Assembly stmPlgAssem = Assembly.LoadFrom(stmPlg);
                catalogs.Add(new AssemblyCatalog(stmPlgAssem));
            }

            
            AggregateCatalog catalog = new AggregateCatalog(catalogs);
               
                      
            // Initialize ToolStripContainer container and MainForm
            ToolStripContainer toolStripContainer = new ToolStripContainer();
            toolStripContainer.Dock = DockStyle.Fill;
            MainForm mainForm = new MainForm(toolStripContainer);
            mainForm.Text = "LevelEditor".Localize("the name of this application, on the title bar");
             
            CompositionContainer container = new CompositionContainer(catalog);
            CompositionBatch batch = new CompositionBatch();
            AttributedModelServices.AddPart(batch, mainForm);
            container.Compose(batch);

            LevelEditorCore.Globals.InitializeComponents(container);
            // Initialize components 

            var gameEngine = container.GetExportedValue<IGameEngineProxy>();
            foreach (IInitializable initializable in container.GetExportedValues<IInitializable>())
            {                
                initializable.Initialize();
            }
            GC.KeepAlive(gameEngine);

            AutoDocumentService autoDocument = container.GetExportedValue<AutoDocumentService>();
            autoDocument.AutoLoadDocuments = false;
            autoDocument.AutoNewDocument = true;
            mainForm.Shown += delegate { SplashForm.CloseForm(); };

            // The settings file is incompatible between languages that LevelEditor and ATF are localized to.
            // For example, the LayoutService saves different Control names depending on the language and so
            //  the Windows layout saved in one language can't be loaded correctly in another language.
            string language = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName; //"en" or "ja"
            if (language == "ja")
            {
                var settingsService = container.GetExportedValue<SettingsService>();
                string nonEnglishPath = settingsService.SettingsPath;
                nonEnglishPath = Path.Combine(Path.GetDirectoryName(nonEnglishPath), "AppSettings_" + language + ".xml");
                settingsService.SettingsPath = nonEnglishPath;
            }

            Application.Run(mainForm); // MAIN LOOP

            container.Dispose();
            GC.KeepAlive(start);
           
        }

        [DllImport("kernel32.dll")]
        private static extern Boolean AllocConsole();
    }
}
