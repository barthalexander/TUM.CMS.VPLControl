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
using TUM.CMS.VplControl.IFC.Utilities;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcReaderNode : Node
    {
        private readonly TextBox _textBox;
        public XbimModel xModel;
        private DrawingControl3D drawingControl3D;
        private DynamicProductSelectionControl productSelectionControl;
        public IfcReaderNode(Core.VplControl hostCanvas) : base(hostCanvas)
        {
            IsResizeable = true;


            AddInputPortToNode("Object", typeof(object));


            // Init
            // productSelectionControl = new DynamicProductSelectionControl();
            // AddControlToNode(productSelectionControl);

            // Init 3DController
            drawingControl3D = new DrawingControl3D
            {
                MinWidth    = 520,
                MinHeight   = 520
            };

            AddControlToNode(drawingControl3D);

            // AddOutputPortToNode("IFCFile", typeof(object));

           
        }

       
        public override void Calculate()
        {
            var modelid = ((ModelInfo)(InputPorts[0].Data)).ModelId;

            if (modelid == null) return;
            ReadIfc(modelid);

            // Ifc3DViewer.Model = _ifcReader.xModel
            // var m = new XbimModel();
            // m.Open("temp.xbim");
            // drawingControl3D.LoadGeometry(m);
            // drawingControl3D.ShowAll();
        }

        public override Node Clone()
        {
            return new IfcReaderNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }

        public void ReadIfc(string modelid)
        {
           
            xModel = DataController.Instance.GetModel(modelid, true);

            // xModel.Close();

            try
            {

                var context = new Xbim3DModelContext(xModel);
                context.CreateContext(XbimGeometryType.PolyhedronBinary);
                drawingControl3D.Model = xModel;
                drawingControl3D.ShowAll();
                drawingControl3D.ReloadModel();
                drawingControl3D.LoadGeometry(xModel);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            // drawingControl3D.ShowAll();

            // productSelectionControl.Model = xModel;

            // xModel.Close();

            // drawingControl3D.LoadGeometry(xModel);
            // drawingControl3D.ShowAll();
        }

    }
}