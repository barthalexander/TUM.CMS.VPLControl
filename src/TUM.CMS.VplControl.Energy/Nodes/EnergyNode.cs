using System;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.IFC.Utilities;
using System.Collections.Generic;
using Xbim.IO;
using Xbim.XbimExtensions;
using System.Linq;
using System.Collections;
using System.Text.RegularExpressions;
using System.Reflection;
using Xbim.Ifc2x3.ProductExtension;
using System.IO;


namespace TUM.CMS.VplControl.Energy.Nodes
{
    public class EnergyNode : Node
    {
        public const double Rsi = 0.13;
        public double l;
        public double Ra;
        public double d_sum;
        public XbimModel xModel;
        public EnergyNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {
            AddInputPortToNode("text", typeof(string));
            AddOutputPortToNode("text", typeof(string));

            var label_l = new Label { Content = "Τhermal conductivity λ" };
            AddControlToNode(label_l);

            //var textBox = new TextBox { MinWidth = 300, MaxWidth = 500, IsHitTestVisible = false };

            var textBox_l = new TextBox { };
            textBox_l.TextChanged += textBox_TextChanged;
            AddControlToNode(textBox_l);
            var label_Rse = new Label { Content = "Rse" };
            AddControlToNode(label_Rse);
            var comboBox_Rse = new ComboBox { };
            ComboboxItem Rse_1 = new ComboboxItem() { Text = "In direct transition wall outside air", Value = 0.04 };
            ComboboxItem Rse_2 = new ComboboxItem() { Text = "A rear-ventilated façade", Value = 0.08 };
            ComboboxItem Rse_3 = new ComboboxItem() { Text = "In the transition to ground", Value = 0.00 };
            comboBox_Rse.Items.Add(Rse_1);
            comboBox_Rse.Items.Add(Rse_2);
            comboBox_Rse.Items.Add(Rse_3); //Ra comboBox has these 3 items
            comboBox_Rse.SelectedIndex = 0;
            AddControlToNode(comboBox_Rse);

            //label for coefficient Uj heat transfer coefficient result for all the wall elements
            var label_Uj = new Label { Content = "Uj = " };
            AddControlToNode(label_Uj);
            //button for the calculation
            var button_Uj = new Button { Content = "Calculate the heat transfer coefficient" };
            button_Uj.Click += button_Click;
            AddControlToNode(button_Uj);
        }
        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Calculate();
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
            ///
            /// ifc wall thickness calculation, saved in ifcWallThickness list, prepare for energy node
            ///
            var modelid = ((ModelInfo)(InputPorts[0].Data)).ModelId;
            if (modelid == null) return;
            xModel = DataController.Instance.GetModel(modelid);
            var ifcWall = xModel.IfcProducts.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcWall>().ToList();
            Console.WriteLine("ifcWall has " + ifcWall.Count + " elements");//12
            Console.WriteLine("############################");
            List<double> ifcWallThickness = new List<double> { };
            for (int i = 0; i < ifcWall.Count; i++)
            {
                var ifcWallVolume = ifcWall[i].PropertySets.ToList()[1].HasProperties.ToList()[2];
                var ifcWallArea = ifcWall[i].PropertySets.ToList()[1].HasProperties.ToList()[0];
                var volume = ifcWallVolume as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                var volumeValue = volume.NominalValue as Xbim.XbimExtensions.SelectTypes.IfcValue;
                object volumeValueTrue = volume.NominalValue.Value;
                double volumeVal = (double)volumeValueTrue;
                var area = ifcWallArea as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                var areaValue = area.NominalValue as Xbim.XbimExtensions.SelectTypes.IfcValue;
                object areaValueTrue = area.NominalValue.Value;
                double areaVal = (double)areaValueTrue;
                ifcWallThickness.Add(volumeVal / areaVal);

            }
            //since we consider having one value for λ, we simply need to calculate all thicknesses' sum:
            d_sum = 0;
            for (int i = 0; i < ifcWallThickness.Count; i++)
                d_sum += ifcWallThickness[i];
            Console.WriteLine("d_sum is " + d_sum);
        }

        public void button_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Button Clicked...");
            var textBox_l_in = ControlElements[1] as TextBox;
            var comboBox_in = ControlElements[3] as ComboBox;
            if (textBox_l_in == null || comboBox_in == null)
                return;     //if λ or Ra do not have values yet, we cannot do the calculation...

            l = Double.Parse(textBox_l_in.Text);
            Console.WriteLine("l = " + l);
            ComboboxItem Selection = (ComboboxItem)(comboBox_in.SelectedItem);
            Console.WriteLine("Combobox's selection is " + Selection.Text + " that is " + Selection.Value);
            Ra = (Double)Selection.Value;

            double denominator = Rsi + d_sum / l + Ra;
            double thermal = 1 / denominator;
            Console.WriteLine("Ηeat transfer coefficient is calculated in " + thermal);
            var label_Uj_in = ControlElements[4] as Label;
            label_Uj_in.Content = "Uj = " + thermal;
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
            return new EnergyNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}