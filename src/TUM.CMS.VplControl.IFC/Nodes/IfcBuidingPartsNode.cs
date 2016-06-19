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
using TUM.CMS.VplControl.IFC.Utilities;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcBuildingPartsNode : Node
    {
        private XbimModel xModel;

        public IfcBuildingPartsNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {

            AddInputPortToNode("IfcFile", typeof(object));




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
       
        public override void Calculate()
        {
            var modelid = ((ModelInfo)(InputPorts[0].Data)).ModelId;
            if (modelid == null) return;
            xModel = DataController.Instance.GetModel(modelid, true);


            var scrollViewer = ControlElements[0] as ScrollViewer;
            if (scrollViewer == null) return;
            var textBlock = scrollViewer.Content as TextBlock;
            if (textBlock == null) return;
            textBlock.Text = "";

           
        

            List<Xbim.Ifc2x3.SharedBldgElements.IfcWall> walls = xModel.IfcProducts.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcWall>().ToList();
            List<Xbim.Ifc2x3.SharedBldgElements.IfcWindow> windows = xModel.IfcProducts.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcWindow>().ToList();
            List<Xbim.Ifc2x3.SharedBldgElements.IfcDoor> doors = xModel.IfcProducts.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcDoor>().ToList();
            
            List<Xbim.Ifc2x3.ProductExtension.IfcSpace> spaces = xModel.IfcProducts.OfType<Xbim.Ifc2x3.ProductExtension.IfcSpace>().ToList();
            textBlock.Text += "Walls: \n\n";
            foreach (var wall in walls)
            {
                textBlock.Text += wall.Name + "\t";

                foreach (var relation in wall.Decomposes)
                {
                    textBlock.Text += string.Format("\tStorey = {0}", relation.RelatingObject.Name) + " \t";
                }
                textBlock.Text += "\n";
            }
            textBlock.Text += "\nWindows: \n\n";

            foreach (var window in windows)
            {
                textBlock.Text += window.Name + "\t" + window.Description + "\t";

                foreach (var relation in window.Decomposes)
                {
                    textBlock.Text += string.Format("\tStorey = {0}", relation.RelatingObject.Name) + " \t";
                }
                textBlock.Text += "\n";
            }
            textBlock.Text += "\nDoors: \n\n";

            foreach (var door in doors)
            {
                textBlock.Text += door.Name + "\t" + door.GetMaterial() + "\t";

                foreach (var relation in door.Decomposes)
                {
                    textBlock.Text += string.Format("\tStorey = {0}", relation.RelatingObject.Name) + " \t";
                }
                textBlock.Text += "\n";
            }






        }
        
        public override Node Clone()
        {
            return new IfcBuildingPartsNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}