using System;
using System.Windows.Controls;
using System.Xml;
using TUM.CMS.VplControl.Core;
using Xbim.Common.Geometry;
using Xbim.Ifc;
using Xbim.Ifc4.GeometricModelResource;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.SharedBldgElements;
using Xbim.Tessellator;


namespace TUM.CMS.VplControl.Test.Nodes
{
    public class IfcReaderNode : Node
    {
        public IfcReaderNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {
            AddInputPortToNode("Test", typeof (string));

            AddControlToNode(new Label {Content = "TemplateNode"});
        }

        public override void Calculate()
        {
            if (InputPorts[0].Data.ToString() != "")
            {
                // var fileName = InputPorts[0].Data.ToString();
                var fileName = "C:\\Users\\Mac\\Downloads\\eeE-P1_Z3_V2.ifc";


                using (var model = IfcStore.Open(fileName))
                {
                    var project = model.Instances.FirstOrDefault<IfcProject>();
                    var walls = model.Instances.OfType<IfcWall>();
                    var doors = model.Instances.OfType<IfcDoor>();
                    Console.WriteLine(project.Name);
                    foreach (var wall in walls)
                    {
                        Console.WriteLine(wall.Name);
                    }
                }
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
            return new IfcReaderNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}