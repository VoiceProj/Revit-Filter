using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Diagnostics;

namespace Filters
{
    /// <summary>
    /// Логика взаимодействия для UserControl1.xaml
    /// </summary>
    /// 

    class CategoryComparer : IEqualityComparer<Category>
    {
        #region Implementation of IEqualityComparer<in Category>

        public bool Equals(Category x, Category y)
        {
            if (x == null || y == null) return false;
            return x.Name.Equals(y.Name);
        }

        public int GetHashCode(Category obj)
        {
            return obj.Id.IntegerValue;
        }

        #endregion
    }

    class CategoryNameComparer : IComparer<Category>
    {
        public int Compare(Category c1, Category c2)
        {
            if (c1 != null && c2 != null && (String.Compare(c1.Name, c2.Name) < 0))
            {
                return -1;
            }
            else return 1;
        }
    }

    public partial class UserControl1 : Window
    {
        Document doc;
        List<Category> categories;
        FilteredElementCollector catCollector;
        IList<Element> elements;
        List<ElementId> elementsId = new List<ElementId>();
        List<ElementId> FilteredElementsId = new List<ElementId>();
        UIDocument uiDoc;

        public UserControl1(ExternalCommandData commandData)
        {
            doc = commandData.Application.ActiveUIDocument.Document;
            InitializeComponent();
            catCollector = new FilteredElementCollector(doc);
            uiDoc = new UIDocument(doc);
            CreateCategoryList();       //Создание categories - списка всех категорий в проекте
        }

        /// <summary>
        /// Создание Dictionary, содержащего ( Имя семейства; Лист всех типов семейства )
        /// </summary>
        /// <param name="doc">Текущий документ</param>
        /// <param name="cat">Категория</param>
        /// <returns>Созданный словарь</returns>
        //public static Dictionary<string, List<FamilySymbol>> FindFamilyTypes(Document doc, BuiltInCategory cat)
        //{
        //    Dictionary<string, List<FamilySymbol>> dict = new FilteredElementCollector(doc)
        //                    .WherePasses(new ElementClassFilter(typeof(FamilySymbol)))
        //                    .WherePasses(new ElementCategoryFilter(cat))
        //                    .Cast<FamilySymbol>()
        //                    .GroupBy(e => e.Family.Name)
        //                    .ToDictionary(e => e.Key, e => e.ToList());
        //    return dict;
        //}

        /// <summary>
        /// Создание списка всех категорий в проекте
        /// </summary>
        public void CreateCategoryList()
        {
           // ICollection<Element> categ = catCollector.OfClass(typeof(Category)).ToElements();
            categories = new FilteredElementCollector(doc).WhereElementIsNotElementType()
                .Where(x => (x.Category != null) && x.GetTypeId().IntegerValue != -1)
                .Select(x => x.Category)
                .Distinct(new CategoryComparer())
                .ToList();
            categories.Sort(new CategoryNameComparer());

            foreach (Category c in categories)
            {

               // Stopwatch swOpenDoc = new Stopwatch();
                ComboBoxItem cbi = new ComboBoxItem();
                
                cbi.HorizontalContentAlignment = HorizontalAlignment.Left;
                cbi.Padding = new Thickness(30, 0, 0, 0);
                cbi.Content = c.Name;
                cbi.Tag = c;
                comboBox.Items.Add(cbi);
            }
        }


        // Переход на новую форму
        private void button_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).Content.ToString() != "Выбрано элементов: 0")
            {
                this.Hide();
                Selector selForm = new Selector(elementsId, doc);
                selForm.Topmost = true;

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
                selForm.Icon = MicroFE1;
                selForm.Title = "Advanced Filter";
                selForm.Show();
                selForm.Closed += SelForm_Closed;
            }

        }

        private void SelForm_Closed(object sender, EventArgs e)
        {
            FilteredElementsId = uiDoc.Selection.GetElementIds().ToList();
            button.Content = "Выбрано элементов: " + FilteredElementsId.Count.ToString();
            this.Show();
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBox.HasItems)
            {
                foreach (ListBoxItem lbi in listBox.Items)
                {
                    lbi.IsSelected = false;
                }
            }
            button.Content = "Выбрано элементов: 0";
            listBox.Items.Clear();
            elementsId.Clear(); // Иначе выбор нескольких категорий
            Category c = (((sender as System.Windows.Controls.ComboBox).SelectedItem as ComboBoxItem).Tag as Category);
            BuiltInCategory myCatEnum = (BuiltInCategory)Enum.Parse(typeof(BuiltInCategory), c.Id.ToString());
            ElementCategoryFilter filter = new ElementCategoryFilter(c.Id);
            IList<Element> famTypes = new FilteredElementCollector(doc)
                .WherePasses(filter)
                .OfClass(typeof(ElementType))
                .ToElements();
            foreach (Element item in famTypes)
            {
                FamilySymbol fs = item as FamilySymbol;
                if (fs != null && fs.Family.IsInPlace == true) continue;
                FilteredElementCollector colls2 = new FilteredElementCollector(doc);
                ICollection<Element> coll2 = colls2.OfCategory(myCatEnum).Where(x => x.Name == item.Name && x.Location != null).ToList();
                if (coll2.Count != 0)
                {
                    ListBoxItem lbi = new ListBoxItem();
                    lbi.Content = item.Name;
                    lbi.Selected += listBoxItem_Selected;
                    lbi.Unselected += listBoxItem_Unselected;
                    lbi.Tag = item;
                    listBox.Items.Add(lbi);
                }
            }
            /*bool flag = false;
            button.Content = "Выбрано элементов: 0";
            listBox.Items.Clear();
            elementsId.Clear(); // Иначе выбор нескольких категорий
            Category c = (((sender as System.Windows.Controls.ComboBox).SelectedItem as ComboBoxItem).Tag as Category);
            //Попытка найти BuiltIn категорию для "c", если получается, то ищем для нее тип
            BuiltInCategory myCatEnum = (BuiltInCategory)Enum.Parse(typeof(BuiltInCategory), c.Id.ToString());
            Dictionary<string, List<FamilySymbol>> winFamilyTypes = FindFamilyTypes(doc, myCatEnum);
            foreach (KeyValuePair<string, List<FamilySymbol>> entry in winFamilyTypes)
            {
                
                foreach (FamilySymbol item in entry.Value)
                {                    
                    IList<Element> instances = new List<Element>();
                    instances = new FilteredElementCollector(doc)           // Поиск экземпляров выбранного типа, ID всех найденных экземпляров пердается через Tag
                        .OfClass(typeof(FamilyInstance))
                        .Where(z => z.Name == item.Name && z.Location != null)
                        .ToList();
                    if (instances.Count != 0)
                    {
                        flag = true;
                        List<ElementId> ids = new List<ElementId>();
                        foreach (Element z in instances)
                        {
                            ids.Add(z.Id);
                        }
                        ListBoxItem lbi = new ListBoxItem();
                        lbi.Content = item.Name + ";  Экземпляров: " + instances.Count.ToString();
                        lbi.Selected += listBoxItem_Selected;
                        lbi.Unselected += listBoxItem_Unselected;
                        lbi.Tag = ids;
                        listBox.Items.Add(lbi);
                    }
                }
            }


            // Если семейство системное, то для него нет FamilySymbol и нужно узнавать тип через ElementType
            if (winFamilyTypes.Count == 0 || !flag)
            {
                Dictionary<string, ICollection<Element>> inst = new Dictionary<string, ICollection<Element>>();
                GetInstancesFromSystemCategory(inst, c);
                foreach (KeyValuePair<string, ICollection<Element>> kv in inst)
                {
                    List<ElementId> ids = new List<ElementId>();
                    foreach (Element z in kv.Value)
                    {
                        ids.Add(z.Id);
                    }
                    ListBoxItem lbi = new ListBoxItem();
                    lbi.Content = kv.Key
                      + ";  Экземпляров: " + kv.Value.Count.ToString();
                    lbi.Tag = ids;
                    lbi.Selected += listBoxItem_Selected;
                    lbi.Unselected += listBoxItem_Unselected;
                    listBox.Items.Add(lbi);
                }
            }*/
        }

        private void listBoxItem_Selected(object sender, EventArgs e)
        {
            ElementType type = (sender as ListBoxItem).Tag as ElementType;
            Category c = (comboBox.SelectedItem as ComboBoxItem).Tag as Category;
            ICollection<Element> inst = GetInstancesFromSystemCategory(c, type);
            if (FilteredElementsId.Count == 0)
            {
                //elementsId.Clear();
                elementsId.AddRange(inst.Select(x=> x.Id));
            }
            else
            {
                elementsId = FilteredElementsId;
                foreach (ElementId ei in inst.Select(x => x.Id))
                    if (elementsId.Contains(ei))
                        break;
                    else
                        elementsId.Add(ei);
            }
            button.Content = "Выбрано элементов: " + elementsId.Count;
            uiDoc.Selection.SetElementIds(elementsId);
        }


        private void listBoxItem_Unselected(object sender, EventArgs e)
        {
            ElementType type = (sender as ListBoxItem).Tag as ElementType;
            Category c = (comboBox.SelectedItem as ComboBoxItem).Tag as Category;
            ICollection<Element> inst = GetInstancesFromSystemCategory(c, type);
            List<ElementId> toRemove = inst.Select(x=>x.Id).ToList();
            foreach (ElementId ei in toRemove)
            {
                FilteredElementsId.Remove(ei);
                elementsId.Remove(ei);
            }
            button.Content = "Выбрано элементов: " + elementsId.Count;
            uiDoc.Selection.SetElementIds(elementsId);
        }


        public ICollection<Element> GetInstancesFromSystemCategory(Category c, ElementType type)
        {
            BuiltInCategory myCatEnum = (BuiltInCategory)Enum.Parse(typeof(BuiltInCategory), c.Id.ToString());
            int ExcCounter = 0;
            //ElementId ei = c.Id;
            //ElementCategoryFilter filter = new ElementCategoryFilter(ei);
            //IList<Element> famTypes = catCollector
            //    .WherePasses(filter)
            //    .OfClass(typeof(ElementType))
            //    .ToElements();

            //foreach (Element ele in famTypes)
            ////{
            //    ElementType type = ele as ElementType;
            FilteredElementCollector colls2 = new FilteredElementCollector(doc);
            ICollection<Element> coll2;
            try
            {
                coll2 = colls2.OfCategory(myCatEnum).Where(x => x.Name == type.Name && x.Location!=null).ToList();
                return coll2;
            }        
            catch (ArgumentException)
            {
                //ExcCounter++;
                //if (coll2.Count != 0)
                //    inst.Add(type.Name + "(" + type.FamilyName + ")", coll2);
            }
            catch { }
            return coll2 = null;
            //}
        }


        //Жутко долго и крашится        
        //public bool CheckCategory(Category c)
        //{
        //    bool flag = false;
        //    BuiltInCategory myCatEnum = (BuiltInCategory)Enum.Parse(typeof(BuiltInCategory), c.Id.ToString());
        //    Dictionary<string, List<FamilySymbol>> winFamilyTypes = FindFamilyTypes(doc, myCatEnum);
        //    foreach (KeyValuePair<string, List<FamilySymbol>> entry in winFamilyTypes)
        //    {
        //        foreach (FamilySymbol item in entry.Value)
        //        {
        //            IList<Element> instances = new List<Element>();
        //            instances = new FilteredElementCollector(doc)           // Поиск экземпляров выбранного типа, ID всех найденных экземпляров пердается через Tag
        //                .OfClass(typeof(FamilyInstance))
        //                .Where(z => z.Name == item.Name && z.Location != null)
        //                .ToList();
        //            if (instances.Count != 0)
        //            {
        //                flag = true;
        //            }
        //        }
        //    }
        //    if (winFamilyTypes.Count == 0)
        //    {
        //        Dictionary<string, IList<Element>> inst = new Dictionary<string, IList<Element>>();
        //        GetInstancesFromSystemCategory(inst, c);
        //        if (inst.Count != 0)
        //            flag = true;
        //    }
        //    return flag;
        //}
    }
}
