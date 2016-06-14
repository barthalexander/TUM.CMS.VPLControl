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
        //private readonly PointSelectionCommand _seCo=new PointSelectionCommand() ;
        public XbimModel XModel;

        public IfcParseGeometryNode(Core.VplControl hostCanvas) : base(hostCanvas)
        {
            // Init UI
            IsResizeable = true;

            AddInputPortToNode("Model", typeof(string));
            AddOutputPortToNode("SelectedEntities", typeof(List<IfcGloballyUniqueId>));

            _viewPort = new HelixViewport3D();
            //_viewPort.MouseDoubleClick += _viewPort_mouseclick;

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
                var mat = GetStyleFromXbimModel(item);

                string itemtype = item.GetType().ToString();
                DiffuseMaterial Material1 = new DiffuseMaterial(new SolidColorBrush(Colors.Violet));

                var test = item.ReferencedBy;

                if (item.GetType().ToString() == "Xbim.Ifc2x3.SharedBldgElements.IfcBeam")
                {
                    Material1 = new DiffuseMaterial(new SolidColorBrush(Colors.Yellow));
                 
                }
                else if (item.GetType().ToString() == "Xbim.Ifc2x3.SharedBldgElements.IfcColumn")
                {
                    Material1 = new DiffuseMaterial(new SolidColorBrush(Colors.Red));
                   
                }
                else if (item.GetType().ToString() == "Xbim.Ifc2x3.SharedBldgElements.IfcCurtainWall")
                {
                    Material1 = new DiffuseMaterial(new SolidColorBrush(Colors.Plum));
                }
                else if (item.GetType().ToString() == "Xbim.Ifc2x3.SharedBldgElements.IfcDoor")
                {
                    Material1 = new DiffuseMaterial(new SolidColorBrush(Colors.SeaGreen));
                }
                else if (item.GetType().ToString() == "Xbim.Ifc2x3.SharedBldgElements.IfcPlate")
                {
                    Material1 = new DiffuseMaterial(new SolidColorBrush(Colors.SpringGreen));
                }
                else if (item.GetType().ToString() == "Xbim.Ifc2x3.SharedBldgElements.IfcRoof")
                {
                    Material1 = new DiffuseMaterial(new SolidColorBrush(Colors.Brown));
                }
                else if (item.GetType().ToString() == "Xbim.Ifc2x3.SharedBldgElements.IfcSlab")
                {
                    Material1 = new DiffuseMaterial(new SolidColorBrush(Colors.Black));
                }
                else if (item.GetType().ToString() == "Xbim.Ifc2x3.SharedBldgElements.IfcStair")
                {
                    Material1 = new DiffuseMaterial(new SolidColorBrush(Colors.Coral));
                }
                else if (item.GetType().ToString() == "Xbim.Ifc2x3.SharedBldgElements.IfcWall")
                {
                    Material1 = new DiffuseMaterial(new SolidColorBrush(Colors.Gray));
                }
                else if (item.GetType().ToString() == "Xbim.Ifc2x3.SharedBldgElements.IfcWindow")
                {
                    Material1 = new DiffuseMaterial(new SolidColorBrush(Colors.HotPink));
                }
               
                var mb = new MeshBuilder(false, false);
                    VisualizeMesh(mb, m, mat);
            }

           
        }

       // private void _viewPort_mouseclick(Object sender,MouseEventArgs e)
        //{

        //}
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
        public bool VisualizeMesh(MeshBuilder meshBuilder, MeshGeometry3D mesh, DiffuseMaterial mat)
        {

            // Output on console
            var points = new List<Point3D>();

            foreach (var item in mesh.Positions)
            {
                points.Add(new Point3D { X = item.X, Y = item.Y, Z = item.Z });
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
                Material = mat,
                //BackMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Red)),
                Geometry = meshBuilder.ToMesh(true)
                // In case that you have to rotate the model ... 
                // Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 90))
            };

            var element = new ModelUIElement3D { Model = myGeometryModel };

            // Add the Mesh to the ViewPort
            _viewPort.Children.Add(element);

            return true;
        }


        public DiffuseMaterial GetStyleFromXbimModel(IPersistIfcEntity item)
        {

            var model = item.ModelOf as XbimModel;
            if (model == null || !(item is IfcProduct))
                return null;

            switch (model.GeometrySupportLevel)
            {
                case 2:
                    try
                    {

                    
                    // Style
                    Dictionary<int, WpfMaterial> styles = new Dictionary<int, WpfMaterial>();
                    Dictionary<int, WpfMeshGeometry3D> meshSets = new Dictionary<int, WpfMeshGeometry3D>();
                    Model3DGroup opaques = new Model3DGroup();
                    Model3DGroup transparents = new Model3DGroup();

                    var context = new Xbim3DModelContext(model);

                    foreach (var style in context.SurfaceStyles())
                    {
                        WpfMaterial wpfMaterial = new WpfMaterial();
                        wpfMaterial.CreateMaterial(style);
                        styles.Add(style.DefinedObjectId, wpfMaterial);
                        WpfMeshGeometry3D mg = new WpfMeshGeometry3D(wpfMaterial, wpfMaterial);
                        meshSets.Add(style.DefinedObjectId, mg);
                        if (style.IsTransparent)
                            transparents.Children.Add(mg);
                        else
                            opaques.Children.Add(mg);

                    }

                    var productShape = context.ShapeInstancesOf((IfcProduct)item)
                        .Where(s => s.RepresentationType != XbimGeometryRepresentationType.OpeningsAndAdditionsExcluded)
                        .ToList();
                    if (!productShape.Any() && item is IfcFeatureElement)
                    {
                        productShape = context.ShapeInstancesOf((IfcProduct)item)
                            .Where(
                                s => s.RepresentationType == XbimGeometryRepresentationType.OpeningsAndAdditionsExcluded)
                            .ToList();
                    }

                    if (!productShape.Any())
                        return null;

                    var shapeInstance = productShape.FirstOrDefault();


                    WpfMaterial material;
                    styles.TryGetValue(shapeInstance.StyleLabel, out material);
                    string stringMaterial = material.Description;
                    string[] materialEntries = stringMaterial.Split(' ');
                    double r, g, b, a;

                    double.TryParse(materialEntries[1].Substring(2), out r);
                    double.TryParse(materialEntries[2].Substring(2), out g);
                    double.TryParse(materialEntries[3].Substring(2), out b);
                    double.TryParse(materialEntries[4].Substring(2), out a);

                    r *= 255;
                    g *= 255;
                    b *= 255;
                    a *= 255;
                    return new DiffuseMaterial(new SolidColorBrush(Color.FromArgb((byte)r, (byte)g, (byte)b, (byte)a)));

                    }
                    catch
                    {
                        return new DiffuseMaterial(new SolidColorBrush(Colors.Red)); ;
                    }

                    break;

            }

            return new DiffuseMaterial(new SolidColorBrush(Colors.Gray)); ;


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

                    var productShape = context.ShapeInstancesOf((IfcProduct)item)
                        .Where(s => s.RepresentationType != XbimGeometryRepresentationType.OpeningsAndAdditionsExcluded)
                        .ToList();
                    if (!productShape.Any() && item is IfcFeatureElement)
                    {
                        productShape = context.ShapeInstancesOf((IfcProduct)item)
                            .Where(
                                s => s.RepresentationType == XbimGeometryRepresentationType.OpeningsAndAdditionsExcluded)
                            .ToList();
                    }


                    foreach(var shapeInstance in productShape)
                    { 
                        IXbimShapeGeometryData shapeGeom = context.ShapeGeometry(shapeInstance.ShapeGeometryLabel);
                        switch ((XbimGeometryType)shapeGeom.Format)
                        {
                            case XbimGeometryType.PolyhedronBinary:
                                m.Read(shapeGeom.ShapeData,
                                    XbimMatrix3D.Multiply(shapeInstance.Transformation, wcsTransform));
                                break;
                            case XbimGeometryType.Polyhedron:
                                m.Read(((XbimShapeGeometry)shapeGeom).ShapeData,
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