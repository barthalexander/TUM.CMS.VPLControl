//This Node is written for potential future use of modify node. 
//It works to some degree but still need to be refined.
//___Yini Wang
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
using Xbim.Ifc2x3.Kernel;
using Xbim.Ifc2x3.ProductExtension;
using Xbim.Ifc2x3.Extensions;
using System.Collections;
using System.Text.RegularExpressions;

namespace TUM.CMS.VplControl.Test.Nodes
{
    public class IfcReadPropertiesNode : Node
    {
        public IfcReadPropertiesNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {

            AddInputPortToNode("IfcProducts", typeof(object));

            AddOutputPortToNode("IfcProducts", typeof(object));


            var textBlock = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                FontSize = 14,
                Padding = new Thickness(5),
                IsHitTestVisible = false
            };
            var scrollViewer = new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                MinWidth = 600,
                MinHeight = 30,
                MaxWidth = 1000,
                MaxHeight = 400,
                CanContentScroll = true,
                Content = textBlock
                //IsHitTestVisible = false
            };

            var comboBox = new ComboBox { };
            comboBox.SelectionChanged += comboBox_SelectionChanged;

            AddControlToNode(scrollViewer);
            AddControlToNode(new Label { Content = "IfcReadPropertiesNode" });
            AddControlToNode(comboBox);
        }

        public override void Calculate()
        {
            var scrollViewer = ControlElements[0] as ScrollViewer;
            if (scrollViewer == null) return;
            var textBlock = scrollViewer.Content as TextBlock;
            if (textBlock == null) return;
            var comboBox = ControlElements[2] as ComboBox;
            if (comboBox == null) return;

            string s=inputDataConvertion();

            var list = InputPorts[0].Data as IList;
            if (list == null) return;

            if (s == "Xbim.Ifc2x3.SharedBldgElements.IfcWall")
            {
                comboBox.Items.Clear();
                for (int i = 0; i < list.Count; i++)
                {
                    var one = list[i] as Xbim.Ifc2x3.SharedBldgElements.IfcWall;
                   
                    comboBox.Items.Add(one.ToString());
                }
            }
            if (s == "Xbim.Ifc2x3.SharedBldgElements.IfcBeam")
            {
                comboBox.Items.Clear();
                for (int i = 0; i < list.Count; i++)
                {
                    var one = list[i] as Xbim.Ifc2x3.SharedBldgElements.IfcBeam;
                   
                    comboBox.Items.Add(one.ToString());
                }
            }

            if (s == "Xbim.Ifc2x3.SharedBldgElements.IfcColumn")
            {
                for (int i = 0; i < list.Count; i++)
                {
                    comboBox.Items.Clear();
                    var one = list[i] as Xbim.Ifc2x3.SharedBldgElements.IfcColumn;
                    comboBox.Items.Add(one.ToString());
                }
            }
            if (s == "Xbim.Ifc2x3.SharedBldgElements.IfcDoor")
            {
                for (int i = 0; i < list.Count; i++)
                {
                    comboBox.Items.Clear();
                    var one = list[i] as Xbim.Ifc2x3.SharedBldgElements.IfcDoor; 
                    comboBox.Items.Add(one.ToString());
                }
            }
            if (s == "Xbim.Ifc2x3.SharedBldgElements.IfcPlate")
            {
                for (int i = 0; i < list.Count; i++)
                {
                    comboBox.Items.Clear();
                    var one = list[i] as Xbim.Ifc2x3.SharedBldgElements.IfcPlate;
                    comboBox.Items.Add(one.ToString());
                }
            }
            if (s == "Xbim.Ifc2x3.SharedBldgElements.IfcSlab")
            {
                for (int i = 0; i < list.Count; i++)
                {
                    comboBox.Items.Clear();
                    var one = list[i] as Xbim.Ifc2x3.SharedBldgElements.IfcSlab;
                    comboBox.Items.Add(one.ToString());
                }
            }
            if (s == "Xbim.Ifc2x3.SharedBldgElements.IfcStair")
            {
                comboBox.Items.Clear();
                for (int i = 0; i < list.Count; i++)
                {
                    var one = list[i] as Xbim.Ifc2x3.SharedBldgElements.IfcStair;
                    comboBox.Items.Add(one.ToString());
                }
            }
            if (s == "")
            { textBlock.Text = "not found!"; }

        }


        private string inputDataConvertion()
        {

            Type t= InputPorts[0].Data.GetType();
            string s="";

            if (t.IsGenericType)
            {
                var list = InputPorts[0].Data as IList;
                if (list == null) return s;

                string p = list[0].GetType().ToString();
                Match m = Regex.Match(p, "Wall");
                if(m.Success)
                {  
                    s= "Xbim.Ifc2x3.SharedBldgElements.IfcWall";
                }
                m = Regex.Match(p, "Beam");
                if(m.Success )
                {
                    s= "Xbim.Ifc2x3.SharedBldgElements.IfcBeam";
                }
                m = Regex.Match(p, "Column");
                if (m.Success)
                {
                    s = "Xbim.Ifc2x3.SharedBldgElements.IfcColumn";
                }
                m = Regex.Match(p, "Door");
                if (m.Success)
                {
                    s = "Xbim.Ifc2x3.SharedBldgElements.IfcDoor";
                }
                m = Regex.Match(p, "Plate");
                if (m.Success)
                {
                    s = "Xbim.Ifc2x3.SharedBldgElements.IfcPlate";
                }
                m = Regex.Match(p, "Slab");
                if (m.Success)
                {
                    s = "Xbim.Ifc2x3.SharedBldgElements.IfcSlab";
                }
                m = Regex.Match(p, "Stair");
                if (m.Success)
                {
                    s = "Xbim.Ifc2x3.SharedBldgElements.IfcStair";
                }

            }
            return s;
        }
          private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            var scrollViewer = ControlElements[0] as ScrollViewer;
            if (scrollViewer == null) return;
            var textBlock = scrollViewer.Content as TextBlock;
            if (textBlock == null) return;
            var comboBox = ControlElements[2] as ComboBox;
            if (comboBox == null) return;

            var list = InputPorts[0].Data as IList;
            if (list == null) return;

            string s = inputDataConvertion();
           

            if (s == "Xbim.Ifc2x3.SharedBldgElements.IfcWall")
            { var one = list[comboBox.SelectedIndex] as Xbim.Ifc2x3.SharedBldgElements.IfcWall;
                textBlock.Text = one.Name.ToString() + " has \n\r";
                var ifcwallpropertyset = one.PropertySets.ToList();
                var ifchasproperties = ifcwallpropertyset[0].HasProperties.ToList();
                for (int k = 0; k < ifchasproperties.Count; k++)
                {
                    var ifcpropertyname = ifchasproperties[k].Name;
                    var ifcpropertyvalue = ifchasproperties[k];
                    textBlock.Text += ifcpropertyname.ToString() + "  "+ifcpropertyvalue.ToString() + "\n\r";
                }
            }
            if (s == "Xbim.Ifc2x3.SharedBldgElements.IfcBeam")
            {
                var one = list[comboBox.SelectedIndex] as Xbim.Ifc2x3.SharedBldgElements.IfcBeam;
                textBlock.Text = one.Name.ToString() + " has \n\r";
                var ifcwallpropertyset = one.PropertySets.ToList();
                var ifchasproperties = ifcwallpropertyset[0].HasProperties.ToList();
                for (int k = 0; k < ifchasproperties.Count; k++)
                {
                    var ifcpropertyname = ifchasproperties[k].Name;
                    var ifcpropertyvalue = ifchasproperties[k];
                    textBlock.Text += ifcpropertyname.ToString() + "  " + ifcpropertyvalue.ToString() + "\n\r";
                }
            }
            if (s == "Xbim.Ifc2x3.SharedBldgElements.IfcColumn")
            {
                var one = list[comboBox.SelectedIndex] as Xbim.Ifc2x3.SharedBldgElements.IfcColumn;
                textBlock.Text = one.Name.ToString() + " has \n\r";
                var ifcwallpropertyset = one.PropertySets.ToList();
                var ifchasproperties = ifcwallpropertyset[0].HasProperties.ToList();
                for (int k = 0; k < ifchasproperties.Count; k++)
                {
                    var ifcpropertyname = ifchasproperties[k].Name;
                    var ifcpropertyvalue = ifchasproperties[k];
                    textBlock.Text += ifcpropertyname.ToString() + "  " + ifcpropertyvalue.ToString() + "\n\r";
                }
            }
            if (s == "Xbim.Ifc2x3.SharedBldgElements.IfcDoor")
            {
                var one = list[comboBox.SelectedIndex] as Xbim.Ifc2x3.SharedBldgElements.IfcDoor;
                textBlock.Text = one.Name.ToString() + " has \n\r";
                var ifcwallpropertyset = one.PropertySets.ToList();
                var ifchasproperties = ifcwallpropertyset[0].HasProperties.ToList();
                for (int k = 0; k < ifchasproperties.Count; k++)
                {
                    var ifcpropertyname = ifchasproperties[k].Name;
                    var ifcpropertyvalue = ifchasproperties[k];
                    textBlock.Text += ifcpropertyname.ToString() + "  " + ifcpropertyvalue.ToString() + "\n\r";
                }
            }
            if (s == "Xbim.Ifc2x3.SharedBldgElements.IfcPlate")
            {
                var one = list[comboBox.SelectedIndex] as Xbim.Ifc2x3.SharedBldgElements.IfcPlate;
                textBlock.Text = one.Name.ToString() + " has \n\r";
                var ifcwallpropertyset = one.PropertySets.ToList();
                var ifchasproperties = ifcwallpropertyset[0].HasProperties.ToList();
                for (int k = 0; k < ifchasproperties.Count; k++)
                {
                    var ifcpropertyname = ifchasproperties[k].Name;
                    var ifcpropertyvalue = ifchasproperties[k];
                    textBlock.Text += ifcpropertyname.ToString() + "  " + ifcpropertyvalue.ToString() + "\n\r";
                }
            }
            if (s == "Xbim.Ifc2x3.SharedBldgElements.IfcSlab")
            {
                var one = list[comboBox.SelectedIndex] as Xbim.Ifc2x3.SharedBldgElements.IfcSlab;
                textBlock.Text = one.Name.ToString() + " has \n\r";
                var ifcwallpropertyset = one.PropertySets.ToList();
                var ifchasproperties = ifcwallpropertyset[0].HasProperties.ToList();
                for (int k = 0; k < ifchasproperties.Count; k++)
                {
                    var ifcpropertyname = ifchasproperties[k].Name;
                    var ifcpropertyvalue = ifchasproperties[k];
                    textBlock.Text += ifcpropertyname.ToString() + "  " + ifcpropertyvalue.ToString() + "\n\r";
                }
            }
            if (s == "Xbim.Ifc2x3.SharedBldgElements.IfcStair")
            {
                var one = list[comboBox.SelectedIndex] as Xbim.Ifc2x3.SharedBldgElements.IfcStair;
                textBlock.Text = one.Name.ToString() + " has \n\r";
                var ifcwallpropertyset = one.PropertySets.ToList();
                var ifchasproperties = ifcwallpropertyset[0].HasProperties.ToList();
                for (int k = 0; k < ifchasproperties.Count; k++)
                {
                    var ifcpropertyname = ifchasproperties[k].Name;
                    var ifcpropertyvalue = ifchasproperties[k];
                    textBlock.Text += ifcpropertyname.ToString() + "  " + ifcpropertyvalue.ToString() + "\n\r";
                }
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
            return new IfcReadPropertiesNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}