using System.Windows.Controls;
using System.Xml;
using TUM.CMS.VplControl.Core;
using System.Collections.Generic;
using Xbim.IO;
using Xbim.XbimExtensions;

using System.Linq;
using System.Collections;
using System.Text.RegularExpressions;
using System;
using System.Windows;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcFilterNode : Node
    {
        public IfcFilterNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {

            AddInputPortToNode("IfcFile", typeof(string));

            AddOutputPortToNode("FilteredProducts", typeof(object));

          
            var label = new Label { Content ="filtered products"};
            var labelProducts = new Label { Content = "product filter:"};
            //var label3 = new Label { Content = "Wall height filter:" };
            //var label4 = new Label { Content = "Wall type filter:" };
            //var label5 = new Label { Content = "Column load bearing filter:" };
            var labelPropertySet = new Label {Content="Property Set filter:" };
            var labelProperties = new Label { Content = "property filter:" };
            var labelPropertyValue = new Label { Content = "Property Value" };
            var comboBox = new ComboBox
            {
                
            };
            comboBox.SelectionChanged += comboBox_SelectionChanged;

            var comboBoxPropertySet = new ComboBox { };
            comboBoxPropertySet.SelectionChanged += comboBoxPropertySet_SelectionChanged;

            var comboBoxProperties = new ComboBox { };
            comboBoxProperties.SelectionChanged += comboBoxProperties_SelectionChanged;

            

            var textBox = new TextBox { };
            textBox.TextChanged += textBox_TextChanged;

            var button = new Button { Content ="filter according to textBox"};
            button.Click += button_Click;
            //var comboBox2 = new ComboBox { };
            /*comboBox2.SelectionChanged += comboBox2_SelectionChanged;
            var comboBox3 = new ComboBox { };
            comboBox3.SelectionChanged += comboBox3_SelectionChanged;
            var comboBox4 = new ComboBox { };
            comboBox4.SelectionChanged += comboBox4_SelectionChanged;*/


            AddControlToNode(label);
            AddControlToNode(labelProducts);
            AddControlToNode(comboBox);
            AddControlToNode(labelPropertySet);
            AddControlToNode(comboBoxPropertySet);
            AddControlToNode(labelProperties);
            AddControlToNode(comboBoxProperties);
            AddControlToNode(labelPropertyValue);            
            AddControlToNode(textBox);
            AddControlToNode(button);


            /*AddControlToNode(label3);
            AddControlToNode(comboBox2);
            AddControlToNode(label4);
            AddControlToNode(comboBox3);
            AddControlToNode(label5);
            AddControlToNode(comboBox4);*/


        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            var textBox = ControlElements[8] as TextBox;
            if (textBox == null) return;
            var button = ControlElements[9] as Button;
            if (button == null) return;
            var comboBox = ControlElements[2] as ComboBox;
            if (comboBox == null) return;
            var comboBoxPropertySet = ControlElements[4] as ComboBox;
            if (comboBoxPropertySet == null) return;
            var comboBoxProperties = ControlElements[6] as ComboBox;
            if (comboBoxProperties == null) return;            
            if (comboBoxProperties.Items.Count == 0) return;

            var selectedItem = (ComboboxItem)(comboBoxProperties.SelectedItem);
            var property = selectedItem.Value as Xbim.Ifc2x3.PropertyResource.IfcProperty;
            if (property == null) return;

            var selectedPropertySet = ((ComboboxItem)(comboBoxPropertySet.SelectedItem)).Value as Xbim.Ifc2x3.Kernel.IfcPropertySet;
                        
            var selectedIfcProducts = ((ComboboxItem)(comboBox.SelectedItem)).Value;
            List<string> sharedBldgElements = new List<string> { "CurtainWall", "Plate", "Wall", "Beam", "Column", "Railing", "Ramp", "Roof", "Slab", "Stair", "Window", "Door", "Member" };
            Match m = Regex.Match(selectedIfcProducts.GetType().ToString (), sharedBldgElements[0]);
            int k = 1;
            while (!m.Success && k < sharedBldgElements.Count)
            {
                m = Regex.Match(selectedIfcProducts.GetType().ToString(), sharedBldgElements[k]);
                k++;
            }
            string shBlEl = sharedBldgElements[k - 1];

            List<double> propertyValueDoubles = new List<double> { };
            List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> searchIDs = new List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> { };


            if (shBlEl=="Wall")
            {
                var selectedProducts = selectedIfcProducts as List<Xbim.Ifc2x3.SharedBldgElements.IfcWall>;
                for (int i = 0; i < selectedProducts.Count; i++)
                {
                    searchIDs.Add(selectedProducts[i].GlobalId);
                }

                for (int i = 0; i < selectedProducts.Count; i++)
                {
                    var propertySet = selectedProducts[i].PropertySets.ToList().Find(x => x.Name == selectedPropertySet.Name);
                    var oneProperty = propertySet.HasProperties.ToList().Find(x => x.Name == property.Name);
                    string propertyType = oneProperty.GetType().ToString();
                    if (propertyType == "Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue")
                    {
                        var property2 = property as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                        var propertyValue = property2.NominalValue as Xbim.XbimExtensions.SelectTypes.IfcValue;
                        object propertyValueTrue = property2.NominalValue.Value;
                        string propertyValueType = propertyValue.UnderlyingSystemType.Name;
                        if (propertyValueType == "Double")
                        {
                            double propertyValueDouble = (double)propertyValueTrue;
                            propertyValueDoubles.Add(propertyValueDouble);
                        }

                    }
                    else { return; }
                }

            }
            if (shBlEl == "Slab")
            {
                var selectedProducts = selectedIfcProducts as List<Xbim.Ifc2x3.SharedBldgElements.IfcSlab>;
                for (int i = 0; i < selectedProducts.Count; i++)
                {
                    searchIDs.Add(selectedProducts[i].GlobalId);
                }
                for (int i = 0; i < selectedProducts.Count; i++)
                {
                    var propertySet = selectedProducts[i].PropertySets.ToList().Find(x => x.Name == selectedPropertySet.Name);
                    var oneProperty = propertySet.HasProperties.ToList().Find(x => x.Name == property.Name);
                    string propertyType = oneProperty.GetType().ToString();
                    if (propertyType == "Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue")
                    {
                        var property2 = property as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                        var propertyValue = property2.NominalValue as Xbim.XbimExtensions.SelectTypes.IfcValue;
                        object propertyValueTrue = property2.NominalValue.Value;
                        string propertyValueType = propertyValue.UnderlyingSystemType.Name;
                        if (propertyValueType == "Double")
                        {
                            double propertyValueDouble = (double)propertyValueTrue;
                            propertyValueDoubles.Add(propertyValueDouble);
                        }

                    }
                    else { return; }
                }
            }
            if (shBlEl == "Column")
            {
                var selectedProducts = selectedIfcProducts as List<Xbim.Ifc2x3.SharedBldgElements.IfcColumn>;
                for (int i = 0; i < selectedProducts.Count; i++)
                {
                    searchIDs.Add(selectedProducts[i].GlobalId);
                }
                for (int i = 0; i < selectedProducts.Count; i++)
                {
                    var propertySet = selectedProducts[i].PropertySets.ToList().Find(x => x.Name == selectedPropertySet.Name);
                    var oneProperty = propertySet.HasProperties.ToList().Find(x => x.Name == property.Name);
                    string propertyType = oneProperty.GetType().ToString();
                    if (propertyType == "Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue")
                    {
                        var property2 = property as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                        var propertyValue = property2.NominalValue as Xbim.XbimExtensions.SelectTypes.IfcValue;
                        object propertyValueTrue = property2.NominalValue.Value;
                        string propertyValueType = propertyValue.UnderlyingSystemType.Name;
                        if (propertyValueType == "Double")
                        {
                            double propertyValueDouble = (double)propertyValueTrue;
                            propertyValueDoubles.Add(propertyValueDouble);
                        }

                    }
                    else { return; }
                }
            }

            if (shBlEl == "CurtainWall")
            {
                var selectedProducts = selectedIfcProducts as List<Xbim.Ifc2x3.SharedBldgElements.IfcCurtainWall>;
                for (int i = 0; i < selectedProducts.Count; i++)
                {
                    searchIDs.Add(selectedProducts[i].GlobalId);
                }

                for (int i = 0; i < selectedProducts.Count; i++)
                {
                    var propertySet = selectedProducts[i].PropertySets.ToList().Find(x => x.Name == selectedPropertySet.Name);
                    var oneProperty = propertySet.HasProperties.ToList().Find(x => x.Name == property.Name);
                    string propertyType = oneProperty.GetType().ToString();
                    if (propertyType == "Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue")
                    {
                        var property2 = property as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                        var propertyValue = property2.NominalValue as Xbim.XbimExtensions.SelectTypes.IfcValue;
                        object propertyValueTrue = property2.NominalValue.Value;
                        string propertyValueType = propertyValue.UnderlyingSystemType.Name;
                        if (propertyValueType == "Double")
                        {
                            double propertyValueDouble = (double)propertyValueTrue;
                            propertyValueDoubles.Add(propertyValueDouble);
                        }

                    }
                    else { return; }
                }
            }

            if (shBlEl == "Plate")
            {
                var selectedProducts = selectedIfcProducts as List<Xbim.Ifc2x3.SharedBldgElements.IfcPlate>;
                for (int i = 0; i < selectedProducts.Count; i++)
                {
                    searchIDs.Add(selectedProducts[i].GlobalId);
                }

                for (int i = 0; i < selectedProducts.Count; i++)
                {
                    var propertySet = selectedProducts[i].PropertySets.ToList().Find(x => x.Name == selectedPropertySet.Name);
                    var oneProperty = propertySet.HasProperties.ToList().Find(x => x.Name == property.Name);
                    //TODO:throw null exception 
                    string propertyType = oneProperty.GetType().ToString();
                    if (propertyType == "Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue")
                    {
                        var property2 = property as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                        var propertyValue = property2.NominalValue as Xbim.XbimExtensions.SelectTypes.IfcValue;
                        object propertyValueTrue = property2.NominalValue.Value;
                        string propertyValueType = propertyValue.UnderlyingSystemType.Name;
                        if (propertyValueType == "Double")
                        {
                            double propertyValueDouble = (double)propertyValueTrue;
                            propertyValueDoubles.Add(propertyValueDouble);
                        }

                    }
                    else { return; }
                }
            }

            if (shBlEl == "Window")
            {
                var selectedProducts = selectedIfcProducts as List<Xbim.Ifc2x3.SharedBldgElements.IfcWindow>;
                for (int i = 0; i < selectedProducts.Count; i++)
                {
                    searchIDs.Add(selectedProducts[i].GlobalId);
                }

                for (int i = 0; i < selectedProducts.Count; i++)
                {
                    var propertySet = selectedProducts[i].PropertySets.ToList().Find(x => x.Name == selectedPropertySet.Name);
                    var oneProperty = propertySet.HasProperties.ToList().Find(x => x.Name == property.Name);
                    string propertyType = oneProperty.GetType().ToString();
                    if (propertyType == "Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue")
                    {
                        var property2 = property as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                        var propertyValue = property2.NominalValue as Xbim.XbimExtensions.SelectTypes.IfcValue;
                        object propertyValueTrue = property2.NominalValue.Value;
                        string propertyValueType = propertyValue.UnderlyingSystemType.Name;
                        if (propertyValueType == "Double")
                        {
                            double propertyValueDouble = (double)propertyValueTrue;
                            propertyValueDoubles.Add(propertyValueDouble);
                        }

                    }
                    else { return; }
                }
            }

            if (shBlEl == "Door")
            {
                var selectedProducts = selectedIfcProducts as List<Xbim.Ifc2x3.SharedBldgElements.IfcDoor>;
                for (int i = 0; i < selectedProducts.Count; i++)
                {
                    searchIDs.Add(selectedProducts[i].GlobalId);
                }

                for (int i = 0; i < selectedProducts.Count; i++)
                {
                    var propertySet = selectedProducts[i].PropertySets.ToList().Find(x => x.Name == selectedPropertySet.Name);
                    var oneProperty = propertySet.HasProperties.ToList().Find(x => x.Name == property.Name);
                    string propertyType = oneProperty.GetType().ToString();
                    if (propertyType == "Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue")
                    {
                        var property2 = property as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                        var propertyValue = property2.NominalValue as Xbim.XbimExtensions.SelectTypes.IfcValue;
                        object propertyValueTrue = property2.NominalValue.Value;
                        string propertyValueType = propertyValue.UnderlyingSystemType.Name;
                        if (propertyValueType == "Double")
                        {
                            double propertyValueDouble = (double)propertyValueTrue;
                            propertyValueDoubles.Add(propertyValueDouble);
                        }

                    }
                    else { return; }
                }
            }

            if (shBlEl == "Roof")
            {
                var selectedProducts = selectedIfcProducts as List<Xbim.Ifc2x3.SharedBldgElements.IfcRoof>;
                for (int i = 0; i < selectedProducts.Count; i++)
                {
                    searchIDs.Add(selectedProducts[i].GlobalId);
                }

                for (int i = 0; i < selectedProducts.Count; i++)
                {
                    var propertySet = selectedProducts[i].PropertySets.ToList().Find(x => x.Name == selectedPropertySet.Name);
                    var oneProperty = propertySet.HasProperties.ToList().Find(x => x.Name == property.Name);
                    string propertyType = oneProperty.GetType().ToString();
                    if (propertyType == "Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue")
                    {
                        var property2 = property as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                        var propertyValue = property2.NominalValue as Xbim.XbimExtensions.SelectTypes.IfcValue;
                        object propertyValueTrue = property2.NominalValue.Value;
                        string propertyValueType = propertyValue.UnderlyingSystemType.Name;
                        if (propertyValueType == "Double")
                        {
                            double propertyValueDouble = (double)propertyValueTrue;
                            propertyValueDoubles.Add(propertyValueDouble);
                        }

                    }
                    else { return; }
                }
            }

            if (shBlEl == "Ramp")
            {
                var selectedProducts = selectedIfcProducts as List<Xbim.Ifc2x3.SharedBldgElements.IfcRamp>;
                for (int i = 0; i < selectedProducts.Count; i++)
                {
                    searchIDs.Add(selectedProducts[i].GlobalId);
                }

                for (int i = 0; i < selectedProducts.Count; i++)
                {
                    var propertySet = selectedProducts[i].PropertySets.ToList().Find(x => x.Name == selectedPropertySet.Name);
                    var oneProperty = propertySet.HasProperties.ToList().Find(x => x.Name == property.Name);
                    string propertyType = oneProperty.GetType().ToString();
                    if (propertyType == "Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue")
                    {
                        var property2 = property as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                        var propertyValue = property2.NominalValue as Xbim.XbimExtensions.SelectTypes.IfcValue;
                        object propertyValueTrue = property2.NominalValue.Value;
                        string propertyValueType = propertyValue.UnderlyingSystemType.Name;
                        if (propertyValueType == "Double")
                        {
                            double propertyValueDouble = (double)propertyValueTrue;
                            propertyValueDoubles.Add(propertyValueDouble);
                        }

                    }
                    else { return; }
                }
            }

            List<double> propertyValueDoublesSelected = new List<double> { };
            List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> searchIDsSelected = new List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> { };

            string toDetect = textBox.Text;
            string ss;
            double s;
            if (toDetect.Contains("<"))
            {
                int j = toDetect.IndexOf("<");
                ss = toDetect.Substring(j+1, toDetect.Length - 1);
                s = double.Parse(ss);
                for(int i = 0; i < propertyValueDoubles.Count; i++)
                {
                    if(propertyValueDoubles[i]<s)
                    {
                        propertyValueDoublesSelected.Add(propertyValueDoubles[i]);
                        searchIDsSelected.Add(searchIDs[i]);
                        
                    }
                }
            }

            if (toDetect.Contains(">"))
            {
                int j = toDetect.IndexOf(">");
                ss = toDetect.Substring(j+1, toDetect.Length - 1);
                s = double.Parse(ss);
                for (int i = 0; i < propertyValueDoubles.Count; i++)
                {
                    if (propertyValueDoubles[i] > s)
                    {
                        propertyValueDoublesSelected.Add(propertyValueDoubles[i]);
                        searchIDsSelected.Add(searchIDs[i]);
                    }
                }
            }

            if (toDetect.Contains("="))
            {
                int j = toDetect.IndexOf("=");
                ss = toDetect.Substring(j+1, toDetect.Length - 1);
                s = double.Parse(ss);
                for (int i = 0; i < propertyValueDoubles.Count; i++)
                {
                    if (propertyValueDoubles[i] == s)
                    {
                        propertyValueDoublesSelected.Add(propertyValueDoubles[i]);
                        searchIDsSelected.Add(searchIDs[i]);
                    }
                }
            }

            OutputPorts[0].Data = searchIDsSelected;



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

           /* public string ToString2()
            {
                string valueInString=null;
                
                for(int i=0;i<Value.Count;i++)
                    { valueInString += Value[i].ToString(); };
                return valueInString;
            }*/
        }

        public override void Calculate()
        {

            XbimModel xModel = new XbimModel();

            var res = xModel.Open(InputPorts[0].Data.ToString(), XbimDBAccess.ReadWrite);

            
            var comboBox = ControlElements[2] as ComboBox;
            if (comboBox == null) return;
            /*var comboBox2 = ControlElements[4] as ComboBox;
            if (comboBox2 == null) return;
            var comboBox3 = ControlElements[6] as ComboBox;
            if (comboBox3 == null) return;
            var comboBox4 = ControlElements[8] as ComboBox;
            if (comboBox4 == null) return;*/

            var ifcwall = xModel.IfcProducts.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcWall>().ToList();

            
            var searchID=ifcwall[0].GlobalId;
            List<Xbim.Ifc2x3.SharedBldgElements.IfcWall> filteredList = ifcwall.Where(x => x.GlobalId == searchID).ToList();

            //wall height filter:
            /*List<Xbim.Ifc2x3.PropertyResource.IfcProperty> wallHeight=new List<Xbim.Ifc2x3.PropertyResource.IfcProperty> { };
            for (int i = 0; i < ifcwall.Count; i++)
            {
                var ifcwallpropertyset = ifcwall[i].PropertySets.ToList();
                var properties = ifcwallpropertyset.Find(x => x.Name == "Abhängigkeiten").HasProperties.ToList();
                wallHeight.Add(properties.Find(x => x.Name == "Nicht verknüpfte Höhe"));
                
            }
            


            List<bool> diffHeight = new List<bool> { };
            
            for (int i = 0; i < wallHeight.Count; i++)
            {
                diffHeight.Add(true);    
            }

            for (int i = 0; i < wallHeight.Count; i++)
            {            
                for (int j = i+1; j < wallHeight.Count; j++)
                {
                    if(diffHeight[i]==true)
                    {
                        if (wallHeight[i].ToString()==wallHeight[j].ToString ()) { diffHeight[i] = false; }
                        
                    }
                }
            }

            

            for (int i = 0; i < diffHeight.Count; i++)
            {
                if (diffHeight[i] == true)
                {
                    comboBox2.Items.Add(wallHeight[i]);
                    
                }
            }*/

            //wall type filter:
            /*List<Xbim.Ifc2x3.PropertyResource.IfcProperty> wallType = new List<Xbim.Ifc2x3.PropertyResource.IfcProperty> { };
            for (int i = 0; i < ifcwall.Count; i++)
            {
                var ifcwallpropertyset = ifcwall[i].PropertySets.ToList();
                var properties = ifcwallpropertyset.Find(x => x.Name == "Sonstige").HasProperties.ToList();
                wallType.Add(properties.Find(x => x.Name == "Typ"));

            }



            List<bool> diffType = new List<bool> { };

            for (int i = 0; i < wallType.Count; i++)
            {
                diffType.Add(true);
            }

            for (int i = 0; i < wallType.Count; i++)
            {
                for (int j = i + 1; j < wallType.Count; j++)
                {
                    if (diffType[i] == true)
                    {
                        if (wallType[i].ToString() == wallType[j].ToString()) { diffType[i] = false; }

                    }
                }
            }



            for (int i = 0; i < diffType.Count; i++)
            {
                if (diffType[i] == true)
                {
                    comboBox3.Items.Add(wallType[i]);

                }
            }*/

            
        
            var ifcbeam = xModel.IfcProducts.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcBeam>().ToList();
            var ifccolumn = xModel.IfcProducts.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcColumn>().ToList();

            //column Load bearing/non-bearing filter:
            /*List<Xbim.Ifc2x3.PropertyResource.IfcProperty> ColumnBearing = new List<Xbim.Ifc2x3.PropertyResource.IfcProperty> { };
            for (int i = 0; i < ifccolumn.Count; i++)
            {
                var ifccolumnpropertyset = ifccolumn[i].PropertySets.ToList();
                var properties = ifccolumnpropertyset.Find(x => x.Name == "Tragwerk").HasProperties.ToList();
                ColumnBearing.Add(properties.Find(x => x.Name == "Berechnungsmodell aktivieren"));

            }

            comboBox4.Items.Add("true ");
            comboBox4.Items.Add("false ");*/
            
            var ifcslab = xModel.IfcProducts.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcSlab>().ToList();
            var ifcwindow = xModel.IfcProducts.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcWindow>().ToList();
            var ifcstair = xModel.IfcProducts.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcStair>().ToList();
            var ifcroof = xModel.IfcProducts.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcRoof>().ToList();
            var ifcramp = xModel.IfcProducts.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcRamp>().ToList();
            var ifcplate = xModel.IfcProducts.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcPlate>().ToList();
            var ifcdoor = xModel.IfcProducts.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcDoor>().ToList();
            var ifccurtainwall = xModel.IfcProducts.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcCurtainWall>().ToList();

            ComboboxItem ifcwalls = new ComboboxItem() { Text = "ifcwall", Value = ifcwall};
            ComboboxItem ifcbeams = new ComboboxItem() { Text = "ifcbeam", Value = ifcbeam };
            ComboboxItem ifccolumns = new ComboboxItem() { Text = "ifccolumn", Value = ifccolumn };
            ComboboxItem ifcslabs = new ComboboxItem() { Text = "ifcslab", Value = ifcslab };
            ComboboxItem ifcwindows = new ComboboxItem() { Text = "ifcwindow", Value = ifcwindow };
            ComboboxItem ifcstairs = new ComboboxItem() { Text = "ifcstair", Value = ifcstair };
            ComboboxItem ifcroofs = new ComboboxItem() { Text = "ifcroof", Value = ifcroof };
            ComboboxItem ifcramps = new ComboboxItem() { Text = "ifcramp", Value = ifcramp };
            ComboboxItem ifcplates = new ComboboxItem() { Text = "ifcplate", Value = ifcplate };
            ComboboxItem ifcdoors = new ComboboxItem() { Text = "ifcdoor", Value = ifcdoor };
            ComboboxItem ifccurtainwalls = new ComboboxItem() { Text = "ifccurtainwall", Value = ifccurtainwall };

            if (ifcwall.Count != 0) { comboBox.Items.Add(ifcwalls); }
            if (ifcbeam.Count != 0) { comboBox.Items.Add(ifcbeams); }
            if (ifccolumn.Count != 0) { comboBox.Items.Add(ifccolumns); }
            if (ifcslab.Count != 0) { comboBox.Items.Add(ifcslabs); }
            if (ifcwindow.Count != 0) { comboBox.Items.Add(ifcwindows); }
            if (ifcstair.Count != 0) { comboBox.Items.Add(ifcstairs); }
            if (ifcroof.Count != 0) { comboBox.Items.Add(ifcroofs); }
            if (ifcramp.Count != 0) { comboBox.Items.Add(ifcramps); }
            if (ifcplate.Count != 0) { comboBox.Items.Add(ifcplates); }
            if (ifcdoor.Count != 0) { comboBox.Items.Add(ifcdoors); }
            if (ifccurtainwall.Count != 0) { comboBox.Items.Add(ifccurtainwalls); }

            comboBox.SelectedIndex = 0;
           
            
            
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            var comboBox = ControlElements[2] as ComboBox;
            if (comboBox == null) return;
            var comboBoxPropertySet = ControlElements[4] as ComboBox;
            if (comboBoxPropertySet == null) return;
            
            comboBoxPropertySet.Items.Clear();

            var selectedItem = (ComboboxItem)(comboBox.SelectedItem);
            string itemType = selectedItem.Value.GetType().ToString ();
            
            List<Xbim.Ifc2x3.Kernel.IfcPropertySet> PropertySet = new List<Xbim.Ifc2x3.Kernel.IfcPropertySet> { };

            List<string> sharedBldgElements = new List<string> { "CurtainWall","Plate", "Wall", "Beam","Column","Railing","Ramp","Roof","Slab","Stair","Window","Door","Member"};
            Match m = Regex.Match(itemType, sharedBldgElements[0]);
            int j = 1;
            while(!m.Success && j<sharedBldgElements.Count)
            {
                m = Regex.Match(itemType.ToString(),sharedBldgElements[j]);
                j++;
            }
            string s = sharedBldgElements[j-1];

            if (s=="Wall")
            {
                var listItems = selectedItem.Value as List<Xbim.Ifc2x3.SharedBldgElements.IfcWall>;
                if (listItems == null) return;

                List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> searchIDs = new List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> { };
                
                for (int i=0;i<listItems.Count;i++)
                {
                    searchIDs.Add (listItems[i].GlobalId);
                }
                OutputPorts[0].Data = searchIDs;

                PropertySet = listItems[0].PropertySets.ToList();
                for (int i = 0; i < PropertySet.Count(); i++)
                {
                    ComboboxItem onePropertySet = new ComboboxItem() { Text = PropertySet[i].Name.ToString(), Value = PropertySet[i] };
                    comboBoxPropertySet.Items.Add(onePropertySet);               
                }
                
                
            }
            
            if (s == "Plate")
            {
                var listItems = selectedItem.Value as List<Xbim.Ifc2x3.SharedBldgElements.IfcPlate>;
                if (listItems == null) return;

                List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> searchIDs = new List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> { };

                for (int i = 0; i < listItems.Count; i++)
                {
                    searchIDs.Add(listItems[i].GlobalId);
                }
                OutputPorts[0].Data = searchIDs;

                PropertySet = listItems[0].PropertySets.ToList();
                for (int i = 0; i < PropertySet.Count(); i++)
                {
                    ComboboxItem onePropertySet = new ComboboxItem() { Text = PropertySet[i].Name.ToString(), Value = PropertySet[i] };
                    comboBoxPropertySet.Items.Add(onePropertySet);
                }
            }
            
            if (s == "CurtainWall")
            {
                var listItems = selectedItem.Value as List<Xbim.Ifc2x3.SharedBldgElements.IfcCurtainWall>;
                if (listItems == null) return;

                List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> searchIDs = new List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> { };

                for (int i = 0; i < listItems.Count; i++)
                {
                    searchIDs.Add(listItems[i].GlobalId);
                }
                OutputPorts[0].Data = searchIDs;

                PropertySet = listItems[0].PropertySets.ToList();
                for (int i = 0; i < PropertySet.Count(); i++)
                {
                    ComboboxItem onePropertySet = new ComboboxItem() { Text = PropertySet[i].Name.ToString(), Value = PropertySet[i] };
                    comboBoxPropertySet.Items.Add(onePropertySet);
                }
            }

            if (s == "Beam")
            {
                var listItems = selectedItem.Value as List<Xbim.Ifc2x3.SharedBldgElements.IfcBeam>;
                if (listItems == null) return;

                List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> searchIDs = new List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> { };

                for (int i = 0; i < listItems.Count; i++)
                {
                    searchIDs.Add(listItems[i].GlobalId);
                }
                OutputPorts[0].Data = searchIDs;

                PropertySet = listItems[0].PropertySets.ToList();
                for (int i = 0; i < PropertySet.Count(); i++)
                {
                    ComboboxItem onePropertySet = new ComboboxItem() { Text = PropertySet[i].Name.ToString(), Value = PropertySet[i] };
                    comboBoxPropertySet.Items.Add(onePropertySet);
                }
            }

            if (s == "Column")
            {
                var listItems = selectedItem.Value as List<Xbim.Ifc2x3.SharedBldgElements.IfcColumn>;
                if (listItems == null) return;

                List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> searchIDs = new List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> { };

                for (int i = 0; i < listItems.Count; i++)
                {
                    searchIDs.Add(listItems[i].GlobalId);
                }
                OutputPorts[0].Data = searchIDs;

                PropertySet = listItems[0].PropertySets.ToList();
                for (int i = 0; i < PropertySet.Count(); i++)
                {
                    ComboboxItem onePropertySet = new ComboboxItem() { Text = PropertySet[i].Name.ToString(), Value = PropertySet[i] };
                    comboBoxPropertySet.Items.Add(onePropertySet);
                }
            }

            if (s == "Railing")
            {
                var listItems = selectedItem.Value as List<Xbim.Ifc2x3.SharedBldgElements.IfcRailing>;
                if (listItems == null) return;

                List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> searchIDs = new List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> { };

                for (int i = 0; i < listItems.Count; i++)
                {
                    searchIDs.Add(listItems[i].GlobalId);
                }
                OutputPorts[0].Data = searchIDs;

                PropertySet = listItems[0].PropertySets.ToList();
                for (int i = 0; i < PropertySet.Count(); i++)
                {
                    ComboboxItem onePropertySet = new ComboboxItem() { Text = PropertySet[i].Name.ToString(), Value = PropertySet[i] };
                    comboBoxPropertySet.Items.Add(onePropertySet);
                }
            }

            if (s == "Ramp")
            {
                var listItems = selectedItem.Value as List<Xbim.Ifc2x3.SharedBldgElements.IfcRamp>;
                if (listItems == null) return;

                List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> searchIDs = new List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> { };

                for (int i = 0; i < listItems.Count; i++)
                {
                    searchIDs.Add(listItems[i].GlobalId);
                }
                OutputPorts[0].Data = searchIDs;

                PropertySet = listItems[0].PropertySets.ToList();
                for (int i = 0; i < PropertySet.Count(); i++)
                {
                    ComboboxItem onePropertySet = new ComboboxItem() { Text = PropertySet[i].Name.ToString(), Value = PropertySet[i] };
                    comboBoxPropertySet.Items.Add(onePropertySet);
                }
            }

            if (s == "Roof")
            {
                var listItems = selectedItem.Value as List<Xbim.Ifc2x3.SharedBldgElements.IfcRoof>;
                if (listItems == null) return;

                List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> searchIDs = new List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> { };

                for (int i = 0; i < listItems.Count; i++)
                {
                    searchIDs.Add(listItems[i].GlobalId);
                }
                OutputPorts[0].Data = searchIDs;

                PropertySet = listItems[0].PropertySets.ToList();
                for (int i = 0; i < PropertySet.Count(); i++)
                {
                    ComboboxItem onePropertySet = new ComboboxItem() { Text = PropertySet[i].Name.ToString(), Value = PropertySet[i] };
                    comboBoxPropertySet.Items.Add(onePropertySet);
                }
            }

            if (s == "Slab")
            {
                var listItems = selectedItem.Value as List<Xbim.Ifc2x3.SharedBldgElements.IfcSlab>;
                if (listItems == null) return;

                List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> searchIDs = new List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> { };

                for (int i = 0; i < listItems.Count; i++)
                {
                    searchIDs.Add(listItems[i].GlobalId);
                }
                OutputPorts[0].Data = searchIDs;

                PropertySet = listItems[0].PropertySets.ToList();
                for (int i = 0; i < PropertySet.Count(); i++)
                {
                    ComboboxItem onePropertySet = new ComboboxItem() { Text = PropertySet[i].Name.ToString(), Value = PropertySet[i] };
                    comboBoxPropertySet.Items.Add(onePropertySet);
                }
            }

            if (s == "Stair")
            {
                var listItems = selectedItem.Value as List<Xbim.Ifc2x3.SharedBldgElements.IfcStair>;
                if (listItems == null) return;

                List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> searchIDs = new List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> { };

                for (int i = 0; i < listItems.Count; i++)
                {
                    searchIDs.Add(listItems[i].GlobalId);
                }
                OutputPorts[0].Data = searchIDs;

                PropertySet = listItems[0].PropertySets.ToList();
                for (int i = 0; i < PropertySet.Count(); i++)
                {
                    ComboboxItem onePropertySet = new ComboboxItem() { Text = PropertySet[i].Name.ToString(), Value = PropertySet[i] };
                    comboBoxPropertySet.Items.Add(onePropertySet);
                }
            }

            if (s == "Window")
            {
                var listItems = selectedItem.Value as List<Xbim.Ifc2x3.SharedBldgElements.IfcWindow>;
                if (listItems == null) return;

                List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> searchIDs = new List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> { };

                for (int i = 0; i < listItems.Count; i++)
                {
                    searchIDs.Add(listItems[i].GlobalId);
                }
                OutputPorts[0].Data = searchIDs;

                PropertySet = listItems[0].PropertySets.ToList();
                for (int i = 0; i < PropertySet.Count(); i++)
                {
                    ComboboxItem onePropertySet = new ComboboxItem() { Text = PropertySet[i].Name.ToString(), Value = PropertySet[i] };
                    comboBoxPropertySet.Items.Add(onePropertySet);
                }
            }

            if (s == "Door")
            {
                var listItems = selectedItem.Value as List<Xbim.Ifc2x3.SharedBldgElements.IfcDoor>;
                if (listItems == null) return;

                List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> searchIDs = new List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> { };

                for (int i = 0; i < listItems.Count; i++)
                {
                    searchIDs.Add(listItems[i].GlobalId);
                }
                OutputPorts[0].Data = searchIDs;

                PropertySet = listItems[0].PropertySets.ToList();
                for (int i = 0; i < PropertySet.Count(); i++)
                {
                    ComboboxItem onePropertySet = new ComboboxItem() { Text = PropertySet[i].Name.ToString(), Value = PropertySet[i] };
                    comboBoxPropertySet.Items.Add(onePropertySet);
                }
            }

            if (s == "Member")
            {
                var listItems = selectedItem.Value as List<Xbim.Ifc2x3.SharedBldgElements.IfcMember>;
                if (listItems == null) return;

                List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> searchIDs = new List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> { };

                for (int i = 0; i < listItems.Count; i++)
                {
                    searchIDs.Add(listItems[i].GlobalId);
                }
                OutputPorts[0].Data = searchIDs;

                PropertySet = listItems[0].PropertySets.ToList();
                for (int i = 0; i < PropertySet.Count(); i++)
                {
                    ComboboxItem onePropertySet = new ComboboxItem() { Text = PropertySet[i].Name.ToString(), Value = PropertySet[i] };
                    comboBoxPropertySet.Items.Add(onePropertySet);
                }
            }

            comboBoxPropertySet.SelectedIndex = 0;

        }

        private void comboBoxPropertySet_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBoxPropertySet = ControlElements[4] as ComboBox;
            if (comboBoxPropertySet == null) return;
            var comboBoxProperties = ControlElements[6] as ComboBox;
            if (comboBoxProperties == null) return;

            comboBoxProperties.Items.Clear();

            if (comboBoxPropertySet.Items.Count == 0) return;

            var selectedItem = (ComboboxItem)(comboBoxPropertySet.SelectedItem);
            var propertySet = selectedItem.Value as Xbim.Ifc2x3.Kernel.IfcPropertySet;
            if (propertySet == null) return;
            var hasProperties=propertySet.HasProperties.ToList();
           
            
            for (int i = 0; i < hasProperties.Count; i++)
            {
                ComboboxItem oneHasProperties = new ComboboxItem() { Text = hasProperties[i].Name.ToString(), Value = hasProperties[i] };           
                comboBoxProperties.Items.Add(oneHasProperties);
            }

            comboBoxProperties.SelectedIndex = 0;


        }

        private void comboBoxProperties_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           /* var comboBox = ControlElements[2] as ComboBox;
            if (comboBox == null) return;
            var comboBoxPropertySet = ControlElements[4] as ComboBox;
            if (comboBoxPropertySet == null) return;
            var comboBoxProperties = ControlElements[6] as ComboBox;
            if (comboBoxProperties == null) return;            
            var textBox = ControlElements[8] as TextBox;
            if (textBox == null) return;


            if (comboBoxProperties.Items.Count == 0) return;

            var selectedItem = (ComboboxItem)(comboBoxProperties.SelectedItem);
            var selectedPropertySet = ((ComboboxItem)(comboBoxPropertySet.SelectedItem)).Value as Xbim.Ifc2x3.Kernel.IfcPropertySet;
            
            var selectedProducts = ((ComboboxItem)(comboBox.SelectedItem)).Value as List<Xbim.Ifc2x3.SharedBldgElements.IfcWall>;

            if (selectedProducts.Count == 0) return;
            if (selectedPropertySet == null) return;

            var property = selectedItem.Value as Xbim.Ifc2x3.PropertyResource.IfcProperty;
            if (property == null) return;

            List<double> propertyValueDoubles = new List<double> { };

            for(int i=0;i<selectedProducts.Count;i++)
            {
                var propertySet=selectedProducts[i].PropertySets.ToList().Find(x => x.Name == selectedPropertySet.Name);
                var oneProperty = propertySet.HasProperties.ToList().Find(x => x.Name == property.Name);
                string propertyType = oneProperty.GetType().ToString();
                if (propertyType == "Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue")
                {
                    var property2 = property as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var propertyValue = property2.NominalValue as Xbim.XbimExtensions.SelectTypes.IfcValue;
                    object propertyValueTrue = property2.NominalValue.Value;                    
                    string propertyValueType = propertyValue.UnderlyingSystemType.Name;
                    if (propertyValueType == "Double")
                    {                        
                        double propertyValueDouble = (double)propertyValueTrue;
                        propertyValueDoubles.Add(propertyValueDouble);
                    }

                }
               else { return; }
            }
            */

            


                    
            


        }

      /*  private void comboBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            var comboBox2 = ControlElements[4] as ComboBox;
            if (comboBox2 == null) return;

            XbimModel xModel = new XbimModel();
            var res = xModel.Open(InputPorts[0].Data.ToString(), XbimDBAccess.ReadWrite);
            
            var selecteditem = comboBox2.SelectedItem.ToString ();


            var ifcwall = xModel.IfcProducts.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcWall>().ToList();

            List<Xbim.Ifc2x3.PropertyResource.IfcProperty> wallHeight = new List<Xbim.Ifc2x3.PropertyResource.IfcProperty> { };
            for (int i = 0; i < ifcwall.Count; i++)
            {
                var ifcwallpropertyset = ifcwall[i].PropertySets.ToList();
                var properties = ifcwallpropertyset.Find(x => x.Name == "Abhängigkeiten").HasProperties.ToList();
                wallHeight.Add(properties.Find(x => x.Name == "Nicht verknüpfte Höhe"));

            }

            List<Xbim.Ifc2x3.SharedBldgElements.IfcWall> selectedWalls = new List<Xbim.Ifc2x3.SharedBldgElements.IfcWall> { };
            foreach (var wall in ifcwall)
            {
                if(wall.PropertySets.ToList().Find(x => x.Name == "Abhängigkeiten").HasProperties.ToList().Find(x => x.Name == "Nicht verknüpfte Höhe").ToString ()== selecteditem)
                {
                    selectedWalls.Add(wall);
                }

            }


            OutputPorts[0].Data = selectedWalls;



        }

        private void comboBox3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            var comboBox3 = ControlElements[6] as ComboBox;
            if (comboBox3 == null) return;

            XbimModel xModel = new XbimModel();
            var res = xModel.Open(InputPorts[0].Data.ToString(), XbimDBAccess.ReadWrite);

            var selecteditem = comboBox3.SelectedItem.ToString();


            var ifcwall = xModel.IfcProducts.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcWall>().ToList();

            List<Xbim.Ifc2x3.PropertyResource.IfcProperty> wallType = new List<Xbim.Ifc2x3.PropertyResource.IfcProperty> { };
            for (int i = 0; i < ifcwall.Count; i++)
            {
                var ifcwallpropertyset = ifcwall[i].PropertySets.ToList();
                var properties = ifcwallpropertyset.Find(x => x.Name == "Sonstige").HasProperties.ToList();
                wallType.Add(properties.Find(x => x.Name == "Typ"));

            }

            List<Xbim.Ifc2x3.SharedBldgElements.IfcWall> selectedWalls = new List<Xbim.Ifc2x3.SharedBldgElements.IfcWall> { };
            foreach (var wall in ifcwall)
            {
                if (wall.PropertySets.ToList().Find(x => x.Name == "Sonstige").HasProperties.ToList().Find(x => x.Name == "Typ").ToString() == selecteditem)
                {
                    selectedWalls.Add(wall);
                }

            }


            OutputPorts[0].Data = selectedWalls;



        }

        private void comboBox4_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            var comboBox4 = ControlElements[8] as ComboBox;
            if (comboBox4 == null) return;

            XbimModel xModel = new XbimModel();
            var res = xModel.Open(InputPorts[0].Data.ToString(), XbimDBAccess.ReadWrite);

            var selecteditem = comboBox4.SelectedItem.ToString();


            var ifccolumn = xModel.IfcProducts.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcColumn>().ToList();

           // List<Xbim.Ifc2x3.PropertyResource.IfcProperty> ColumnBearing = new List<Xbim.Ifc2x3.PropertyResource.IfcProperty> { };
           // for (int i = 0; i < ifccolumn.Count; i++)
           // {
           //     var ifccolumnpropertyset = ifccolumn[i].PropertySets.ToList();
           //     var properties = ifccolumnpropertyset.Find(x => x.Name == "Tragwerk").HasProperties.ToList();
           //     ColumnBearing.Add(properties.Find(x => x.Name == "Berechnungsmodell aktivieren"));

           // }
          //  var abc = ColumnBearing[2].ToString();

            List<Xbim.Ifc2x3.SharedBldgElements.IfcColumn> selectedColumns = new List<Xbim.Ifc2x3.SharedBldgElements.IfcColumn> { };
            foreach (var column in ifccolumn)
            {
                var propertyset = column.PropertySets.ToList().Find(x => x.Name == "Tragwerk");
                var hasproperty = propertyset.HasProperties.ToList().Find(x => x.Name == "Berechnungsmodell aktivieren");             
                if (hasproperty.ToString() == selecteditem)
                {
                    selectedColumns.Add(column);
                }

            }


            OutputPorts[0].Data = selectedColumns;



        }*/

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
            return new IfcFilterNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}