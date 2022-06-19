using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.IO;
using System.Windows.Media.Imaging;

using Autodesk; 
using Autodesk.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;

namespace SketchFull
{
    /// <summary>
    /// Implements the Revit add-in interface IExternalApplication
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class SketchFullApp : IExternalApplication
    {
        public static AreaDirect areaDirect = AreaDirect.Main;
        public static AreaLayer areaLayer = AreaLayer.Up;

        static string AddInPath = typeof(SketchFullApp).Assembly.Location;
        // Button icons directory
        static string ButtonIconsFolder = Path.GetDirectoryName(AddInPath);
        
        #region IExternalApplication Members
        /// <summary>
        /// Implements the OnShutdown event
        /// </summary>
        /// <param name="application"></param>
        /// <returns></returns>
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        /// <summary>
        /// Implements the OnStartup event
        /// </summary>
        /// <param name="application"></param>
        /// <returns></returns>
        public Result OnStartup(UIControlledApplication application)
        {            
            Autodesk.Revit.ApplicationServices.LanguageType lt = application.ControlledApplication.Language;
            if (lt.ToString() == "Russian") Resourses.Strings.Texts.Culture = new System.Globalization.CultureInfo("ru-RU");

            RibbonPanel ribbonPanel = application.CreateRibbonPanel(Resourses.Strings.Texts.NamePanel);
            PushButtonData styleSettingButton = new PushButtonData("DatumStyle", Resourses.Strings.Texts.NameImage, AddInPath, "SketchFull.SketchCommand");
            styleSettingButton.LargeImage = new BitmapImage(new Uri(Path.Combine(ButtonIconsFolder + "\\Resources\\Images\\", "RebarDrawing.png"), UriKind.Absolute)); ;

            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            ContextualHelp ch2 = new ContextualHelp(ContextualHelpType.ChmFile, path + Resourses.Strings.Texts.pathToHelp);
            styleSettingButton.SetContextualHelp(ch2);
            ribbonPanel.AddItem(styleSettingButton);
          
            return Result.Succeeded;
        }

        #endregion
    }
}
