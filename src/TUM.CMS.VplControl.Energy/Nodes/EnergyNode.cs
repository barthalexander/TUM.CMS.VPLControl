using System;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.IFC.Utilities;
using System.Collections.Generic;
using Xbim.IO;
using Xbim.XbimExtensions;
using System.Linq;
using System.Collections;
using System.Text.RegularExpressions;
using System.Reflection;
using Xbim.Ifc2x3.ProductExtension;
using System.IO;

namespace TUM.CMS.VplControl.Energy.Nodes
{
    public class EnergyNode : Node
    {
        public XbimModel xModel;
        public EnergyNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {
            AddInputPortToNode("text", typeof(string));

            AddOutputPortToNode("text", typeof(string));

            var textBox = new TextBox { MinWidth = 300, MaxWidth = 500, IsHitTestVisible = false };
            textBox.TextChanged += textBox_TextChanged;
            AddControlToNode(textBox);
            AddControlToNode(new Label { Content = "CountIfc" });
        }
        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Calculate();
        }
        public override void Calculate()
        {
            ///
            /// ifc wall thickness calculation, saved in ifcWallThickness list, prepare for energy node
            ///
            var modelid = ((ModelInfo)(InputPorts[0].Data)).ModelId;
            if (modelid == null) return;
            xModel = DataController.Instance.GetModel(modelid);
            var ifcWall = xModel.IfcProducts.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcWall>().ToList();
            List<double> ifcWallThickness = new List<double> { };
            for (int i = 0; i < ifcWall.Count; i++)
            {
                var ifcWallVolume = ifcWall[i].PropertySets.ToList()[1].HasProperties.ToList()[2];
                var ifcWallArea = ifcWall[i].PropertySets.ToList()[1].HasProperties.ToList()[0];
                var volume = ifcWallVolume as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                var volumeValue = volume.NominalValue as Xbim.XbimExtensions.SelectTypes.IfcValue;
                object volumeValueTrue = volume.NominalValue.Value;
                double volumeVal = (double)volumeValueTrue;
                var area = ifcWallArea as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                var areaValue = area.NominalValue as Xbim.XbimExtensions.SelectTypes.IfcValue;
                object areaValueTrue = area.NominalValue.Value;
                double areaVal = (double)areaValueTrue;
                ifcWallThickness.Add(volumeVal / areaVal);

            }
            
            
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
            return new EnergyNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}