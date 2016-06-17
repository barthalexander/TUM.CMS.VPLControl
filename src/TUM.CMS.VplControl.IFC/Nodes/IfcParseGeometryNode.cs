using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
using TUM.CMS.VplControl.IFC.Utilities;
using System.Windows.Input;
using System.Windows.Threading;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcParseGeometryNode : Node
    {
        private readonly HelixViewport3D _viewPort;
        //private readonly PointSelectionCommand _seCo=new PointSelectionCommand() ;
        private XbimModel xModel;
        public List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> SelectedModels;
        private readonly Material _selectionMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Crimson));
        private BackgroundWorker worker;

        public IfcParseGeometryNode(Core.VplControl hostCanvas) : base(hostCanvas)
        {
            // Init UI
            IsResizeable = true;

            AddInputPortToNode("Model", typeof(string), true);
            AddOutputPortToNode("FilteredElements", typeof(object));

            _viewPort = new HelixViewport3D
            {
                MinWidth = 520,
                MinHeight = 520
            };

            //_viewPort.MouseDoubleClick += _viewPort_mouseclick;
            SelectedModels = new List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId>();

            AddControlToNode(_viewPort);
        }


        public override void Calculate()
        {
            OutputPorts[0].Data = null;
            // Init the viewport

            _viewPort.Children.Clear();
            _viewPort.Children.Add(new SunLight());

            Type t = InputPorts[0].Data.GetType();
            if (t.IsGenericType)
            {
                var collection = InputPorts[0].Data as ICollection;
                foreach (var model in collection)
                {
                    var modelid = ((ModelInfo)(model)).ModelId;
                    xModel = DataController.Instance.GetModel(modelid, true);

                    var context = new Xbim3DModelContext(xModel);
                    //upgrade to new geometry represenation, uses the default 3D model
                    context.CreateContext(XbimGeometryType.PolyhedronBinary);

                    worker = new BackgroundWorker();

                    worker.DoWork += new DoWorkEventHandler(worker_DoWork);
                    worker.RunWorkerAsync(xModel);
                    worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

                }
            }
            else
            {
                var file = InputPorts[0].Data.ToString();

                var modelid = ((ModelInfo)(InputPorts[0].Data)).ModelId;
                if (modelid == null) return;

                xModel = DataController.Instance.GetModel(modelid, true);

                var context = new Xbim3DModelContext(xModel);
                //upgrade to new geometry represenation, uses the default 3D model
                context.CreateContext(XbimGeometryType.PolyhedronBinary);

                worker = new BackgroundWorker();

                worker.DoWork += new DoWorkEventHandler(worker_DoWork);
                worker.RunWorkerAsync(xModel);
                worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

               
                
                
            }

           
        }



        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Loop through Entities and visualze them in the viewport

            xModel = (XbimModel) e.Argument;
            foreach (var item in xModel.Instances.OfType<IfcProduct>())
            {
                var m = new MeshGeometry3D();
                GetGeometryFromXbimModel(m, item, XbimMatrix3D.Identity);
                var mat = GetStyleFromXbimModel(item);

                var mb = new MeshBuilder(false, false);

                VisualizeMesh(mb, m, mat, item.GlobalId);
                e.Result = xModel;
            }

        }
        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            xModel.Close();
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
        public bool VisualizeMesh(MeshBuilder meshBuilder, MeshGeometry3D mesh, DiffuseMaterial mat, IfcGloballyUniqueId globalId)
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
            element.MouseDown += (sender1, e1) => OnElementMouseDown(sender1, e1, this, globalId);
            // Add the Mesh to the ViewPort
            if (Application.Current.Dispatcher.CheckAccess())
            {
                _viewPort.Children.Add(element);
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                    _viewPort.Children.Add(element);
                }));
            }
            
            return true;
        }

        protected void OnElementMouseDown(object sender, MouseButtonEventArgs e, IfcParseGeometryNode ifcParseGeometryNode, IfcGloballyUniqueId globalId)
        {
            // Check null expression
            // if (e == null) throw new ArgumentNullException(nameof(e));

            // 1-CLick event
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            // Get sender
            var element = sender as ModelUIElement3D;

            // Check Type
            if (element != null)
            {
                var geometryModel3D = element.Model as GeometryModel3D;
                if (geometryModel3D == null)
                    return;

                // If it is already selected ... Deselect
                if (SelectedModels.Contains(globalId))
                {
                    geometryModel3D.Material = geometryModel3D.BackMaterial;
                    SelectedModels.Remove(globalId);
                }
                // If not ... Select!
                else
                {
                    SelectedModels.Add(globalId);
                    geometryModel3D.Material = _selectionMaterial;
                }
            }

            // Set selected models to Output ...  
            if (SelectedModels != null && SelectedModels.Count != 0)
            {
                OutputPorts[0].Data = SelectedModels;
            }

            e.Handled = true;
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