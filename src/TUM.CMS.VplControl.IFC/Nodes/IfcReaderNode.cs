using System.ComponentModel;
using TUM.CMS.VplControl.Core;
using Xbim.IO;
using Xbim.ModelGeometry.Scene;
using Xbim.Presentation;
using TUM.CMS.VplControl.IFC.Utilities;
using Xbim.Common.Geometry;
using Xbim.Ifc;
using Xbim.Ifc2x3.IO;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcReaderNode : Node
    {
        public IfcStore xModel;
        private DrawingControl3D drawingControl3D;
        public IfcReaderNode(Core.VplControl hostCanvas) : base(hostCanvas)
        {
            IsResizeable = true;

            AddInputPortToNode("Object", typeof(object));


            // Init 3DController
            drawingControl3D = new DrawingControl3D
            {
                MinWidth    = 520,
                MinHeight   = 520
            };

            AddControlToNode(drawingControl3D);

            // AddOutputPortToNode("IFCFile", typeof(object));

           
        }


        /// <summary>
        /// Running the Plotting Process for an IFC File.
        /// Its important to run it with a Background Worker for no interruption.
        /// 
        /// ModelId is stored in the Class ModelInfo. 
        /// Addiontional: ElementList is also stored in the Class ModelInfo
        /// </summary>
        public override void Calculate()
        {
            var modelid = ((ModelInfo)(InputPorts[0].Data)).ModelId;

            if (modelid == null) return;

            BackgroundWorker worker = new BackgroundWorker();

            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerAsync(modelid);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

        }


        /// <summary>
        /// Background Worker for the Transformation of the xModel
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">ModelId: The Guid is the Filepath</param>
        public void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var modelid = e.Argument.ToString();
            xModel = DataController.Instance.GetModel(modelid, true);

            var context = new Xbim3DModelContext(xModel);
            context.CreateContext();

            e.Result = xModel;
        }
        /// <summary>
        /// Transformatíon of the xModel must be completed to start the drawing control. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            xModel = (IfcStore) e.Result;
            drawingControl3D.Model = xModel;
            drawingControl3D.ShowAll();
            drawingControl3D.ReloadModel();
            drawingControl3D.LoadGeometry(xModel);
        }

        public override Node Clone()
        {
            return new IfcReaderNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }

    }
}