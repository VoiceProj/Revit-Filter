using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections;
using System.Globalization;

namespace Filters
{
    /// <summary>
    /// Логика взаимодействия для Selector.xaml
    /// </summary>
    /// 

    class ParameterComparer : IEqualityComparer<Parameter>
    {
        public bool Equals(Parameter x, Parameter y)
        {
            if (Object.ReferenceEquals(x, y)) return true;
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;
            return (Math.Round(x.AsDouble()) == Math.Round(y.AsDouble()) && x.AsElementId() == y.AsElementId() && x.AsInteger() == y.AsInteger() && x.AsString() == y.AsString() && x.AsValueString() == y.AsValueString());
        }
        public int GetHashCode(Parameter product)
        {
            return 1;
        }

    }

    class SortComparer : IComparer<Parameter>
    {
        public int Compare(Parameter x, Parameter y)
        {
            return String.Compare(x.Definition.Name, y.Definition.Name);
        }

        //int IComparer.Compare(object x, object y)
        //{
        //    Parameter a = x as Parameter;
        //    Parameter b = y as Parameter;
        //    return String.Compare(a.Definition.Name, b.Definition.Name);
        //}
    }
    class SortValueComparer : IComparer
    {
        int IComparer.Compare(object x, object y)
        {
            double d1, d2;
            string[] s1 = (x as Parameter).AsValueString().Split(' ');
            string[] s2 = (y as Parameter).AsValueString().Split(' ');
            //Double.TryParse((x as Parameter).AsValueString(), out d1);
            Double.TryParse(s1[0], NumberStyles.Number, CultureInfo.CreateSpecificCulture("en-US"), out d1);
            Double.TryParse(s2[0], NumberStyles.Number, CultureInfo.CreateSpecificCulture("en-US"), out d2);
            if (d1 > d2) return 1;
            else if (d1 < d2) return -1;
            else return 0;
        }
    }

    public partial class Selector : Window
    {
        Document doc;
        ParameterComparer comparer = new ParameterComparer();
        SortComparer sortSet = new SortComparer();
        SortValueComparer svcomp = new SortValueComparer();
        List<ElementId> elementsId;
        List<ElementId> filteredElementId = new List<ElementId>();
        public Selector(List<ElementId> elements, Document d)
        {
            InitializeComponent();
            elementsId = elements;
            btnExemplar.IsChecked = true;
            doc = d;
            Get_Param();
        }

        private void btnExemplar_Checked(object sender, RoutedEventArgs e)
        {
            btnExemplar.IsChecked = true;
            btnType.IsChecked = false;
            btnExemplar.Background = (Brush)((new BrushConverter()).ConvertFrom("#CCCCCC"));
            btnType.Background = (Brush)((new BrushConverter()).ConvertFrom("White"));
        }

        private void btnType_Checked(object sender, RoutedEventArgs e)
        {
            btnExemplar.IsChecked = false;
            btnType.IsChecked = true;
            btnType.Background = (Brush)((new BrushConverter()).ConvertFrom("#CCCCCC"));
            btnExemplar.Background = (Brush)((new BrushConverter()).ConvertFrom("White"));

        }

        private void Get_Param()
        {
            Element el = doc.GetElement(elementsId.First());
            ParameterSet Param = el.Parameters;

            //ArrayList q = new ArrayList();
            List<Parameter> q = new List<Parameter>();
            q = q.Distinct().ToList();
            foreach (Parameter o in Param)
                q.Add(o);
            q.Sort(sortSet);
            
            foreach (Parameter p in q)
            {
                if (p.HasValue)
                    treeView.Items.Add(CreateTreeViewItem(p));
            }
        }

        private TreeViewItem CreateTreeViewItem(Parameter p)
        {

            TreeViewItem newItem = new TreeViewItem();
            CheckBox cb = new CheckBox();
            cb.Checked += checkBox_Checked;
            cb.Unchecked += checkBox_Unchecked;
            cb.Content = p.Definition.Name;
            newItem.Header = cb;

            List<Parameter> differentParam = new List<Parameter>();
            foreach (ElementId ei in elementsId)
            {
                Element el = doc.GetElement(ei);
                if (el.get_Parameter(p.Definition) != null)
                {
                    if (el.get_Parameter(p.Definition).HasValue)
                    {
                        Parameter param = el.get_Parameter(p.Definition);
                        if (differentParam.Count != 0)
                        {
                            if (!differentParam.Contains(param, comparer))
                            {
                                differentParam.Add(param);
                            }
                        }
                        else
                        {
                            differentParam.Add(param);
                        }
                    }
                }
            }
            try
            {                                           
                ArrayList q = new ArrayList();         
                foreach (Parameter o in differentParam) 
                    q.Add(o);
                q.Sort(svcomp);
                differentParam.Clear();
                foreach (Parameter pra in q)
                {
                    differentParam.Add(pra);
                }
            }
            catch { }
            foreach (Parameter param in differentParam)
            {
                string content = "";
                string toolTipString = "Варианты представления:\n";
#pragma warning disable CS0472 // Результат значения всегда одинаковый, так как значение этого типа никогда не равно NULL
                if (param.AsInteger() != null)
                    toolTipString += "As Integer: " + param.AsInteger().ToString() + ";\n";
#pragma warning disable CS0472 // Результат значения всегда одинаковый, так как значение этого типа никогда не равно NULL
                if (param.AsDouble() != null)
                    toolTipString += "As Double: " + param.AsDouble().ToString() + ";\n";
                if (param.AsString() != null)
                    toolTipString += "As String: " + param.AsString() + ";\n";
                if (param.AsValueString() != null)
                    toolTipString += "As Value String: " + param.AsValueString() + ";";

                content = CheckParam(param);
                if (content == "")
                {
                    switch (param.StorageType)
                    {
                        case StorageType.Integer:
                            content = param.AsInteger().ToString();
                            break;
                        case StorageType.Double:
                            if (param.AsValueString() != null)
                                content = param.AsValueString();
                            else
                                content = param.AsDouble().ToString("#.###");
                            break;
                        case StorageType.ElementId:
                            content = param.AsValueString();
                            break;
                        case StorageType.String:
                            if (param.AsString() != null)
                                content = param.AsString();
                            else
                                content = param.AsValueString();
                            break;
                    }
                }

                TreeViewItem newItem1 = new TreeViewItem();

                CheckBox cb1 = new CheckBox();
                cb1.Checked += checkBox_Checked;
                cb1.Unchecked += checkBox_Unchecked;
                cb1.Content = content;
                cb1.ToolTip = toolTipString;
                cb1.Tag = param;

                newItem1.Header = cb1;

                newItem.Items.Add(newItem1);
            }

            return newItem;
        }
        private void checkBox_Checked(object sender, EventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            TreeViewItem tvi = cb.Parent as TreeViewItem;
            if (tvi.HasItems)
            {
                foreach (TreeViewItem tvitest in tvi.Items)
                {
                    (tvitest.Header as CheckBox).IsChecked = true;
                }
            }
            else
            {
                Parameter p = cb.Tag as Parameter;
                foreach (ElementId ei in elementsId)
                {
                    Parameter par = doc.GetElement(ei).get_Parameter(p.Definition);
                    if (Math.Round(par.AsDouble()) == Math.Round(p.AsDouble()) && par.AsElementId() == p.AsElementId() && par.AsInteger() == p.AsInteger() && par.AsString() == p.AsString() && par.AsValueString() == p.AsValueString())
                        filteredElementId.Add(ei);
                }
                filteredElementId = filteredElementId
                    .Distinct()
                    .ToList();
            }
            label1.Content = "Выбрано элементов: " + filteredElementId.Count;
        }

        private void checkBox_Unchecked(object sender, EventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            TreeViewItem tvi = cb.Parent as TreeViewItem;
            if (tvi.HasItems)
            {
                foreach (TreeViewItem tvitest in tvi.Items)
                {
                    (tvitest.Header as CheckBox).IsChecked = false;
                }
            }
            else
            {
                Parameter p = cb.Tag as Parameter;
                foreach (ElementId ei in elementsId)
                {
                    Parameter par = doc.GetElement(ei).get_Parameter(p.Definition);
                    if (Math.Round(par.AsDouble()) == Math.Round(p.AsDouble()) && par.AsElementId() == p.AsElementId() && par.AsInteger() == p.AsInteger() && par.AsString() == p.AsString() && par.AsValueString() == p.AsValueString())
                        filteredElementId.Remove(ei);
                }
                filteredElementId = filteredElementId
                    .Distinct()
                    .ToList();
                label1.Content = "Выбрано элементов: " + filteredElementId.Count;
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (filteredElementId.Count != 0)
            {
                UIDocument uiDoc = new UIDocument(doc);
                uiDoc.Selection.SetElementIds(filteredElementId);
            }
            filteredElementId.Clear();
            this.Close();
        }

        private string CheckParam(Parameter p)
        {
            List<string> boolList = new List<string>() { "Вариант конструкции", "Включить аналитическую модель", "Граница помещения", "Использование в конструкции", "Несущие конструкции", "Примыкание сверху", "Примыкание снизу", "Связь с формообразующим элементом", "Перемещать с соседними элементами", "3D", "Метка сверху", "Метка снизу", "Показать стрелку вверх на всех видах", "Стрелка сверху", "Стрелка снизу", "Выноска", "Код основы", "Перемещать с сеткой", "Расчет коэффициента использования", "Сохранять пропорции", "Построение этажа", "Mortat Visibility" };
            List<string> valueList = new List<string>() { "Слой построений", "Выравнивание", "Рабочий набор", "Линия привязки", "Правило компоновки", "Выравнивание по оси Y", "Выравнивание по оси Z", "Выравнивание по осям YZ", "Использование в конструкции", "Местоположение условного знака", "Тип привязки в конце", "Тип привязки в начале", "Привязка к грани", "Ориентация", "Тип выноски", "Стиль колонн", "Слой построений" };
            if (boolList.Contains(p.Definition.Name))
            {
                if (p.AsInteger() == 0)
                    return "Нет";
                else if (p.AsInteger() == 1)
                    return "Да";
                else if (p.AsInteger() == -1)
                    return "Отстствует";
            }
            if (valueList.Contains(p.Definition.Name))
            {
                return p.AsValueString();
            }
            
            return "";
        }
    }
}
