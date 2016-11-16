using System;
using System.Windows.Controls;
using System.Xml;
using TUM.CMS.VplControl.Core;
using System.Collections.Generic;
using TUM.CMS.VplControl.IFC.Utilities;
using System.Linq;
using Xbim.Ifc;
using TUM.CMS.VplControl.IFC.Controls;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcTypeFilterNode : Node
    {
        public IfcStore xModel;
        public ModelInfoIFC2x3 OutputInfoIfc2x3;
        public ModelInfoIFC4 OutputInfoIfc4;
        public Type IfcVersionType = null;

        public IfcTypeFilterNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {

            AddInputPortToNode("IfcFile", typeof(object));

            AddOutputPortToNode("FilteredProducts", typeof(object));

            UserControl usercontrol = new UserControl();
            Grid grid = new Grid();
            usercontrol.Content = grid;

            IfcTypeFilterControl ifcTypeFilterControl = new IfcTypeFilterControl();
            ifcTypeFilterControl.comboBox.SelectionChanged += comboBox_SelectionChanged;


            AddControlToNode(ifcTypeFilterControl);



        }


        public class ComboboxItem
        {
            public string Text { get; set; }
            public List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> ValueIfcGloballyUniqueIds2x3 { get; set; }
            public List<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId> ValueIfcGloballyUniqueIds4 { get; set; }

            public override string ToString()
            { return Text; }


        }

        public override void Calculate()
        {
            if (InputPorts[0].Data == null) return;
            OutputPorts[0].Data = null;

            IfcVersionType = InputPorts[0].Data.GetType();
            if (IfcVersionType.Name == "ModelInfoIFC2x3")
            {
                var modelid = ((ModelInfoIFC2x3)(InputPorts[0].Data)).ModelId;

                if (modelid == null) return;
                OutputInfoIfc2x3 = (ModelInfoIFC2x3)(InputPorts[0].Data);
                xModel = DataController.Instance.GetModel(modelid);

                var ifcTypeFilterControl = ControlElements[0] as IfcTypeFilterControl;
                var comboBox = ifcTypeFilterControl.comboBox;
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

                List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> ifcFilteredIds = ((ModelInfoIFC2x3)(InputPorts[0].Data)).ElementIds;

                List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> ifcWallsFiltered = new List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> { };
                List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> ifcBeamsFiltered = new List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> { };
                List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> ifcColumnsFiltered = new List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> { };
                List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> ifcSlabsFiltered = new List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> { };
                List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> ifcWindowsFiltered = new List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> { };
                List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> ifcStairsFiltered = new List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> { };
                List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> ifcRoofsFiltered = new List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> { };
                List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> ifcRampsFiltered = new List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> { };
                List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> ifcPlatesFiltered = new List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> { };
                List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> ifcDoorsFiltered = new List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> { };
                List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> ifcCurtainwallsFiltered = new List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> { };


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









                ComboboxItem ifcwalls = new ComboboxItem() { Text = "ifcwall", ValueIfcGloballyUniqueIds2x3 = ifcWallsFiltered };
                ComboboxItem ifcbeams = new ComboboxItem() { Text = "ifcbeam", ValueIfcGloballyUniqueIds2x3 = ifcBeamsFiltered };
                ComboboxItem ifccolumns = new ComboboxItem() { Text = "ifccolumn", ValueIfcGloballyUniqueIds2x3 = ifcColumnsFiltered };
                ComboboxItem ifcslabs = new ComboboxItem() { Text = "ifcslab", ValueIfcGloballyUniqueIds2x3 = ifcSlabsFiltered };
                ComboboxItem ifcwindows = new ComboboxItem() { Text = "ifcwindow", ValueIfcGloballyUniqueIds2x3 = ifcWindowsFiltered };
                ComboboxItem ifcstairs = new ComboboxItem() { Text = "ifcstair", ValueIfcGloballyUniqueIds2x3 = ifcStairsFiltered };
                ComboboxItem ifcroofs = new ComboboxItem() { Text = "ifcroof", ValueIfcGloballyUniqueIds2x3 = ifcRoofsFiltered };
                ComboboxItem ifcramps = new ComboboxItem() { Text = "ifcramp", ValueIfcGloballyUniqueIds2x3 = ifcRampsFiltered };
                ComboboxItem ifcplates = new ComboboxItem() { Text = "ifcplate", ValueIfcGloballyUniqueIds2x3 = ifcPlatesFiltered };
                ComboboxItem ifcdoors = new ComboboxItem() { Text = "ifcdoor", ValueIfcGloballyUniqueIds2x3 = ifcDoorsFiltered };
                ComboboxItem ifccurtainwalls = new ComboboxItem() { Text = "ifccurtainwall", ValueIfcGloballyUniqueIds2x3 = ifcCurtainwallsFiltered };
                ComboboxItem defaultItem = new ComboboxItem() { Text = "-Ifc Type-", ValueIfcGloballyUniqueIds2x3 = null };
                if (ifcWallsFiltered.Count != 0)
                {
                    comboBox.Items.Add(ifcwalls);
                }
                if (ifcBeamsFiltered.Count != 0)
                {
                    comboBox.Items.Add(ifcbeams);
                }
                if (ifcColumnsFiltered.Count != 0)
                {
                    comboBox.Items.Add(ifccolumns);
                }
                if (ifcSlabsFiltered.Count != 0)
                {
                    comboBox.Items.Add(ifcslabs);
                }
                if (ifcWindowsFiltered.Count != 0)
                {
                    comboBox.Items.Add(ifcwindows);
                }
                if (ifcStairsFiltered.Count != 0)
                {
                    comboBox.Items.Add(ifcstairs);
                }
                if (ifcRoofsFiltered.Count != 0)
                {
                    comboBox.Items.Add(ifcroofs);
                }
                if (ifcRampsFiltered.Count != 0)
                {
                    comboBox.Items.Add(ifcramps);
                }
                if (ifcPlatesFiltered.Count != 0)
                {
                    comboBox.Items.Add(ifcplates);
                }
                if (ifcDoorsFiltered.Count != 0)
                {
                    comboBox.Items.Add(ifcdoors);
                }
                if (ifcCurtainwallsFiltered.Count != 0)
                {
                    comboBox.Items.Add(ifccurtainwalls);
                }


                comboBox.SelectedItem = defaultItem;
            }
            else if (IfcVersionType.Name == "ModelInfoIFC4")
            {
                var modelid = ((ModelInfoIFC4)(InputPorts[0].Data)).ModelId;

                if (modelid == null) return;
                OutputInfoIfc4 = (ModelInfoIFC4)(InputPorts[0].Data);
                xModel = DataController.Instance.GetModel(modelid);

                var comboBox = ControlElements[2] as ComboBox;
                if (comboBox != null && comboBox.Items.Count > 0)
                {
                    comboBox.SelectedItem = -1;
                    comboBox.Items.Clear();
                }
                var ifcwall = xModel.Instances.OfType<Xbim.Ifc4.SharedBldgElements.IfcWall>().ToList();
                var ifcbeam = xModel.Instances.OfType<Xbim.Ifc4.SharedBldgElements.IfcBeam>().ToList();
                var ifccolumn = xModel.Instances.OfType<Xbim.Ifc4.SharedBldgElements.IfcColumn>().ToList();
                var ifcslab = xModel.Instances.OfType<Xbim.Ifc4.SharedBldgElements.IfcSlab>().ToList();
                var ifcwindow = xModel.Instances.OfType<Xbim.Ifc4.SharedBldgElements.IfcWindow>().ToList();
                var ifcstair = xModel.Instances.OfType<Xbim.Ifc4.SharedBldgElements.IfcStair>().ToList();
                var ifcroof = xModel.Instances.OfType<Xbim.Ifc4.SharedBldgElements.IfcRoof>().ToList();
                var ifcramp = xModel.Instances.OfType<Xbim.Ifc4.SharedBldgElements.IfcRamp>().ToList();
                var ifcplate = xModel.Instances.OfType<Xbim.Ifc4.SharedBldgElements.IfcPlate>().ToList();
                var ifcdoor = xModel.Instances.OfType<Xbim.Ifc4.SharedBldgElements.IfcDoor>().ToList();
                var ifccurtainwall = xModel.Instances.OfType<Xbim.Ifc4.SharedBldgElements.IfcCurtainWall>().ToList();

                List<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId> ifcFilteredIds = ((ModelInfoIFC4)(InputPorts[0].Data)).ElementIds;

                List<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId> ifcWallsFiltered = new List<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId> { };
                List<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId> ifcBeamsFiltered = new List<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId> { };
                List<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId> ifcColumnsFiltered = new List<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId> { };
                List<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId> ifcSlabsFiltered = new List<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId> { };
                List<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId> ifcWindowsFiltered = new List<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId> { };
                List<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId> ifcStairsFiltered = new List<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId> { };
                List<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId> ifcRoofsFiltered = new List<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId> { };
                List<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId> ifcRampsFiltered = new List<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId> { };
                List<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId> ifcPlatesFiltered = new List<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId> { };
                List<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId> ifcDoorsFiltered = new List<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId> { };
                List<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId> ifcCurtainwallsFiltered = new List<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId> { };


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









                ComboboxItem ifcwalls = new ComboboxItem() { Text = "ifcwall", ValueIfcGloballyUniqueIds4 = ifcWallsFiltered };
                ComboboxItem ifcbeams = new ComboboxItem() { Text = "ifcbeam", ValueIfcGloballyUniqueIds4 = ifcBeamsFiltered };
                ComboboxItem ifccolumns = new ComboboxItem() { Text = "ifccolumn", ValueIfcGloballyUniqueIds4 = ifcColumnsFiltered };
                ComboboxItem ifcslabs = new ComboboxItem() { Text = "ifcslab", ValueIfcGloballyUniqueIds4 = ifcSlabsFiltered };
                ComboboxItem ifcwindows = new ComboboxItem() { Text = "ifcwindow", ValueIfcGloballyUniqueIds4 = ifcWindowsFiltered };
                ComboboxItem ifcstairs = new ComboboxItem() { Text = "ifcstair", ValueIfcGloballyUniqueIds4 = ifcStairsFiltered };
                ComboboxItem ifcroofs = new ComboboxItem() { Text = "ifcroof", ValueIfcGloballyUniqueIds4 = ifcRoofsFiltered };
                ComboboxItem ifcramps = new ComboboxItem() { Text = "ifcramp", ValueIfcGloballyUniqueIds4 = ifcRampsFiltered };
                ComboboxItem ifcplates = new ComboboxItem() { Text = "ifcplate", ValueIfcGloballyUniqueIds4 = ifcPlatesFiltered };
                ComboboxItem ifcdoors = new ComboboxItem() { Text = "ifcdoor", ValueIfcGloballyUniqueIds4 = ifcDoorsFiltered };
                ComboboxItem ifccurtainwalls = new ComboboxItem() { Text = "ifccurtainwall", ValueIfcGloballyUniqueIds4 = ifcCurtainwallsFiltered };
                ComboboxItem defaultItem = new ComboboxItem() { Text = "-Ifc Type-", ValueIfcGloballyUniqueIds4 = null };

                if (ifcWallsFiltered.Count != 0)
                {
                    comboBox.Items.Add(ifcwalls);
                }
                if (ifcBeamsFiltered.Count != 0)
                {
                    comboBox.Items.Add(ifcbeams);
                }
                if (ifcColumnsFiltered.Count != 0)
                {
                    comboBox.Items.Add(ifccolumns);
                }
                if (ifcSlabsFiltered.Count != 0)
                {
                    comboBox.Items.Add(ifcslabs);
                }
                if (ifcWindowsFiltered.Count != 0)
                {
                    comboBox.Items.Add(ifcwindows);
                }
                if (ifcStairsFiltered.Count != 0)
                {
                    comboBox.Items.Add(ifcstairs);
                }
                if (ifcRoofsFiltered.Count != 0)
                {
                    comboBox.Items.Add(ifcroofs);
                }
                if (ifcRampsFiltered.Count != 0)
                {
                    comboBox.Items.Add(ifcramps);
                }
                if (ifcPlatesFiltered.Count != 0)
                {
                    comboBox.Items.Add(ifcplates);
                }
                if (ifcDoorsFiltered.Count != 0)
                {
                    comboBox.Items.Add(ifcdoors);
                }
                if (ifcCurtainwallsFiltered.Count != 0)
                {
                    comboBox.Items.Add(ifccurtainwalls);
                }


                comboBox.SelectedItem = defaultItem;
            }
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            var ifcTypeFilterControl = ControlElements[0] as IfcTypeFilterControl;
            var comboBox = ifcTypeFilterControl.comboBox;
            if (comboBox == null) return;

            if (IfcVersionType.Name == "ModelInfoIFC2x3")
            {
                List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> searchIDs = ((ComboboxItem)(comboBox.SelectedItem)).ValueIfcGloballyUniqueIds2x3;
                OutputInfoIfc2x3.ElementIds.Clear();
                foreach (var item in searchIDs)
                {
                    OutputInfoIfc2x3.AddElementIds(item);
                }
                OutputPorts[0].Data = OutputInfoIfc2x3;
            }
            else if (IfcVersionType.Name == "ModelInfoIFC4")
            {
                List<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId> searchIDs = ((ComboboxItem)(comboBox.SelectedItem)).ValueIfcGloballyUniqueIds4;
                OutputInfoIfc4.ElementIds.Clear();
                foreach (var item in searchIDs)
                {
                    OutputInfoIfc4.AddElementIds(item);
                }
                OutputPorts[0].Data = OutputInfoIfc4;
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
            return new IfcTypeFilterNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}