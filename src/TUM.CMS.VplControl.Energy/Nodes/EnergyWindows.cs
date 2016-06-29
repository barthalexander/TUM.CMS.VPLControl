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


namespace TUM.CMS.VplControl.Energy.Nodes
{
    public class EnergyWindows : Node
    {
        public const double Rsi = 0.13;
        public ComboboxItem Rse_1 = new ComboboxItem() { Text = "In direct transition wall outside air", Value = 0.04 };
        public ComboboxItem Rse_2 = new ComboboxItem() { Text = "A rear-ventilated façade", Value = 0.08 };
        public ComboboxItem Rse_3 = new ComboboxItem() { Text = "In the transition to ground", Value = 0.00 };

        public double l;
        public double Rse;
        public XbimModel xModel;
        public List<double> WinsThermalT;

        public EnergyWindows(Core.VplControl hostCanvas) : base(hostCanvas)
        {
            AddInputPortToNode("text", typeof(string));//the ifc parsed file
            AddOutputPortToNode("text", typeof(List<double>));//List with windows thermal transmittance

            var label_l = new Label { Content = "Τhermal conductivity λ" };
            AddControlToNode(label_l);  //#0
            // var textBox_l = new TextBox { MinWidth = 300, MaxWidth = 500, IsHitTestVisible = false };
            var textBox_l = new TextBox { };
            textBox_l.TextChanged += textBox_TextChanged;//load windows' properties n calculate thermal transmittances whenever this textbox's value is changed...
            AddControlToNode(textBox_l);//#1
            var label_Rse = new Label { Content = "Rse" };
            AddControlToNode(label_Rse); //#2
            var comboBox_Rse = new ComboBox { };
            comboBox_Rse.Items.Add(Rse_1);
            comboBox_Rse.Items.Add(Rse_2);
            comboBox_Rse.Items.Add(Rse_3);//Ra comboBox has these 3 items
            comboBox_Rse.SelectedIndex = 0;
            comboBox_Rse.SelectionChanged += selection_changed;//load windows' properties n calculate thermal transmittances whenever this checkbox's selection is changed...
            AddControlToNode(comboBox_Rse);  //#3
            //label for Uj total thermal transm for all the windows elements
            var label_Uj = new Label { Content = "Uj = " };
            AddControlToNode(label_Uj);     //#4
            var button_Uj = new Button { Content = "Calculate the heat transfer coefficient of Windows" };
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

            l = Double.Parse(textBox_l_in.Text);
            Console.WriteLine("l = " + l);
            ComboboxItem Selection = (ComboboxItem)(comboBox_in.SelectedItem);
            Console.WriteLine("Combobox's selection is " + Selection.Text + " that is " + Selection.Value);
            Rse = (Double)Selection.Value;
            ///
            /// ifc win thickness calculation, saved in ifcWinThickness list, prepare for energy node
            ///
            var modelid = ((ModelInfo)(InputPorts[0].Data)).ModelId;
            if (modelid == null)
                return;
            xModel = DataController.Instance.GetModel(modelid);
            var ifcWin = xModel.IfcProducts.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcWindow>().ToList();
            Console.WriteLine("ifcWindow has " + ifcWin.Count + " elements");//how many windows?--> 16 windows
       
            List<double> ifcWinThickness = new List<double> { };
            for (int i = 0; i < ifcWin.Count; i++)
            {
                var ifcWinVolume = ifcWin[i].PropertySets.ToList()[1].HasProperties.ToList()[2];//is it the right property..?
                var ifcWinArea = ifcWin[i].PropertySets.ToList()[1].HasProperties.ToList()[0];//is it the right property..?
                var volume = ifcWinVolume as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                var volumeValue = volume.NominalValue as Xbim.XbimExtensions.SelectTypes.IfcValue;
                object volumeValueTrue = volume.NominalValue.Value;
                double volumeVal = (double)volumeValueTrue;
                var area = ifcWinArea as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                var areaValue = area.NominalValue as Xbim.XbimExtensions.SelectTypes.IfcValue;
                object areaValueTrue = area.NominalValue.Value;
                double areaVal = (double)areaValueTrue;
                ifcWinThickness.Add(volumeVal / areaVal);
            }
            //create a new List which will contain all windows' thermal trasmittances
            WinsThermalT = new List<double> { };
            Console.WriteLine("Total of " + ifcWinThickness.Count + " windows elements.");
            for (int i = 0; i < ifcWinThickness.Count; i++)
            {
                double denominator = Rsi + ifcWinThickness[i] / l + Rse;//we consider that all windows are not double nor triple layered....
                double thermo = 1 / denominator;
                WinsThermalT.Add(thermo);
                Console.WriteLine("#" + i + " thermo:" + thermo);
            }


            //ModelInfo outputInfo = new ModelInfo(((ModelInfo)(InputPorts[0].Data)).ModelId);
            /* foreach (var item in WallsThermalT)
             {
                 outputInfo.AddElementIds(item);//
             }
             OutputPorts[0].Data = outputInfo; */
            OutputPorts[0].Data = WinsThermalT;
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
            foreach (var thermo in WinsThermalT)
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
