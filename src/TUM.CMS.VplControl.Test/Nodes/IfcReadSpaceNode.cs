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

namespace TUM.CMS.VplControl.Test.Nodes
{
    public class IfcReadSpacesNode : Node
    {
        public IfcReadSpacesNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {

            AddInputPortToNode("IfcFile", typeof(string));

            AddOutputPortToNode("FilteredProducts", typeof(object));


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


            AddControlToNode(scrollViewer);
            AddControlToNode(new Label { Content = "IfcProductsNode" });
        }
        //private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    Calculate();
        //}
        public override void Calculate()
        {
           
            XbimModel xModel = new XbimModel();
           
            var res = xModel.Open(InputPorts[0].Data.ToString(), XbimDBAccess.ReadWrite);

            var scrollViewer = ControlElements[0] as ScrollViewer;
            if (scrollViewer == null) return;
            var textBlock = scrollViewer.Content as TextBlock;
            if (textBlock == null) return;
            textBlock.Text = "";

            
            List<Xbim.Ifc2x3.ProductExtension.IfcSpace> spaces = xModel.IfcProducts.OfType<Xbim.Ifc2x3.ProductExtension.IfcSpace>().ToList();
            foreach (var space in spaces)
            {
                textBlock.Text += space.Name + "\t" + space.LongName + "\t";

                Console.WriteLine(space.Name);
                Console.WriteLine(space.LongName);

                foreach (var relation in space.Decomposes)
                {
                    textBlock.Text += string.Format("\tStorey = {0}", relation.RelatingObject.Name) + " \t";
                    Console.WriteLine(string.Format("\tStorey = {0}", relation.RelatingObject.Name));
                    Xbim.Ifc2x3.QuantityResource.IfcPhysicalSimpleQuantity area = space.GetElementPhysicalSimpleQuantity("GSA Space Areas", "GSA BIM Area");
                    Xbim.Ifc2x3.QuantityResource.IfcQuantityArea areaMeasure = area as Xbim.Ifc2x3.QuantityResource.IfcQuantityArea;
                    if(area != null) {
                        
                        double areaInMetre = areaMeasure.AreaValue / 1000000;
                        textBlock.Text += areaInMetre;
                    }


                }
                textBlock.Text += "\n";
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
            return new IfcReadSpaceNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}