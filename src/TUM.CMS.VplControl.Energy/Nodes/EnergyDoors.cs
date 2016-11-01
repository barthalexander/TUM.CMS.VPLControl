using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.IFC.Utilities;
using Xbim.IO;
using System.Windows;
using System.Windows.Controls;
using Xbim.Ifc;


namespace TUM.CMS.VplControl.Energy.Nodes
{
    public class EnergyDoors : Node
    {
        public const double Rsi = 0.13;
        public ComboboxItem Rse_1 = new ComboboxItem() { Text = "In direct transition wall outside air", Value = 0.04 };
        public ComboboxItem Rse_2 = new ComboboxItem() { Text = "A rear-ventilated façade", Value = 0.08 };
        public ComboboxItem Rse_3 = new ComboboxItem() { Text = "In the transition to ground", Value = 0.00 };

        public double l;
        public double Rse;
        public IfcStore xModel;
        public List<double> DoorsThermalT;

        public EnergyDoors(Core.VplControl hostCanvas) : base(hostCanvas)
        {
            AddInputPortToNode("text", typeof(string));//the ifc parsed file
            AddOutputPortToNode("text", typeof(List<double>));//List with doors thermal transmittance

            var label_l = new Label { Content = "Τhermal conductivity λ" };
            AddControlToNode(label_l);  //#0
            // var textBox_l = new TextBox { MinWidth = 300, MaxWidth = 500, IsHitTestVisible = false };
            var textBox_l = new TextBox { };
            textBox_l.TextChanged += textBox_TextChanged;//load doors' properties n calculate thermal transmittances whenever this textbox's value is changed...
            AddControlToNode(textBox_l);//#1
            var label_Rse = new Label { Content = "Rse" };
            AddControlToNode(label_Rse); //#2
            var comboBox_Rse = new ComboBox { };
            comboBox_Rse.Items.Add(Rse_1);
            comboBox_Rse.Items.Add(Rse_2);
            comboBox_Rse.Items.Add(Rse_3);//Ra comboBox has these 3 items
            comboBox_Rse.SelectedIndex = 0;
            comboBox_Rse.SelectionChanged += selection_changed;//load doors' properties n calculate thermal transmittances whenever this checkbox's selection is changed...
            AddControlToNode(comboBox_Rse);  //#3
            //label for Uj total thermal transm for all the doors elements
            var label_Uj = new Label { Content = "Uj = " };
            AddControlToNode(label_Uj);     //#4
            var button_Uj = new Button { Content = "Calculate the heat transfer coefficient of Doors" };
            button_Uj.Click += button_Click1;
            AddControlToNode(button_Uj);    //#5
        }

        public override void Calculate()
        { }

        public /*override*/ void Calculate1()
        {
            var textBox_l_in = ControlElements[1] as TextBox;
            var comboBox_in = ControlElements[3] as ComboBox;
            if (textBox_l_in == null || comboBox_in == null)
                return;     //if λ or Rse do not have values yet, we cannot do the calculation...

            l = Double.Parse(textBox_l_in.Text.Replace(",", "."));
            Console.WriteLine("l = " + l);
            ComboboxItem Selection = (ComboboxItem)(comboBox_in.SelectedItem);
            Console.WriteLine("Combobox's selection is " + Selection.Text + " that is " + Selection.Value);
            Rse = (Double)Selection.Value;
            ///
            /// ifc door thickness calculation, saved in ifcDoorThickness list, prepare for energy node
            ///
            
       

            var modelid = ((ModelInfoIFC2x3)(InputPorts[0].Data)).ModelId;
            if (modelid == null)
                return;
            xModel = DataController.Instance.GetModel(modelid);
            var ifcDoor = xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcDoor>().ToList();
            Console.WriteLine("ifcDoor has " + ifcDoor.Count + " elements");

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
            DoorsThermalT = new List<double> { };
            Console.WriteLine("Total of " + ifcDoorThickness.Count + " door-elements with their Properties in place...");
            for (int i = 0; i < ifcDoorThickness.Count; i++)
            {
                double denominator = Rsi + ifcDoorThickness[i] / l + Rse;//we consider that all doors are not double nor triple layered....
                double thermo = 1 / denominator;
                DoorsThermalT.Add(thermo);
                Console.WriteLine("#" + i + " thermo:" + thermo);
            }


            //ModelInfo OutputInfoIfc2x3 = new ModelInfo(((ModelInfo)(InputPorts[0].Data)).ModelId);
            /* foreach (var item in WallsThermalT)
             {
                 OutputInfoIfc2x3.AddElementIds(item);//
             }
             OutputPorts[0].Data = OutputInfoIfc2x3; */
            OutputPorts[0].Data = DoorsThermalT;
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Calculate1();
        }

        private void selection_changed(object sender, SelectionChangedEventArgs e)
        {
            Calculate1();
        }

        public void button_Click1(object sender, RoutedEventArgs e)
        {
            double thermo_sum = 0;
            foreach (var thermo in DoorsThermalT)
            {
                thermo_sum += thermo;
            }
            Console.WriteLine("Total Thermo is calculated in " + thermo_sum);
            var label_Uj_in = ControlElements[4] as Label;
            label_Uj_in.Content = "Uj = " + thermo_sum;
        }

        public class ComboboxItem
        {
            public string Text { get; set; }
            public object Value { get; set; }

            public override string ToString()
            { return Text; }

        }

        public override Node Clone()
        {
            return new EnergyNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}
