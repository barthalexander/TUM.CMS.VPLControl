using System;
using System.Windows.Controls;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.IFC.Utilities;
using Xbim.Ifc;
using Xbim.Presentation;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcTreeNode : Node
    {
        private readonly TextBox _textBox;
        public IfcStore xModel;
        private XbimTreeview treeview;

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
        }

        /// <summary>
        /// Create a Tree of the given IFC File using xBIM Toolkit
        /// </summary>
        public override void Calculate()
        {
            if (InputPorts[0].Data == null)
                return;

            // Check for the given IFC Version
            Type IfcVersionType = InputPorts[0].Data.GetType();
            if (IfcVersionType.Name == "ModelInfoIFC2x3")
            {
                var modelid = ((ModelInfoIFC2x3)(InputPorts[0].Data)).ModelId;
                treeview.Model = DataController.Instance.GetModel(modelid);
            }
            else if (IfcVersionType.Name == "ModelInfoIFC4")
            {
                var modelid = ((ModelInfoIFC4)(InputPorts[0].Data)).ModelId;
                treeview.Model = DataController.Instance.GetModel(modelid);
            }
            
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