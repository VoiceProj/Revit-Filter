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
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class ExternalCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UserControl1 userControl = new UserControl1(commandData);
                userControl.Topmost = true;

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

                userControl.Icon = MicroFE1;
                userControl.Title = "Advanced Filter";
                userControl.Show();
            }
            catch
            {
                MessageBox.Show("Возможно, не открыт проект Revit", "Advanced Filter", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return Result.Succeeded;
        }
    }


}
