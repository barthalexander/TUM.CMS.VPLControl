using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.IFC.Utilities;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class mvdXMLReaderNode : Node
    {
        public mvdXMLReaderNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {
            AddInputPortToNode("file", typeof (string));

            AddOutputPortToNode("object", typeof (object));

            var label = new Label
            {
                Content = "Reading mvdXML",
                Width = 120,
                FontSize = 15,
                HorizontalContentAlignment = HorizontalAlignment.Center
            };

            AddControlToNode(label);
        }
        public override void Calculate()
        {
            var file = InputPorts[0].Data.ToString();
            if (file != "" && File.Exists(file))
            {
                MvdXMLReader mvdXmlReader = new MvdXMLReader(file);
                mvdXmlReader.readXML();
                OutputPorts[0].Data = mvdXmlReader.GetModelView();
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
            return new mvdXMLReaderNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}