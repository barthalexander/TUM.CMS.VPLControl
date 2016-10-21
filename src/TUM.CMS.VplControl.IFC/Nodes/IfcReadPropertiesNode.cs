//Documentation
//This Node is written for potential future use of modify node. (perhaps) 
//This Node can be used after IfcFilterNode.
//After filtering some specific ifc products(eg. IfcWall), 
//you can use this node to choose one of them(eg. one Wall) and see his owned properties and corresponding values.
//___Yini Wang
using System;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using TUM.CMS.VplControl.Core;
using System.Linq;
using System.Collections;
using System.Text.RegularExpressions;

namespace TUM.CMS.VplControl.IFC.Nodes
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
            comboBox.SelectionChanged += comboBox_selectionChanged;

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

            
            comboBox.Items.Clear();
            
            
           // char[] somechar=new char[] {' '};
           // s.CopyTo(31, somechar, 17, 7);
            textBlock.Text = "you have chosen "+s+", now choose one of the "+s+" from comboBox to see its properties.";
            if (s == "Walls")
            {
               
                for (int i = 0; i < list.Count; i++)
                {
                    var one = list[i] as Xbim.Ifc2x3.SharedBldgElements.IfcWall;
                   
                    comboBox.Items.Add(one.ToString());
                }
                
            }
            if (s == "Beams")
            {
                
                for (int i = 0; i < list.Count; i++)
                {
                    var one = list[i] as Xbim.Ifc2x3.SharedBldgElements.IfcBeam;
                   
                    comboBox.Items.Add(one.ToString());
                }
            }

            if (s == "Columns")
            {
                for (int i = 0; i < list.Count; i++)
                {
                    
                    var one = list[i] as Xbim.Ifc2x3.SharedBldgElements.IfcColumn;
                    comboBox.Items.Add(one.ToString());
                }
            }
            if (s == "Doors")
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var one = list[i] as Xbim.Ifc2x3.SharedBldgElements.IfcDoor; 
                    comboBox.Items.Add(one.ToString());
                }
            }
            if (s == "Plates")
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var one = list[i] as Xbim.Ifc2x3.SharedBldgElements.IfcPlate;
                    comboBox.Items.Add(one.ToString());
                }
            }
            if (s == "Slabs")
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var one = list[i] as Xbim.Ifc2x3.SharedBldgElements.IfcSlab;
                    comboBox.Items.Add(one.ToString());
                }
            }
            if (s == "Stairs")
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var one = list[i] as Xbim.Ifc2x3.SharedBldgElements.IfcStair;
                    comboBox.Items.Add(one.ToString());
                }
            }
            if (s == "CurtainWalls")
            {

                for (int i = 0; i < list.Count; i++)
                {
                    var one = list[i] as Xbim.Ifc2x3.SharedBldgElements.IfcCurtainWall;

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
                if (list.Count==0) return s;

                string p = list[0].GetType().ToString();
                Match m = Regex.Match(p, "Wall");
                if(m.Success)
                {  
                    s= "Walls";
                }
                m = Regex.Match(p, "Beam");
                if(m.Success )
                {
                    s= "Beams";
                }
                m = Regex.Match(p, "Column");
                if (m.Success)
                {
                    s = "Columns";
                }
                m = Regex.Match(p, "Door");
                if (m.Success)
                {
                    s = "Doors";
                }
                m = Regex.Match(p, "Plate");
                if (m.Success)
                {
                    s = "Plates";
                }
                m = Regex.Match(p, "Slab");
                if (m.Success)
                {
                    s = "Slabs";
                }
                m = Regex.Match(p, "Stair");
                if (m.Success)
                {
                    s = "Stairs";
                }
                m = Regex.Match(p, "CurtainWall");
                if (m.Success)
                {
                    s = "CurtainWalls";
                }

            }
            return s;
        }
          private void comboBox_selectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            var scrollViewer = ControlElements[0] as ScrollViewer;
            if (scrollViewer == null) return;
            var textBlock = scrollViewer.Content as TextBlock;
            if (textBlock == null) return;
            var comboBox = ControlElements[2] as ComboBox;
            if (comboBox == null) return;
            if(comboBox .SelectedIndex ==-1) return;

            var list = InputPorts[0].Data as IList;
            if (list == null) return;

            string s = inputDataConvertion();
           

            if (s == "Walls")
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
            if (s == "Beams")
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
            if (s == "Columns")
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
            if (s == "Doors")
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
            if (s == "Plates")
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
            if (s == "Slabs")
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
            if (s == "Stairs")
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
            if (s == "CurtainWalls")
            {
                var one = list[comboBox.SelectedIndex] as Xbim.Ifc2x3.SharedBldgElements.IfcCurtainWall;
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