using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Controls;
using TUM.CMS.VplControl.Core;
using System.Windows.Media;
using TUM.CMS.VplControl.IFC.Utilities;
using Xbim.Common.Step21;
using Xbim.Ifc;
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

        private BackgroundWorker _worker;
        /// <summary>
        /// Reads the file String and looks if its existing.
        /// Create a new xModel inside the Temp Folder with a Random Number in the FileName
        /// Safe the Model in an Dictonary (DataController)
        /// 
        /// Output is the ModelInfoClass
        /// </summary>
        public override void Calculate()
        {
            if (InputPorts[0].Data == null)
                return;

            var file = InputPorts[0].Data.ToString();
            if (file != "" && File.Exists(file) && Path.GetExtension(file).ToUpper().ToString() == ".IFC")
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
                textBlock.Text = "Please select a true IFC File!";
            }
        }
        
        /// <summary>
        /// Background Worker
        /// 
        /// Open IFC File and read all elements.
        /// Add all elements to the ModelInfoClass
        /// 
        /// Safes the new xModel to the DataController
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Value File is the FilePath</param>
        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Dublicate the File for multi access

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
        /// Output the ModelInfoClass
        /// 
        /// Prints the result in a textBlock
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