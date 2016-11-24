using System;
using System.Windows;
using System.Windows.Controls;
using TUM.CMS.VplControl.Core;
using System.Collections.Generic;
using TUM.CMS.VplControl.IFC.Utilities;
using System.Linq;
using Xbim.Ifc;
using TUM.CMS.VplControl.Energy.Controls;

namespace TUM.CMS.VplControl.Energy.Nodes
{
    public class EnergyCalculatorNode : Node
    {
        public const double Rsi = 0.13;
        public ComboboxItemLocal Rse_0 = new ComboboxItemLocal() { Text = "Select Rse" };
        public ComboboxItemLocal Rse_1 = new ComboboxItemLocal() { Text = "In direct transition wall outside air", Value = 0.04 };
        public ComboboxItemLocal Rse_2 = new ComboboxItemLocal() { Text = "A rear-ventilated façade", Value = 0.08 };
        public ComboboxItemLocal Rse_3 = new ComboboxItemLocal() { Text = "In the transition to ground", Value = 0.00 };

        public ComboboxItemFilter ifcLabel = new ComboboxItemFilter() { Text = "Select type of ifcElements" };

        public double l;
        public double Rse;
        public IfcStore xModel;
        public List<double> ElementsThermalT;//store all Elements' TT in here when we calculate them (when TT not available)
        public double sumTT;
        public ModelInfoIFC2x3 InfoIfc2x3;//it has a List of <IfcGloballyUniqueId>
        public ModelInfoIFC4 InfoIfc4;    //it has a List of <IfcGloballyUniqueId>
        public Type IfcVersionType = null;
        public bool TTExists;

        public EnergyCalculatorNode(Core.VplControl hostCanvas) : base(hostCanvas)
        {
            Console.WriteLine("--1--");
            AddInputPortToNode("text", typeof(string));//the ifc parsed file                                            

            UserControl usercontrol = new UserControl();
            Grid grid = new Grid();
            usercontrol.Content = grid;
            EnergyCalculatorControl energyCalculatorControl = new EnergyCalculatorControl();

            ComboBox comboBoxFilter = energyCalculatorControl.ComboBoxFilter;
            comboBoxFilter.Items.Add(ifcLabel);
            comboBoxFilter.SelectedIndex = 0;
            comboBoxFilter.SelectionChanged += selection_changed1;//ifcElement selection changed & check whether TT is available or not... act accordingly

            TextBox textBox_l = energyCalculatorControl.TextBox_l;
            textBox_l.Text = "Τhermal conductivity λ";
            textBox_l.MouseDoubleClick += TextBox_l_MouseDoubleClick;
            textBox_l.TextChanged += textBox_TextChanged;//when textBox is changed nothing will happen...

            ComboBox comboBox_Rse = energyCalculatorControl.ComboBoxRse;
            comboBox_Rse.Items.Add(Rse_0);//this Rse_0 is only the label
            comboBox_Rse.Items.Add(Rse_1);
            comboBox_Rse.Items.Add(Rse_2);
            comboBox_Rse.Items.Add(Rse_3);//Ra comboBox has these 3 items
            comboBox_Rse.SelectedIndex = 0;
            comboBox_Rse.SelectionChanged += selection_changed2;//load doors' properties n calculate thermal transmittances whenever this checkbox's selection is changed...

            //label for Uj total thermal transm for all the doors elements
            Label label_Uj = energyCalculatorControl.Label_Uj;

            Button button_Uj = energyCalculatorControl.Button_Uj;
            button_Uj.Click += button_Click1;//sum up all elements' TT n show it
            
            Console.WriteLine("--2--");

            AddControlToNode(energyCalculatorControl);
        }

        private void TextBox_l_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TextBox textBox_l = ((EnergyCalculatorControl)ControlElements[0]).TextBox_l;
            textBox_l.Clear();
        }

        public override void Calculate()
        {
            if (InputPorts[0].Data == null)
                return;

            Console.WriteLine("--3--");
            IfcVersionType = InputPorts[0].Data.GetType();
            if (IfcVersionType.Name == "ModelInfoIFC2x3")
            {
                var modelid = ((ModelInfoIFC2x3)(InputPorts[0].Data)).ModelId;
                Console.WriteLine("--4a--");
                if (modelid == null) return;
                InfoIfc2x3 = (ModelInfoIFC2x3)(InputPorts[0].Data);
                xModel = DataController.Instance.GetModel(modelid);
                Console.WriteLine("--4.a.i--");
                ComboBox comboBox = ((EnergyCalculatorControl)ControlElements[0]).ComboBoxFilter;
                
                Console.WriteLine("--4.a.ii--");
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
                Console.WriteLine("--5a--");
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


                bool wallTT = true;
                foreach (var item in ifcwall)
                {
                    if (ifcFilteredIds.Contains(item.GlobalId))
                        ifcWallsFiltered.Add(item.GlobalId);
                    if (GetTTifExists(item).Equals(-1))
                        wallTT = false;
                }
                bool beamTT = true;
                foreach (var item in ifcbeam)
                {
                    if (ifcFilteredIds.Contains(item.GlobalId))
                        ifcBeamsFiltered.Add(item.GlobalId);
                    if (GetTTifExists(item).Equals(-1))
                        beamTT = false;
                }
                bool columnTT = true;
                foreach (var item in ifccolumn)
                {
                    if (ifcFilteredIds.Contains(item.GlobalId))
                        ifcColumnsFiltered.Add(item.GlobalId);
                    if (GetTTifExists(item).Equals(-1))
                        columnTT = false;
                }
                bool slabTT = true;
                foreach (var item in ifcslab)
                {
                    if (ifcFilteredIds.Contains(item.GlobalId))
                        ifcSlabsFiltered.Add(item.GlobalId);
                    if (GetTTifExists(item).Equals(-1))
                        slabTT = false;
                }
                bool windowTT = true;
                foreach (var item in ifcwindow)
                {
                    if (ifcFilteredIds.Contains(item.GlobalId))
                        ifcWindowsFiltered.Add(item.GlobalId);
                    if (GetTTifExists(item).Equals(-1))
                        windowTT = false;
                }
                bool stairTT = true;
                foreach (var item in ifcstair)
                {
                    if (ifcFilteredIds.Contains(item.GlobalId))
                        ifcStairsFiltered.Add(item.GlobalId);
                    if (GetTTifExists(item).Equals(-1))
                        stairTT = false;
                }
                bool roofTT = true;
                foreach (var item in ifcroof)
                {
                    if (ifcFilteredIds.Contains(item.GlobalId))
                        ifcRoofsFiltered.Add(item.GlobalId);
                    if (GetTTifExists(item).Equals(-1))
                        roofTT = false;
                }
                bool rampTT = true;
                foreach (var item in ifcramp)
                {
                    if (ifcFilteredIds.Contains(item.GlobalId))
                        ifcRampsFiltered.Add(item.GlobalId);
                    if (GetTTifExists(item).Equals(-1))
                        rampTT = false;
                }
                bool plateTT = true;
                foreach (var item in ifcplate)
                {
                    if (ifcFilteredIds.Contains(item.GlobalId))
                        ifcPlatesFiltered.Add(item.GlobalId);
                    if (GetTTifExists(item).Equals(-1))
                        plateTT = false;
                }
                bool doorTT = true;
                foreach (var item in ifcdoor)
                {
                    if (ifcFilteredIds.Contains(item.GlobalId))
                        ifcDoorsFiltered.Add(item.GlobalId);
                    if (GetTTifExists(item).Equals(-1))
                        doorTT = false;
                }
                bool curtainwallTT = true;
                foreach (var item in ifccurtainwall)
                {
                    if (ifcFilteredIds.Contains(item.GlobalId))
                        ifcCurtainwallsFiltered.Add(item.GlobalId);
                    if (GetTTifExists(item).Equals(-1))
                        curtainwallTT = false;
                }

                Console.WriteLine("--6a--");
                ComboboxItemFilter ifcwalls = new ComboboxItemFilter() { Text = "ifcwall", ValueIfcGloballyUniqueIds2x3 = ifcWallsFiltered, TTAvailable = wallTT };
                ComboboxItemFilter ifcbeams = new ComboboxItemFilter() { Text = "ifcbeam", ValueIfcGloballyUniqueIds2x3 = ifcBeamsFiltered, TTAvailable = beamTT };
                ComboboxItemFilter ifccolumns = new ComboboxItemFilter() { Text = "ifccolumn", ValueIfcGloballyUniqueIds2x3 = ifcColumnsFiltered, TTAvailable = columnTT };
                ComboboxItemFilter ifcslabs = new ComboboxItemFilter() { Text = "ifcslab", ValueIfcGloballyUniqueIds2x3 = ifcSlabsFiltered, TTAvailable = slabTT };
                ComboboxItemFilter ifcwindows = new ComboboxItemFilter() { Text = "ifcwindow", ValueIfcGloballyUniqueIds2x3 = ifcWindowsFiltered, TTAvailable = windowTT };
                ComboboxItemFilter ifcstairs = new ComboboxItemFilter() { Text = "ifcstair", ValueIfcGloballyUniqueIds2x3 = ifcStairsFiltered, TTAvailable = stairTT };
                ComboboxItemFilter ifcroofs = new ComboboxItemFilter() { Text = "ifcroof", ValueIfcGloballyUniqueIds2x3 = ifcRoofsFiltered, TTAvailable = roofTT };
                ComboboxItemFilter ifcramps = new ComboboxItemFilter() { Text = "ifcramp", ValueIfcGloballyUniqueIds2x3 = ifcRampsFiltered, TTAvailable = rampTT };
                ComboboxItemFilter ifcplates = new ComboboxItemFilter() { Text = "ifcplate", ValueIfcGloballyUniqueIds2x3 = ifcPlatesFiltered, TTAvailable = plateTT };
                ComboboxItemFilter ifcdoors = new ComboboxItemFilter() { Text = "ifcdoor", ValueIfcGloballyUniqueIds2x3 = ifcDoorsFiltered, TTAvailable = doorTT };
                ComboboxItemFilter ifccurtainwalls = new ComboboxItemFilter() { Text = "ifccurtainwall", ValueIfcGloballyUniqueIds2x3 = ifcCurtainwallsFiltered, TTAvailable = curtainwallTT };

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


                Console.WriteLine("--7a--");
            }
            else if (IfcVersionType.Name == "ModelInfoIFC4")
            {
                var modelid = ((ModelInfoIFC4)(InputPorts[0].Data)).ModelId;
                Console.WriteLine("--4b--");
                if (modelid == null) return;
                InfoIfc4 = (ModelInfoIFC4)(InputPorts[0].Data);
                xModel = DataController.Instance.GetModel(modelid);

                ComboBox comboBox = ((EnergyCalculatorControl)ControlElements[0]).ComboBoxFilter;
                
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
                Console.WriteLine("--5b--");
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

                bool wallTT = true;
                foreach (var item in ifcwall)
                {
                    if (ifcFilteredIds.Contains(item.GlobalId))
                        ifcWallsFiltered.Add(item.GlobalId);
                    if (GetTTifExists(item).Equals(-1))
                        wallTT = false;
                }
                bool beamTT = true;
                foreach (var item in ifcbeam)
                {
                    if (ifcFilteredIds.Contains(item.GlobalId))
                        ifcBeamsFiltered.Add(item.GlobalId);
                    if (GetTTifExists(item).Equals(-1))
                        beamTT = false;
                }
                bool columnTT = true;
                foreach (var item in ifccolumn)
                {
                    if (ifcFilteredIds.Contains(item.GlobalId))
                        ifcColumnsFiltered.Add(item.GlobalId);
                    if (GetTTifExists(item).Equals(-1))
                        columnTT = false;
                }
                bool slabTT = true;
                foreach (var item in ifcslab)
                {
                    if (ifcFilteredIds.Contains(item.GlobalId))
                        ifcSlabsFiltered.Add(item.GlobalId);
                    if (GetTTifExists(item).Equals(-1))
                        slabTT = false;
                }
                bool windowTT = true;
                foreach (var item in ifcwindow)
                {
                    if (ifcFilteredIds.Contains(item.GlobalId))
                        ifcWindowsFiltered.Add(item.GlobalId);
                    if (GetTTifExists(item).Equals(-1))
                        windowTT = false;
                }
                bool stairTT = true;
                foreach (var item in ifcstair)
                {
                    if (ifcFilteredIds.Contains(item.GlobalId))
                        ifcStairsFiltered.Add(item.GlobalId);
                    if (GetTTifExists(item).Equals(-1))
                        stairTT = false;
                }
                bool roofTT = true;
                foreach (var item in ifcroof)
                {
                    if (ifcFilteredIds.Contains(item.GlobalId))
                        ifcRoofsFiltered.Add(item.GlobalId);
                    if (GetTTifExists(item).Equals(-1))
                        roofTT = false;
                }
                bool rampTT = true;
                foreach (var item in ifcramp)
                {
                    if (ifcFilteredIds.Contains(item.GlobalId))
                        ifcRampsFiltered.Add(item.GlobalId);
                    if (GetTTifExists(item).Equals(-1))
                        rampTT = false;
                }
                bool plateTT = true;
                foreach (var item in ifcplate)
                {
                    if (ifcFilteredIds.Contains(item.GlobalId))
                        ifcPlatesFiltered.Add(item.GlobalId);
                    if (GetTTifExists(item).Equals(-1))
                        plateTT = false;
                }
                bool doorTT = true;
                foreach (var item in ifcdoor)
                {
                    if (ifcFilteredIds.Contains(item.GlobalId))
                        ifcDoorsFiltered.Add(item.GlobalId);
                    if (GetTTifExists(item).Equals(-1))
                        doorTT = false;
                }
                bool curtainwallTT = true;
                foreach (var item in ifccurtainwall)
                {
                    if (ifcFilteredIds.Contains(item.GlobalId))
                        ifcCurtainwallsFiltered.Add(item.GlobalId);
                    if (GetTTifExists(item).Equals(-1))
                        curtainwallTT = false;
                }

                Console.WriteLine("--6b--");
                ComboboxItemFilter ifcwalls = new ComboboxItemFilter() { Text = "ifcwall", ValueIfcGloballyUniqueIds4 = ifcWallsFiltered, TTAvailable = wallTT };
                ComboboxItemFilter ifcbeams = new ComboboxItemFilter() { Text = "ifcbeam", ValueIfcGloballyUniqueIds4 = ifcBeamsFiltered, TTAvailable = beamTT };
                ComboboxItemFilter ifccolumns = new ComboboxItemFilter() { Text = "ifccolumn", ValueIfcGloballyUniqueIds4 = ifcColumnsFiltered, TTAvailable = columnTT };
                ComboboxItemFilter ifcslabs = new ComboboxItemFilter() { Text = "ifcslab", ValueIfcGloballyUniqueIds4 = ifcSlabsFiltered, TTAvailable = slabTT };
                ComboboxItemFilter ifcwindows = new ComboboxItemFilter() { Text = "ifcwindow", ValueIfcGloballyUniqueIds4 = ifcWindowsFiltered, TTAvailable = windowTT };
                ComboboxItemFilter ifcstairs = new ComboboxItemFilter() { Text = "ifcstair", ValueIfcGloballyUniqueIds4 = ifcStairsFiltered, TTAvailable = stairTT };
                ComboboxItemFilter ifcroofs = new ComboboxItemFilter() { Text = "ifcroof", ValueIfcGloballyUniqueIds4 = ifcRoofsFiltered, TTAvailable = roofTT };
                ComboboxItemFilter ifcramps = new ComboboxItemFilter() { Text = "ifcramp", ValueIfcGloballyUniqueIds4 = ifcRampsFiltered, TTAvailable = rampTT };
                ComboboxItemFilter ifcplates = new ComboboxItemFilter() { Text = "ifcplate", ValueIfcGloballyUniqueIds4 = ifcPlatesFiltered, TTAvailable = plateTT };
                ComboboxItemFilter ifcdoors = new ComboboxItemFilter() { Text = "ifcdoor", ValueIfcGloballyUniqueIds4 = ifcDoorsFiltered, TTAvailable = doorTT };
                ComboboxItemFilter ifccurtainwalls = new ComboboxItemFilter() { Text = "ifccurtainwall", ValueIfcGloballyUniqueIds4 = ifcCurtainwallsFiltered, TTAvailable = curtainwallTT };

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



                Console.WriteLine("--7b--");
            }
        }

        public void Calculate1()
        {//gets λ n Rse
            TextBox textBox_l_in = ((EnergyCalculatorControl)ControlElements[0]).TextBox_l;
            ComboBox comboBox_in = ((EnergyCalculatorControl)ControlElements[0]).ComboBoxRse;

            if (textBox_l_in == null || comboBox_in == null)
                return;     //if λ or Rse do not have values yet, we cannot do the calculation...
            if (textBox_l_in.Text == "")
                return;
            if (textBox_l_in.Text == "Τhermal conductivity λ")
                return;
           
            ComboboxItemLocal Selection = (ComboboxItemLocal)(comboBox_in.SelectedItem);
            if (Selection.Text == "Select Rse")
                return;

            try
            {
                l = Double.Parse(textBox_l_in.Text.Replace(",", "."));
            }
            catch (FormatException fe)
            { return; }
            Console.WriteLine("l = " + l);

            Console.WriteLine("Combobox's selection is " + Selection.Text + " that is " + Selection.Value);
            Rse = (Double)Selection.Value;
        }

        public void CalculateIfc2x3Columns()
        {
            var ifcColumns = xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcColumn>().ToList();

            List<double> ifcColumnThickness = new List<double> { };
            for (int i = 0; i < ifcColumns.Count; i++)
            {
                Console.WriteLine("###" + ifcColumns[i].Tag + "###" + ifcColumns[i].GetType() + "###");
                try
                {
                    var ifcColumnVolume = ifcColumns[i].PropertySets.ToList()[2].HasProperties.ToList()[1];//is it the right property..?-->Yes (Volumen)
                    var ifcColumnArea = ifcColumns[i].PropertySets.ToList()[2].HasProperties.ToList()[0];//is it the right property..?-->Yes (Flache)
                    var volume = ifcColumnVolume as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var volumeValue = volume.NominalValue;
                    object volumeValueTrue = volume.NominalValue.Value;
                    double volumeVal = (double)volumeValueTrue;
                    var area = ifcColumnArea as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var areaValue = area.NominalValue;
                    Console.WriteLine("##Flache:" + areaValue + " -- Volumen:" + volumeVal);
                    object areaValueTrue = area.NominalValue.Value;
                    double areaVal = (double)areaValueTrue;//
                    ifcColumnThickness.Add(volumeVal / areaVal);
                }
                catch (System.InvalidCastException castwrong)
                {
                    Console.WriteLine(ifcColumns[i].Tag + " although being " + ifcColumns[i].GetType() + ", it does not have the Flache and Volumen Properties where it should... Therefore we choose to ignore it...");
                }
                catch (System.ArgumentOutOfRangeException notenough)
                {
                    Console.WriteLine(ifcColumns[i].Tag + " although being " + ifcColumns[i].GetType() + ", it does not have its Properties where it should... Therefore we choose to ignore it...");
                }
            }
            //create a new List which will contain all doors' thermal trasmittances
            ElementsThermalT = new List<double> { };
            Console.WriteLine("Total of " + ifcColumnThickness.Count + " door-elements with their Properties in place...");
            for (int i = 0; i < ifcColumnThickness.Count; i++)
            {
                double denominator = Rsi + ifcColumnThickness[i] / l + Rse;//we consider that all doors are not double nor triple layered....
                double thermo = 1 / denominator;
                ElementsThermalT.Add(thermo);
                Console.WriteLine("#" + i + " thermo:" + thermo);
            }
        }
        public void CalculateIfc4Columns()
        {
            var ifcColumns = xModel.Instances.OfType<Xbim.Ifc4.SharedBldgElements.IfcColumn>().ToList();

            List<double> ifcColumnThickness = new List<double> { };
            for (int i = 0; i < ifcColumns.Count; i++)
            {
                Console.WriteLine("###" + ifcColumns[i].Tag + "###" + ifcColumns[i].GetType() + "###");
                try
                {
                    var ifcColumnVolume = ifcColumns[i].PropertySets.ToList()[2].HasProperties.ToList()[1];//is it the right property..?-->Yes (Volumen)
                    var ifcColumnArea = ifcColumns[i].PropertySets.ToList()[2].HasProperties.ToList()[0];//is it the right property..?-->Yes (Flache)
                    var volume = ifcColumnVolume as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var volumeValue = volume.NominalValue;
                    object volumeValueTrue = volume.NominalValue.Value;
                    double volumeVal = (double)volumeValueTrue;
                    var area = ifcColumnArea as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var areaValue = area.NominalValue;
                    Console.WriteLine("##Flache:" + areaValue + " -- Volumen:" + volumeVal);
                    object areaValueTrue = area.NominalValue.Value;
                    double areaVal = (double)areaValueTrue;//
                    ifcColumnThickness.Add(volumeVal / areaVal);
                }
                catch (System.InvalidCastException castwrong)
                {
                    Console.WriteLine(ifcColumns[i].Tag + " although being " + ifcColumns[i].GetType() + ", it does not have the Flache and Volumen Properties where it should... Therefore we choose to ignore it...");
                }
                catch (System.ArgumentOutOfRangeException notenough)
                {
                    Console.WriteLine(ifcColumns[i].Tag + " although being " + ifcColumns[i].GetType() + ", it does not have its Properties where it should... Therefore we choose to ignore it...");
                }
            }
            //create a new List which will contain all columns' thermal trasmittances
            ElementsThermalT = new List<double> { };
          
            for (int i = 0; i < ifcColumnThickness.Count; i++)
            {
                double denominator = Rsi + ifcColumnThickness[i] / l + Rse;//we consider that all columns are not double nor triple layered....
                double thermo = 1 / denominator;
                ElementsThermalT.Add(thermo);
                Console.WriteLine("#" + i + " thermo:" + thermo);
            }
        }
        public void CalculateIfc2x3Doors()
        {
            var ifcDoor = xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcDoor>().ToList();

            List<double> ifcDoorThickness = new List<double> { };
            for (int i = 0; i < ifcDoor.Count; i++)
            {
                Console.WriteLine("###" + ifcDoor[i].Tag + "###" + ifcDoor[i].GetType() + "###");
                try
                {
                    var ifcDoorVolume = ifcDoor[i].PropertySets.ToList()[2].HasProperties.ToList()[1];//is it the right property..?-->Yes (Volumen)
                    var ifcDoorArea = ifcDoor[i].PropertySets.ToList()[2].HasProperties.ToList()[0];//is it the right property..?-->Yes (Flache)
                    var volume = ifcDoorVolume as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var volumeValue = volume.NominalValue;
                    object volumeValueTrue = volume.NominalValue.Value;
                    double volumeVal = (double)volumeValueTrue;
                    var area = ifcDoorArea as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var areaValue = area.NominalValue;
                    Console.WriteLine("##Flache:" + areaValue + " -- Volumen:" + volumeVal);
                    object areaValueTrue = area.NominalValue.Value;
                    double areaVal = (double)areaValueTrue;//
                    ifcDoorThickness.Add(volumeVal / areaVal);
                }
                catch (System.InvalidCastException castwrong)
                {
                    Console.WriteLine(ifcDoor[i].Tag + " although being " + ifcDoor[i].GetType() + ", it does not have the Flache and Volumen Properties where it should... Therefore we choose to ignore it...");
                }
                catch (System.ArgumentOutOfRangeException notenough)
                {
                    Console.WriteLine(ifcDoor[i].Tag + " although being " + ifcDoor[i].GetType() + ", it does not have its Properties where it should... Therefore we choose to ignore it...");
                }
            }
            //create a new List which will contain all doors' thermal trasmittances
            ElementsThermalT = new List<double> { };
      
            for (int i = 0; i < ifcDoorThickness.Count; i++)
            {
                double denominator = Rsi + ifcDoorThickness[i] / l + Rse;//we consider that all doors are not double nor triple layered....
                double thermo = 1 / denominator;
                ElementsThermalT.Add(thermo);
                Console.WriteLine("#" + i + " thermo:" + thermo);
            }
        }
        public void CalculateIfc4Doors()
        {
            var ifcDoor = xModel.Instances.OfType<Xbim.Ifc4.SharedBldgElements.IfcDoor>().ToList();

            List<double> ifcDoorThickness = new List<double> { };
            for (int i = 0; i < ifcDoor.Count; i++)
            {
                Console.WriteLine("###" + ifcDoor[i].Tag + "###" + ifcDoor[i].GetType() + "###");
                try
                {
                    var ifcDoorVolume = ifcDoor[i].PropertySets.ToList()[2].HasProperties.ToList()[1];//is it the right property..?-->Yes (Volumen)
                    var ifcDoorArea = ifcDoor[i].PropertySets.ToList()[2].HasProperties.ToList()[0];//is it the right property..?-->Yes (Flache)
                    var volume = ifcDoorVolume as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var volumeValue = volume.NominalValue;
                    object volumeValueTrue = volume.NominalValue.Value;
                    double volumeVal = (double)volumeValueTrue;
                    var area = ifcDoorArea as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var areaValue = area.NominalValue;
                    Console.WriteLine("##Flache:" + areaValue + " -- Volumen:" + volumeVal);
                    object areaValueTrue = area.NominalValue.Value;
                    double areaVal = (double)areaValueTrue;//
                    ifcDoorThickness.Add(volumeVal / areaVal);
                }
                catch (System.InvalidCastException castwrong)
                {
                    Console.WriteLine(ifcDoor[i].Tag + " although being " + ifcDoor[i].GetType() + ", it does not have the Flache and Volumen Properties where it should... Therefore we choose to ignore it...");
                }
                catch (System.ArgumentOutOfRangeException notenough)
                {
                    Console.WriteLine(ifcDoor[i].Tag + " although being " + ifcDoor[i].GetType() + ", it does not have its Properties where it should... Therefore we choose to ignore it...");
                }
            }
            //create a new List which will contain all doors' thermal trasmittances
            ElementsThermalT = new List<double> { };
       
            for (int i = 0; i < ifcDoorThickness.Count; i++)
            {
                double denominator = Rsi + ifcDoorThickness[i] / l + Rse;//we consider that all doors are not double nor triple layered....
                double thermo = 1 / denominator;
                ElementsThermalT.Add(thermo);
                Console.WriteLine("#" + i + " thermo:" + thermo);
            }
        }
        public void CalculateIfc2x3CurtainWalls()
        {
            var ifcCurtainWall = xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcCurtainWall>().ToList();

            List<double> ifcCWallThickness = new List<double> { };
            for (int i = 0; i < ifcCurtainWall.Count; i++)
            {
                Console.WriteLine("###" + ifcCurtainWall[i].Tag + "###" + ifcCurtainWall[i].GetType() + "###");
                try
                {
                    var ifcCWallVolume = ifcCurtainWall[i].PropertySets.ToList()[2].HasProperties.ToList()[1];//is it the right property..?-->Yes (Volumen)
                    var ifcCWallArea = ifcCurtainWall[i].PropertySets.ToList()[2].HasProperties.ToList()[0];//is it the right property..?-->Yes (Flache)
                    var volume = ifcCWallVolume as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var volumeValue = volume.NominalValue;
                    object volumeValueTrue = volume.NominalValue.Value;
                    double volumeVal = (double)volumeValueTrue;
                    var area = ifcCWallArea as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var areaValue = area.NominalValue;
                    Console.WriteLine("##Flache:" + areaValue + " -- Volumen:" + volumeVal);
                    object areaValueTrue = area.NominalValue.Value;
                    double areaVal = (double)areaValueTrue;//
                    ifcCWallThickness.Add(volumeVal / areaVal);
                }
                catch (System.InvalidCastException castwrong)
                {
                    Console.WriteLine(ifcCurtainWall[i].Tag + " although being " + ifcCurtainWall[i].GetType() + ", it does not have the Flache and Volumen Properties where it should... Therefore we choose to ignore it...");
                }
                catch (System.ArgumentOutOfRangeException notenough)
                {
                    Console.WriteLine(ifcCurtainWall[i].Tag + " although being " + ifcCurtainWall[i].GetType() + ", it does not have its Properties where it should... Therefore we choose to ignore it...");
                }
            }
            //create a new List which will contain all curtain walls' thermal trasmittances
            ElementsThermalT = new List<double> { };
         
            for (int i = 0; i < ifcCWallThickness.Count; i++)
            {
                double denominator = Rsi + ifcCWallThickness[i] / l + Rse;//we consider that all curtain walls are not double nor triple layered....
                double thermo = 1 / denominator;
                ElementsThermalT.Add(thermo);
                Console.WriteLine("#" + i + " thermo:" + thermo);
            }
        }
        public void CalculateIfc4CurtainWalls()
        {
            var ifcCurtainWall = xModel.Instances.OfType<Xbim.Ifc4.SharedBldgElements.IfcCurtainWall>().ToList();

            List<double> ifcCWallThickness = new List<double> { };
            for (int i = 0; i < ifcCurtainWall.Count; i++)
            {
                Console.WriteLine("###" + ifcCurtainWall[i].Tag + "###" + ifcCurtainWall[i].GetType() + "###");
                try
                {
                    var ifcCWallVolume = ifcCurtainWall[i].PropertySets.ToList()[2].HasProperties.ToList()[1];//is it the right property..?-->Yes (Volumen)
                    var ifcCWallArea = ifcCurtainWall[i].PropertySets.ToList()[2].HasProperties.ToList()[0];//is it the right property..?-->Yes (Flache)
                    var volume = ifcCWallVolume as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var volumeValue = volume.NominalValue;
                    object volumeValueTrue = volume.NominalValue.Value;
                    double volumeVal = (double)volumeValueTrue;
                    var area = ifcCWallArea as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var areaValue = area.NominalValue;
                    Console.WriteLine("##Flache:" + areaValue + " -- Volumen:" + volumeVal);
                    object areaValueTrue = area.NominalValue.Value;
                    double areaVal = (double)areaValueTrue;//
                    ifcCWallThickness.Add(volumeVal / areaVal);
                }
                catch (System.InvalidCastException castwrong)
                {
                    Console.WriteLine(ifcCurtainWall[i].Tag + " although being " + ifcCurtainWall[i].GetType() + ", it does not have the Flache and Volumen Properties where it should... Therefore we choose to ignore it...");
                }
                catch (System.ArgumentOutOfRangeException notenough)
                {
                    Console.WriteLine(ifcCurtainWall[i].Tag + " although being " + ifcCurtainWall[i].GetType() + ", it does not have its Properties where it should... Therefore we choose to ignore it...");
                }
            }
            //create a new List which will contain all curtain walls' thermal trasmittances
            ElementsThermalT = new List<double> { };
           
            for (int i = 0; i < ifcCWallThickness.Count; i++)
            {
                double denominator = Rsi + ifcCWallThickness[i] / l + Rse;//we consider that all curtain walls are not double nor triple layered....
                double thermo = 1 / denominator;
                ElementsThermalT.Add(thermo);
                Console.WriteLine("#" + i + " thermo:" + thermo);
            }
        }
        public void CalculateIfc2x3Beams()
        {
            var ifcBeam = xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcBeam>().ToList();

            List<double> ifcBeamThickness = new List<double> { };
            for (int i = 0; i < ifcBeam.Count; i++)
            {
                Console.WriteLine("###" + ifcBeam[i].Tag + "###" + ifcBeam[i].GetType() + "###");
                try
                {
                    var ifcBeamVolume = ifcBeam[i].PropertySets.ToList()[2].HasProperties.ToList()[1];//is it the right property..?-->Yes (Volumen)
                    var ifcBeamArea = ifcBeam[i].PropertySets.ToList()[2].HasProperties.ToList()[0];//is it the right property..?-->Yes (Flache)
                    var volume = ifcBeamVolume as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var volumeValue = volume.NominalValue;
                    object volumeValueTrue = volume.NominalValue.Value;
                    double volumeVal = (double)volumeValueTrue;
                    var area = ifcBeamArea as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var areaValue = area.NominalValue;
                    Console.WriteLine("##Flache:" + areaValue + " -- Volumen:" + volumeVal);
                    object areaValueTrue = area.NominalValue.Value;
                    double areaVal = (double)areaValueTrue;//
                    ifcBeamThickness.Add(volumeVal / areaVal);
                }
                catch (System.InvalidCastException castwrong)
                {
                    Console.WriteLine(ifcBeam[i].Tag + " although being " + ifcBeam[i].GetType() + ", it does not have the Flache and Volumen Properties where it should... Therefore we choose to ignore it...");
                }
                catch (System.ArgumentOutOfRangeException notenough)
                {
                    Console.WriteLine(ifcBeam[i].Tag + " although being " + ifcBeam[i].GetType() + ", it does not have its Properties where it should... Therefore we choose to ignore it...");
                }
            }
            //create a new List which will contain all beams' thermal trasmittances
            ElementsThermalT = new List<double> { };
   
            for (int i = 0; i < ifcBeamThickness.Count; i++)
            {
                double denominator = Rsi + ifcBeamThickness[i] / l + Rse;//we consider that all beams are not double nor triple layered....
                double thermo = 1 / denominator;
                ElementsThermalT.Add(thermo);
                Console.WriteLine("#" + i + " thermo:" + thermo);
            }
        }
        public void CalculateIfc4Beams()
        {
            var ifcBeam = xModel.Instances.OfType<Xbim.Ifc4.SharedBldgElements.IfcBeam>().ToList();

            List<double> ifcBeamThickness = new List<double> { };
            for (int i = 0; i < ifcBeam.Count; i++)
            {
                Console.WriteLine("###" + ifcBeam[i].Tag + "###" + ifcBeam[i].GetType() + "###");
                try
                {
                    var ifcBeamVolume = ifcBeam[i].PropertySets.ToList()[2].HasProperties.ToList()[1];//is it the right property..?-->Yes (Volumen)
                    var ifcBeamArea = ifcBeam[i].PropertySets.ToList()[2].HasProperties.ToList()[0];//is it the right property..?-->Yes (Flache)
                    var volume = ifcBeamVolume as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var volumeValue = volume.NominalValue;
                    object volumeValueTrue = volume.NominalValue.Value;
                    double volumeVal = (double)volumeValueTrue;
                    var area = ifcBeamArea as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var areaValue = area.NominalValue;
                    Console.WriteLine("##Flache:" + areaValue + " -- Volumen:" + volumeVal);
                    object areaValueTrue = area.NominalValue.Value;
                    double areaVal = (double)areaValueTrue;//
                    ifcBeamThickness.Add(volumeVal / areaVal);
                }
                catch (System.InvalidCastException castwrong)
                {
                    Console.WriteLine(ifcBeam[i].Tag + " although being " + ifcBeam[i].GetType() + ", it does not have the Flache and Volumen Properties where it should... Therefore we choose to ignore it...");
                }
                catch (System.ArgumentOutOfRangeException notenough)
                {
                    Console.WriteLine(ifcBeam[i].Tag + " although being " + ifcBeam[i].GetType() + ", it does not have its Properties where it should... Therefore we choose to ignore it...");
                }
            }
            //create a new List which will contain all beams' thermal trasmittances
            ElementsThermalT = new List<double> { };
           
            for (int i = 0; i < ifcBeamThickness.Count; i++)
            {
                double denominator = Rsi + ifcBeamThickness[i] / l + Rse;//we consider that all beams are not double nor triple layered....
                double thermo = 1 / denominator;
                ElementsThermalT.Add(thermo);
                Console.WriteLine("#" + i + " thermo:" + thermo);
            }
        }
        public void CalculateIfc2x3Slabs()
        {
            var ifcSlab = xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcSlab>().ToList();

            List<double> ifcSlabThickness = new List<double> { };
            for (int i = 0; i < ifcSlab.Count; i++)
            {
                Console.WriteLine("###" + ifcSlab[i].Tag + "###" + ifcSlab[i].GetType() + "###");
                try
                {
                    var ifcSlabVolume = ifcSlab[i].PropertySets.ToList()[2].HasProperties.ToList()[1];//is it the right property..?-->Yes (Volumen)
                    var ifcSlabArea = ifcSlab[i].PropertySets.ToList()[2].HasProperties.ToList()[0];//is it the right property..?-->Yes (Flache)
                    var volume = ifcSlabVolume as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var volumeValue = volume.NominalValue;
                    object volumeValueTrue = volume.NominalValue.Value;
                    double volumeVal = (double)volumeValueTrue;
                    var area = ifcSlabArea as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var areaValue = area.NominalValue;
                    Console.WriteLine("##Flache:" + areaValue + " -- Volumen:" + volumeVal);
                    object areaValueTrue = area.NominalValue.Value;
                    double areaVal = (double)areaValueTrue;//
                    ifcSlabThickness.Add(volumeVal / areaVal);
                }
                catch (System.InvalidCastException castwrong)
                {
                    Console.WriteLine(ifcSlab[i].Tag + " although being " + ifcSlab[i].GetType() + ", it does not have the Flache and Volumen Properties where it should... Therefore we choose to ignore it...");
                }
                catch (System.ArgumentOutOfRangeException notenough)
                {
                    Console.WriteLine(ifcSlab[i].Tag + " although being " + ifcSlab[i].GetType() + ", it does not have its Properties where it should... Therefore we choose to ignore it...");
                }
            }
            //create a new List which will contain all slabs' thermal trasmittances
            ElementsThermalT = new List<double> { };
            
            for (int i = 0; i < ifcSlabThickness.Count; i++)
            {
                double denominator = Rsi + ifcSlabThickness[i] / l + Rse;//we consider that all slabs are not double nor triple layered....
                double thermo = 1 / denominator;
                ElementsThermalT.Add(thermo);
                Console.WriteLine("#" + i + " thermo:" + thermo);
            }
        }
        public void CalculateIfc4Slabs()
        {
            var ifcSlab = xModel.Instances.OfType<Xbim.Ifc4.SharedBldgElements.IfcSlab>().ToList();

            List<double> ifcSlabThickness = new List<double> { };
            for (int i = 0; i < ifcSlab.Count; i++)
            {
                Console.WriteLine("###" + ifcSlab[i].Tag + "###" + ifcSlab[i].GetType() + "###");
                try
                {
                    var ifcSlabVolume = ifcSlab[i].PropertySets.ToList()[2].HasProperties.ToList()[1];//is it the right property..?-->Yes (Volumen)
                    var ifcSlabArea = ifcSlab[i].PropertySets.ToList()[2].HasProperties.ToList()[0];//is it the right property..?-->Yes (Flache)
                    var volume = ifcSlabVolume as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var volumeValue = volume.NominalValue;
                    object volumeValueTrue = volume.NominalValue.Value;
                    double volumeVal = (double)volumeValueTrue;
                    var area = ifcSlabArea as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var areaValue = area.NominalValue;
                    Console.WriteLine("##Flache:" + areaValue + " -- Volumen:" + volumeVal);
                    object areaValueTrue = area.NominalValue.Value;
                    double areaVal = (double)areaValueTrue;//
                    ifcSlabThickness.Add(volumeVal / areaVal);
                }
                catch (System.InvalidCastException castwrong)
                {
                    Console.WriteLine(ifcSlab[i].Tag + " although being " + ifcSlab[i].GetType() + ", it does not have the Flache and Volumen Properties where it should... Therefore we choose to ignore it...");
                }
                catch (System.ArgumentOutOfRangeException notenough)
                {
                    Console.WriteLine(ifcSlab[i].Tag + " although being " + ifcSlab[i].GetType() + ", it does not have its Properties where it should... Therefore we choose to ignore it...");
                }
            }
            //create a new List which will contain all slabs' thermal trasmittances
            ElementsThermalT = new List<double> { };
            Console.WriteLine("Total of " + ifcSlabThickness.Count + " door-elements with their Properties in place...");
            for (int i = 0; i < ifcSlabThickness.Count; i++)
            {
                double denominator = Rsi + ifcSlabThickness[i] / l + Rse;//we consider that all slabs are not double nor triple layered....
                double thermo = 1 / denominator;
                ElementsThermalT.Add(thermo);
                Console.WriteLine("#" + i + " thermo:" + thermo);
            }
        }
        public void CalculateIfc2x3Windows()
        {
            var ifcWindow = xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcWindow>().ToList();

            List<double> ifcWindowThickness = new List<double> { };
            for (int i = 0; i < ifcWindow.Count; i++)
            {
                Console.WriteLine("###" + ifcWindow[i].Tag + "###" + ifcWindow[i].GetType() + "###");
                try
                {
                    var ifcWindowVolume = ifcWindow[i].PropertySets.ToList()[2].HasProperties.ToList()[1];//is it the right property..?-->Yes (Volumen)
                    var ifcWindowArea = ifcWindow[i].PropertySets.ToList()[2].HasProperties.ToList()[0];//is it the right property..?-->Yes (Flache)
                    var volume = ifcWindowVolume as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var volumeValue = volume.NominalValue;
                    object volumeValueTrue = volume.NominalValue.Value;
                    double volumeVal = (double)volumeValueTrue;
                    var area = ifcWindowArea as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var areaValue = area.NominalValue;
                    Console.WriteLine("##Flache:" + areaValue + " -- Volumen:" + volumeVal);
                    object areaValueTrue = area.NominalValue.Value;
                    double areaVal = (double)areaValueTrue;//
                    ifcWindowThickness.Add(volumeVal / areaVal);
                }
                catch (System.InvalidCastException castwrong)
                {
                    Console.WriteLine(ifcWindow[i].Tag + " although being " + ifcWindow[i].GetType() + ", it does not have the Flache and Volumen Properties where it should... Therefore we choose to ignore it...");
                }
                catch (System.ArgumentOutOfRangeException notenough)
                {
                    Console.WriteLine(ifcWindow[i].Tag + " although being " + ifcWindow[i].GetType() + ", it does not have its Properties where it should... Therefore we choose to ignore it...");
                }
            }
            //create a new List which will contain all windows' thermal trasmittances
            ElementsThermalT = new List<double> { };
           
            for (int i = 0; i < ifcWindowThickness.Count; i++)
            {
                double denominator = Rsi + ifcWindowThickness[i] / l + Rse;//we consider that all windows are not double nor triple layered....
                double thermo = 1 / denominator;
                ElementsThermalT.Add(thermo);
                Console.WriteLine("#" + i + " thermo:" + thermo);
            }
        }
        public void CalculateIfc4Windows()
        {
            var ifcWindow = xModel.Instances.OfType<Xbim.Ifc4.SharedBldgElements.IfcWindow>().ToList();

            List<double> ifcWindowThickness = new List<double> { };
            for (int i = 0; i < ifcWindow.Count; i++)
            {
                Console.WriteLine("###" + ifcWindow[i].Tag + "###" + ifcWindow[i].GetType() + "###");
                try
                {
                    var ifcWindowVolume = ifcWindow[i].PropertySets.ToList()[2].HasProperties.ToList()[1];//is it the right property..?-->Yes (Volumen)
                    var ifcWindowArea = ifcWindow[i].PropertySets.ToList()[2].HasProperties.ToList()[0];//is it the right property..?-->Yes (Flache)
                    var volume = ifcWindowVolume as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var volumeValue = volume.NominalValue;
                    object volumeValueTrue = volume.NominalValue.Value;
                    double volumeVal = (double)volumeValueTrue;
                    var area = ifcWindowArea as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var areaValue = area.NominalValue;
                    Console.WriteLine("##Flache:" + areaValue + " -- Volumen:" + volumeVal);
                    object areaValueTrue = area.NominalValue.Value;
                    double areaVal = (double)areaValueTrue;//
                    ifcWindowThickness.Add(volumeVal / areaVal);
                }
                catch (System.InvalidCastException castwrong)
                {
                    Console.WriteLine(ifcWindow[i].Tag + " although being " + ifcWindow[i].GetType() + ", it does not have the Flache and Volumen Properties where it should... Therefore we choose to ignore it...");
                }
                catch (System.ArgumentOutOfRangeException notenough)
                {
                    Console.WriteLine(ifcWindow[i].Tag + " although being " + ifcWindow[i].GetType() + ", it does not have its Properties where it should... Therefore we choose to ignore it...");
                }
            }
            //create a new List which will contain all windows' thermal trasmittances
            ElementsThermalT = new List<double> { };
            
            for (int i = 0; i < ifcWindowThickness.Count; i++)
            {
                double denominator = Rsi + ifcWindowThickness[i] / l + Rse;//we consider that all windows are not double nor triple layered....
                double thermo = 1 / denominator;
                ElementsThermalT.Add(thermo);
                Console.WriteLine("#" + i + " thermo:" + thermo);
            }
        }
        public void CalculateIfc2x3Stairs()
        {
            var ifcStair = xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcStair>().ToList();

            List<double> ifcStairThickness = new List<double> { };
            for (int i = 0; i < ifcStair.Count; i++)
            {
                Console.WriteLine("###" + ifcStair[i].Tag + "###" + ifcStair[i].GetType() + "###");
                try
                {
                    var ifcStairVolume = ifcStair[i].PropertySets.ToList()[2].HasProperties.ToList()[1];//is it the right property..?-->Yes (Volumen)
                    var ifcStairArea = ifcStair[i].PropertySets.ToList()[2].HasProperties.ToList()[0];//is it the right property..?-->Yes (Flache)
                    var volume = ifcStairVolume as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var volumeValue = volume.NominalValue;
                    object volumeValueTrue = volume.NominalValue.Value;
                    double volumeVal = (double)volumeValueTrue;
                    var area = ifcStairArea as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var areaValue = area.NominalValue;
                    Console.WriteLine("##Flache:" + areaValue + " -- Volumen:" + volumeVal);
                    object areaValueTrue = area.NominalValue.Value;
                    double areaVal = (double)areaValueTrue;//
                    ifcStairThickness.Add(volumeVal / areaVal);
                }
                catch (System.InvalidCastException castwrong)
                {
                    Console.WriteLine(ifcStair[i].Tag + " although being " + ifcStair[i].GetType() + ", it does not have the Flache and Volumen Properties where it should... Therefore we choose to ignore it...");
                }
                catch (System.ArgumentOutOfRangeException notenough)
                {
                    Console.WriteLine(ifcStair[i].Tag + " although being " + ifcStair[i].GetType() + ", it does not have its Properties where it should... Therefore we choose to ignore it...");
                }
            }
            //create a new List which will contain all stairs' thermal trasmittances
            ElementsThermalT = new List<double> { };
            Console.WriteLine("Total of " + ifcStairThickness.Count + " door-elements with their Properties in place...");
            for (int i = 0; i < ifcStairThickness.Count; i++)
            {
                double denominator = Rsi + ifcStairThickness[i] / l + Rse;//we consider that all stairs are not double nor triple layered....
                double thermo = 1 / denominator;
                ElementsThermalT.Add(thermo);
                Console.WriteLine("#" + i + " thermo:" + thermo);
            }
        }
        public void CalculateIfc4Stairs()
        {
            var ifcStair = xModel.Instances.OfType<Xbim.Ifc4.SharedBldgElements.IfcStair>().ToList();

            List<double> ifcStairThickness = new List<double> { };
            for (int i = 0; i < ifcStair.Count; i++)
            {
                Console.WriteLine("###" + ifcStair[i].Tag + "###" + ifcStair[i].GetType() + "###");
                try
                {
                    var ifcStairVolume = ifcStair[i].PropertySets.ToList()[2].HasProperties.ToList()[1];//is it the right property..?-->Yes (Volumen)
                    var ifcStairArea = ifcStair[i].PropertySets.ToList()[2].HasProperties.ToList()[0];//is it the right property..?-->Yes (Flache)
                    var volume = ifcStairVolume as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var volumeValue = volume.NominalValue;
                    object volumeValueTrue = volume.NominalValue.Value;
                    double volumeVal = (double)volumeValueTrue;
                    var area = ifcStairArea as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var areaValue = area.NominalValue;
                    Console.WriteLine("##Flache:" + areaValue + " -- Volumen:" + volumeVal);
                    object areaValueTrue = area.NominalValue.Value;
                    double areaVal = (double)areaValueTrue;//
                    ifcStairThickness.Add(volumeVal / areaVal);
                }
                catch (System.InvalidCastException castwrong)
                {
                    Console.WriteLine(ifcStair[i].Tag + " although being " + ifcStair[i].GetType() + ", it does not have the Flache and Volumen Properties where it should... Therefore we choose to ignore it...");
                }
                catch (System.ArgumentOutOfRangeException notenough)
                {
                    Console.WriteLine(ifcStair[i].Tag + " although being " + ifcStair[i].GetType() + ", it does not have its Properties where it should... Therefore we choose to ignore it...");
                }
            }
            //create a new List which will contain all stairs' thermal trasmittances
            ElementsThermalT = new List<double> { };
            
            for (int i = 0; i < ifcStairThickness.Count; i++)
            {
                double denominator = Rsi + ifcStairThickness[i] / l + Rse;//we consider that all stairs are not double nor triple layered....
                double thermo = 1 / denominator;
                ElementsThermalT.Add(thermo);
                Console.WriteLine("#" + i + " thermo:" + thermo);
            }
        }
        public void CalculateIfc2x3Roofs()
        {
            var ifcRoof = xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcRoof>().ToList();

            List<double> ifcRoofThickness = new List<double> { };
            for (int i = 0; i < ifcRoof.Count; i++)
            {
                Console.WriteLine("###" + ifcRoof[i].Tag + "###" + ifcRoof[i].GetType() + "###");
                try
                {
                    var ifcRoofVolume = ifcRoof[i].PropertySets.ToList()[2].HasProperties.ToList()[1];//is it the right property..?-->Yes (Volumen)
                    var ifcRoofArea = ifcRoof[i].PropertySets.ToList()[2].HasProperties.ToList()[0];//is it the right property..?-->Yes (Flache)
                    var volume = ifcRoofVolume as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var volumeValue = volume.NominalValue;
                    object volumeValueTrue = volume.NominalValue.Value;
                    double volumeVal = (double)volumeValueTrue;
                    var area = ifcRoofArea as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var areaValue = area.NominalValue;
                    Console.WriteLine("##Flache:" + areaValue + " -- Volumen:" + volumeVal);
                    object areaValueTrue = area.NominalValue.Value;
                    double areaVal = (double)areaValueTrue;//
                    ifcRoofThickness.Add(volumeVal / areaVal);
                }
                catch (System.InvalidCastException castwrong)
                {
                    Console.WriteLine(ifcRoof[i].Tag + " although being " + ifcRoof[i].GetType() + ", it does not have the Flache and Volumen Properties where it should... Therefore we choose to ignore it...");
                }
                catch (System.ArgumentOutOfRangeException notenough)
                {
                    Console.WriteLine(ifcRoof[i].Tag + " although being " + ifcRoof[i].GetType() + ", it does not have its Properties where it should... Therefore we choose to ignore it...");
                }
            }
            //create a new List which will contain all roofs' thermal trasmittances
            ElementsThermalT = new List<double> { };
            
            for (int i = 0; i < ifcRoofThickness.Count; i++)
            {
                double denominator = Rsi + ifcRoofThickness[i] / l + Rse;//we consider that all roofs are not double nor triple layered....
                double thermo = 1 / denominator;
                ElementsThermalT.Add(thermo);
                Console.WriteLine("#" + i + " thermo:" + thermo);
            }
        }
        public void CalculateIfc4Roofs()
        {
            var ifcRoof = xModel.Instances.OfType<Xbim.Ifc4.SharedBldgElements.IfcRoof>().ToList();

            List<double> ifcRoofThickness = new List<double> { };
            for (int i = 0; i < ifcRoof.Count; i++)
            {
                Console.WriteLine("###" + ifcRoof[i].Tag + "###" + ifcRoof[i].GetType() + "###");
                try
                {
                    var ifcRoofVolume = ifcRoof[i].PropertySets.ToList()[2].HasProperties.ToList()[1];//is it the right property..?-->Yes (Volumen)
                    var ifcRoofArea = ifcRoof[i].PropertySets.ToList()[2].HasProperties.ToList()[0];//is it the right property..?-->Yes (Flache)
                    var volume = ifcRoofVolume as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var volumeValue = volume.NominalValue;
                    object volumeValueTrue = volume.NominalValue.Value;
                    double volumeVal = (double)volumeValueTrue;
                    var area = ifcRoofArea as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var areaValue = area.NominalValue;
                    Console.WriteLine("##Flache:" + areaValue + " -- Volumen:" + volumeVal);
                    object areaValueTrue = area.NominalValue.Value;
                    double areaVal = (double)areaValueTrue;//
                    ifcRoofThickness.Add(volumeVal / areaVal);
                }
                catch (System.InvalidCastException castwrong)
                {
                    Console.WriteLine(ifcRoof[i].Tag + " although being " + ifcRoof[i].GetType() + ", it does not have the Flache and Volumen Properties where it should... Therefore we choose to ignore it...");
                }
                catch (System.ArgumentOutOfRangeException notenough)
                {
                    Console.WriteLine(ifcRoof[i].Tag + " although being " + ifcRoof[i].GetType() + ", it does not have its Properties where it should... Therefore we choose to ignore it...");
                }
            }
            //create a new List which will contain all roofs' thermal trasmittances
            ElementsThermalT = new List<double> { };
           
            for (int i = 0; i < ifcRoofThickness.Count; i++)
            {
                double denominator = Rsi + ifcRoofThickness[i] / l + Rse;//we consider that all roofs are not double nor triple layered....
                double thermo = 1 / denominator;
                ElementsThermalT.Add(thermo);
                Console.WriteLine("#" + i + " thermo:" + thermo);
            }
        }
        public void CalculateIfc2x3Ramps()
        {
            var ifcRamp = xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcRamp>().ToList();

            List<double> ifcRampThickness = new List<double> { };
            for (int i = 0; i < ifcRamp.Count; i++)
            {
                Console.WriteLine("###" + ifcRamp[i].Tag + "###" + ifcRamp[i].GetType() + "###");
                try
                {
                    var ifcRampVolume = ifcRamp[i].PropertySets.ToList()[2].HasProperties.ToList()[1];//is it the right property..?-->Yes (Volumen)
                    var ifcRampArea = ifcRamp[i].PropertySets.ToList()[2].HasProperties.ToList()[0];//is it the right property..?-->Yes (Flache)
                    var volume = ifcRampVolume as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var volumeValue = volume.NominalValue;
                    object volumeValueTrue = volume.NominalValue.Value;
                    double volumeVal = (double)volumeValueTrue;
                    var area = ifcRampArea as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var areaValue = area.NominalValue;
                    Console.WriteLine("##Flache:" + areaValue + " -- Volumen:" + volumeVal);
                    object areaValueTrue = area.NominalValue.Value;
                    double areaVal = (double)areaValueTrue;//
                    ifcRampThickness.Add(volumeVal / areaVal);
                }
                catch (System.InvalidCastException castwrong)
                {
                    Console.WriteLine(ifcRamp[i].Tag + " although being " + ifcRamp[i].GetType() + ", it does not have the Flache and Volumen Properties where it should... Therefore we choose to ignore it...");
                }
                catch (System.ArgumentOutOfRangeException notenough)
                {
                    Console.WriteLine(ifcRamp[i].Tag + " although being " + ifcRamp[i].GetType() + ", it does not have its Properties where it should... Therefore we choose to ignore it...");
                }
            }
            //create a new List which will contain all Ramps' thermal trasmittances
            ElementsThermalT = new List<double> { };
            
            for (int i = 0; i < ifcRampThickness.Count; i++)
            {
                double denominator = Rsi + ifcRampThickness[i] / l + Rse;//we consider that all Ramps are not double nor triple layered....
                double thermo = 1 / denominator;
                ElementsThermalT.Add(thermo);
                Console.WriteLine("#" + i + " thermo:" + thermo);
            }
        }
        public void CalculateIfc4Ramps()
        {
            var ifcRamp = xModel.Instances.OfType<Xbim.Ifc4.SharedBldgElements.IfcRamp>().ToList();

            List<double> ifcRampThickness = new List<double> { };
            for (int i = 0; i < ifcRamp.Count; i++)
            {
                Console.WriteLine("###" + ifcRamp[i].Tag + "###" + ifcRamp[i].GetType() + "###");
                try
                {
                    var ifcRampVolume = ifcRamp[i].PropertySets.ToList()[2].HasProperties.ToList()[1];//is it the right property..?-->Yes (Volumen)
                    var ifcRampArea = ifcRamp[i].PropertySets.ToList()[2].HasProperties.ToList()[0];//is it the right property..?-->Yes (Flache)
                    var volume = ifcRampVolume as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var volumeValue = volume.NominalValue;
                    object volumeValueTrue = volume.NominalValue.Value;
                    double volumeVal = (double)volumeValueTrue;
                    var area = ifcRampArea as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var areaValue = area.NominalValue;
                    Console.WriteLine("##Flache:" + areaValue + " -- Volumen:" + volumeVal);
                    object areaValueTrue = area.NominalValue.Value;
                    double areaVal = (double)areaValueTrue;//
                    ifcRampThickness.Add(volumeVal / areaVal);
                }
                catch (System.InvalidCastException castwrong)
                {
                    Console.WriteLine(ifcRamp[i].Tag + " although being " + ifcRamp[i].GetType() + ", it does not have the Flache and Volumen Properties where it should... Therefore we choose to ignore it...");
                }
                catch (System.ArgumentOutOfRangeException notenough)
                {
                    Console.WriteLine(ifcRamp[i].Tag + " although being " + ifcRamp[i].GetType() + ", it does not have its Properties where it should... Therefore we choose to ignore it...");
                }
            }
            //create a new List which will contain all Ramps' thermal trasmittances
            ElementsThermalT = new List<double> { };
          
            for (int i = 0; i < ifcRampThickness.Count; i++)
            {
                double denominator = Rsi + ifcRampThickness[i] / l + Rse;//we consider that all Ramps are not double nor triple layered....
                double thermo = 1 / denominator;
                ElementsThermalT.Add(thermo);
                Console.WriteLine("#" + i + " thermo:" + thermo);
            }
        }
        public void CalculateIfc2x3Plates()
        {
            var ifcPlate = xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcPlate>().ToList();

            List<double> ifcPlateThickness = new List<double> { };
            for (int i = 0; i < ifcPlate.Count; i++)
            {
                Console.WriteLine("###" + ifcPlate[i].Tag + "###" + ifcPlate[i].GetType() + "###");
                try
                {
                    var ifcPlateVolume = ifcPlate[i].PropertySets.ToList()[2].HasProperties.ToList()[1];//is it the right property..?-->Yes (Volumen)
                    var ifcPlateArea = ifcPlate[i].PropertySets.ToList()[2].HasProperties.ToList()[0];//is it the right property..?-->Yes (Flache)
                    var volume = ifcPlateVolume as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var volumeValue = volume.NominalValue;
                    object volumeValueTrue = volume.NominalValue.Value;
                    double volumeVal = (double)volumeValueTrue;
                    var area = ifcPlateArea as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var areaValue = area.NominalValue;
                    Console.WriteLine("##Flache:" + areaValue + " -- Volumen:" + volumeVal);
                    object areaValueTrue = area.NominalValue.Value;
                    double areaVal = (double)areaValueTrue;//
                    ifcPlateThickness.Add(volumeVal / areaVal);
                }
                catch (System.InvalidCastException castwrong)
                {
                    Console.WriteLine(ifcPlate[i].Tag + " although being " + ifcPlate[i].GetType() + ", it does not have the Flache and Volumen Properties where it should... Therefore we choose to ignore it...");
                }
                catch (System.ArgumentOutOfRangeException notenough)
                {
                    Console.WriteLine(ifcPlate[i].Tag + " although being " + ifcPlate[i].GetType() + ", it does not have its Properties where it should... Therefore we choose to ignore it...");
                }
            }
            //create a new List which will contain all plates' thermal trasmittances
            ElementsThermalT = new List<double> { };
          
            for (int i = 0; i < ifcPlateThickness.Count; i++)
            {
                double denominator = Rsi + ifcPlateThickness[i] / l + Rse;//we consider that all plates are not double nor triple layered....
                double thermo = 1 / denominator;
                ElementsThermalT.Add(thermo);
                Console.WriteLine("#" + i + " thermo:" + thermo);
            }
        }
        public void CalculateIfc4Plates()
        {
            var ifcPlate = xModel.Instances.OfType<Xbim.Ifc4.SharedBldgElements.IfcPlate>().ToList();

            List<double> ifcPlateThickness = new List<double> { };
            for (int i = 0; i < ifcPlate.Count; i++)
            {
                Console.WriteLine("###" + ifcPlate[i].Tag + "###" + ifcPlate[i].GetType() + "###");
                try
                {
                    var ifcPlateVolume = ifcPlate[i].PropertySets.ToList()[2].HasProperties.ToList()[1];//is it the right property..?-->Yes (Volumen)
                    var ifcPlateArea = ifcPlate[i].PropertySets.ToList()[2].HasProperties.ToList()[0];//is it the right property..?-->Yes (Flache)
                    var volume = ifcPlateVolume as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var volumeValue = volume.NominalValue;
                    object volumeValueTrue = volume.NominalValue.Value;
                    double volumeVal = (double)volumeValueTrue;
                    var area = ifcPlateArea as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var areaValue = area.NominalValue;
                    Console.WriteLine("##Flache:" + areaValue + " -- Volumen:" + volumeVal);
                    object areaValueTrue = area.NominalValue.Value;
                    double areaVal = (double)areaValueTrue;//
                    ifcPlateThickness.Add(volumeVal / areaVal);
                }
                catch (System.InvalidCastException castwrong)
                {
                    Console.WriteLine(ifcPlate[i].Tag + " although being " + ifcPlate[i].GetType() + ", it does not have the Flache and Volumen Properties where it should... Therefore we choose to ignore it...");
                }
                catch (System.ArgumentOutOfRangeException notenough)
                {
                    Console.WriteLine(ifcPlate[i].Tag + " although being " + ifcPlate[i].GetType() + ", it does not have its Properties where it should... Therefore we choose to ignore it...");
                }
            }
            //create a new List which will contain all plates' thermal trasmittances
            ElementsThermalT = new List<double> { };
           
            for (int i = 0; i < ifcPlateThickness.Count; i++)
            {
                double denominator = Rsi + ifcPlateThickness[i] / l + Rse;//we consider that all plates are not double nor triple layered....
                double thermo = 1 / denominator;
                ElementsThermalT.Add(thermo);
                Console.WriteLine("#" + i + " thermo:" + thermo);
            }
        }
        public void CalculateIfc2x3Walls()
        {
            var ifcWall = xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcWall>().ToList();

            List<double> ifcWallThickness = new List<double> { };
            for (int i = 0; i < ifcWall.Count; i++)
            {
                Console.WriteLine("###" + ifcWall[i].Tag + "###" + ifcWall[i].GetType() + "###");
                try
                {
                    var ifcWallVolume = ifcWall[i].PropertySets.ToList()[2].HasProperties.ToList()[1];//is it the right property..?-->Yes (Volumen)
                    var ifcWallArea = ifcWall[i].PropertySets.ToList()[2].HasProperties.ToList()[0];//is it the right property..?-->Yes (Flache)
                    var volume = ifcWallVolume as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var volumeValue = volume.NominalValue;
                    object volumeValueTrue = volume.NominalValue.Value;
                    double volumeVal = (double)volumeValueTrue;
                    var area = ifcWallArea as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var areaValue = area.NominalValue;
                    Console.WriteLine("##Flache:" + areaValue + " -- Volumen:" + volumeVal);
                    object areaValueTrue = area.NominalValue.Value;
                    double areaVal = (double)areaValueTrue;//
                    ifcWallThickness.Add(volumeVal / areaVal);
                }
                catch (System.InvalidCastException castwrong)
                {
                    Console.WriteLine(ifcWall[i].Tag + " although being " + ifcWall[i].GetType() + ", it does not have the Flache and Volumen Properties where it should... Therefore we choose to ignore it...");
                }
                catch (System.ArgumentOutOfRangeException notenough)
                {
                    Console.WriteLine(ifcWall[i].Tag + " although being " + ifcWall[i].GetType() + ", it does not have its Properties where it should... Therefore we choose to ignore it...");
                }
            }
            //create a new List which will contain all walls' thermal trasmittances
            ElementsThermalT = new List<double> { };
         
            for (int i = 0; i < ifcWallThickness.Count; i++)
            {
                double denominator = Rsi + ifcWallThickness[i] / l + Rse;//we consider that all walls are not double nor triple layered....
                double thermo = 1 / denominator;
                ElementsThermalT.Add(thermo);
                Console.WriteLine("#" + i + " thermo:" + thermo);
            }
        }
        public void CalculateIfc4Walls()
        {
            var ifcWall = xModel.Instances.OfType<Xbim.Ifc4.SharedBldgElements.IfcWall>().ToList();

            List<double> ifcWallThickness = new List<double> { };
            for (int i = 0; i < ifcWall.Count; i++)
            {
                Console.WriteLine("###" + ifcWall[i].Tag + "###" + ifcWall[i].GetType() + "###");
                try
                {
                    var ifcWallVolume = ifcWall[i].PropertySets.ToList()[2].HasProperties.ToList()[1];//is it the right property..?-->Yes (Volumen)
                    var ifcWallArea = ifcWall[i].PropertySets.ToList()[2].HasProperties.ToList()[0];//is it the right property..?-->Yes (Flache)
                    var volume = ifcWallVolume as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var volumeValue = volume.NominalValue;
                    object volumeValueTrue = volume.NominalValue.Value;
                    double volumeVal = (double)volumeValueTrue;
                    var area = ifcWallArea as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                    var areaValue = area.NominalValue;
                    Console.WriteLine("##Flache:" + areaValue + " -- Volumen:" + volumeVal);
                    object areaValueTrue = area.NominalValue.Value;
                    double areaVal = (double)areaValueTrue;//
                    ifcWallThickness.Add(volumeVal / areaVal);
                }
                catch (System.InvalidCastException castwrong)
                {
                    Console.WriteLine(ifcWall[i].Tag + " although being " + ifcWall[i].GetType() + ", it does not have the Flache and Volumen Properties where it should... Therefore we choose to ignore it...");
                }
                catch (System.ArgumentOutOfRangeException notenough)
                {
                    Console.WriteLine(ifcWall[i].Tag + " although being " + ifcWall[i].GetType() + ", it does not have its Properties where it should... Therefore we choose to ignore it...");
                }
            }
            //create a new List which will contain all walls' thermal trasmittances
            ElementsThermalT = new List<double> { };
            
            for (int i = 0; i < ifcWallThickness.Count; i++)
            {
                double denominator = Rsi + ifcWallThickness[i] / l + Rse;//we consider that all walls are not double nor triple layered....
                double thermo = 1 / denominator;
                ElementsThermalT.Add(thermo);
                Console.WriteLine("#" + i + " thermo:" + thermo);
            }
        }

        public void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {   //we dont have 2 do anything when textBox is changed
        }

        public void selection_changed1(object sender, SelectionChangedEventArgs e)
        {
            //select Type of ifcElements
            //load their IfcGloballyUniqueIds in InfoIfc2x3 or InfoIfc4
            ComboBox comboBox = ((EnergyCalculatorControl)ControlElements[0]).ComboBoxFilter;
            if (comboBox == null) return;
            if (comboBox.SelectedIndex == 0) return;//in index 0 it is only a label we already said that
            if (IfcVersionType.Name == "ModelInfoIFC2x3")
            {
                List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> searchIDs = ((ComboboxItemFilter)(comboBox.SelectedItem)).ValueIfcGloballyUniqueIds2x3;
                InfoIfc2x3.ElementIds.Clear();
                foreach (var item in searchIDs)
                {
                    InfoIfc2x3.AddElementIds(item);
                }

            }
            else if (IfcVersionType.Name == "ModelInfoIFC4")
            {
                List<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId> searchIDs = ((ComboboxItemFilter)(comboBox.SelectedItem)).ValueIfcGloballyUniqueIds4;
                InfoIfc4.ElementIds.Clear();
                foreach (var item in searchIDs)
                {
                    InfoIfc4.AddElementIds(item);
                }
            }

            TTExists = ((ComboboxItemFilter)comboBox.SelectedItem).TTAvailable;
            Label label_TT_exists = ((EnergyCalculatorControl)ControlElements[0]).Label_TT_Exists;
            
            if (TTExists)
            {
                label_TT_exists.Content = "TT is available";

                sumExistinTTs((ComboboxItemFilter)comboBox.SelectedItem);
                Console.WriteLine("Total Thermo is " + sumTT);
                Label label_Uj_in = ((EnergyCalculatorControl)ControlElements[0]).Label_Uj;
                label_Uj_in.Content = "Uj = " + System.Math.Round(sumTT,3);

                ((EnergyCalculatorControl)ControlElements[0]).TextBox_l.Visibility = Visibility.Hidden;
                ((EnergyCalculatorControl)ControlElements[0]).ComboBoxRse.Visibility = Visibility.Hidden;
                ((EnergyCalculatorControl)ControlElements[0]).Button_Uj.Visibility = Visibility.Hidden;
            }
            else
            {
                label_TT_exists.Content = "TT NOT available";
                //we want to calculate here the sum of TTs cause we are not sure if the right values for λ, R have yet been selected
                //the calculation will take place when the button is pressed
                Label label_Uj_in = ((EnergyCalculatorControl)ControlElements[0]).Label_Uj;
                label_Uj_in.Content = "Uj = ";

                ((EnergyCalculatorControl)ControlElements[0]).TextBox_l.Visibility = Visibility.Visible;
                ((EnergyCalculatorControl)ControlElements[0]).ComboBoxRse.Visibility = Visibility.Visible;
                ((EnergyCalculatorControl)ControlElements[0]).Button_Uj.Visibility = Visibility.Visible;
            }
        }

        public void selection_changed2(object sender, SelectionChangedEventArgs e)
        {   
            Calculate1();
        }

        public void button_Click1(object sender, RoutedEventArgs e)
        {
            if (TTExists)
            {
                Console.WriteLine("No need 2 press the Calculate button since the sum is calculated by itself !!");
                return;
            }

            Label label_Uj_in = ((EnergyCalculatorControl)ControlElements[0]).Label_Uj;

            ComboBox comboBox = ((EnergyCalculatorControl)ControlElements[0]).ComboBoxFilter;
            if (comboBox == null) return;

            if (((ComboboxItemFilter)comboBox.SelectedItem).Text == "Select type of ifcElements")
            {
                Console.WriteLine("Select type of ifcElements first..!!");
                return;
            }
            ComboBox comboBox_Rse = ((EnergyCalculatorControl)ControlElements[0]).ComboBoxRse;
            if (((ComboboxItemLocal)comboBox_Rse.SelectedItem).Text == "Select Rse")
            {
                Console.WriteLine("Select Rse value first..!!");
                return;
            }

            Calculate1();

            //since we got up 2 here, TT wont be available...
            if (IfcVersionType.Name == "ModelInfoIFC2x3")
            {
                if (((ComboboxItemFilter)comboBox.SelectedItem).Text.Equals("ifccolumn"))
                    CalculateIfc2x3Columns();
                else if (((ComboboxItemFilter)comboBox.SelectedItem).Text.Equals("ifcdoor"))
                    CalculateIfc2x3Doors();
                else if (((ComboboxItemFilter)comboBox.SelectedItem).Text.Equals("ifccurtainwall"))
                    CalculateIfc2x3CurtainWalls();
                else if (((ComboboxItemFilter)comboBox.SelectedItem).Text.Equals("ifcbeam"))
                    CalculateIfc2x3Beams();
                else if (((ComboboxItemFilter)comboBox.SelectedItem).Text.Equals("ifcslab"))
                    CalculateIfc2x3Slabs();
                else if (((ComboboxItemFilter)comboBox.SelectedItem).Text.Equals("ifcwindow"))
                    CalculateIfc2x3Windows();
                else if (((ComboboxItemFilter)comboBox.SelectedItem).Text.Equals("ifcstair"))
                    CalculateIfc2x3Stairs();
                else if (((ComboboxItemFilter)comboBox.SelectedItem).Text.Equals("ifcroof"))
                    CalculateIfc2x3Roofs();
                else if (((ComboboxItemFilter)comboBox.SelectedItem).Text.Equals("ifcramp"))
                    CalculateIfc2x3Ramps();
                else if (((ComboboxItemFilter)comboBox.SelectedItem).Text.Equals("ifcplate"))
                    CalculateIfc2x3Plates();
                else if (((ComboboxItemFilter)comboBox.SelectedItem).Text.Equals("ifcwall"))
                    CalculateIfc2x3Walls();

            }
            else if (IfcVersionType.Name == "ModelInfoIFC4")
            {
                if (((ComboboxItemFilter)comboBox.SelectedItem).Text.Equals("ifccolumn"))
                    CalculateIfc4Columns();
                else if (((ComboboxItemFilter)comboBox.SelectedItem).Text.Equals("ifcdoor"))
                    CalculateIfc4Doors();
                else if (((ComboboxItemFilter)comboBox.SelectedItem).Text.Equals("ifccurtainwall"))
                    CalculateIfc4CurtainWalls();
                else if (((ComboboxItemFilter)comboBox.SelectedItem).Text.Equals("ifcbeam"))
                    CalculateIfc4Beams();
                else if (((ComboboxItemFilter)comboBox.SelectedItem).Text.Equals("ifcslab"))
                    CalculateIfc4Slabs();
                else if (((ComboboxItemFilter)comboBox.SelectedItem).Text.Equals("ifcwindow"))
                    CalculateIfc4Windows();
                else if (((ComboboxItemFilter)comboBox.SelectedItem).Text.Equals("ifcstair"))
                    CalculateIfc4Stairs();
                else if (((ComboboxItemFilter)comboBox.SelectedItem).Text.Equals("ifcroof"))
                    CalculateIfc4Roofs();
                else if (((ComboboxItemFilter)comboBox.SelectedItem).Text.Equals("ifcramp"))
                    CalculateIfc4Ramps();
                else if (((ComboboxItemFilter)comboBox.SelectedItem).Text.Equals("ifcplate"))
                    CalculateIfc4Plates();
                else if (((ComboboxItemFilter)comboBox.SelectedItem).Text.Equals("ifcwall"))
                    CalculateIfc4Walls();
            }

            sumTT = 0;
            foreach (var thermo in ElementsThermalT)
            {
                sumTT += thermo;
            }
            Console.WriteLine("Total Thermo is calculated in " + sumTT);

            label_Uj_in.Content = "Uj = " + System.Math.Round(sumTT,3);
        }

        public void sumExistinTTs(ComboboxItemFilter selectedItem)
        {
            sumTT = 0;

            if (IfcVersionType.Name == "ModelInfoIFC2x3")
            {
                foreach (var item in xModel.Instances.OfType<Xbim.Ifc2x3.Kernel.IfcProduct>())
                {
                    if (InfoIfc2x3.ElementIds.Contains(item.GlobalId))
                        sumTT += GetTTifExists(item);//since we are here there s not even one TT not being available
                }
            }
            else if (IfcVersionType.Name == "ModelInfoIFC4")
            {
                foreach (var item in xModel.Instances.OfType<Xbim.Ifc4.Kernel.IfcProduct>())
                {
                    if (InfoIfc4.ElementIds.Contains(item.GlobalId))
                        sumTT += GetTTifExists(item);//since we are here there s not even one TT not being available
                }
            }
        }

        public double GetTTifExists(Xbim.Ifc2x3.Kernel.IfcProduct ifcProduct)
        {
            //bool found = false;
            var propertySets = ifcProduct.PropertySets.ToList() as List<Xbim.Ifc2x3.Kernel.IfcPropertySet>;
            int ii = 0;
            while (ii < propertySets.Count)
            {
                var onepropertySet = propertySets[ii].HasProperties.ToList();
                int jj = 0;
                while (jj < onepropertySet.Count)
                {
                    if (onepropertySet[jj].Name == "ThermalTransmittance")
                    {
                        return (double)((Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue)onepropertySet[jj]).NominalValue.Value;
                    }
                    jj++;
                }
                ii++;
            }
            return -1;//TT property was not found in this element...
        }

        public double GetTTifExists(Xbim.Ifc4.Kernel.IfcProduct ifcProduct)
        {
            //bool found = false;
            var propertySets = ifcProduct.PropertySets.ToList();
            int ii = 0;
            while (ii < propertySets.Count)
            {
                var onepropertySet = propertySets[ii].HasProperties.ToList();
                int jj = 0;
                while (jj < onepropertySet.Count)
                {
                    if (onepropertySet[jj].Name == "ThermalTransmittance")
                    {
                        return (double)((Xbim.Ifc4.PropertyResource.IfcPropertySingleValue)onepropertySet[jj]).NominalValue.Value;
                    }
                    jj++;
                }
                ii++;
            }
            return -1;//TT property was not found in this element...
        }

        public class ComboboxItemLocal
        {
            public string Text { get; set; }
            public object Value { get; set; }

            public override string ToString()
            { return Text; }

        }
        public class ComboboxItemFilter
        {
            public string Text { get; set; }
            public List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> ValueIfcGloballyUniqueIds2x3 { get; set; }
            public List<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId> ValueIfcGloballyUniqueIds4 { get; set; }
            public bool TTAvailable { get; set; }

            public override string ToString()
            { return Text; }

        }

        public override Node Clone()
        {
            return new EnergyCalculatorNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}
