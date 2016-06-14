using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.IFC.Utilities;
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
                MinHeight = 100
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
            var modelid = ((ModelInfo) (InputPorts[0].Data)).ModelId;
            var elementids = ((ModelInfo)(InputPorts[0].Data)).ElementIds;
            treeview.Model = DataController.Instance.GetModel(modelid);
        }

        public override Node Clone()
        {
            return new IfcTreeNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }

    }
}