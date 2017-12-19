using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using System.Windows;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.IO;

namespace Filters
{
    public class Availability : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication a, CategorySet b)
        {
            return true;
        }
    }
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    class ExternalApplication : IExternalApplication
    {

        public Result OnStartup(UIControlledApplication application)
        {
            string tabname = "BIMACAD";
            try
            {
                application.CreateRibbonTab(tabname);
            }
            catch { }
            string sPanelName = "Advanced Filter";
            RibbonPanel pan = application.CreateRibbonPanel(tabname, sPanelName);
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
            string buttonName = "Filter";
            string buttonText = "Advanced Filter";
            string buttonClass = "Filters.ExternalCommand";
            PushButtonData buttonData = new PushButtonData(buttonName, buttonText, thisAssemblyPath, buttonClass);
            buttonData.AvailabilityClassName = "Filters.Availability";
            PushButton pushButton = pan.AddItem(buttonData) as PushButton;
            byte[] MicroFE = Container.MicroFE;
            BitmapImage MicroFE1 = null;
            using (MemoryStream byteStream = new MemoryStream(MicroFE))
            {
                BitmapImage ko = new BitmapImage();
                ko.BeginInit();
                ko.CacheOption = BitmapCacheOption.OnLoad;
                ko.StreamSource = byteStream;
                ko.EndInit();
                MicroFE1 = ko;
                byteStream.Close();
            }
            byte[] MicroFES = Container.MicroFE;
            BitmapImage MicroFES1 = null;
            using (MemoryStream byteStream = new MemoryStream(MicroFES))
            {
                BitmapImage ko = new BitmapImage();
                ko.BeginInit();
                ko.CacheOption = BitmapCacheOption.OnLoad;
                ko.StreamSource = byteStream;
                ko.EndInit();
                MicroFES1 = ko;
                byteStream.Close();
            }
            string buttonToolTip = "Advanced Filter";
            pushButton.ToolTip = buttonToolTip;
            string buttonItemText = @"Advanced Filter";
            pushButton.ItemText = buttonItemText;
            pushButton.LargeImage = MicroFE1;
            pushButton.Image = MicroFES1;

            return Result.Succeeded;
        }
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

    }
}
