using System;
using System.Collections.Generic;
using System.ComponentModel;
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

            var button = new Button { Content = "Clean Database" };
            button.Click += button_Click;

            AddControlToNode(label);
            AddControlToNode(textBlock);
            AddControlToNode(button);

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            var models = DataController.Instance.modelStorage.ToList();
            if (models.Count > 1)
            {
                for (int i = 0; i < models.Count - 1; i++)
                {
                    DataController.Instance.RemoveModel(models[i].Key);
                }
            }

        }
        
        private BackgroundWorker worker;
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
            if (file != "" && File.Exists(file))
            {
                worker = new BackgroundWorker();
                worker.DoWork += new DoWorkEventHandler(worker_DoWork);
                worker.RunWorkerAsync(file);
                worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            }
            else
            {
                var textBlock = ControlElements[1] as TextBlock;
                textBlock.Background = Brushes.Red;
                textBlock.Text = "Please select a true File!";
            }



        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var file = e.Argument.ToString();
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
            e.Result = modelInfo;
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            OutputPorts[0].Data = e.Result;
            var textBlock = ControlElements[1] as TextBlock;
            textBlock.Background = Brushes.White;
            textBlock.Text = "File is Valid!";
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