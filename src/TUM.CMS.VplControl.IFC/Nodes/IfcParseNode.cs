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
using TUM.CMS.VplControl.IFC.Utilities;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcParseNode : Node
    {
        public XbimModel xModel;
        public IfcParseNode(Core.VplControl hostCanvas) : base(hostCanvas)
        {
            AddInputPortToNode("Test", typeof(string));

            AddOutputPortToNode("GUID", typeof(string));

            var label = new Label
            {
                Content = "IFC File Reading",
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

        /// <summary>
        /// Reads the file String and looks if its existing.
        /// Create a new xModel inside the Temp Folder with a Random Number in the FileName
        /// Safe the Model in an Dictonary (DataController)
        /// 
        /// Output is the GUID (FilePath)
        /// </summary>
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

                var fileString = path + "temp_reader" + number + ".xbim";

                DataController.Instance.AddModel(fileString, xModel);
                



                ModelInfo modelInfo = new ModelInfo(fileString);
                xModel = DataController.Instance.GetModel(fileString);
                List<Xbim.Ifc2x3.ProductExtension.IfcBuildingElement> elements = xModel.Instances.OfType<Xbim.Ifc2x3.ProductExtension.IfcBuildingElement>().ToList();
                foreach (var element in elements)
                {
                    modelInfo.AddElementIds(element.GlobalId); 
                }
                
                OutputPorts[0].Data = modelInfo;

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