using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TUM.CMS.VplControl.Core;
using Xbim.IO;
using Xbim.ModelGeometry.Scene;
using Xbim.XbimExtensions;
using XbimGeometry.Interfaces;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using Xbim.Geometry.Engine.Interop;
using Xbim.Ifc2x3.Extensions;
using Xbim.Ifc2x3.GeometricModelResource;
using Xbim.Ifc2x3.GeometryResource;
using Xbim.ModelGeometry.Scene.Extensions;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcParseGeometryNode : Node
    {
        public XbimModel xModel;

        private HelixViewport3D viewPort;

        public IfcParseGeometryNode(Core.VplControl hostCanvas) : base(hostCanvas)
        {
            // Init UI
            IsResizeable = true;

            AddInputPortToNode("Test", typeof(string));

            AddOutputPortToNode("Test", typeof(string));

            viewPort = new HelixViewport3D();
            AddControlToNode(viewPort);
        }
       
        public override void Calculate()
        {
            var file = InputPorts[0].Data.ToString();
            if(file != null && File.Exists(file))
            {
                Random zufall = new Random();
                int number = zufall.Next(1, 1000);

                var path = Path.GetTempPath();
                xModel = new XbimModel();
                xModel.CreateFrom(file, path + "temp_reader" + number + ".xbim");
                xModel.Close();


                var res = xModel.Open(path + "temp_reader" + number + ".xbim", XbimDBAccess.ReadWrite);

                if (res == false)
                {
                    var err = xModel.Validate(TextWriter.Null, ValidationFlags.All);
                    MessageBox.Show("ERROR in reading process!");
                }

                IXbimSolid solid;
                XbimGeometryEngine _xbimGeometryCreator = new XbimGeometryEngine();

                foreach (var item in xModel.Instances.OfType<IfcExtrudedAreaSolid>())
                {
                    solid = _xbimGeometryCreator.CreateSolid(item);
                    var fbrep = _xbimGeometryCreator.CreateShapeGeometry(solid, 0.01, 10);

                    // IXbimGeometryObject

                    // Output on console - WORKS!
                    // foreach (var vertex in mesh.Positions)
                    // {
                    //     Console.WriteLine(vertex.X + ", " + vertex.Y + ", " + vertex.Z);
                    // }
                    // 
                    // for (int i = 0; i < mesh.TriangleIndices.Count; i += 3)
                    // {
                    //     Console.WriteLine(mesh.TriangleIndices[i] + ", " + mesh.TriangleIndices[i + 1] + ", " + mesh.TriangleIndices[i + 2]);
                    // }
                    // 
                    // Console.ReadLine();

                    XbimMeshGeometry3D mesh = new XbimMeshGeometry3D();

                    // mesh.Read()
                    mesh.Read(fbrep.ShapeData.ToString());
                    mesh.Read(System.Text.Encoding.ASCII.GetString(fbrep.ShapeData));

                    // Use the Mesh Builder now to create a Geometry, which can be viewed by the Helix Toolkit -> MeshBuilder
                    // Create the meshbuilder
                    var meshBuilder = new MeshBuilder(false, false);
                    CreateMesh(meshBuilder, mesh);
                }
            }
        }

        public override Node Clone()
        {
            return new IfcParseGeometryNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }

        /// <summary>
        /// CreateMesh
        /// </summary>
        /// <param name="meshBuilder"></param>
        /// <param name="mesh"></param>
        public void CreateMesh(MeshBuilder meshBuilder, XbimMeshGeometry3D mesh)
        {
            // Output on console
            var points = new List<Point3D>();

            foreach (var item in mesh.Positions)
            {
                points.Add(new Point3D { X = item.X, Y = item.Y, Z = item.Z });
            }

            for (int i = 0; i < mesh.TriangleIndices.Count; i += 3)
            {
                meshBuilder.AddTriangle(points[mesh.TriangleIndices[i]], points[mesh.TriangleIndices[i + 1]],
                            points[mesh.TriangleIndices[i + 2]]);
            }

            // Create the Geometry
            var myGeometryModel = new GeometryModel3D
            {
                Material = new DiffuseMaterial(new SolidColorBrush(Colors.Aqua)),
                BackMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Red)),
                Geometry = meshBuilder.ToMesh(true)
                // In case that you have to rotate the model ... 
                // Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 90))
            };

            var element = new ModelUIElement3D { Model = myGeometryModel };

            // Click event Handler
            // element.MouseDown += (sender1, e1) => OnElementMouseDown(sender1, e1, this);

            // Add the Mesh to the ViewPort
            viewPort.Children.Add(element);
        }
    }
}