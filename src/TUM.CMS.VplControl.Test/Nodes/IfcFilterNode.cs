//Documentation:
//This filter node take a list of selected ifc products as output.
//This filter has now combined 4 kinds of filters, ifc products filter, wall height filter, wall type filter and column load bearing filter. 
//Of course it can combine more filters, though I think it's better not too rash to do so 
//before we discuss the current platform with our tutors Cornelius and Dominic. 
//Yini Wang
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
using System.Collections.Generic;

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
            var label2 = new Label { Content = "products filter:"};
            var label3 = new Label { Content = "Wall height filter:" };
            var label4 = new Label { Content = "Wall type filter:" };
            var label5 = new Label { Content = "Column load bearing filter:" };
            var comboBox = new ComboBox { };
            comboBox.SelectionChanged += comboBox_SelectionChanged;
            var comboBox2 = new ComboBox { };
            comboBox2.SelectionChanged += comboBox2_SelectionChanged;
            var comboBox3 = new ComboBox { };
            comboBox3.SelectionChanged += comboBox3_SelectionChanged;
            var comboBox4 = new ComboBox { };
            comboBox4.SelectionChanged += comboBox4_SelectionChanged;



            AddControlToNode(label);
            AddControlToNode(label2);
            AddControlToNode(comboBox);
            AddControlToNode(label3);
            AddControlToNode(comboBox2);
            AddControlToNode(label4);
            AddControlToNode(comboBox3);
            AddControlToNode(label5);
            AddControlToNode(comboBox4);

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
            var comboBox2 = ControlElements[4] as ComboBox;
            if (comboBox2 == null) return;
            var comboBox3 = ControlElements[6] as ComboBox;
            if (comboBox3 == null) return;
            var comboBox4 = ControlElements[8] as ComboBox;
            if (comboBox4 == null) return;





            var ifcwall = xModel.IfcProducts.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcWall>().ToList();

            //wall height filter:
            List<Xbim.Ifc2x3.PropertyResource.IfcProperty> wallHeight=new List<Xbim.Ifc2x3.PropertyResource.IfcProperty> { };
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
            }

            //wall type filter:
            List<Xbim.Ifc2x3.PropertyResource.IfcProperty> wallType = new List<Xbim.Ifc2x3.PropertyResource.IfcProperty> { };
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
            }

            
        
            var ifcbeam = xModel.IfcProducts.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcBeam>().ToList();
            var ifccolumn = xModel.IfcProducts.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcColumn>().ToList();

            //column Load bearing/non-bearing filter:
            List<Xbim.Ifc2x3.PropertyResource.IfcProperty> ColumnBearing = new List<Xbim.Ifc2x3.PropertyResource.IfcProperty> { };
            for (int i = 0; i < ifccolumn.Count; i++)
            {
                var ifccolumnpropertyset = ifccolumn[i].PropertySets.ToList();
                var properties = ifccolumnpropertyset.Find(x => x.Name == "Tragwerk").HasProperties.ToList();
                ColumnBearing.Add(properties.Find(x => x.Name == "Berechnungsmodell aktivieren"));

            }

            comboBox4.Items.Add("true ");
            comboBox4.Items.Add("false ");

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
           
            

        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            var comboBox = ControlElements[2] as ComboBox;
            if (comboBox == null) return;

           

            
            var selecteditem = (ComboboxItem)(comboBox.SelectedItem);
            OutputPorts[0].Data= selecteditem.Value;

            
            
            


        }

        private void comboBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
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