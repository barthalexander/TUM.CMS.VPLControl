using System;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using TUM.CMS.VplControl.Core;

namespace TUM.CMS.VplControl.Test.Nodes
{
    public class SqrtNode : Node
    {
        public SqrtNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {
            AddInputPortToNode("Test", typeof (double));

            AddOutputPortToNode("Test", typeof (double));

            var label = new Label
            {
                Content = "\u221A",
                Width = 60,
                FontSize = 30,
                HorizontalContentAlignment = HorizontalAlignment.Center
            };

            AddControlToNode(label);
        }

        public override void Calculate()
        {
            if(Double.Parse(InputPorts[0].Data.ToString()) > 0)
            {
                OutputPorts[0].Data = Math.Sqrt(Double.Parse(InputPorts[0].Data.ToString()));
            }
            else
            {
                OutputPorts[0].Data = "Bitte geben Sie eine positive Zahl ein";
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
            return new SqrtNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}