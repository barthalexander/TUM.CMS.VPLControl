using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
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
using TUM.CMS.VplControl.IFC.Controls;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcViewerNode : Node
    {
        private HelixViewport3D _viewPort;
        private IfcStore _xModel;
        public ModelInfoIFC2x3 ModelInfoIfc2X3;
        public ModelInfoIFC4 ModelInfoIfc4;

        public List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> elementIdsList2x3;
        public List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> SelectedProductsIFC2x3;

        public List<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId> elementIdsList4;
        public List<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId> SelectedProductsIFC4;

        private readonly Material _selectionMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Crimson));
        private BackgroundWorker worker;
        private Type IfcVersionType = null;

        public IfcViewerNode(Core.VplControl hostCanvas) : base(hostCanvas)
        {
            IsResizeable = true;

            AddInputPortToNode("Model", typeof(object), false);
            AddOutputPortToNode("FilteredElements", typeof(object));

            UserControl usercontrol = new UserControl();
            Grid grid = new Grid();
            usercontrol.Content = grid;

            IFCViewerControl ifcViewerControl = new IFCViewerControl();

            AddControlToNode(ifcViewerControl);
        }

        /// <summary>
        /// Visualize the given IFC file
        /// </summary>
        public override void Calculate()
        {
            if (InputPorts[0].Data == null)
                return;


            var ifcViewerControl = ControlElements[0] as IFCViewerControl;
            OutputPorts[0].Data = null;
            var button_1 = ifcViewerControl.RadioButton_1;
            if (button_1 == null) return;
            var button_2 = ifcViewerControl.RadioButton_2;
            if (button_2 == null) return;
            
            
            // Init the viewport
            _viewPort = ifcViewerControl.Viewport3D;
            _viewPort.MinWidth = 300;
            _viewPort.MinHeight = 300;
            _viewPort.Children.Clear();
            _viewPort.Children.Add(new SunLight());
            _viewPort.ZoomExtentsWhenLoaded = true;

            // Check for the IFC Version
            IfcVersionType = InputPorts[0].Data.GetType();

            // Important for refresh node
            button_2.IsChecked = false;
            button_2.IsChecked = true;

            // Differs between choosen IFC Version
            if (IfcVersionType.Name == "ModelInfoIFC2x3")
            {
                var modelId = ((ModelInfoIFC2x3)(InputPorts[0].Data)).ModelId;
                elementIdsList2x3 = null;

                elementIdsList2x3 = ((ModelInfoIFC2x3)(InputPorts[0].Data)).ElementIds;
                if (modelId == null) return;
                
                _xModel = DataController.Instance.GetModel(modelId, true);

                SelectedProductsIFC2x3 = null;
                SelectedProductsIFC4 = null;

                SelectedProductsIFC2x3 = new List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId>();

                var context = new Xbim3DModelContext(_xModel);
                context.CreateContext();

                worker_DoWork_IFC2x3(_xModel, elementIdsList2x3);

                // Important for refreshing the node
                List<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId> elementIdsList4 = null;

                button_1.Checked += (sender, e) => button_1_Checked(sender, e);
                button_2.Checked += (sender, e) => button_2_Checked(sender, e);

            }
            else if (IfcVersionType.Name == "ModelInfoIFC4")
            {
                var modelId = ((ModelInfoIFC4)(InputPorts[0].Data)).ModelId;
                elementIdsList4 = null;

                elementIdsList4 = ((ModelInfoIFC4)(InputPorts[0].Data)).ElementIds;
                if (modelId == null) return;
                
                _xModel = DataController.Instance.GetModel(modelId, true);

                SelectedProductsIFC2x3 = null;
                SelectedProductsIFC4 = null;

                SelectedProductsIFC4 = new List<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId>();


                var context = new Xbim3DModelContext(_xModel);
                context.CreateContext();

                worker_DoWork_IFC4(_xModel, elementIdsList4);

                // Important for refreshing the node
                List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> elementIdsList2x3 = null;

                button_1.Checked += (sender, e) => button_1_Checked(sender, e);
                button_2.Checked += (sender, e) => button_2_Checked(sender, e);

            }
        }

        /// <summary>
        /// Outputs the complete IFC File
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_1_Checked(object sender, RoutedEventArgs e)
        {
            if (IfcVersionType.Name == "ModelInfoIFC2x3" && elementIdsList2x3 != null)
            {
                var modelId = ((ModelInfoIFC2x3) (InputPorts[0].Data)).ModelId;
                ModelInfoIFC2x3 modelInfoIfc2X3 = new ModelInfoIFC2x3(modelId);
                foreach (var item in elementIdsList2x3)
                {
                    modelInfoIfc2X3.AddElementIds(item);
                }
                OutputPorts[0].Data = modelInfoIfc2X3;
            }
            else if (IfcVersionType.Name == "ModelInfoIFC4" && elementIdsList4 != null)
            {
                var modelId = ((ModelInfoIFC4) (InputPorts[0].Data)).ModelId;
                ModelInfoIFC4 modelInfoIfc4 = new ModelInfoIFC4(modelId);
                foreach (var item in elementIdsList4)
                {
                    modelInfoIfc4.AddElementIds(item);
                }
                OutputPorts[0].Data = modelInfoIfc4;
            }

            else
            {
                OutputPorts[0].Data = null;
            }

        }
        
        /// <summary>
        /// Outputs only selected elements
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="routedEventArgs"></param>
        private void button_2_Checked(object sender, RoutedEventArgs routedEventArgs)
        {
            if (IfcVersionType.Name == "ModelInfoIFC2x3" && SelectedProductsIFC2x3 != null)
            {
                var modelId = ((ModelInfoIFC2x3)(InputPorts[0].Data)).ModelId;
                ModelInfoIFC2x3 modelInfoIfc2X3 = new ModelInfoIFC2x3(modelId);
                foreach (var item in SelectedProductsIFC2x3)
                {
                    modelInfoIfc2X3.AddElementIds(item);
                }
                OutputPorts[0].Data = modelInfoIfc2X3;
            }
            else if (IfcVersionType.Name == "ModelInfoIFC4" && SelectedProductsIFC4 != null)
            {
                var modelId = ((ModelInfoIFC4) (InputPorts[0].Data)).ModelId;
                ModelInfoIFC4 modelInfoIfc4 = new ModelInfoIFC4(modelId);
                foreach (var item in SelectedProductsIFC4)
                {
                    modelInfoIfc4.AddElementIds(item);
                }
                OutputPorts[0].Data = modelInfoIfc4;
            }
            else
            {
                OutputPorts[0].Data = null;
            }
        }
        
        private void worker_DoWork_IFC2x3(IfcStore xModel, List<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> elementIdsList)
        {
            // Loop through Entities and visualze them in the viewport
            var res = new HashSet<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId>(elementIdsList);

            // Test for parallel Loop. Doesn't work for viewport
            var parallel = false;
            List<ModelUIElement3D> elementList = new List<ModelUIElement3D>();
            switch (parallel)
            {
                case true:
                    Parallel.ForEach(xModel.Instances.OfType<Xbim.Ifc2x3.Kernel.IfcProduct>(), item =>
                    {
                        if (res.Contains(item.GlobalId))
                        {
                            XbimModelPositioningCollection ModelPositions = new XbimModelPositioningCollection();

                            short userDefinedId = 0;
                            xModel.UserDefinedId = userDefinedId;


                            ModelPositions.AddModel(xModel.ReferencingModel);

                            if (xModel.IsFederation)
                            {
                                foreach (var refModel in xModel.ReferencedModels)
                                {
                                    refModel.Model.UserDefinedId = ++userDefinedId;
                                    var v = refModel.Model as IfcStore;
                                    if (v != null)
                                        ModelPositions.AddModel(v.ReferencingModel);
                                }
                            }

                            // Important for centralize the model in the viewer
                            var ModelBounds = ModelPositions.GetEnvelopeInMeters();

                            var p = ModelBounds.Centroid();
                            var _modelTranslation = new XbimVector3D(-p.X, -p.Y, -p.Z);
                            var oneMeter = xModel.ModelFactors.OneMetre;
                            var translation = XbimMatrix3D.CreateTranslation(_modelTranslation*oneMeter);
                            var scaling = XbimMatrix3D.CreateScale(1/oneMeter);
                            XbimMatrix3D Transform = translation*scaling;

                            var m = new MeshGeometry3D();
                            GetGeometryFromXbimModel_IFC2x3(m, item, Transform);

                            var mat = GetStyleFromXbimModel_IFC2x3(item);

                            var mb = new MeshBuilder(false, false);

                            var element = VisualizeMesh_IFC2x3(mb, m, mat, item);
                            elementList.Add(element);
                        }
                        // Show whole building with opacity 0.03
                        else
                        {
                            XbimModelPositioningCollection ModelPositions = new XbimModelPositioningCollection();

                            short userDefinedId = 0;
                            xModel.UserDefinedId = userDefinedId;


                            ModelPositions.AddModel(xModel.ReferencingModel);

                            if (xModel.IsFederation)
                            {
                                foreach (var refModel in xModel.ReferencedModels)
                                {
                                    refModel.Model.UserDefinedId = ++userDefinedId;
                                    var v = refModel.Model as IfcStore;
                                    if (v != null)
                                        ModelPositions.AddModel(v.ReferencingModel);
                                }
                            }
                            
                            // Important for centralize the model in the viewer
                            var ModelBounds = ModelPositions.GetEnvelopeInMeters();

                            var p = ModelBounds.Centroid();
                            var _modelTranslation = new XbimVector3D(-p.X, -p.Y, -p.Z);
                            var oneMeter = xModel.ModelFactors.OneMetre;
                            var translation = XbimMatrix3D.CreateTranslation(_modelTranslation*oneMeter);
                            var scaling = XbimMatrix3D.CreateScale(1/oneMeter);
                            XbimMatrix3D Transform = translation*scaling;

                            var m = new MeshGeometry3D();
                            GetGeometryFromXbimModel_IFC2x3(m, item, Transform);
                            var mat = GetStyleFromXbimModel_IFC2x3(item, 0.03);

                            var mb = new MeshBuilder(false, false);

                            var element = VisualizeMesh_IFC2x3(mb, m, mat, item);
                            elementList.Add(element);
                        }

                    });

                    // Adds all elements to the viewport
                    foreach (var element in elementList)
                    {
                        if (element != null)
                            _viewPort.Children.Add(element);
                    }

                    break;

                case false:
                    foreach (var item in xModel.Instances.OfType<Xbim.Ifc2x3.Kernel.IfcProduct>())
                    {

                        if (res.Contains(item.GlobalId))
                        {
                            XbimModelPositioningCollection ModelPositions = new XbimModelPositioningCollection();

                            short userDefinedId = 0;
                            xModel.UserDefinedId = userDefinedId;


                            ModelPositions.AddModel(xModel.ReferencingModel);

                            if (xModel.IsFederation)
                            {
                                foreach (var refModel in xModel.ReferencedModels)
                                {
                                    refModel.Model.UserDefinedId = ++userDefinedId;
                                    var v = refModel.Model as IfcStore;
                                    if (v != null)
                                        ModelPositions.AddModel(v.ReferencingModel);
                                }
                            }
                            var ModelBounds = ModelPositions.GetEnvelopeInMeters();

                            var p = ModelBounds.Centroid();
                            var _modelTranslation = new XbimVector3D(-p.X, -p.Y, -p.Z);
                            var oneMeter = xModel.ModelFactors.OneMetre;
                            var translation = XbimMatrix3D.CreateTranslation(_modelTranslation * oneMeter);
                            var scaling = XbimMatrix3D.CreateScale(1 / oneMeter);
                            XbimMatrix3D Transform = translation * scaling;

                            var m = new MeshGeometry3D();
                            GetGeometryFromXbimModel_IFC2x3(m, item, Transform);

                            var mat = GetStyleFromXbimModel_IFC2x3(item);

                            var mb = new MeshBuilder(false, false);

                            var element = VisualizeMesh_IFC2x3(mb, m, mat, item);
                            elementList.Add(element);

                        }

                        // Show whole building with opacity 0.03
                        else
                        {
                            XbimModelPositioningCollection ModelPositions = new XbimModelPositioningCollection();

                            short userDefinedId = 0;
                            xModel.UserDefinedId = userDefinedId;


                            ModelPositions.AddModel(xModel.ReferencingModel);

                            if (xModel.IsFederation)
                            {
                                foreach (var refModel in xModel.ReferencedModels)
                                {
                                    refModel.Model.UserDefinedId = ++userDefinedId;
                                    var v = refModel.Model as IfcStore;
                                    if (v != null)
                                        ModelPositions.AddModel(v.ReferencingModel);
                                }
                            }
                            var ModelBounds = ModelPositions.GetEnvelopeInMeters();

                            var p = ModelBounds.Centroid();
                            var _modelTranslation = new XbimVector3D(-p.X, -p.Y, -p.Z);
                            var oneMeter = xModel.ModelFactors.OneMetre;
                            var translation = XbimMatrix3D.CreateTranslation(_modelTranslation * oneMeter);
                            var scaling = XbimMatrix3D.CreateScale(1 / oneMeter);
                            XbimMatrix3D Transform = translation * scaling;

                            var m = new MeshGeometry3D();
                            GetGeometryFromXbimModel_IFC2x3(m, item, Transform);
                            var mat = GetStyleFromXbimModel_IFC2x3(item, 0.03);

                            var mb = new MeshBuilder(false, false);

                            var element = VisualizeMesh_IFC2x3(mb, m, mat, item);
                            elementList.Add(element);
                        }
                    }
                    foreach (var element in elementList)
                    {
                        if (element != null)
                            _viewPort.Children.Add(element);
                    }
                    break;
            }
            
        }
        private void worker_DoWork_IFC4(IfcStore xModel, List<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId> elementIdsList)
        {
            // Loop through Entities and visualze them in the viewport
            var res = new HashSet<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId>(elementIdsList);

            List<ModelUIElement3D> elementList = new List<ModelUIElement3D>();

            foreach (var item in xModel.Instances.OfType<Xbim.Ifc4.Kernel.IfcProduct>())
            {
                if (res.Contains(item.GlobalId))
                {
                    XbimModelPositioningCollection ModelPositions = new XbimModelPositioningCollection();

                    short userDefinedId = 0;
                    xModel.UserDefinedId = userDefinedId;


                    ModelPositions.AddModel(xModel.ReferencingModel);

                    if (xModel.IsFederation)
                    {
                        foreach (var refModel in xModel.ReferencedModels)
                        {
                            refModel.Model.UserDefinedId = ++userDefinedId;
                            var v = refModel.Model as IfcStore;
                            if (v != null)
                                ModelPositions.AddModel(v.ReferencingModel);
                        }
                    }
                    var ModelBounds = ModelPositions.GetEnvelopeInMeters();

                    var p = ModelBounds.Centroid();
                    var _modelTranslation = new XbimVector3D(-p.X, -p.Y, -p.Z);
                    var oneMeter = xModel.ModelFactors.OneMetre;
                    var translation = XbimMatrix3D.CreateTranslation(_modelTranslation * oneMeter);
                    var scaling = XbimMatrix3D.CreateScale(1 / oneMeter);
                    XbimMatrix3D Transform = translation * scaling;


                    var m = new MeshGeometry3D();
                    GetGeometryFromXbimModel_IFC4(m, item, Transform);
                    var mat = GetStyleFromXbimModel_IFC4(item);

                    var mb = new MeshBuilder(false, false);

                    
                    var element = VisualizeMesh_IFC4(mb, m, mat, item);
                    elementList.Add(element);
                }

                // Show whole building with opacity 0.03
                else
                {
                    XbimModelPositioningCollection ModelPositions = new XbimModelPositioningCollection();

                    short userDefinedId = 0;
                    xModel.UserDefinedId = userDefinedId;


                    ModelPositions.AddModel(xModel.ReferencingModel);

                    if (xModel.IsFederation)
                    {
                        foreach (var refModel in xModel.ReferencedModels)
                        {
                            refModel.Model.UserDefinedId = ++userDefinedId;
                            var v = refModel.Model as IfcStore;
                            if (v != null)
                                ModelPositions.AddModel(v.ReferencingModel);
                        }
                    }
                    var ModelBounds = ModelPositions.GetEnvelopeInMeters();

                    var p = ModelBounds.Centroid();
                    var _modelTranslation = new XbimVector3D(-p.X, -p.Y, -p.Z);
                    var oneMeter = xModel.ModelFactors.OneMetre;
                    var translation = XbimMatrix3D.CreateTranslation(_modelTranslation * oneMeter);
                    var scaling = XbimMatrix3D.CreateScale(1 / oneMeter);
                    XbimMatrix3D Transform = translation * scaling;
                    var m = new MeshGeometry3D();
                    GetGeometryFromXbimModel_IFC4(m, item, Transform);
                    var mat = GetStyleFromXbimModel_IFC4(item, 0.03);

                    var mb = new MeshBuilder(false, false);

                    var element = VisualizeMesh_IFC4(mb, m, mat, item);
                    elementList.Add(element);
                }
            }
            foreach (var element in elementList)
            {
                if (element != null)
                    _viewPort.Children.Add(element);
            }

        }



        

        /// <summary>
        ///     VisualizeMesh in the Viewport
        /// </summary>
        /// <param name="meshBuilder"></param>
        /// <param name="mesh"></param>
        /// <param name="mat"></param>
        /// <param name="itemModel"></param>
        /// <param name="indexOfModel"></param>
        public ModelUIElement3D VisualizeMesh_IFC2x3(MeshBuilder meshBuilder, MeshGeometry3D mesh, Material mat, Xbim.Ifc2x3.Kernel.IfcProduct itemModel)
        {
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

            var myGeometryModel = new GeometryModel3D
            {               
                Material = mat,
                BackMaterial = mat,
                Geometry = meshBuilder.ToMesh(true)
            };

           
            var element = new ModelUIElement3D { Model = myGeometryModel };
            element.MouseDown += (sender1, e1) => OnElementMouseDown_IFC2x3(sender1, e1, this, itemModel);
            
            return element;
        }
        public ModelUIElement3D VisualizeMesh_IFC4(MeshBuilder meshBuilder, MeshGeometry3D mesh, Material mat, Xbim.Ifc4.Kernel.IfcProduct itemModel)
        {
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

            var myGeometryModel = new GeometryModel3D
            {               
                Material = mat,
                BackMaterial = mat,
                Geometry = meshBuilder.ToMesh(true)
            };

           
            var element = new ModelUIElement3D { Model = myGeometryModel };
            element.MouseDown += (sender1, e1) => OnElementMouseDown_IFC4(sender1, e1, this, itemModel);
            
            return element;
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
        protected void OnElementMouseDown_IFC2x3(object sender, MouseButtonEventArgs e, IfcViewerNode ifcParseGeometryNode, Xbim.Ifc2x3.Kernel.IfcProduct itemModel)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;
           
            var element = sender as ModelUIElement3D;

            if (element != null)
            {
                var geometryModel3D = element.Model as GeometryModel3D;
                if (geometryModel3D == null)
                    return;

                // If it is already selected ... Deselect
                if (SelectedProductsIFC2x3 != null && SelectedProductsIFC2x3.Contains(itemModel.GlobalId))
                {
                    geometryModel3D.Material = geometryModel3D.BackMaterial;
                    SelectedProductsIFC2x3.Remove(itemModel.GlobalId);
                }
                // If not ... Select!
                else
                {
                    SelectedProductsIFC2x3.Add(itemModel.GlobalId);
                    geometryModel3D.Material = _selectionMaterial;
                }
            }
            var ifcViewerControl = ControlElements[0] as IFCViewerControl;
            var button_2 = ifcViewerControl.RadioButton_2;

            if (button_2 == null) return;

            if ((bool)button_2.IsChecked)
            {
                var modelId = ((ModelInfoIFC2x3)(InputPorts[0].Data)).ModelId;
                ModelInfoIFC2x3 modelInfoIfc2X3 = new ModelInfoIFC2x3(modelId);
                foreach (var item in SelectedProductsIFC2x3)
                {
                    modelInfoIfc2X3.AddElementIds(item);
                }
                OutputPorts[0].Data = modelInfoIfc2X3;
            }
            
            e.Handled = true;
        }
        protected void OnElementMouseDown_IFC4(object sender, MouseButtonEventArgs e, IfcViewerNode ifcParseGeometryNode, Xbim.Ifc4.Kernel.IfcProduct itemModel)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;
           
            var element = sender as ModelUIElement3D;

            if (element != null)
            {
                var geometryModel3D = element.Model as GeometryModel3D;
                if (geometryModel3D == null)
                    return;

                if (SelectedProductsIFC4 != null && SelectedProductsIFC4.Contains(itemModel.GlobalId))
                {
                    geometryModel3D.Material = geometryModel3D.BackMaterial;
                    SelectedProductsIFC4.Remove(itemModel.GlobalId);
                }
                else
                {
                    SelectedProductsIFC4.Add(itemModel.GlobalId);
                    geometryModel3D.Material = _selectionMaterial;
                }
            }
            var ifcViewerControl = ControlElements[0] as IFCViewerControl;
            var button_2 = ifcViewerControl.RadioButton_2;

            if (button_2 == null) return;

            if ((bool)button_2.IsChecked)
            {
                var modelId = ((ModelInfoIFC4)(InputPorts[0].Data)).ModelId;
                ModelInfoIFC4 modelInfoIfc4 = new ModelInfoIFC4(modelId);
                foreach (var item in SelectedProductsIFC4)
                {
                    modelInfoIfc4.AddElementIds(item);
                }
                OutputPorts[0].Data = modelInfoIfc4;
            }

            e.Handled = true;
        }

        /// <summary>
        /// Get Style of each Item
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Material GetStyleFromXbimModel_IFC2x3(Xbim.Ifc2x3.Kernel.IfcProduct item, double opacity = 1)
        {
            var context = new Xbim3DModelContext(item.Model);

            var productShape = context.ShapeInstancesOf((Xbim.Ifc4.Interfaces.IIfcProduct)item)
                .Where(s => s.RepresentationType != XbimGeometryRepresentationType.OpeningsAndAdditionsExcluded)
                .ToList();
            Material wpfMaterial = null;
            if (productShape.Count > 0)
            {
                wpfMaterial = GetWpfMaterial(item.Model, productShape[0].StyleLabel);
            }
            else
            {
                wpfMaterial = GetWpfMaterial(item.Model, 0);
            }

            ((System.Windows.Media.Media3D.DiffuseMaterial)wpfMaterial).Brush.Opacity = opacity;
            return wpfMaterial;
        }

        public Material GetStyleFromXbimModel_IFC4(Xbim.Ifc4.Kernel.IfcProduct item, double opacity = 1)
        {
            var context = new Xbim3DModelContext(item.Model);

            var productShape = context.ShapeInstancesOf((Xbim.Ifc4.Interfaces.IIfcProduct)item)
                .Where(s => s.RepresentationType != XbimGeometryRepresentationType.OpeningsAndAdditionsExcluded)
                .ToList();

            Material wpfMaterial = null;
            if (productShape.Count > 0)
            {
                wpfMaterial = GetWpfMaterial(item.Model, productShape[0].StyleLabel);
            }
            else
            {
                wpfMaterial = GetWpfMaterial(item.Model, 0);
            }

            ((System.Windows.Media.Media3D.DiffuseMaterial)wpfMaterial).Brush.Opacity = opacity;
            return wpfMaterial;
        }


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
        /// <summary>
        /// Get the Material of a given element
        /// </summary>
        /// <param name="model"></param>
        /// <param name="styleId"></param>
        /// <returns></returns>
        private static Material GetWpfMaterial(IModel model, int styleId)
        {
            var sStyle = model.Instances[styleId] as Xbim.Ifc4.Interfaces.IIfcSurfaceStyle;
                            var wpfMaterial = new WpfMaterial();

            if (sStyle != null)
            {
                var texture = XbimTexture.Create(sStyle);
                texture.DefinedObjectId = styleId;
                wpfMaterial.CreateMaterial(texture);

                return wpfMaterial;
            }
            else
            {
                var defautMaterial = Xbim.Presentation.ModelDataProvider.DefaultMaterials;

                Material material;
                if (defautMaterial.TryGetValue(model.GetType().Name, out material))
                {
                    //((System.Windows.Media.Media3D.DiffuseMaterial)material).Brush.Opacity = opacity;
                    return material;
                }
                else
                {
                    XbimColour color = new XbimColour("red",1,1,1);
                    wpfMaterial.CreateMaterial(color);
                    return wpfMaterial;
                    // return defautMaterial["IfcProduct"];
                    //((System.Windows.Media.Media3D.DiffuseMaterial)mat).Brush.Opacity = opacity;

                }
            }
        }
        public override Node Clone()
        {
            return new IfcViewerNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}