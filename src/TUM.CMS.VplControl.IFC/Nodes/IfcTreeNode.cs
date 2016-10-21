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