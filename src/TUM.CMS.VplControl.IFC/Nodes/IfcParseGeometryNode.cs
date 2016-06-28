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
using System.Threading;
using System.Windows.Controls;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcParseGeometryNode : Node
    {
        private readonly HelixViewport3D _viewPort;
        //private readonly PointSelectionCommand _seCo=new PointSelectionCommand() ;
        private XbimModel _xModel;
        public List<ModelInfo> ModelList;
        public List<ModelInfo> ModelListAll;
        private readonly Material _selectionMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Crimson));
        private BackgroundWorker worker;
        public IfcParseGeometryNode(Core.VplControl hostCanvas) : base(hostCanvas)
        {
            // Init UI
            IsResizeable = true;

            AddInputPortToNode("Model", typeof(object), true);
            AddOutputPortToNode("FilteredElements", typeof(object));

            _viewPort = new HelixViewport3D
            {
                MinWidth = 520,
                MinHeight = 520
            };

            RadioButton radioButton_1 = new RadioButton { Content = "pass all" };
            RadioButton radioButton_2 = new RadioButton { Content = "pass selected", IsChecked = true };
            
            

            //_viewPort.MouseDoubleClick += _viewPort_mouseclick;

            AddControlToNode(_viewPort);
            AddControlToNode(radioButton_1);
            AddControlToNode(radioButton_2);
        }


        public override void Calculate()
        {
            OutputPorts[0].Data = null;
            var button_1 = ControlElements[1] as RadioButton;
            if (button_1 == null) return;
            var button_2 = ControlElements[2] as RadioButton;
            if (button_2 == null) return;
            // Init the viewport

            _viewPort.Children.Clear();
            _viewPort.Children.Add(new SunLight());
            ModelList = new List<ModelInfo>();
            ModelListAll = new List<ModelInfo>();
            Type t = InputPorts[0].Data.GetType();
            if (t.IsGenericType)
            {
                var collection = InputPorts[0].Data as ICollection;
                if (collection != null)
                    foreach (var model in collection)
                    {
                        var modelId = ((ModelInfo)(model)).ModelId;
                        var elementIdsList = ((ModelInfo)(model)).ElementIds;
                        _xModel = DataController.Instance.GetModel(modelId, true);

                        ModelList.Add(new ModelInfo(modelId));
                        ModelListAll.Add(new ModelInfo(modelId));
                        int indexOfModel = 0;
                        foreach (var item in ModelList)
                        {
                            if (item.ModelId == modelId)
                            {
                                indexOfModel = ModelList.IndexOf(item);
                                break;
                            }
                        }
                        foreach (var item in ModelListAll)
                        {
                            if (item.ModelId == modelId)
                            {
                                indexOfModel = ModelListAll.IndexOf(item);
                                break;
                            }
                        }

                        var context = new Xbim3DModelContext(_xModel);
                        //upgrade to new geometry represenation, uses the default 3D model
                        context.CreateContext(XbimGeometryType.PolyhedronBinary);
                        worker_DoWork(_xModel, indexOfModel, elementIdsList);
                        button_1.Checked += (sender, e) => button_1_Checked(sender, e, elementIdsList, indexOfModel);
                        // worker = new BackgroundWorker();

                        // worker.DoWork += new DoWorkEventHandler(worker_DoWork);
                        //  worker.RunWorkerAsync(xModel);
                        //  worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

                    }
            }
            else
            {
                var file = InputPorts[0].Data.ToString();

                var modelId = ((ModelInfo)(InputPorts[0].Data)).ModelId;
                var elementIdsList = ((ModelInfo)(InputPorts[0].Data)).ElementIds;
                if (modelId == null) return;
                int indexOfModel = 0;
                foreach (var item in ModelList)
                {
                    if (item.ModelId == modelId)
                    {
                        indexOfModel = ModelList.IndexOf(item);
                        break;
                    }
                }
                foreach (var item in ModelListAll)
                {
                    if (item.ModelId == modelId)
                    {
                        indexOfModel = ModelListAll.IndexOf(item);
                        break;
                    }
                }
                _xModel = DataController.Instance.GetModel(modelId, true);

                ModelList.Add(new ModelInfo(modelId));
                ModelListAll.Add(new ModelInfo(modelId));
                var context = new Xbim3DModelContext(_xModel);
                //upgrade to new geometry represenation, uses the default 3D model
                context.CreateContext(XbimGeometryType.PolyhedronBinary);
                worker_DoWork(_xModel, indexOfModel, elementIdsList);
                button_1.Checked += (sender,e)=>button_1_Checked(sender, e, elementIdsList, indexOfModel);              

                /*worker = new BackgroundWorker();

                worker.DoWork += new DoWorkEventHandler(worker_DoWork);
                worker.RunWorkerAsync(xModel);
                worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);*/





            }
        }

             
            

        private void button_1_Checked(object sender, RoutedEventArgs e, List<IfcGloballyUniqueId> elementIdsList, int indexOfModel )
        {
            
            if (ModelListAll.Count == 1)
            {
                ModelListAll[0].ElementIds = elementIdsList;
                OutputPorts[0].Data = ModelListAll[0];             
            }

            if (ModelListAll.Count > 1)
            {
                ModelListAll[indexOfModel].ElementIds = elementIdsList;
                OutputPorts[0].Data = ModelListAll;
               
            }
        }

        private void worker_DoWork(XbimModel xModel, int indexOfModel, List<IfcGloballyUniqueId> elementIdsList)
        {
            // Loop through Entities and visualze them in the viewport
            var res = new HashSet<IfcGloballyUniqueId>(elementIdsList);
           // xModel = (XbimModel) e.Argument;
            foreach (var item in xModel.Instances.OfType<IfcProduct>())
            {
                if (res.Contains(item.GlobalId))
                {
                    var m = new MeshGeometry3D();
                    GetGeometryFromXbimModel(m, item, XbimMatrix3D.Identity);
                    var mat = GetStyleFromXbimModel(item);

                    var mb = new MeshBuilder(false, false);

                    VisualizeMesh(mb, m, mat, item, indexOfModel);
                }
                
               // e.Result = xModel;
            }

        }
       /* private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            xModel.Close();
        }*/


       
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
        /// <param name="mat"></param>
        /// <param name="itemModel"></param>
        /// <param name="indexOfModel"></param>
        public bool VisualizeMesh(MeshBuilder meshBuilder, MeshGeometry3D mesh, DiffuseMaterial mat, IfcProduct itemModel, int indexOfModel)
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
                BackMaterial = mat,
                Geometry = meshBuilder.ToMesh(true)
                // In case that you have to rotate the model ... 
                // Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 90))
            };

           
                var element = new ModelUIElement3D { Model = myGeometryModel };
                element.MouseDown += (sender1, e1) => OnElementMouseDown(sender1, e1, this, itemModel, indexOfModel);


                // Add the Mesh to the ViewPort

                _viewPort.Children.Add(element);

                // Do all UI related work here... }
            
            
            
            
           
            
            return true;
        }
        /// <summary>
        /// On Mouse Select Event
        /// 
        /// Output is an List of IDs (ModelInfo)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="ifcParseGeometryNode"></param>
        /// <param name="itemModel"></param>
        /// <param name="indexOfModel"></param>
        protected void OnElementMouseDown(object sender, MouseButtonEventArgs e, IfcParseGeometryNode ifcParseGeometryNode, IfcProduct itemModel, int indexOfModel)
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
                if (ModelList[indexOfModel].ElementIds.Contains(itemModel.GlobalId))
                {
                    geometryModel3D.Material = geometryModel3D.BackMaterial;
                    ModelList[indexOfModel].ElementIds.Remove(itemModel.GlobalId);
                }
                // If not ... Select!
                else
                {
                    ModelList[indexOfModel].AddElementIds(itemModel.GlobalId);
                    geometryModel3D.Material = _selectionMaterial;
                }
            }           

            var button_2 = ControlElements[2] as RadioButton;
            if (button_2 == null) return;
            if((bool)button_2.IsChecked)
            {
                if (ModelList.Count == 1)
            {
                OutputPorts[0].Data = ModelList[0];

            }
            // Set selected models to Output ...  
            if (ModelList.Count > 1)
            {
                OutputPorts[0].Data = ModelList;

            }

            }
            
           // button_2.Checked += (sender2,e2)=>button_2_Checked(sender2,e2,indexOfModel);
            
            e.Handled = true;
        }
        private void button_2_Checked(object sender2, RoutedEventArgs e2,int indexOfModel)
        {
            
        }

        /// <summary>
        /// Get Style if each Item
        /// 
        /// TODO: Exception Error 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
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

                        if (material != null)
                        {
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
                            return
                                new DiffuseMaterial(
                                    new SolidColorBrush(Color.FromArgb((byte) r, (byte) g, (byte) b, (byte) a)));
                        }
                        else
                        {
                            return new DiffuseMaterial(new SolidColorBrush(Colors.Red));
                        }
                    }
                    catch
                    {
                        return new DiffuseMaterial(new SolidColorBrush(Colors.Red));
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