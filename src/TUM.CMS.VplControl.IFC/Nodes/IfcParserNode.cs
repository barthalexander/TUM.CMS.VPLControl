using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using TUM.CMS.VplControl.Core;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Media;
using TUM.CMS.VplControl.IFC.Utilities;
using Xbim.Common;
using Xbim.Common.Metadata;
using Xbim.Common.Step21;
using Xbim.Ifc;
using Xbim.Presentation;
using TUM.CMS.VplControl.IFC.Controls;


namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcParserNode : Node
    {
        public IfcStore xModel;
        public IfcParserNode(Core.VplControl hostCanvas) : base(hostCanvas)
        {
            AddInputPortToNode("Test", typeof(string));

            AddOutputPortToNode("GUID", typeof(object));

            TitleTextboxControl titleTextboxControl = new TitleTextboxControl();
            Label label = new Label();
            label = titleTextboxControl.Title;
            label.Content = "IfC Parser";
            
            AddControlToNode(titleTextboxControl);

        }

        /// <summary>
        /// Cleaning the DataController
        /// 
        /// When Reading Files, they are stored in the DataController.
        /// To prevent a big DataController, the user can delete all stored Models except the actual Model
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        
        private BackgroundWorker _worker;
        /// <summary>
        /// Reads the file String and looks if its existing.
        /// Create a new xModel inside the Temp Folder with a Random Number in the FileName
        /// Safe the Model in an Dictonary (DataController)
        /// 
        /// Output is the GUID (FilePath)
        /// </summary>
        public override void Calculate()
        {
            if (InputPorts[0].Data == null)
                return;

            var file = InputPorts[0].Data.ToString();
            if (file != "" && File.Exists(file))
            {
                _worker = new BackgroundWorker();
                _worker.DoWork += new DoWorkEventHandler(worker_DoWork);
                _worker.RunWorkerAsync(file);
                _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            }
            else
            {
                var titleTextboxControl = ControlElements[0] as TitleTextboxControl;
                TextBlock textBlock = new TextBlock();
                textBlock = titleTextboxControl.TextBlock;
                textBlock.Background = Brushes.Red;
                textBlock.Text = "Please select a true File!";
            }



        }
        
        /// <summary>
        /// Background Worker
        /// 
        /// Create xBIM File and Add it to the new DataController.
        /// The xBIM File is therefor stored in a Database
        /// 
        /// The ModelId (FilePath) and the ElementList are stored in the ModelInfo Class
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Value File is the FilePath</param>
        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var file = e.Argument.ToString();
            Random zufall = new Random();
            int number = zufall.Next(1, 1000);

            string result = Path.GetTempPath();
            string copyFile = result + "copy" + number + ".ifc";
            while (File.Exists(copyFile))
            {
                number = zufall.Next(1, 1000);
                copyFile = result + "copy" + number + ".ifc";
            }

            
            File.Copy(file, copyFile);
            using (xModel = IfcStore.Open(file))
            {
                if (xModel.IfcSchemaVersion == IfcSchemaVersion.Ifc2X3)
                {
                    ModelInfoIFC2x3 modelInfo = new ModelInfoIFC2x3(copyFile);
                    foreach (var item in xModel.Instances.OfType<Xbim.Ifc2x3.Kernel.IfcProduct>())
                    {
                        modelInfo.AddElementIds(item.GlobalId);
                    }
                    e.Result = modelInfo;
                }
                else
                {
                    ModelInfoIFC4 modelInfo = new ModelInfoIFC4(copyFile);
                    foreach (var item in xModel.Instances.OfType<Xbim.Ifc4.Kernel.IfcProduct>())
                    {
                        modelInfo.AddElementIds(item.GlobalId);

                    }
                    e.Result = modelInfo;
                }
            }


            xModel.Close();

            DataController.Instance.AddModel(copyFile, xModel);

           
        }

        /// <summary>
        /// Its important to split the Creation of the xBIM File and the OutputPort
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Result is the modelInfo Class</param>
        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                OutputPorts[0].Data = e.Result;
                var titleTextboxControl = ControlElements[0] as TitleTextboxControl;
                TextBlock textBlock = new TextBlock();
                textBlock = titleTextboxControl.TextBlock;
                textBlock.Background = Brushes.White;
                textBlock.Text = "File is Valid!";
            }
            catch (Exception exception)
            {
                Console.WriteLine("An error occurred: '{0}'", exception);
            }
            DataController.Instance.CloseModel(xModel);
            
        }



            
       

        public override Node Clone()
        {
            return new IfcParserNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }



    }
}