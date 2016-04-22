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

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcTreeNode : Node
    {
        private readonly TextBox _textBox;
        public XbimModel xModel;

        private XbimTreeview treeview;
        // private DynamicProductSelectionControl productSelectionControl;
        public IfcTreeNode(Core.VplControl hostCanvas) : base(hostCanvas)
        {
            IsResizeable = true;
            
            treeview = new XbimTreeview
            {
                MinWidth = 620,
                MinHeight = 100,
            };

            AddInputPortToNode("Object", typeof(object));

            AddControlToNode(treeview);

            // Init
           // productSelectionControl = new DynamicProductSelectionControl();
           // AddControlToNode(productSelectionControl);

            // Init 3DController

            // AddOutputPortToNode("IFCFile", typeof(object));

            
        }

       

        public override void Calculate()
        {
            var file = InputPorts[0].Data.ToString();
            if (file == null) return;
            ReadIfc(file);

            // Ifc3DViewer.Model = _ifcReader.xModel
            // var m = new XbimModel();
            // m.Open("temp.xbim");
            // drawingControl3D.LoadGeometry(m);
            // drawingControl3D.ShowAll();
        }

        public override Node Clone()
        {
            return new IfcTreeNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }

        public void ReadIfc(string filepath)
        {
            Random zufall = new Random();
            int number = zufall.Next(1, 100);

            var path = Path.GetTempPath();
            xModel = new XbimModel();
            xModel.CreateFrom(filepath, path + "temp_tree"+ number +".xbim");
            xModel.Close();


            var res = xModel.Open(path + "temp_tree" + number + ".xbim", XbimDBAccess.ReadWrite);

            if (res == false)
            {
                var err = xModel.Validate(TextWriter.Null, ValidationFlags.All);
                MessageBox.Show("ERROR in reading process!");
            }

            // xModel.Close();


            // drawingControl3D.ShowAll();

            treeview.Model = xModel;
           //  productSelectionControl.Model = xModel;

            // xModel.Close();

            // drawingControl3D.LoadGeometry(xModel);
            // drawingControl3D.ShowAll();
        }

    }
}