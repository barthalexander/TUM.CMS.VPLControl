using System;
using System.Windows.Controls;
using System.Xml;
using TUM.CMS.VplControl.Core;
using System.Collections.Generic;
using TUM.CMS.VplControl.IFC.Utilities;
using System.Linq;
using System.Windows;
using Xbim.Ifc;
using Xbim.Properties;
using Version = System.Version;
using TUM.CMS.VplControl.IFC.Controls;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcPropertyFilterNode : Node
    {
        public IfcStore xModel;
        private Type IfcVersionType = null;

        public IfcPropertyFilterNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {

            AddInputPortToNode("IfcProducts", typeof(object));

            AddOutputPortToNode("FilteredProducts", typeof(object));

            UserControl usercontrol = new UserControl();
            Grid grid = new Grid();
            usercontrol.Content = grid;

            IfcPropertyFilterControl ifcPropertyFilterControl = new IfcPropertyFilterControl();
            ifcPropertyFilterControl.comboBoxPropertySet.SelectionChanged += comboBoxPropertySet_SelectionChanged;
            ifcPropertyFilterControl.comboBoxProperties.SelectionChanged += comboBoxProperties_SelectionChanged;
            ifcPropertyFilterControl.textBox.TextChanged += textBox_TextChanged;
            ifcPropertyFilterControl.button.Click += button_Click;

            AddControlToNode(ifcPropertyFilterControl);


        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (IfcVersionType.Name == "ModelInfoIFC2x3")
            {
                var ifcPropertyFilterControl = ControlElements[0] as IfcPropertyFilterControl;
                if (ifcPropertyFilterControl == null) return;
                
                var textBox = ifcPropertyFilterControl.textBox as TextBox;
                if (textBox == null) return;
                var button = ifcPropertyFilterControl.button as Button;
                if (button == null) return;
                var comboBoxPropertySet = ifcPropertyFilterControl.comboBoxPropertySet as ComboBox;
                if (comboBoxPropertySet == null) return;
                var comboBoxProperties = ifcPropertyFilterControl.comboBoxProperties as ComboBox;
                if (comboBoxProperties == null) return;
                if (comboBoxProperties.Items.Count == 0) return;

                var selectedItem = (ComboboxItem)(comboBoxProperties.SelectedItem);
                var property = selectedItem.Value as Xbim.Ifc2x3.PropertyResource.IfcProperty;
                if (property == null) return;

                var selectedPropertySet = ((ComboboxItem)(comboBoxPropertySet.SelectedItem)).Value as Xbim.Ifc2x3.Kernel.IfcPropertySet;

                var selectedItemIds = ((ModelInfoIFC2x3)(InputPorts[0].Data)).ElementIds;
                if (selectedItemIds == null) return;

                List<double> propertyValueDoubles = new List<double> { };
                List<bool> propertyValueBools = new List<bool> { };
                List<string> propertyValueStrings = new List<string> { };


                for (int i = 0; i < selectedItemIds.Count; i++)
                {
                    var selectedProduct = xModel.Instances.OfType<Xbim.Ifc2x3.ProductExtension.IfcElement>().ToList().Find(x => x.GlobalId == selectedItemIds[i]);
                    var propertySet = selectedProduct.PropertySets.ToList().Find(x => x.Name == selectedPropertySet.Name);
                    var oneProperty = propertySet.HasProperties.ToList().Find(x => x.Name == property.Name);
                    string propertyType = oneProperty.GetType().ToString();
                    if (propertyType == "Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue")
                    {
                        var property2 = property as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                        var propertyValue = property2.NominalValue;
                        object propertyValueTrue = property2.NominalValue.Value;
                        string propertyValueType = propertyValue.UnderlyingSystemType.Name;
                        if (propertyValueType == "Double")
                        {
                            double propertyValueDouble = (double)propertyValueTrue;
                            propertyValueDoubles.Add(propertyValueDouble);
                        }
                        if (propertyValueType == "Boolean")
                        {
                            bool propertyValueBool = (bool)propertyValueTrue;
                            propertyValueBools.Add(propertyValueBool);
                        }
                        if (propertyValueType == "String")
                        {
                            string propertyValueString = (string)propertyValueTrue;
                            propertyValueStrings.Add(propertyValueString);
                        }

                    }
                    else { return; }
                }



                List<double> propertyValueDoublesSelected = new List<double> { };
                List<bool> propertyValueBoolsSelected = new List<bool> { };
                List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> searchIDsSelected = new List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> { };

                string toDetect = textBox.Text;
                string ss;
                double s;
                if (toDetect.Contains("<"))
                {
                    int j = toDetect.IndexOf("<");
                    ss = toDetect.Substring(j + 1, toDetect.Length - 1);
                    s = double.Parse(ss);
                    for (int i = 0; i < propertyValueDoubles.Count; i++)
                    {
                        if (propertyValueDoubles[i] < s)
                        {
                            propertyValueDoublesSelected.Add(propertyValueDoubles[i]);
                            searchIDsSelected.Add(selectedItemIds[i]);

                        }
                    }
                }

                if (toDetect.Contains(">"))
                {
                    int j = toDetect.IndexOf(">");
                    ss = toDetect.Substring(j + 1, toDetect.Length - 1);
                    s = double.Parse(ss);
                    for (int i = 0; i < propertyValueDoubles.Count; i++)
                    {
                        if (propertyValueDoubles[i] > s)
                        {
                            propertyValueDoublesSelected.Add(propertyValueDoubles[i]);
                            searchIDsSelected.Add(selectedItemIds[i]);
                        }
                    }
                }

                if (toDetect.Contains("="))
                {
                    int j = toDetect.IndexOf("=");
                    ss = toDetect.Substring(j + 1, toDetect.Length - 1);
                    s = double.Parse(ss);
                    for (int i = 0; i < propertyValueDoubles.Count; i++)
                    {
                        if (propertyValueDoubles[i] == s)
                        {
                            propertyValueDoublesSelected.Add(propertyValueDoubles[i]);
                            searchIDsSelected.Add(selectedItemIds[i]);
                        }
                    }
                }

                if (toDetect == "True" || toDetect == "False")
                {
                    for (int i = 0; i < propertyValueBools.Count; i++)
                    {
                        string trueorfalse = propertyValueBools[i].ToString();
                        if (propertyValueBools[i].ToString() == toDetect)
                        {
                            propertyValueBoolsSelected.Add(propertyValueBools[i]);
                            searchIDsSelected.Add(selectedItemIds[i]);
                        }
                    }
                }

                ModelInfoIFC2x3 outputInfo = new ModelInfoIFC2x3(((ModelInfoIFC2x3)(InputPorts[0].Data)).ModelId);
                foreach (var item in searchIDsSelected)
                {
                    outputInfo.AddElementIds(item);
                }
                OutputPorts[0].Data = outputInfo;
            }
            else if (IfcVersionType.Name == "ModelInfoIFC4")
            {
                var ifcPropertyFilterControl = ControlElements[0] as IfcPropertyFilterControl;
                if (ifcPropertyFilterControl == null) return;

                var textBox = ifcPropertyFilterControl.textBox as TextBox;
                if (textBox == null) return;
                var button = ifcPropertyFilterControl.button as Button;
                if (button == null) return;
                var comboBoxPropertySet = ifcPropertyFilterControl.comboBoxPropertySet as ComboBox;
                if (comboBoxPropertySet == null) return;
                var comboBoxProperties = ifcPropertyFilterControl.comboBoxProperties as ComboBox;
                if (comboBoxProperties == null) return;
                if (comboBoxProperties.Items.Count == 0) return;

                var selectedItem = (ComboboxItem)(comboBoxProperties.SelectedItem);
                var property = selectedItem.Value as Xbim.Ifc4.PropertyResource.IfcProperty;
                if (property == null) return;

                var selectedPropertySet = ((ComboboxItem)(comboBoxPropertySet.SelectedItem)).Value as Xbim.Ifc4.Kernel.IfcPropertySet;

                var selectedItemIds = ((ModelInfoIFC4)(InputPorts[0].Data)).ElementIds;
                if (selectedItemIds == null) return;

                List<double> propertyValueDoubles = new List<double> { };
                List<bool> propertyValueBools = new List<bool> { };
                List<string> propertyValueStrings = new List<string> { };


                for (int i = 0; i < selectedItemIds.Count; i++)
                {
                    var selectedProduct = xModel.Instances.OfType<Xbim.Ifc4.ProductExtension.IfcElement>().ToList().Find(x => x.GlobalId == selectedItemIds[i]);
                    var propertySet = selectedProduct.PropertySets.ToList().Find(x => x.Name == selectedPropertySet.Name);
                    var oneProperty = propertySet.HasProperties.ToList().Find(x => x.Name == property.Name);
                    string propertyType = oneProperty.GetType().ToString();
                    if (propertyType == "Xbim.Ifc4.PropertyResource.IfcPropertySingleValue")
                    {
                        var property2 = property as Xbim.Ifc4.PropertyResource.IfcPropertySingleValue;
                        var propertyValue = property2.NominalValue;
                        object propertyValueTrue = property2.NominalValue.Value;
                        string propertyValueType = propertyValue.UnderlyingSystemType.Name;
                        if (propertyValueType == "Double")
                        {
                            double propertyValueDouble = (double)propertyValueTrue;
                            propertyValueDoubles.Add(propertyValueDouble);
                        }
                        if (propertyValueType == "Boolean")
                        {
                            bool propertyValueBool = (bool)propertyValueTrue;
                            propertyValueBools.Add(propertyValueBool);
                        }
                        if (propertyValueType == "String")
                        {
                            string propertyValueString = (string)propertyValueTrue;
                            propertyValueStrings.Add(propertyValueString);
                        }

                    }
                    else { return; }
                }



                List<double> propertyValueDoublesSelected = new List<double> { };
                List<bool> propertyValueBoolsSelected = new List<bool> { };
                List<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId> searchIDsSelected = new List<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId> { };

                string toDetect = textBox.Text;
                string ss;
                double s;
                if (toDetect.Contains("<"))
                {
                    int j = toDetect.IndexOf("<");
                    ss = toDetect.Substring(j + 1, toDetect.Length - 1);
                    s = double.Parse(ss);
                    for (int i = 0; i < propertyValueDoubles.Count; i++)
                    {
                        if (propertyValueDoubles[i] < s)
                        {
                            propertyValueDoublesSelected.Add(propertyValueDoubles[i]);
                            searchIDsSelected.Add(selectedItemIds[i]);

                        }
                    }
                }

                if (toDetect.Contains(">"))
                {
                    int j = toDetect.IndexOf(">");
                    ss = toDetect.Substring(j + 1, toDetect.Length - 1);
                    s = double.Parse(ss);
                    for (int i = 0; i < propertyValueDoubles.Count; i++)
                    {
                        if (propertyValueDoubles[i] > s)
                        {
                            propertyValueDoublesSelected.Add(propertyValueDoubles[i]);
                            searchIDsSelected.Add(selectedItemIds[i]);
                        }
                    }
                }

                if (toDetect.Contains("="))
                {
                    int j = toDetect.IndexOf("=");
                    ss = toDetect.Substring(j + 1, toDetect.Length - 1);
                    s = double.Parse(ss);
                    for (int i = 0; i < propertyValueDoubles.Count; i++)
                    {
                        if (propertyValueDoubles[i] == s)
                        {
                            propertyValueDoublesSelected.Add(propertyValueDoubles[i]);
                            searchIDsSelected.Add(selectedItemIds[i]);
                        }
                    }
                }

                if (toDetect == "True" || toDetect == "False")
                {
                    for (int i = 0; i < propertyValueBools.Count; i++)
                    {
                        string trueorfalse = propertyValueBools[i].ToString();
                        if (propertyValueBools[i].ToString() == toDetect)
                        {
                            propertyValueBoolsSelected.Add(propertyValueBools[i]);
                            searchIDsSelected.Add(selectedItemIds[i]);
                        }
                    }
                }

                ModelInfoIFC4 outputInfo = new ModelInfoIFC4(((ModelInfoIFC4)(InputPorts[0].Data)).ModelId);
                foreach (var item in searchIDsSelected)
                {
                    outputInfo.AddElementIds(item);
                }
                OutputPorts[0].Data = outputInfo;
            }
        }


        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        public class ComboboxItem
        {
            public string Text { get; set; }
            public object Value { get; set; }

            public override string ToString()
            { return Text; }


        }

        public override void Calculate()
        {
            OutputPorts[0].Data = null;
            IfcVersionType = InputPorts[0].Data.GetType();
            if (IfcVersionType.Name == "ModelInfoIFC2x3")
            {
                var modelid = ((ModelInfoIFC2x3)(InputPorts[0].Data)).ModelId;

                if (modelid == null) return;
                ModelInfoIFC2x3 outputInfo = new ModelInfoIFC2x3(modelid);
                xModel = DataController.Instance.GetModel(modelid);

                var ifcPropertyFilterControl = ControlElements[0] as IfcPropertyFilterControl;
                if (ifcPropertyFilterControl == null) return;
                var comboBoxPropertySet = ifcPropertyFilterControl.comboBoxPropertySet as ComboBox;
                if (comboBoxPropertySet == null) return;
                comboBoxPropertySet.Items.Clear();

                var selectedItemIds = ((ModelInfoIFC2x3)(InputPorts[0].Data)).ElementIds;
                if (selectedItemIds == null) return;

                // MemberInfo info = typeof(Xbim.Ifc2x3.Kernel.IfcProduct).GetMethod("GlobalId");
                // IfcPersistedEntityAttribute attr = (IfcPersistedEntityAttribute)Attribute.GetCustomAttribute(info, typeof(IfcPersistedEntityAttribute));
                /* var mgr = new Definitions<PropertySetDef>(Xbim.Properties.Version.IFC2x3);
                mgr.LoadAllDefault();
               
                List<string> ListofAllProducts = new List<string>();
                foreach (var item in xModel.Instances.OfType<Xbim.Ifc2x3.Kernel.IfcProduct>())
                {
                    Type t = item.GetType();
                    var productName = t.Name;
                    if (!ListofAllProducts.Contains(productName))
                    {
                        ListofAllProducts.Add(productName);
                    }
                }
                foreach (var element in ListofAllProducts)
                {
                    var result = mgr.DefinitionSets.Where(p => p.ApplicableClasses.Any(c => c.ClassName == element));
                    foreach (var pSet in result)
                    {
                        ComboboxItem onePropertySet = new ComboboxItem() { Text = pSet.Name, Value = pSet };
                        if (!comboBoxPropertySet.Items.Contains(onePropertySet))
                        {
                            comboBoxPropertySet.Items.Add(onePropertySet);
                        }
                    }
                }*/


                var selectedProduct = xModel.Instances.OfType<Xbim.Ifc2x3.ProductExtension.IfcElement>().ToList().Find(x => x.GlobalId == selectedItemIds[0]);
                List<Xbim.Ifc2x3.Kernel.IfcPropertySet> PropertySet = new List<Xbim.Ifc2x3.Kernel.IfcPropertySet> { };

                List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> searchIDs = new List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> { };



                PropertySet = selectedProduct.PropertySets.ToList();
                for (int i = 0; i < PropertySet.Count(); i++)
                {
                    ComboboxItem onePropertySet = new ComboboxItem() { Text = PropertySet[i].Name.ToString(), Value = PropertySet[i] };
                    comboBoxPropertySet.Items.Add(onePropertySet);
                }

                comboBoxPropertySet.SelectedIndex = 0;
            }
            else if (IfcVersionType.Name == "ModelInfoIFC4")
            {
                var modelid = ((ModelInfoIFC4)(InputPorts[0].Data)).ModelId;

                if (modelid == null) return;
                ModelInfoIFC4 outputInfo = new ModelInfoIFC4(modelid);
                xModel = DataController.Instance.GetModel(modelid);


                var ifcPropertyFilterControl = ControlElements[0] as IfcPropertyFilterControl;
                if (ifcPropertyFilterControl == null) return;
                var comboBoxPropertySet = ifcPropertyFilterControl.comboBoxPropertySet as ComboBox;
                if (comboBoxPropertySet == null) return;
                comboBoxPropertySet.Items.Clear();

                var selectedItemIds = ((ModelInfoIFC4)(InputPorts[0].Data)).ElementIds;
                if (selectedItemIds == null) return;

                // MemberInfo info = typeof(Xbim.Ifc4.Kernel.IfcProduct).GetMethod("GlobalId");
                // IfcPersistedEntityAttribute attr = (IfcPersistedEntityAttribute)Attribute.GetCustomAttribute(info, typeof(IfcPersistedEntityAttribute));


                var selectedProduct = xModel.Instances.OfType<Xbim.Ifc4.ProductExtension.IfcElement>().ToList().Find(x => x.GlobalId == selectedItemIds[0]);



                List<Xbim.Ifc4.Interfaces.IIfcPropertySet> PropertySet = new List<Xbim.Ifc4.Interfaces.IIfcPropertySet> { };

                List<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId> searchIDs = new List<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId> { };



                PropertySet = selectedProduct.PropertySets.ToList();
                for (int i = 0; i < PropertySet.Count(); i++)
                {
                    ComboboxItem onePropertySet = new ComboboxItem() { Text = PropertySet[i].Name.ToString(), Value = PropertySet[i] };
                    comboBoxPropertySet.Items.Add(onePropertySet);
                }

                comboBoxPropertySet.SelectedIndex = 0;
            }


        }

        private void comboBoxPropertySet_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var ifcPropertyFilterControl = ControlElements[0] as IfcPropertyFilterControl;
            if (ifcPropertyFilterControl == null) return;
            var comboBoxPropertySet = ifcPropertyFilterControl.comboBoxPropertySet as ComboBox;
            if (comboBoxPropertySet == null) return;
            var comboBoxProperties = ifcPropertyFilterControl.comboBoxProperties as ComboBox;
            if (comboBoxProperties == null) return;
            comboBoxProperties.Items.Clear();
            if (comboBoxPropertySet.Items.Count == 0) return;

            if (IfcVersionType.Name == "ModelInfoIFC2x3")
            {
                var selectedItem = (ComboboxItem)(comboBoxPropertySet.SelectedItem);
                var propertySet = selectedItem.Value as Xbim.Ifc2x3.Kernel.IfcPropertySet;
                if (propertySet == null) return;
                var hasProperties = propertySet.HasProperties.ToList();


                for (int i = 0; i < hasProperties.Count; i++)
                {
                    ComboboxItem oneHasProperties = new ComboboxItem() { Text = hasProperties[i].Name.ToString(), Value = hasProperties[i] };
                    comboBoxProperties.Items.Add(oneHasProperties);
                }

                comboBoxProperties.SelectedIndex = 0;
            }
            else if (IfcVersionType.Name == "ModelInfoIFC4")
            {
                var selectedItem = (ComboboxItem)(comboBoxPropertySet.SelectedItem);
                var propertySet = selectedItem.Value as Xbim.Ifc4.Kernel.IfcPropertySet;
                if (propertySet == null) return;
                var hasProperties = propertySet.HasProperties.ToList();


                for (int i = 0; i < hasProperties.Count; i++)
                {
                    ComboboxItem oneHasProperties = new ComboboxItem() { Text = hasProperties[i].Name.ToString(), Value = hasProperties[i] };
                    comboBoxProperties.Items.Add(oneHasProperties);
                }

                comboBoxProperties.SelectedIndex = 0;
            }



        }

        private void comboBoxProperties_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var ifcPropertyFilterControl = ControlElements[0] as IfcPropertyFilterControl;
            if (ifcPropertyFilterControl == null) return;

            var textBox = ifcPropertyFilterControl.textBox as TextBox;
            if (textBox == null) return;
            var button = ifcPropertyFilterControl.button as Button;
            if (button == null) return;
            var comboBoxPropertySet = ifcPropertyFilterControl.comboBoxPropertySet as ComboBox;
            if (comboBoxPropertySet == null) return;
            var comboBoxProperties = ifcPropertyFilterControl.comboBoxProperties as ComboBox;
            if (comboBoxProperties == null) return;
            if (comboBoxProperties.Items.Count == 0) return;


            if (IfcVersionType.Name == "ModelInfoIFC2x3")
            {
                var selectedItem = (ComboboxItem)(comboBoxProperties.SelectedItem);
                var property = selectedItem.Value as Xbim.Ifc2x3.PropertyResource.IfcProperty;
                if (property == null) return;

                var selectedPropertySet = ((ComboboxItem)(comboBoxPropertySet.SelectedItem)).Value as Xbim.Ifc2x3.Kernel.IfcPropertySet;

                var selectedItemIds = ((ModelInfoIFC2x3)(InputPorts[0].Data)).ElementIds;
                if (selectedItemIds == null) return;

                List<double> propertyValueDoubles = new List<double> { };
                List<bool> propertyValueBools = new List<bool> { };
                List<string> propertyValueStrings = new List<string> { };


                for (int j = 0; j < selectedItemIds.Count; j++)
                {
                    var selectedProduct =
                        xModel.Instances.OfType<Xbim.Ifc2x3.ProductExtension.IfcElement>().ToList().Find(x => x.GlobalId == selectedItemIds[j]);
                    var propertySet = selectedProduct.PropertySets.ToList()
                        .Find(x => x.Name == selectedPropertySet.Name);
                    var oneProperty = propertySet.HasProperties.ToList().Find(x => x.Name == property.Name);
                    string propertyType = oneProperty.GetType().ToString();
                    if (propertyType == "Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue")
                    {
                        var property2 = property as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                        var propertyValue = property2.NominalValue;
                        object propertyValueTrue = property2.NominalValue.Value;
                        string propertyValueType = propertyValue.UnderlyingSystemType.Name;
                        if (propertyValueType == "Double")
                        {
                            double propertyValueDouble = (double)propertyValueTrue;
                            propertyValueDoubles.Add(propertyValueDouble);
                            textBox.Text = "Double: give range";
                        }
                        if (propertyValueType == "Boolean")
                        {
                            bool propertyValueBool = (bool)propertyValueTrue;
                            propertyValueBools.Add(propertyValueBool);
                            textBox.Text = "Boolean: True or False";
                        }
                        if (propertyValueType == "String")
                        {
                            string propertyValueString = (string)propertyValueTrue;
                            propertyValueStrings.Add(propertyValueString);

                            //print only different value of 'string' to the textbox
                            List<bool> diffString = new List<bool> { };

                            for (int i = 0; i < propertyValueStrings.Count; i++)
                            {
                                diffString.Add(true);
                            }

                            for (int i = 0; i < propertyValueStrings.Count; i++)
                            {
                                for (int k = i + 1; k < propertyValueStrings.Count; k++)
                                {
                                    if (diffString[i] == true)
                                    {
                                        if (propertyValueStrings[i] == propertyValueStrings[k])
                                        {
                                            diffString[i] = false;
                                        }

                                    }
                                }
                            }


                            textBox.Text = "String: has ";
                            for (int i = 0; i < diffString.Count; i++)
                            {
                                if (diffString[i] == true)
                                {
                                    textBox.Text += propertyValueStrings[i];

                                }
                            }

                        }

                    }
                    else
                    {
                        return;
                    }
                }
            }
            else if (IfcVersionType.Name == "ModelInfoIFC4")
            {
                var selectedItem = (ComboboxItem)(comboBoxProperties.SelectedItem);
                var property = selectedItem.Value as Xbim.Ifc4.PropertyResource.IfcProperty;
                if (property == null) return;

                var selectedPropertySet = ((ComboboxItem)(comboBoxPropertySet.SelectedItem)).Value as Xbim.Ifc4.Kernel.IfcPropertySet;

                var selectedItemIds = ((ModelInfoIFC4)(InputPorts[0].Data)).ElementIds;
                if (selectedItemIds == null) return;

                List<double> propertyValueDoubles = new List<double> { };
                List<bool> propertyValueBools = new List<bool> { };
                List<string> propertyValueStrings = new List<string> { };


                for (int j = 0; j < selectedItemIds.Count; j++)
                {
                    var selectedProduct =
                        xModel.Instances.OfType<Xbim.Ifc4.ProductExtension.IfcElement>().ToList().Find(x => x.GlobalId == selectedItemIds[j]);
                    var propertySet = selectedProduct.PropertySets.ToList()
                        .Find(x => x.Name == selectedPropertySet.Name);
                    var oneProperty = propertySet.HasProperties.ToList().Find(x => x.Name == property.Name);
                    string propertyType = oneProperty.GetType().ToString();
                    if (propertyType == "Xbim.Ifc4.PropertyResource.IfcPropertySingleValue")
                    {
                        var property2 = property as Xbim.Ifc4.PropertyResource.IfcPropertySingleValue;
                        var propertyValue = property2.NominalValue;
                        object propertyValueTrue = property2.NominalValue.Value;
                        string propertyValueType = propertyValue.UnderlyingSystemType.Name;
                        if (propertyValueType == "Double")
                        {
                            double propertyValueDouble = (double)propertyValueTrue;
                            propertyValueDoubles.Add(propertyValueDouble);
                            textBox.Text = "Double: give range";
                        }
                        if (propertyValueType == "Boolean")
                        {
                            bool propertyValueBool = (bool)propertyValueTrue;
                            propertyValueBools.Add(propertyValueBool);
                            textBox.Text = "Boolean: True or False";
                        }
                        if (propertyValueType == "String")
                        {
                            string propertyValueString = (string)propertyValueTrue;
                            propertyValueStrings.Add(propertyValueString);

                            //print only different value of 'string' to the textbox
                            List<bool> diffString = new List<bool> { };

                            for (int i = 0; i < propertyValueStrings.Count; i++)
                            {
                                diffString.Add(true);
                            }

                            for (int i = 0; i < propertyValueStrings.Count; i++)
                            {
                                for (int k = i + 1; k < propertyValueStrings.Count; k++)
                                {
                                    if (diffString[i] == true)
                                    {
                                        if (propertyValueStrings[i] == propertyValueStrings[k])
                                        {
                                            diffString[i] = false;
                                        }

                                    }
                                }
                            }


                            textBox.Text = "String: has ";
                            for (int i = 0; i < diffString.Count; i++)
                            {
                                if (diffString[i] == true)
                                {
                                    textBox.Text += propertyValueStrings[i];

                                }
                            }

                        }

                    }
                    else
                    {
                        return;
                    }
                }
            }
        }



        public override void SerializeNetwork(XmlWriter xmlWriter)
        {
            base.SerializeNetwork(xmlWriter);

            // add your xml serialization methods here
        }

        public override void DeserializeNetwork(XmlReader xmlReader)
        {
            base.DeserializeNetwork(xmlReader);

            // add your xml deserialization methods here
        }

        public override Node Clone()
        {
            return new IfcPropertyFilterNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}