using System;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using TUM.CMS.VplControl.Core;

namespace TUM.CMS.VplControl.Test.Nodes
{
    public class FacultyNode : Node
    {
        public FacultyNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {
            AddInputPortToNode("Test", typeof (double));

            AddOutputPortToNode("Test", typeof (double));

            var label = new Label
            {
                Content = "!",
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
                var number = Double.Parse(InputPorts[0].Data.ToString());
                var result = 1;
                for (var i = 1; i <= number; i++)
                {
                    result = result * i;
                }
                OutputPorts[0].Data = result.ToString();
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
            return new FacultyNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}