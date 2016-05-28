using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using TUM.CMS.VplControl.Core;
using Xbim.IO;
using Xbim.ModelGeometry.Scene;
using Xbim.Presentation;
using Xbim.XbimExtensions;
using XbimGeometry.Interfaces;
using System.Linq;
using System.Windows.Media;

namespace TUM.CMS.VplControl.Test.Nodes
{
    public class IfcParseNode : Node
    {
        public XbimModel xModel;
        public IfcParseNode(Core.VplControl hostCanvas) : base(hostCanvas)
        {
            AddInputPortToNode("Test", typeof(string));

            AddOutputPortToNode("Test", typeof(string));

            var label = new Label
            {
                Content = "IFC File Rading",
                Width = 100,
                FontSize = 15,
                HorizontalContentAlignment = HorizontalAlignment.Center
            };
            var textBlock = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                FontSize = 10,
                Padding = new Thickness(5),
                IsHitTestVisible = false
            };
            AddControlToNode(label);
            AddControlToNode(textBlock);


        }

       
        public override void Calculate()
        {
            var file = InputPorts[0].Data.ToString();
            if(file != null && File.Exists(file))
            {
                Random zufall = new Random();
                int number = zufall.Next(1, 1000);

                var path = Path.GetTempPath();
                xModel = new XbimModel();
                xModel.CreateFrom(file, path + "temp_reader" + number + ".xbim");
                xModel.Close();

                OutputPorts[0].Data = path + "temp_reader" + number + ".xbim";
                var textBlock = ControlElements[1] as TextBlock;
                textBlock.Background = Brushes.White;
                textBlock.Text = "File is Valid!";
            }
            else
            {
                var textBlock = ControlElements[1] as TextBlock;
                textBlock.Background = Brushes.Red;
                textBlock.Text = "Please select a true File!";
            }
            


        }

        public override Node Clone()
        {
            return new IfcParseNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }



    }
}