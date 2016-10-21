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
using Xbim.Ifc;
using Xbim.Ifc2x3.UtilityResource;
using Xbim.Ifc2x3.SharedBldgElements;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcFilterNode : Node
    {
        public IfcStore xModel;
        public ModelInfo outputInfo; 
        public IfcFilterNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {

            AddInputPortToNode("IfcFile", typeof(object));

            AddOutputPortToNode("FilteredProducts", typeof(object));

          
            var label = new Label { Content ="filtered products"};
            var labelProducts = new Label { Content = "product filter:"};           
   
            var comboBox = new ComboBox
            {
                
            };
            comboBox.SelectionChanged += comboBox_SelectionChanged;

            

            AddControlToNode(label);
            AddControlToNode(labelProducts);
            AddControlToNode(comboBox);

    

        }


        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        public class ComboboxItem
        {
            public string Text { get; set; }
            public List<IfcGloballyUniqueId> Value { get; set; }

            public override string ToString()
            { return Text; }

           
        }

        public override void Calculate()
        {
            OutputPorts[0].Data = null;
            var modelid = ((ModelInfo)(InputPorts[0].Data)).ModelId;

            if (modelid == null) return;
            outputInfo = new ModelInfo(modelid);
            //outputInfo = (ModelInfo) (InputPorts[0].Data);
            xModel = DataController.Instance.GetModel(modelid);

            var comboBox = ControlElements[2] as ComboBox;
            if (comboBox != null && comboBox.Items.Count > 0)
            {
                comboBox.SelectedItem = -1;
                comboBox.Items.Clear();
            }            
                var ifcwall = xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcWall>().ToList();
                var ifcbeam = xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcBeam>().ToList();
                var ifccolumn = xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcColumn>().ToList();
                var ifcslab = xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcSlab>().ToList();
                var ifcwindow = xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcWindow>().ToList();
                var ifcstair = xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcStair>().ToList();
                var ifcroof = xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcRoof>().ToList();
                var ifcramp = xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcRamp>().ToList();
                var ifcplate = xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcPlate>().ToList();
                var ifcdoor = xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcDoor>().ToList();
                var ifccurtainwall = xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcCurtainWall>().ToList();

                List<IfcGloballyUniqueId> ifcFilteredIds = ((ModelInfo)(InputPorts[0].Data)).ElementIds;

                List<IfcGloballyUniqueId> ifcWallsFiltered = new List<IfcGloballyUniqueId> { };
                List<IfcGloballyUniqueId> ifcBeamsFiltered = new List<IfcGloballyUniqueId> { };
                List<IfcGloballyUniqueId> ifcColumnsFiltered = new List<IfcGloballyUniqueId> { };
                List<IfcGloballyUniqueId> ifcSlabsFiltered = new List<IfcGloballyUniqueId> { };
                List<IfcGloballyUniqueId> ifcWindowsFiltered = new List<IfcGloballyUniqueId> { };
                List<IfcGloballyUniqueId> ifcStairsFiltered = new List<IfcGloballyUniqueId> { };
                List<IfcGloballyUniqueId> ifcRoofsFiltered = new List<IfcGloballyUniqueId> { };
                List<IfcGloballyUniqueId> ifcRampsFiltered = new List<IfcGloballyUniqueId> { };
                List<IfcGloballyUniqueId> ifcPlatesFiltered = new List<IfcGloballyUniqueId> { };
                List<IfcGloballyUniqueId> ifcDoorsFiltered = new List<IfcGloballyUniqueId> { };
                List<IfcGloballyUniqueId> ifcCurtainwallsFiltered = new List<IfcGloballyUniqueId> { };


                foreach (var item in ifcwall)
                {
                    if (ifcFilteredIds.Contains(item.GlobalId))
                    {
                        ifcWallsFiltered.Add(item.GlobalId);
                    }
                }

                foreach (var item in ifcbeam)
                {
                    if (ifcFilteredIds.Contains(item.GlobalId))
                    {
                        ifcBeamsFiltered.Add(item.GlobalId);
                    }
                }
                foreach (var item in ifccolumn)
                {
                    if (ifcFilteredIds.Contains(item.GlobalId))
                    {
                        ifcColumnsFiltered.Add(item.GlobalId);
                    }
                }
                foreach (var item in ifcslab)
                {
                    if (ifcFilteredIds.Contains(item.GlobalId))
                    {
                        ifcSlabsFiltered.Add(item.GlobalId);
                    }
                }
                foreach (var item in ifcwindow)
                {
                    if (ifcFilteredIds.Contains(item.GlobalId))
                    {
                        ifcWindowsFiltered.Add(item.GlobalId);
                    }
                }
                foreach (var item in ifcstair)
                {
                    if (ifcFilteredIds.Contains(item.GlobalId))
                    {
                        ifcStairsFiltered.Add(item.GlobalId);
                    }
                }
                foreach (var item in ifcroof)
                {
                    if (ifcFilteredIds.Contains(item.GlobalId))
                    {
                        ifcRoofsFiltered.Add(item.GlobalId);
                    }
                }
                foreach (var item in ifcramp)
                {
                    if (ifcFilteredIds.Contains(item.GlobalId))
                    {
                        ifcRampsFiltered.Add(item.GlobalId);
                    }
                }
                foreach (var item in ifcplate)
                {
                    if (ifcFilteredIds.Contains(item.GlobalId))
                    {
                        ifcPlatesFiltered.Add(item.GlobalId);
                    }
                }
                foreach (var item in ifcdoor)
                {
                    if (ifcFilteredIds.Contains(item.GlobalId))
                    {
                        ifcDoorsFiltered.Add(item.GlobalId);
                    }
                }
                foreach (var item in ifccurtainwall)
                {
                    if (ifcFilteredIds.Contains(item.GlobalId))
                    {
                        ifcCurtainwallsFiltered.Add(item.GlobalId);
                    }
                }


           






                ComboboxItem ifcwalls = new ComboboxItem() { Text = "ifcwall", Value = ifcWallsFiltered };
                ComboboxItem ifcbeams = new ComboboxItem() { Text = "ifcbeam", Value = ifcBeamsFiltered };
                ComboboxItem ifccolumns = new ComboboxItem() { Text = "ifccolumn", Value = ifcColumnsFiltered };
                ComboboxItem ifcslabs = new ComboboxItem() { Text = "ifcslab", Value = ifcSlabsFiltered };
                ComboboxItem ifcwindows = new ComboboxItem() { Text = "ifcwindow", Value = ifcWindowsFiltered };
                ComboboxItem ifcstairs = new ComboboxItem() { Text = "ifcstair", Value = ifcStairsFiltered };
                ComboboxItem ifcroofs = new ComboboxItem() { Text = "ifcroof", Value = ifcRoofsFiltered };
                ComboboxItem ifcramps = new ComboboxItem() { Text = "ifcramp", Value = ifcRampsFiltered };
                ComboboxItem ifcplates = new ComboboxItem() { Text = "ifcplate", Value = ifcPlatesFiltered };
                ComboboxItem ifcdoors = new ComboboxItem() { Text = "ifcdoor", Value = ifcDoorsFiltered };
                ComboboxItem ifccurtainwalls = new ComboboxItem() { Text = "ifccurtainwall", Value = ifcCurtainwallsFiltered };

                if (ifcWallsFiltered.Count != 0) { comboBox.Items.Add(ifcwalls); }
                if (ifcBeamsFiltered.Count != 0) { comboBox.Items.Add(ifcbeams); }
                if (ifcColumnsFiltered.Count != 0) { comboBox.Items.Add(ifccolumns); }
                if (ifcSlabsFiltered.Count != 0) { comboBox.Items.Add(ifcslabs); }
                if (ifcWindowsFiltered.Count != 0) { comboBox.Items.Add(ifcwindows); }
                if (ifcStairsFiltered.Count != 0) { comboBox.Items.Add(ifcstairs); }
                if (ifcRoofsFiltered.Count != 0) { comboBox.Items.Add(ifcroofs); }
                if (ifcRampsFiltered.Count != 0) { comboBox.Items.Add(ifcramps); }
                if (ifcPlatesFiltered.Count != 0) { comboBox.Items.Add(ifcplates); }
                if (ifcDoorsFiltered.Count != 0) { comboBox.Items.Add(ifcdoors); }
                if (ifcCurtainwallsFiltered.Count != 0) { comboBox.Items.Add(ifccurtainwalls); }


                comboBox.SelectedItem = null;
            
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            var comboBox = ControlElements[2] as ComboBox;
            if (comboBox == null) return;


            List<IfcGloballyUniqueId> searchIDs = ((ComboboxItem)(comboBox.SelectedItem)).Value;
            outputInfo.ElementIds.Clear();
            foreach (var item in searchIDs)
            {
                outputInfo.AddElementIds(item);
            }
            // outputInfo.ElementIds = searchIDs;
            OutputPorts[0].Data = outputInfo ;
            
            //List<Xbim.Ifc2x3.Kernel.IfcPropertySet> PropertySet = new List<Xbim.Ifc2x3.Kernel.IfcPropertySet> { };

           /* var selectedProduct = (selectedItem as IList)[0] as IfcElement;
            if (selectedProduct == null) return;

            

            for (int i = 0; i < (selectedItem as IList).Count; i++)
            {
                searchIDs.Add(((selectedItem as IList)[i] as IfcElement).GlobalId);
            }*/

            

            /*PropertySet = selectedProduct.PropertySets.ToList();
            for (int i = 0; i < PropertySet.Count(); i++)
            {
                ComboboxItem onePropertySet = new ComboboxItem() { Text = PropertySet[i].Name.ToString(), Value = PropertySet[i] };
                comboBoxPropertySet.Items.Add(onePropertySet);
            }

            comboBoxPropertySet.SelectedIndex = 0;
            */
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