using System;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using TUM.CMS.VplControl.Core;

namespace TUM.CMS.VplControl.Energy.Nodes
{
    public class EnergyNode : Node
    {
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