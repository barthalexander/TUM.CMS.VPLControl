using System;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using TUM.CMS.VplControl.Core;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;
using Microsoft.Win32;
using Xbim.IO;
using Xbim.ModelGeometry.Scene;
using Xbim.Presentation;
using Xbim.XbimExtensions;
using XbimGeometry.Interfaces;
using Xbim.Ifc;

using System.ComponentModel;
using System.Linq;

namespace TUM.CMS.VplControl.Test.Nodes
{
    public class IfcFilterNode : Node
    {
        public IfcFilterNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {

            AddInputPortToNode("IfcFile", typeof(string));

            AddOutputPortToNode("FilteredProducts", typeof(object));

            var label = new Label { Content ="filtered products"};
            var label2 = new Label { Content = "choose products to filter"};
            var comboBox = new ComboBox
            {
                 
            };
            comboBox.SelectionChanged += comboBox_SelectionChanged;

            TextBlock textBlock = new TextBlock { };

           

            AddControlToNode(label);
            AddControlToNode(label2);

            AddControlToNode(comboBox);
            AddControlToNode(textBlock);

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

            
            


            var ifcwall = xModel.IfcProducts.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcWall>().ToList();

           // var ifcwallpropertyset = ifcwall[0].PropertySets.ToList ();
            //var ifchasproperties = ifcwallpropertyset[0].HasProperties.ToList();
           // var ifc = ifchasproperties[0].Name;

            
            
            var ifcbeam = xModel.IfcProducts.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcBeam>().ToList();
            var ifccolumn = xModel.IfcProducts.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcColumn>().ToList();
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

            if (ifcwall != null) { comboBox.Items.Add(ifcwalls); }
            if (ifcbeam != null) { comboBox.Items.Add(ifcbeams); }
            if (ifccolumn != null) { comboBox.Items.Add(ifccolumns); }
            if (ifcslab != null) { comboBox.Items.Add(ifcslabs); }
            if (ifcwindow != null) { comboBox.Items.Add(ifcwindows); }
            if (ifcstair != null) { comboBox.Items.Add(ifcstairs); }
            if (ifcroof != null) { comboBox.Items.Add(ifcroofs); }
            if (ifcramp != null) { comboBox.Items.Add(ifcramps); }
            if (ifcplate != null) { comboBox.Items.Add(ifcplates); }
            if (ifcdoor != null) { comboBox.Items.Add(ifcdoors); }
            if (ifccurtainwall != null) { comboBox.Items.Add(ifccurtainwalls); }

            comboBox.SelectedIndex = 0;
            //MessageBox.Show((comboBox.SelectedItem as ComboboxItem).Value.ToString());
            

        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            var comboBox = ControlElements[2] as ComboBox;
            if (comboBox == null) return;

           // Type t = comboBox.SelectedItem.GetType();

            
            var selecteditem = (ComboboxItem)(comboBox.SelectedItem);
            OutputPorts[0].Data= selecteditem.Value;

            
            
            var textBlock = ControlElements[3] as TextBlock;
            if (textBlock == null) return;
            


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
            return new IfcFilterNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}