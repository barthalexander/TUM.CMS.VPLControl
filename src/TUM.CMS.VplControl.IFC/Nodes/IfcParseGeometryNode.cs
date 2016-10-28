using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using TUM.CMS.VplControl.Core;
using Xbim.Common.Geometry;
using Xbim.ModelGeometry.Scene;
using Xbim.Presentation;
using TUM.CMS.VplControl.IFC.Utilities;
using System.Windows.Input;
using System.Windows.Controls;
using Xbim.Common;
using Xbim.Ifc;
using Xbim.IO.Esent;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcParseGeometryNode : Node
    {
        private readonly HelixViewport3D _viewPort;
        //private readonly PointSelectionCommand _seCo=new PointSelectionCommand() ;
        private IfcStore _xModel;
        public List<ModelInfoIFC2x3> ModelListsIfc2x3;
        public List<ModelInfoIFC2x3> ModelListAll2x3;

        public List<ModelInfoIFC4> ModelListsIfc4;
        public List<ModelInfoIFC4> ModelListAll4;

        private readonly Material _selectionMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Crimson));
        private BackgroundWorker worker;
        private Type IfcVersionType = null;
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

            ModelListsIfc2x3 = new List<ModelInfoIFC2x3>();
            ModelListAll2x3 = new List<ModelInfoIFC2x3>();

            ModelListsIfc4 = new List<ModelInfoIFC4>();
            ModelListAll4 = new List<ModelInfoIFC4>();

            Type t = InputPorts[0].Data.GetType();
            if (t.IsGenericType)
            {
                var collection = InputPorts[0].Data as ICollection;
                if (collection != null)
                {
                    string ifcMergeVersionType = "";
                    foreach (var model in collection)
                    {
                        IfcVersionType = model.GetType();
                        if (ifcMergeVersionType == "")
                        {
                            ifcMergeVersionType = IfcVersionType.Name;
                        }

                        if (IfcVersionType.Name != ifcMergeVersionType)
                        {
                            MessageBox.Show("The IFC Versions are not the same!", "My Application", MessageBoxButton.OK);
                            return;
                        }
                        if (IfcVersionType.Name == "ModelInfoIFC2x3")
                        {
                            var modelId = ((ModelInfoIFC2x3)(model)).ModelId;
                            var elementIdsList = ((ModelInfoIFC2x3)(model)).ElementIds;
                            _xModel = DataController.Instance.GetModel(modelId, true);

                            ModelListsIfc2x3.Add(new ModelInfoIFC2x3(modelId));
                            ModelListAll2x3.Add(new ModelInfoIFC2x3(modelId));
                            int indexOfModel = 0;
                            foreach (var item in ModelListsIfc2x3)
                            {
                                if (item.ModelId == modelId)
                                {
                                    indexOfModel = ModelListsIfc2x3.IndexOf(item);
                                    break;
                                }
                            }
                            foreach (var item in ModelListAll2x3)
                            {
                                if (item.ModelId == modelId)
                                {
                                    indexOfModel = ModelListAll2x3.IndexOf(item);
                                    break;
                                }
                            }

                            var context = new Xbim3DModelContext(_xModel);
                            //upgrade to new geometry represenation, uses the default 3D model
                            context.CreateContext();
                            worker_DoWork_IFC2x3(_xModel, indexOfModel, elementIdsList);
                            button_1.Checked += (sender, e) => button_1_Checked_IFC2x3(sender, e, elementIdsList, indexOfModel);
                            worker = new BackgroundWorker();
                        }
                        else if (IfcVersionType.Name == "ModelInfoIFC4")
                        {
                            var modelId = ((ModelInfoIFC4)(model)).ModelId;
                            var elementIdsList = ((ModelInfoIFC4)(model)).ElementIds;
                            _xModel = DataController.Instance.GetModel(modelId, true);

                            ModelListsIfc4.Add(new ModelInfoIFC4(modelId));
                            ModelListAll4.Add(new ModelInfoIFC4(modelId));
                            int indexOfModel = 0;
                            foreach (var item in ModelListsIfc4)
                            {
                                if (item.ModelId == modelId)
                                {
                                    indexOfModel = ModelListsIfc4.IndexOf(item);
                                    break;
                                }
                            }
                            foreach (var item in ModelListAll4)
                            {
                                if (item.ModelId == modelId)
                                {
                                    indexOfModel = ModelListAll4.IndexOf(item);
                                    break;
                                }
                            }

                            var context = new Xbim3DModelContext(_xModel);
                            //upgrade to new geometry represenation, uses the default 3D model
                            context.CreateContext();
                            worker_DoWork_IFC4(_xModel, indexOfModel, elementIdsList);
                            button_1.Checked += (sender, e) => button_1_Checked_IFC4(sender, e, elementIdsList, indexOfModel);
                            worker = new BackgroundWorker();
                        }
                    }
                }
            }
            else
            {
                IfcVersionType = InputPorts[0].Data.GetType();
                if (IfcVersionType.Name == "ModelInfoIFC2x3")
                {
                    var modelId = ((ModelInfoIFC2x3)(InputPorts[0].Data)).ModelId;
                    var elementIdsList = ((ModelInfoIFC2x3)(InputPorts[0].Data)).ElementIds;
                    if (modelId == null) return;
                    int indexOfModel = 0;
                    foreach (var item in ModelListsIfc2x3)
                    {
                        if (item.ModelId == modelId)
                        {
                            indexOfModel = ModelListsIfc2x3.IndexOf(item);
                            break;
                        }
                    }
                    foreach (var item in ModelListAll2x3)
                    {
                        if (item.ModelId == modelId)
                        {
                            indexOfModel = ModelListAll2x3.IndexOf(item);
                            break;
                        }
                    }
                    _xModel = DataController.Instance.GetModel(modelId, true);




                    ModelListsIfc2x3.Add(new ModelInfoIFC2x3(modelId));
                    ModelListAll2x3.Add(new ModelInfoIFC2x3(modelId));
                    var context = new Xbim3DModelContext(_xModel);
                    //upgrade to new geometry represenation, uses the default 3D model
                    context.CreateContext();
                    worker_DoWork_IFC2x3(_xModel, indexOfModel, elementIdsList);
                    button_1.Checked += (sender, e) => button_1_Checked_IFC2x3(sender, e, elementIdsList, indexOfModel);

                }
                else if (IfcVersionType.Name == "ModelInfoIFC4")
                {
                    var modelId = ((ModelInfoIFC4)(InputPorts[0].Data)).ModelId;
                    var elementIdsList = ((ModelInfoIFC4)(InputPorts[0].Data)).ElementIds;
                    if (modelId == null) return;
                    int indexOfModel = 0;
                    foreach (var item in ModelListsIfc4)
                    {
                        if (item.ModelId == modelId)
                        {
                            indexOfModel = ModelListsIfc4.IndexOf(item);
                            break;
                        }
                    }
                    foreach (var item in ModelListAll4)
                    {
                        if (item.ModelId == modelId)
                        {
                            indexOfModel = ModelListAll4.IndexOf(item);
                            break;
                        }
                    }
                    _xModel = DataController.Instance.GetModel(modelId, true);




                    ModelListsIfc4.Add(new ModelInfoIFC4(modelId));
                    ModelListAll4.Add(new ModelInfoIFC4(modelId));
                    var context = new Xbim3DModelContext(_xModel);
                    //upgrade to new geometry represenation, uses the default 3D model
                    context.CreateContext();
                    worker_DoWork_IFC4(_xModel, indexOfModel, elementIdsList);
                    button_1.Checked += (sender, e) => button_1_Checked_IFC4(sender, e, elementIdsList, indexOfModel);
                }
            }
        }

        private void button_1_Checked_IFC2x3(object sender, RoutedEventArgs e, List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> elementIdsList, int indexOfModel )
        {
            
            if (ModelListAll2x3.Count == 1)
            {
                ModelListAll2x3[0].ElementIds = elementIdsList;
                OutputPorts[0].Data = ModelListAll2x3[0];             
            }

            if (ModelListAll2x3.Count > 1)
            {
                ModelListAll2x3[indexOfModel].ElementIds = elementIdsList;
                OutputPorts[0].Data = ModelListAll2x3;
               
            }
        }
        private void button_1_Checked_IFC4(object sender, RoutedEventArgs e, List<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId> elementIdsList, int indexOfModel )
        {
            
            if (ModelListAll4.Count == 1)
            {
                ModelListAll4[0].ElementIds = elementIdsList;
                OutputPorts[0].Data = ModelListAll4[0];             
            }

            if (ModelListAll4.Count > 1)
            {
                ModelListAll4[indexOfModel].ElementIds = elementIdsList;
                OutputPorts[0].Data = ModelListAll4;
               
            }
        }

        private void worker_DoWork_IFC2x3(IfcStore xModel, int indexOfModel, List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> elementIdsList)
        {
            // Loop through Entities and visualze them in the viewport
            var res = new HashSet<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId>(elementIdsList);
            // xModel = (IfcStore) e.Argument;
            foreach (var item in xModel.Instances.OfType<Xbim.Ifc2x3.Kernel.IfcProduct>())
            {
                if (res.Contains(item.GlobalId))
                {
                    var m = new MeshGeometry3D();
                    GetGeometryFromXbimModel_IFC2x3(m, item, XbimMatrix3D.Identity);
                    var mat = GetStyleFromXbimModel_IFC2x3(item);
                    // var mat = Xbim.Presentation.ModelDataProvider.DefaultMaterials;

                    var mb = new MeshBuilder(false, false);

                    VisualizeMesh_IFC2x3(mb, m, mat, item, indexOfModel);
                }

                // Show whole building with opacity 0.03
                else
                {
                    var m = new MeshGeometry3D();
                    GetGeometryFromXbimModel_IFC2x3(m, item, XbimMatrix3D.Identity);
                    var mat = GetStyleFromXbimModel_IFC2x3(item, 0.03);

                    var mb = new MeshBuilder(false, false);

                    VisualizeMesh_IFC2x3(mb, m, mat, item, indexOfModel);
                }
                
             }

        }
        private void worker_DoWork_IFC4(IfcStore xModel, int indexOfModel, List<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId> elementIdsList)
        {
            // Loop through Entities and visualze them in the viewport
            var res = new HashSet<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId>(elementIdsList);
            // xModel = (IfcStore) e.Argument;
            foreach (var item in xModel.Instances.OfType<Xbim.Ifc4.Kernel.IfcProduct>())
            {
                if (res.Contains(item.GlobalId))
                {
                    var m = new MeshGeometry3D();
                    GetGeometryFromXbimModel_IFC4(m, item, XbimMatrix3D.Identity);
                    var mat = GetStyleFromXbimModel_IFC4(item);
                    // var mat = Xbim.Presentation.ModelDataProvider.DefaultMaterials;

                    var mb = new MeshBuilder(false, false);

                    VisualizeMesh_IFC4(mb, m, mat, item, indexOfModel);
                }

                // Show whole building with opacity 0.03
                else
                {
                    var m = new MeshGeometry3D();
                    GetGeometryFromXbimModel_IFC4(m, item, XbimMatrix3D.Identity);
                    var mat = GetStyleFromXbimModel_IFC4(item, 0.03);

                    var mb = new MeshBuilder(false, false);

                    VisualizeMesh_IFC4(mb, m, mat, item, indexOfModel);
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
        ///     VisualizeMesh in the Viewport
        /// </summary>
        /// <param name="meshBuilder"></param>
        /// <param name="mesh"></param>
        /// <param name="mat"></param>
        /// <param name="itemModel"></param>
        /// <param name="indexOfModel"></param>
        public bool VisualizeMesh_IFC2x3(MeshBuilder meshBuilder, MeshGeometry3D mesh, Material mat, Xbim.Ifc2x3.Kernel.IfcProduct itemModel, int indexOfModel)
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
                element.MouseDown += (sender1, e1) => OnElementMouseDown_IFC2x3(sender1, e1, this, itemModel, indexOfModel);


                // Add the Mesh to the ViewPort

                _viewPort.Children.Add(element);

                // Do all UI related work here... }
            
            
            
            
           
            
            return true;
        }
         public bool VisualizeMesh_IFC4(MeshBuilder meshBuilder, MeshGeometry3D mesh, Material mat, Xbim.Ifc4.Kernel.IfcProduct itemModel, int indexOfModel)
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
                element.MouseDown += (sender1, e1) => OnElementMouseDown_IFC4(sender1, e1, this, itemModel, indexOfModel);


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
        protected void OnElementMouseDown_IFC2x3(object sender, MouseButtonEventArgs e, IfcParseGeometryNode ifcParseGeometryNode, Xbim.Ifc2x3.Kernel.IfcProduct itemModel, int indexOfModel)
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
                if (ModelListsIfc2x3[indexOfModel].ElementIds.Contains(itemModel.GlobalId))
                {
                    geometryModel3D.Material = geometryModel3D.BackMaterial;
                    ModelListsIfc2x3[indexOfModel].ElementIds.Remove(itemModel.GlobalId);
                }
                // If not ... Select!
                else
                {
                    ModelListsIfc2x3[indexOfModel].AddElementIds(itemModel.GlobalId);
                    geometryModel3D.Material = _selectionMaterial;
                }
            }           

            var button_2 = ControlElements[2] as RadioButton;
            if (button_2 == null) return;
            if((bool)button_2.IsChecked)
            {
                if (ModelListsIfc2x3.Count == 1)
            {
                OutputPorts[0].Data = ModelListsIfc2x3[0];

            }
            // Set selected models to Output ...  
            if (ModelListsIfc2x3.Count > 1)
            {
                OutputPorts[0].Data = ModelListsIfc2x3;

            }

            }
            
           // button_2.Checked += (sender2,e2)=>button_2_Checked(sender2,e2,indexOfModel);
            
            e.Handled = true;
        }
        protected void OnElementMouseDown_IFC4(object sender, MouseButtonEventArgs e, IfcParseGeometryNode ifcParseGeometryNode, Xbim.Ifc4.Kernel.IfcProduct itemModel, int indexOfModel)
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
                if (ModelListsIfc4[indexOfModel].ElementIds.Contains(itemModel.GlobalId))
                {
                    geometryModel3D.Material = geometryModel3D.BackMaterial;
                    ModelListsIfc4[indexOfModel].ElementIds.Remove(itemModel.GlobalId);
                }
                // If not ... Select!
                else
                {
                    ModelListsIfc4[indexOfModel].AddElementIds(itemModel.GlobalId);
                    geometryModel3D.Material = _selectionMaterial;
                }
            }           

            var button_2 = ControlElements[2] as RadioButton;
            if (button_2 == null) return;
            if((bool)button_2.IsChecked)
            {
                if (ModelListsIfc4.Count == 1)
            {
                OutputPorts[0].Data = ModelListsIfc4[0];

            }
            // Set selected models to Output ...  
            if (ModelListsIfc4.Count > 1)
            {
                OutputPorts[0].Data = ModelListsIfc4;

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
        public Material GetStyleFromXbimModel_IFC2x3(Xbim.Ifc2x3.Kernel.IfcProduct item, double opacity = 1)
        {
            var defautMaterial = Xbim.Presentation.ModelDataProvider.DefaultMaterials;
            Material mat = null;
            Material material;
            if (defautMaterial.TryGetValue(item.GetType().Name, out material))
            {
                ((System.Windows.Media.Media3D.DiffuseMaterial) material).Brush.Opacity = opacity;
                mat = material;
            }
            else
            {
                mat = defautMaterial["IfcProduct"];
                ((System.Windows.Media.Media3D.DiffuseMaterial)mat).Brush.Opacity = opacity;
            }
            return mat;
        }
        public Material GetStyleFromXbimModel_IFC4(Xbim.Ifc4.Kernel.IfcProduct item, double opacity = 1)
        {
            var defautMaterial = Xbim.Presentation.ModelDataProvider.DefaultMaterials;
            Material mat = null;
            Material material;
            if (defautMaterial.TryGetValue(item.GetType().Name, out material))
            {
                //((System.Windows.Media.Media3D.DiffuseMaterial)material).Brush.Opacity = opacity;
                mat = material;
            }
            else
            {
                mat = defautMaterial["IfcProduct"];
                //((System.Windows.Media.Media3D.DiffuseMaterial)mat).Brush.Opacity = opacity;

            }
            return mat;
        }
//                public DiffuseMaterial GetStyleFromXbimModel(IPersistEntity item, double opacity = 1)
//                {
//                    var model = item.Model as IfcStore;
//                    if (model == null || !(item is Xbim.Ifc2x3.Kernel.IfcProduct))
//                        return null;
//                    SolidColorBrush fillColor = new SolidColorBrush();
//
//                    switch (model.GeometrySupportLevel)
//                    {
//                        case 2:
//                            try
//                            {
//                                // Style
//                                Dictionary<int, WpfMaterial> styles = new Dictionary<int, WpfMaterial>();
//                                Dictionary<int, WpfMeshGeometry3D> meshSets = new Dictionary<int, WpfMeshGeometry3D>();
//                                Model3DGroup opaques = new Model3DGroup();
//                                Model3DGroup transparents = new Model3DGroup();
//                        var styledItemsGroup = model.Instances
//                                                .OfType<Xbim.Ifc2x3.Interfaces.IIfcStyledItem>()
//                                                .Where(s => s.Item != null)
//                                                .GroupBy(s => s.Item.EntityLabel);
//
//                        var context = new Xbim3DModelContext(model);
//                                // foreach (var style in context.SurfaceStyles)
//                                // {
//                                //     WpfMaterial wpfMaterial = new WpfMaterial();
//                                //     wpfMaterial.CreateMaterial(   );
//                                //     styles.Add(style.DefinedObjectId, wpfMaterial);
//                                //     WpfMeshGeometry3D mg = new WpfMeshGeometry3D(wpfMaterial, wpfMaterial);
//                                //     meshSets.Add(style.DefinedObjectId, mg);
//                                //     if (style.IsTransparent)
//                                //         transparents.Children.Add(mg);
//                                //     else
//                                //         opaques.Children.Add(mg);
//                                // 
//                                // }
//        
//                                var productShape = context.ShapeInstancesOf((Xbim.Ifc2x3.Kernel.IfcProduct)item)
//                                    .Where(s => s.RepresentationType != XbimGeometryRepresentationType.OpeningsAndAdditionsExcluded)
//                                    .ToList();
//                                if (!productShape.Any() && item is Xbim.Ifc2x3.ProductExtension.IfcFeatureElement)
//                                {
//                                    productShape = context.ShapeInstancesOf((Xbim.Ifc2x3.Kernel.IfcProduct)item)
//                                        .Where(
//                                            s => s.RepresentationType == XbimGeometryRepresentationType.OpeningsAndAdditionsExcluded)
//                                        .ToList();
//                                }
//        
//                                if (!productShape.Any())
//                                    return null;
//        
//                                var shapeInstance = productShape.FirstOrDefault();
//        
//        
//                                WpfMaterial material;
//                                styles.TryGetValue(shapeInstance.StyleLabel, out material);
//        
//                                if (material != null)
//                                {
//                                    string stringMaterial = material.Description;
//                                    string[] materialEntries = stringMaterial.Split(' ');
//                                    double r, g, b, a;
//        
//                                    double.TryParse(materialEntries[1].Substring(2), out r);
//                                    double.TryParse(materialEntries[2].Substring(2), out g);
//                                    double.TryParse(materialEntries[3].Substring(2), out b);
//                                    double.TryParse(materialEntries[4].Substring(2), out a);
//        
//                                    r *= 255;
//                                    g *= 255;
//                                    b *= 255;
//                                    a *= 255;
//                                    fillColor = new SolidColorBrush(Color.FromArgb((byte)r, (byte)g, (byte)b, (byte)a));
//                                    fillColor.Opacity = opacity;
//                                    return new DiffuseMaterial(fillColor);
//                                }
//                                else
//                                {
//                                    fillColor = new SolidColorBrush(Colors.Gray);
//                                    fillColor.Opacity = opacity;
//                                    return new DiffuseMaterial(fillColor);
//                                }
//                            }
//                            catch
//                            {
//                                fillColor = new SolidColorBrush(Colors.Gray);
//                                fillColor.Opacity = opacity;
//                                return new DiffuseMaterial(fillColor);
//                            }
//        
//                            break;
//        
//                    }
//        
//                    fillColor = new SolidColorBrush(Colors.Gray);
//                    fillColor.Opacity = opacity;
//                    return new DiffuseMaterial(fillColor);
//        
//        
//        
//                }


        /// <summary>
        ///     Create MeshGeometry3D
        /// </summary>
        /// <param name="m"></param>
        /// <param name="item"></param>
        /// <param name="wcsTransform"></param>
        public void GetGeometryFromXbimModel_IFC2x3(MeshGeometry3D m, IPersistEntity item, XbimMatrix3D wcsTransform)
        {
            if (item.Model == null || !(item is Xbim.Ifc2x3.Interfaces.IIfcProduct)) return;

            var context = new Xbim3DModelContext(item.Model);

            var productShape = context.ShapeInstancesOf((Xbim.Ifc4.Interfaces.IIfcProduct) item)
                .Where(s => s.RepresentationType != XbimGeometryRepresentationType.OpeningsAndAdditionsExcluded)
                .ToList();
            if (!productShape.Any() && item is Xbim.Ifc2x3.Interfaces.IIfcFeatureElement)
            {
                productShape = context.ShapeInstancesOf((Xbim.Ifc4.Interfaces.IIfcProduct) item)
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

        }
        public void GetGeometryFromXbimModel_IFC4(MeshGeometry3D m, IPersistEntity item, XbimMatrix3D wcsTransform)
        {
            if (item.Model == null || !(item is Xbim.Ifc4.Kernel.IfcProduct)) return;

            var context = new Xbim3DModelContext(item.Model);

            var productShape = context.ShapeInstancesOf((Xbim.Ifc4.Interfaces.IIfcProduct) item)
                .Where(s => s.RepresentationType != XbimGeometryRepresentationType.OpeningsAndAdditionsExcluded)
                .ToList();
            if (!productShape.Any() && item is Xbim.Ifc4.Interfaces.IIfcFeatureElement)
            {
                productShape = context.ShapeInstancesOf((Xbim.Ifc4.Interfaces.IIfcProduct) item)
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

        }

    }
}