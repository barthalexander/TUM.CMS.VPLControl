using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using TUM.CMS.VplControl.Core;
using Xbim.Common.Geometry;
using Xbim.Geometry.Engine.Interop;
using Xbim.Ifc2x3.Kernel;
using Xbim.Ifc2x3.ProductExtension;
using Xbim.Ifc2x3.UtilityResource;
using Xbim.IO;
using Xbim.ModelGeometry.Scene;
using Xbim.Presentation;
using Xbim.XbimExtensions;
using Xbim.XbimExtensions.Interfaces;
using XbimGeometry.Interfaces;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcParseGeometryNode : Node
    {
        private readonly HelixViewport3D _viewPort;
        public XbimModel XModel;

        public IfcParseGeometryNode(Core.VplControl hostCanvas) : base(hostCanvas)
        {
            // Init UI
            IsResizeable = true;

            AddInputPortToNode("Model", typeof(string));
            AddOutputPortToNode("SelectedEntities", typeof(List<IfcGloballyUniqueId>));

            _viewPort = new HelixViewport3D();

            AddControlToNode(_viewPort);
        }

        public override void Calculate()
        {
            // Init the viewport
            _viewPort.Children.Clear();
            _viewPort.Children.Add(new SunLight());

            var file = InputPorts[0].Data.ToString();

            if (file == null || !File.Exists(file)) return;

            var zufall = new Random();
            var number = zufall.Next(1, 1000);

            var path = Path.GetTempPath();
            XModel = new XbimModel();
            XModel.CreateFrom(file, path + "temp_reader" + number + ".xbim");
            XModel.Close();

            var res = XModel.Open(path + "temp_reader" + number + ".xbim", XbimDBAccess.ReadWrite);

            var context = new Xbim3DModelContext(XModel);
                //upgrade to new geometry represenation, uses the default 3D model
            context.CreateContext(XbimGeometryType.PolyhedronBinary);

            if (res == false)
            {
                var err = XModel.Validate(TextWriter.Null, ValidationFlags.All);
                MessageBox.Show("ERROR in reading process!");
            }

            // Loop through Entities and visualze them in the viewport
            foreach (var item in XModel.Instances.OfType<IfcProduct>())
            {
                var m = new MeshGeometry3D();
                GetGeometryFromXbimModel(m, item, XbimMatrix3D.Identity);

                var mb = new MeshBuilder(false, false);
                VisualizeMesh(mb, m);
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
        ///     VisualizeMesh in the Viewport
        /// </summary>
        /// <param name="meshBuilder"></param>
        /// <param name="mesh"></param>
        public bool VisualizeMesh(MeshBuilder meshBuilder, MeshGeometry3D mesh)
        {
            // Output on console
            var points = new List<Point3D>();

            foreach (var item in mesh.Positions)
            {
                points.Add(new Point3D {X = item.X, Y = item.Y, Z = item.Z});
            }

            for (var i = 0; i < mesh.TriangleIndices.Count; i += 3)
            {
                meshBuilder.AddTriangle(points[mesh.TriangleIndices[i]], points[mesh.TriangleIndices[i + 1]],
                    points[mesh.TriangleIndices[i + 2]]);
            }

            // TODO: Color has to be read! 

            // Create the Geometry
            var myGeometryModel = new GeometryModel3D
            {
                Material = new DiffuseMaterial(new SolidColorBrush(Colors.Aqua)),
                BackMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Red)),
                Geometry = meshBuilder.ToMesh(true)
                // In case that you have to rotate the model ... 
                // Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 90))
            };

            var element = new ModelUIElement3D {Model = myGeometryModel};

            // Add the Mesh to the ViewPort
            _viewPort.Children.Add(element);

            return true;
        }

        /// <summary>
        ///     Create MeshGeometry3D
        /// </summary>
        /// <param name="m"></param>
        /// <param name="item"></param>
        /// <param name="wcsTransform"></param>
        public void GetGeometryFromXbimModel(MeshGeometry3D m, IPersistIfcEntity item, XbimMatrix3D wcsTransform)
        {
            var model = item.ModelOf as XbimModel;
            if (model == null || !(item is IfcProduct))
                return;

            switch (model.GeometrySupportLevel)
            {
                case 2:
                    var context = new Xbim3DModelContext(model);

                    var productShape = context.ShapeInstancesOf((IfcProduct) item)
                        .Where(s => s.RepresentationType != XbimGeometryRepresentationType.OpeningsAndAdditionsExcluded)
                        .ToList();
                    if (!productShape.Any() && item is IfcFeatureElement)
                    {
                        productShape = context.ShapeInstancesOf((IfcProduct) item)
                            .Where(
                                s => s.RepresentationType == XbimGeometryRepresentationType.OpeningsAndAdditionsExcluded)
                            .ToList();
                    }

                    if (!productShape.Any())
                        return;
                    foreach (var shapeInstance in productShape)
                    {
                        IXbimShapeGeometryData shapeGeom =
                            context.ShapeGeometry(shapeInstance.ShapeGeometryLabel);
                        switch ((XbimGeometryType) shapeGeom.Format)
                        {
                            case XbimGeometryType.PolyhedronBinary:
                                m.Read(shapeGeom.ShapeData,
                                    XbimMatrix3D.Multiply(shapeInstance.Transformation, wcsTransform));
                                break;
                            case XbimGeometryType.Polyhedron:
                                m.Read(((XbimShapeGeometry) shapeGeom).ShapeData,
                                    XbimMatrix3D.Multiply(shapeInstance.Transformation, wcsTransform));
                                break;
                        }
                    }
                    break;
                case 1:
                    var xm3D = new XbimMeshGeometry3D();
                    var geomDataSet = model.GetGeometryData(item.EntityLabel, XbimGeometryType.TriangulatedMesh);
                    foreach (var geomData in geomDataSet)
                    {
#pragma warning disable 618
                        var gd = geomData.TransformBy(wcsTransform);
#pragma warning restore 618
                        xm3D.Add(gd);
                    }
                    m.Add(xm3D);
                    break;
            }
        }
    }
}