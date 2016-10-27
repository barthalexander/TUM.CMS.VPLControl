using System.Windows.Controls;
using System.Xml;
using TUM.CMS.VplControl.Core;
using System.Collections.Generic;
using Xbim.IO;
using Xbim.XbimExtensions;
using TUM.CMS.VplControl.IFC.Utilities;
using System.Linq;
using System.Collections;
using System.Text.RegularExpressions;
using System;
using System.Windows;
using System.Reflection;
using Xbim.Ifc2x3.ProductExtension;
using System.IO;
using Xbim.Ifc2x3.UtilityResource;
using Xbim.Ifc2x3.SharedBldgElements;


namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcMvdFilter : Node
    {
        
        
        public List<ModelInfo> ModelList;
        private XbimModel _xModel;

        

        public IfcMvdFilter(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {

            AddInputPortToNode("Mvd", typeof(object),true);
            AddInputPortToNode("Ifc", typeof(object), true);

            AddOutputPortToNode("FilteredProducts", typeof(object));

           // AddControlToNode();

    

        }

        public override void Calculate()
        {
            ModelView modelView = (ModelView)InputPorts[0].Data;
            List<ModelInfo> modelList = (List<ModelInfo>)InputPorts[1].Data;
            Dictionary<string, ConceptRoot> roots= modelView.GetRoots();
           // rootsValue = roots.Values.ToString();

        }

        private void doWork(XbimModel xModel, int indexOfModel, List<IfcGloballyUniqueId> elementIdsList)
        {
        }

      

        public override void SerializeNetwork(XmlWriter xmlWriter)
        {
            base.SerializeNetwork(xmlWriter);

            // add your xml serialization methods here
        }

        public override void DeserializeNetwork(XmlReader xmlReader)
        {
            base.DeserializeNetwork(xmlReader);

            // add your xml deserialization methods here
        }

        public override Node Clone()
        {
            return new IfcMvdFilter(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}