using System.Diagnostics;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using CSharpHomes.Extensions;
using CSharpHomes.CmdsTools;
using CSharpHomes.CmdsLinks;
using cRib = CSharpHomes.Utilities.RibbonUtils;

// using CSharpHomes.CmdsTools;
// using CSharpHomes.Extensions;

namespace CSharpHomes
{
    /// <summary>
    ///     Application entry point
    /// </summary>
// [UsedImplicitly]
    public class Application : IExternalApplication
    {
        #region Properties

        // Make a private uiCtlApp
        private static UIControlledApplication _uiCtlApp;

        #endregion

        public Result OnStartup(UIControlledApplication uiCtlApp)
        {
            #region Globals registration
            
            // Store _uiCtlApp, register on idling
            _uiCtlApp = uiCtlApp;

            try
            {
                _uiCtlApp.Idling += RegisterUiApp;
            }
            catch
            {
                Globals.UiApp = null;
                Globals.UsernameRevit = null;
            }

            // Registering globals
            Globals.RegisterProperties(uiCtlApp);
            Globals.RegisterTooltips($"{Globals.AddinName}.Resources.Files.Tooltips");

            #endregion

            CreateRibbon(uiCtlApp);
            
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        private void CreateRibbon(UIControlledApplication uiCtlApp)
        {
            uiCtlApp.Ext_AddRibbonTab(Globals.AddinName);
            
            // Create Links panel
            var panelResources = uiCtlApp.Ext_AddRibbonPanel(Globals.AddinName, "Links");
            
            // Create Links stack
            var stackResources1 = cRib.NewPushButtonData<CmdResCode>("ResCode");
            var stackResources2 = cRib.NewPushButtonData<CmdNCC>("NCC");
            var stackResources3 = cRib.NewPushButtonData<CmdFolder>("Project Folder");
            // var stackResources = panelResources.AddStackedItems(
            panelResources.AddStackedItems( stackResources1, stackResources2, stackResources3);
            
            // Create Tools panel
            var panelTools = uiCtlApp.Ext_AddRibbonPanel(Globals.AddinName, "Tools");

            var pushRotate = panelTools.Ext_AddPushButton<CmdRotate>("Rotate");
            // pushRotate.AddShortcuts("RS");
            var pushExpDWG = panelTools.Ext_AddPushButton<CmdExpCAD>("Exp DWG");
            var pushExpTrans = panelTools.Ext_AddPushButton<CmdDocTrans>("Doc Transmittal");
            var pushProjSetup = panelTools.Ext_AddPushButton<CmdProjSetup>("Proj. Setup"); 
        }
        
        #region Use idling to register UiApp

        /// <summary>
        /// Registers the UiApp and Revit username globally.
        /// </summary>
        /// <param name="sender">Sender of the Idling event.</param>
        /// <param name="e">Idling event arguments.</param>
        private static void RegisterUiApp(object sender, IdlingEventArgs e)
        {
            _uiCtlApp.Idling -= RegisterUiApp;

            if (sender is UIApplication uiApp)
            {
                Globals.UiApp = uiApp;
                Globals.UsernameRevit = uiApp.Application.Username;
            }
        }

        #endregion
    }
}