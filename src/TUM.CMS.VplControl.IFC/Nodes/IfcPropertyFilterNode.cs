using System.Windows.Controls;
using System.Xml;
using TUM.CMS.VplControl.Core;
using System.Collections.Generic;
using Xbim.IO;
using Xbim.XbimExtensions;
using TUM.CMS.VplControl.IFC.Utilities;
using System.Linq;
using System.Collections;
using System.Text.RegularExpressions;
using System;
using System.Windows;
using System.Reflection;
using Xbim.Ifc2x3.ProductExtension;
using System.IO;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcPropertyFilterNode : Node
    {
        public XbimModel xModel;
        public IfcPropertyFilterNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {

            AddInputPortToNode("IfcProducts", typeof(object));

            AddOutputPortToNode("FilteredProducts", typeof(object));

          
            
                     
            var labelPropertySet = new Label {Content="Property Set filter:" };
            var labelProperties = new Label { Content = "property filter:" };
            var labelPropertyValue = new Label { Content = "Property Value" };


            var comboBoxPropertySet = new ComboBox { };
            comboBoxPropertySet.SelectionChanged += comboBoxPropertySet_SelectionChanged;

            var comboBoxProperties = new ComboBox { };
            comboBoxProperties.SelectionChanged += comboBoxProperties_SelectionChanged;

            

            var textBox = new TextBox { };
            textBox.TextChanged += textBox_TextChanged;

            var button = new Button { Content ="filter according to textBox"};
            button.Click += button_Click;
            

            AddControlToNode(labelPropertySet);
            AddControlToNode(comboBoxPropertySet);
            AddControlToNode(labelProperties);
            AddControlToNode(comboBoxProperties);
            AddControlToNode(labelPropertyValue);            
            AddControlToNode(textBox);
            AddControlToNode(button);
    

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            var textBox = ControlElements[5] as TextBox;
            if (textBox == null) return;
            var button = ControlElements[6] as Button;
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

            List<double> propertyValueDoubles = new List<double> { };
            List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> searchIDs = new List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> { };

         
            for(int i=0;i<(selectedIfcProducts as IList).Count;i++ )
            {
                var selectedProduct = (selectedIfcProducts as IList)[i] as IfcElement;
                searchIDs.Add(selectedProduct.GlobalId);
                var propertySet = selectedProduct.PropertySets.ToList().Find(x => x.Name == selectedPropertySet.Name);
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

           
        }

        public override void Calculate()
        {
            var modelid = ((ModelInfo)(InputPorts[0].Data)).ModelId;
            if (modelid == null) return;
            xModel = DataController.Instance.GetModel(modelid);

            var comboBoxPropertySet = ControlElements[1] as ComboBox;
            if (comboBoxPropertySet == null) return;
            comboBoxPropertySet.Items.Clear();

            var selectedItemIds = ((ModelInfo)(InputPorts[0].Data)).ElementIds;
            if (selectedItemIds == null) return;

            // MemberInfo info = typeof(Xbim.Ifc2x3.Kernel.IfcProduct).GetMethod("GlobalId");
            // IfcPersistedEntityAttribute attr = (IfcPersistedEntityAttribute)Attribute.GetCustomAttribute(info, typeof(IfcPersistedEntityAttribute));

           
           var selectedProduct = xModel.IfcProducts.OfType<IfcElement>().ToList().Find(x => x.GlobalId == selectedItemIds[0]);

            

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

        private void comboBoxPropertySet_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBoxPropertySet = ControlElements[1] as ComboBox;
            if (comboBoxPropertySet == null) return;
            var comboBoxProperties = ControlElements[3] as ComboBox;
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