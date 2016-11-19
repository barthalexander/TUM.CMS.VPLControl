using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.IFC.Controls;
using TUM.CMS.VplControl.IFC.Utilities;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class MvdParserNode : Node
    {
        public MvdParserNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {
            AddInputPortToNode("file", typeof (string));

            AddOutputPortToNode("object", typeof (object));

            TitleTextboxControl titleTextboxControl = new TitleTextboxControl();
            titleTextboxControl.Title.Content = "mvdXML Reader";


            AddControlToNode(titleTextboxControl);
        }
        public override void Calculate()
        {
            if (InputPorts[0].Data == null) return;
            var file = InputPorts[0].Data.ToString();

            var titleTextboxControl = ControlElements[0] as TitleTextboxControl;
            TextBlock textBlock = new TextBlock();
            textBlock = titleTextboxControl.TextBlock;

            if (file != "" && File.Exists(file) && Path.GetExtension(file).ToUpper().ToString() == ".XML")
            {
                MvdXMLReader mvdXmlReader = new MvdXMLReader(file);
                mvdXmlReader.readXML();
                OutputPorts[0].Data = mvdXmlReader.GetModelView();

                
                textBlock.Background = Brushes.White;
                textBlock.Text = "File is Valid!";

            }
            else
            {
                textBlock.Background = Brushes.Red;
                textBlock.Text = "Please select a true mvdXML File!";
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
            return new MvdParserNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}