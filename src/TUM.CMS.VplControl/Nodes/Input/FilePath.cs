using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using Microsoft.Win32;
using TUM.CMS.VplControl.Controls;
using TUM.CMS.VplControl.Core;

namespace TUM.CMS.VplControl.Nodes.Input
{
    public class FilePathNode : Node
    {
        private readonly TextBlock textBlock;
        private string file;
        public FilePathNode(Core.VplControl hostCanvas): base(hostCanvas)
        {
            AddOutputPortToNode("String", typeof (string));

            FilePathControl filePathControl = new FilePathControl();
            filePathControl.Button.Click += button_Click;

            AddControlToNode(filePathControl);
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            var filePathControl = ControlElements[0] as FilePathControl;

            var openFileDialog = new OpenFileDialog
            {
                Multiselect = false
                //Filter = "vplXML (.vplxml)|*.vplxml"
            };


            if (openFileDialog.ShowDialog() == true)
            {
                filePathControl.SelectedFile.Visibility = Visibility.Visible;
                filePathControl.MainGrid.Width = 170;
                var shortFileName = "";
                if (openFileDialog.SafeFileName.Length >= 10)
                {
                    shortFileName = openFileDialog.SafeFileName.Substring(0, 10);
                }
                else
                {
                    shortFileName = openFileDialog.SafeFileName;
                }


                filePathControl.SelectedFile.Content = "Selected File: " + shortFileName + "...";
                filePathControl.SelectedFile.ToolTip = openFileDialog.SafeFileName;
                file = openFileDialog.FileName;
                Calculate();
            }
        }

        private void TextNode_MouseLeave(object sender, MouseEventArgs e)
        {
            Border.Focusable = true;
            Border.Focus();
            Border.Focusable = false;
        }

        public override void Calculate()
        {
            if (!string.IsNullOrEmpty(file))
            {
                OutputPorts[0].Data = file;
            }
            
        }

        public override void SerializeNetwork(XmlWriter xmlWriter)
        {
            base.SerializeNetwork(xmlWriter);
            var filePathControl = ControlElements[0] as FilePathControl;
            if (filePathControl == null) return;

            xmlWriter.WriteStartAttribute("Text");
            xmlWriter.WriteValue(filePathControl.SelectedFile.Content);
            xmlWriter.WriteEndAttribute();
        }

        public override void DeserializeNetwork(XmlReader xmlReader)
        {
            base.DeserializeNetwork(xmlReader);

            var filePathControl = ControlElements[0] as FilePathControl;
            if (filePathControl == null) return;

            filePathControl.SelectedFile.Content = xmlReader.GetAttribute("Text");
        }

        public override Node Clone()
        {
            return new FilePathNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}