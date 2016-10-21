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
using Xbim.Ifc;


namespace TUM.CMS.VplControl.Energy.Nodes
{
    public class EnergyNode : Node
    {
        public const double Rsi = 0.13;
        public ComboboxItem Rse_1 = new ComboboxItem() { Text = "In direct transition wall outside air", Value = 0.04 };
        public ComboboxItem Rse_2 = new ComboboxItem() { Text = "A rear-ventilated façade", Value = 0.08 };
        public ComboboxItem Rse_3 = new ComboboxItem() { Text = "In the transition to ground", Value = 0.00 };
        public ComboboxItem Uf_1 = new ComboboxItem() { Text = "Metallic" };
        public ComboboxItem Uf_2 = new ComboboxItem() { Text = "Synthetic" };
        public ComboboxItem Uf_3 = new ComboboxItem() { Text = "Wooden" };
        public ComboboxItem th1_1 = new ComboboxItem() { Text = "7.0", Value = 7.0 };
        public ComboboxItem th1_2 = new ComboboxItem() { Text = "1.0-4.0", Value = 1.0 }; //what s the right value?
        public ComboboxItem th2_1 = new ComboboxItem() { Text = "2.8", Value = 2.8 };
        public ComboboxItem th2_2 = new ComboboxItem() { Text = "2.2", Value = 2.2 };
        public ComboboxItem th2_3 = new ComboboxItem() { Text = "2.0", Value = 2.0 };
        public ComboboxItem th2_4 = new ComboboxItem() { Text = "1.0-2.0", Value = 1.0 }; //what s the right value?
        public ComboboxItem th3_1 = new ComboboxItem() { Text = "2.4", Value = 2.4 };
        public ComboboxItem th3_2 = new ComboboxItem() { Text = "2.0", Value = 2.0 };
        public ComboboxItem th3_3 = new ComboboxItem() { Text = "1.7", Value = 1.7 };
        public ComboboxItem th3_4 = new ComboboxItem() { Text = "1.5", Value = 1.5 };
        public ComboboxItem Y1 = new ComboboxItem() { Text = "Metallic - no thermal break" };
        public ComboboxItem Y2 = new ComboboxItem() { Text = "Metallic - with thermal break" };
        public ComboboxItem Y3 = new ComboboxItem() { Text = "Synthetic" };
        public ComboboxItem Y4 = new ComboboxItem() { Text = "Wooden" };
        public ComboboxItem y1_1 = new ComboboxItem() { Text = "0.02", Value = 0.02 };
        public ComboboxItem y1_2 = new ComboboxItem() { Text = "0.05", Value = 0.05 };
        public ComboboxItem y2_1 = new ComboboxItem() { Text = "0.08", Value = 0.08 };
        public ComboboxItem y2_2 = new ComboboxItem() { Text = "0.11", Value = 0.11 };
        public ComboboxItem y3_1 = new ComboboxItem() { Text = "0.06", Value = 0.06 };
        public ComboboxItem y3_2 = new ComboboxItem() { Text = "0.08", Value = 0.08 };
        public ComboboxItem y4_1 = new ComboboxItem() { Text = "0.06", Value = 0.06 };
        public ComboboxItem y4_2 = new ComboboxItem() { Text = "0.08", Value = 0.08 };
        public double l;
        public double Rse;
        public double d_sum;
        public IfcStore xModel;
        public EnergyNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {
            AddInputPortToNode("text", typeof(object));
            AddOutputPortToNode("text", typeof(string));

            var label_l = new Label { Content = "Τhermal conductivity λ" };
            AddControlToNode(label_l);   //0

            //var textBox = new TextBox { MinWidth = 300, MaxWidth = 500, IsHitTestVisible = false };

            var textBox_l = new TextBox { };
            textBox_l.TextChanged += textBox_TextChanged;
            AddControlToNode(textBox_l);  //1
            var label_Rse = new Label { Content = "Rse" };
            AddControlToNode(label_Rse); //2
            var comboBox_Rse = new ComboBox { };
            comboBox_Rse.Items.Add(Rse_1);
            comboBox_Rse.Items.Add(Rse_2);
            comboBox_Rse.Items.Add(Rse_3); //Ra comboBox has these 3 items
            comboBox_Rse.SelectedIndex = 0;
            AddControlToNode(comboBox_Rse);

            //label for coefficient Uj heat transfer coefficient result for all the wall elements
            var label_Uj = new Label { Content = "Uj = " };  //4
            AddControlToNode(label_Uj);
            //button for the calculation
            var button_Uj = new Button { Content = "Calculate the heat transfer coefficient" };
            button_Uj.Click += button_Click1;
            AddControlToNode(button_Uj);

            var label_Uf = new Label { Content = "Uf" };  // Thermal tranfer coefficient frame windows
            AddControlToNode(label_Uf);     //#8
            //comboBox 4 the material
            var comboBox_Uf_first = new ComboBox { };
            comboBox_Uf_first.Items.Add(Uf_1);
            comboBox_Uf_first.Items.Add(Uf_2);
            comboBox_Uf_first.Items.Add(Uf_3);//3 starting options 4 Uf_first comboBox
            comboBox_Uf_first.SelectedIndex = 0;
            comboBox_Uf_first.SelectionChanged += select_OptionChanged;
            AddControlToNode(comboBox_Uf_first);  //#9

            //comboBox 4 the actual thermo factor's value
            var comboBox_Uf_second = new ComboBox { };
            comboBox_Uf_second.Items.Add(th1_1);//
            comboBox_Uf_second.Items.Add(th1_2);//these are going to change whenever comboBox_Uf_first's Selection is changed
            comboBox_Uf_second.SelectedIndex = 0;
            AddControlToNode(comboBox_Uf_second);  //#10

            var label_Af = new Label { Content = "Af" };
            AddControlToNode(label_Af);     //#11
            var textBox_f = new TextBox { };
            textBox_f.TextChanged += textBox_f_TextChanged;//chose to load windows' properties whenever this textbox's value is changed...
            AddControlToNode(textBox_f);    //#12

            var label_Ug = new Label { Content = "Ug" };  // Thermal tranfer coefficient glass
            AddControlToNode(label_Ug);     //#13
            var textBox_g = new TextBox { };
            AddControlToNode(textBox_g);    //#14

            var label_Yg = new Label { Content = "Ψg" };
            AddControlToNode(label_Yg);     //#15
            var comboBox_Yg_first = new ComboBox { };
            comboBox_Yg_first.Items.Add(Y1);
            comboBox_Yg_first.Items.Add(Y2);
            comboBox_Yg_first.Items.Add(Y3);
            comboBox_Yg_first.Items.Add(Y4);
            comboBox_Yg_first.SelectedIndex = 0;
            comboBox_Yg_first.SelectionChanged += select_Option_g_Changed;
            AddControlToNode(comboBox_Yg_first);    //#16
            var comboBox_Yg_second = new ComboBox { };
            comboBox_Yg_second.Items.Add(y1_1);
            comboBox_Yg_second.Items.Add(y1_2);
            AddControlToNode(comboBox_Yg_second);   //#17


        }
        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Calculate1();
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
            //have 2 implement abstract class Node's Calculate() method...whether we want 2 or not
        }

        public void Calculate1()
        {
            ///
            /// ifc wall thickness calculation, saved in ifcWallThickness list, prepare for energy node
            ///
            var modelid = ((ModelInfo)(InputPorts[0].Data)).ModelId;
            if (modelid == null) return;
            xModel = DataController.Instance.GetModel(modelid);
            var ifcWall = xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcWall>().ToList();
            Console.WriteLine("ifcWall has " + ifcWall.Count + " elements");//12
            List<double> ifcWallThickness = new List<double> { };
            for (int i = 0; i < ifcWall.Count; i++)
            {
                var ifcWallVolume = ifcWall[i].PropertySets.ToList()[1].HasProperties.ToList()[2];
                var ifcWallArea = ifcWall[i].PropertySets.ToList()[1].HasProperties.ToList()[0];
                var volume = ifcWallVolume as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                var volumeValue = volume.NominalValue;
                object volumeValueTrue = volume.NominalValue.Value;
                double volumeVal = (double)volumeValueTrue;
                var area = ifcWallArea as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                var areaValue = area.NominalValue;
                object areaValueTrue = area.NominalValue.Value;
                double areaVal = (double)areaValueTrue;
                ifcWallThickness.Add(volumeVal / areaVal);

            }
            //Having one value for λ, we calculate all thicknesses' sum:
            d_sum = 0;
            for (int i = 0; i < ifcWallThickness.Count; i++)
                d_sum += ifcWallThickness[i];
            Console.WriteLine("d_sum is " + d_sum);
        }

        public void button_Click1(object sender, RoutedEventArgs e)
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
            Rse = (Double)Selection.Value;

            double denominator = Rsi + d_sum / l + Rse;
            double thermal = 1 / denominator;
            Console.WriteLine("Ηeat transfer coefficient is calculated in " + thermal);
            var label_Uj_in = ControlElements[4] as Label;
            label_Uj_in.Content = "Uj = " + thermal;
        }


        public void textBox_f_TextChanged(object sender, TextChangedEventArgs e)
        {
            Calculate2();
        }

        public void Calculate2()
        {
            var modelid = ((ModelInfo)(InputPorts[0].Data)).ModelId;
            if (modelid == null)
                return;
            xModel = DataController.Instance.GetModel(modelid);
            var ifcWin = xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcWindow>().ToList();
            Console.WriteLine("ifcWin has " + ifcWin.Count + " elements");//
            List<width_heigth> ifcWinWidthHeight = new List<width_heigth> { };



        }


        public void select_OptionChanged(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Combobox Uf1_first's selection has changed...");
            var comboBox_Uf_first = ControlElements[9] as ComboBox;
            if (comboBox_Uf_first == null)
                return;
            ComboboxItem Selection = (ComboboxItem)(comboBox_Uf_first.SelectedItem);
            Console.WriteLine("Combobox's selection is " + Selection.Text);
            var comboBox_Uf_second = ControlElements[10] as ComboBox;
            comboBox_Uf_second.Items.Clear();
            if (Selection.Text.Equals("Metallic"))
            {
                comboBox_Uf_second.Items.Add(th1_1); comboBox_Uf_second.Items.Add(th1_2);
            }
            else if (Selection.Text.Equals("Synthetic"))
            {
                comboBox_Uf_second.Items.Add(th2_1); comboBox_Uf_second.Items.Add(th2_2);
                comboBox_Uf_second.Items.Add(th2_3); comboBox_Uf_second.Items.Add(th2_4);
            }
            else if (Selection.Text.Equals("Wooden"))
            {
                comboBox_Uf_second.Items.Add(th3_1); comboBox_Uf_second.Items.Add(th3_2);
                comboBox_Uf_second.Items.Add(th3_3); comboBox_Uf_second.Items.Add(th3_4);
            }
            comboBox_Uf_second.SelectedIndex = 0;
        }


        public void select_Option_g_Changed(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Combobox Ug_first's selection has changed...");
            var comboBox_Ug_first = ControlElements[16] as ComboBox;
            if (comboBox_Ug_first == null)
                return;
            ComboboxItem Selection = (ComboboxItem)(comboBox_Ug_first.SelectedItem);
            Console.WriteLine("Combobox's selection is " + Selection.Text);
            var comboBox_Ug_second = ControlElements[17] as ComboBox;
            comboBox_Ug_second.Items.Clear();
            if (Selection.Text.Equals("Metallic - no thermal break"))
            {
                comboBox_Ug_second.Items.Add(y1_1); comboBox_Ug_second.Items.Add(y1_2);
            }
            else if (Selection.Text.Equals("Metallic - with thermal break"))
            {
                comboBox_Ug_second.Items.Add(y2_1); comboBox_Ug_second.Items.Add(y2_2);
            }
            else if (Selection.Text.Equals("Synthetic"))
            {
                comboBox_Ug_second.Items.Add(y3_1); comboBox_Ug_second.Items.Add(y3_2);
            }
            else if (Selection.Text.Equals("Wooden"))
            {
                comboBox_Ug_second.Items.Add(y4_1); comboBox_Ug_second.Items.Add(y4_2);
            }
            comboBox_Ug_second.SelectedIndex = 0;
        }



        public void button_Click2(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Button2 Clicked...");


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

    struct width_heigth
    {
        public double width;
        public double height;
    }

}