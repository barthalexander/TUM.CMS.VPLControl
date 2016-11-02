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
using Xbim.Ifc2x3.Kernel;
using Xbim.Ifc2x3.ProductExtension;
using Xbim.Ifc2x3.UtilityResource;
using Xbim.ModelGeometry.Scene;
using Xbim.Presentation;
using TUM.CMS.VplControl.IFC.Utilities;
using System.Reflection;
using System.Collections;
using System.Windows.Input;
using Xbim.Common.Enumerations;
using Xbim.Ifc;
using System.Windows.Controls;
using Xbim.Ifc2x3.IO;
using Xbim.IO.Esent;

using System.Collections;
using System.Drawing;
using Xbim.Common;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcEnergyVisualizeTT : Node
    {
        private readonly HelixViewport3D _viewPort;
        public IfcStore XModel;

        public List<ModelInfoIFC2x3> ModelListIFC2x3;
        public List<ModelInfoIFC2x3> ModelListAll2x3;

        private readonly Material _selectionMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Crimson));
        

        private Hashtable TTValueColor;
        private readonly Color NoTTColor = Colors.Red;
        private List<KeyValuePair<String, object>> _Colors;

        public IfcEnergyVisualizeTT(Core.VplControl hostCanvas) : base(hostCanvas)
        {
            // Init UI
            IsResizeable = true;

            AddInputPortToNode("Model", typeof(string));
            AddOutputPortToNode("SelectedEntities", typeof(List<IfcGloballyUniqueId>));

            _viewPort = new HelixViewport3D
            {
                MinWidth = 520,
                MinHeight = 520
            };
            RadioButton radioButton_1 = new RadioButton { Content = "pass all" };
            RadioButton radioButton_2 = new RadioButton { Content = "pass selected", IsChecked = true };
            //create a bar
            ProgressBar ColorBar = new ProgressBar();//creates a new progress bar, it can be any other control, but this will work

            ColorBar.Height = 20;//defines the height
            ColorBar.Width = 400;//defines the width
            //ColorBar.Background=new Color(Color)
            ColorBar.Value = 0;//just keeps he progress bar empty
            _Colors = GetStaticPropertyBag(typeof(Colors)).ToList();

            AddControlToNode(_viewPort);
            AddControlToNode(radioButton_1);
            AddControlToNode(radioButton_2);
            AddControlToNode(ColorBar);
        }

        public override void Calculate()
        {
            //in TTValueColor hashTable we will have <TT,Material(Color)> key-value pairs
            TTValueColor = new Hashtable();
            //get all colors available in a list
            Console.WriteLine("Total of " + _Colors.Count + " Colors available!!");
            //NoTTMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Red));//when TT is not available we use Red
            OutputPorts[0].Data = null;
            var button_1 = ControlElements[1] as RadioButton;
            if (button_1 == null) return;
            var button_2 = ControlElements[2] as RadioButton;
            if (button_2 == null) return;
            // Init the viewport

            _viewPort.Children.Clear();
            _viewPort.Children.Add(new SunLight());
            ModelListIFC2x3 = new List<ModelInfoIFC2x3>();
            ModelListAll2x3 = new List<ModelInfoIFC2x3>();
            Type t = InputPorts[0].Data.GetType();
            if (t.IsGenericType)
            {
                var collection = InputPorts[0].Data as ICollection;
                if (collection != null)
                    foreach (var model in collection)
                    {
                        var modelId = ((ModelInfo)(model)).ModelId;
                        var elementIdsList = ((ModelInfo)(model)).ElementIds;
                        XModel = DataController.Instance.GetModel(modelId, true);

                        ModelListIFC2x3.Add(new ModelInfoIFC2x3(modelId));
                        ModelListAll2x3.Add(new ModelInfoIFC2x3(modelId));
                        int indexOfModel = 0;
                        foreach (var item in ModelListIFC2x3)
                        {
                            if (item.ModelId == modelId)
                            {
                                indexOfModel = ModelListIFC2x3.IndexOf(item);
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

                        var context = new Xbim3DModelContext(XModel);
                        //upgrade to new geometry represenation, uses the default 3D model
                        context.CreateContext();
                        worker_DoWork(XModel, indexOfModel, elementIdsList);
                        button_1.Checked += (sender, e) => button_1_Checked(sender, e, elementIdsList, indexOfModel);
                        
                    }
            }
            else
            {
                var file = InputPorts[0].Data.ToString();

                var modelId = ((ModelInfoIFC2x3)(InputPorts[0].Data)).ModelId;
                var elementIdsList = ((ModelInfoIFC2x3)(InputPorts[0].Data)).ElementIds;
                if (modelId == null) return;
                int indexOfModel = 0;
                foreach (var item in ModelListIFC2x3)
                {
                    if (item.ModelId == modelId)
                    {
                        indexOfModel = ModelListIFC2x3.IndexOf(item);
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
                XModel = DataController.Instance.GetModel(modelId, true);

                ModelListIFC2x3.Add(new ModelInfoIFC2x3(modelId));
                ModelListAll2x3.Add(new ModelInfoIFC2x3(modelId));
                var context = new Xbim3DModelContext(XModel);
                //upgrade to new geometry represenation, uses the default 3D model
                context.CreateContext();


                worker_DoWork(XModel, indexOfModel, elementIdsList);
                button_1.Checked += (sender, e) => button_1_Checked(sender, e, elementIdsList, indexOfModel);

            }
        }



        private void button_1_Checked(object sender, RoutedEventArgs e, List<IfcGloballyUniqueId> elementIdsList, int indexOfModel)
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

        private void worker_DoWork(IfcStore xModel, int indexOfModel, List<IfcGloballyUniqueId> elementIdsList)
        {
            // Loop through Entities and visualize them in the viewport
            var res = new HashSet<IfcGloballyUniqueId>(elementIdsList);
            // xModel = (XbimModel) e.Argument;
            foreach (var item in xModel.Instances.OfType<IfcProduct>())
            {
                if (res.Contains(item.GlobalId))
                {
                    var m = new MeshGeometry3D();
                    GetGeometryFromXbimModel_IFC4(m, item, XbimMatrix3D.Identity);
                    double TT = GetTTifExists(item);//it TT-property does not exist, it returns -1
             
                    DiffuseMaterial Material = null;
                    Color thisColor;
                    if (TTValueColor.ContainsKey(TT))
                        thisColor = (Color)TTValueColor[TT];
                    else  //new TT - Material(Color) pair
                    {
                        if (TT == -1)//the TT-Material pair when TT is not available...
                        {
                            thisColor = NoTTColor;
                        }
                        else
                        {
                            thisColor = (Color)_Colors[TTValueColor.Count].Value;

                        }
                        TTValueColor.Add(TT, thisColor);
                    }
                    var mb = new MeshBuilder(false, false);
                    VisualizeMesh(mb, m, Material);
                    //VisualizeMesh(mb, m, mat, item, indexOfModel);
                }

                // Show whole building with opacity 0.03
                else
                {
                    var m = new MeshGeometry3D();
                    GetGeometryFromXbimModel_IFC4(m, item, XbimMatrix3D.Identity);
                    double TT = GetTTifExists(item);//it TT-property does not exist, it returns -1

                    //var mat = GetStyleFromXbimModel(item, 0.03);
                    DiffuseMaterial Material = null;
                    Color thisColor;
                    if (TTValueColor.ContainsKey(TT))
                        thisColor = (Color)TTValueColor[TT];
                    else  //new TT - Material(Color) pair
                    {
                        if (TT == -1)//the TT-Material pair when TT is not available...
                        {
                            thisColor = NoTTColor;
                        }
                        else
                        {
                            thisColor = (Color)_Colors[TTValueColor.Count].Value;
                        }
                        TTValueColor.Add(TT, thisColor);
                    }
                    Material = new DiffuseMaterial(new SolidColorBrush(thisColor));
                    var mb = new MeshBuilder(false, false);
                    VisualizeMesh(mb, m, Material);
                    //VisualizeMesh(mb, m, mat, item, indexOfModel);
                }

                Console.WriteLine("The colors that are actually being used are " + TTValueColor.Keys.Count + " (distinct TT-Values).");
                List<double> SortedTTs = TTValueColor.Keys.OfType<double>().ToList();
                SortedTTs.Sort();
                SortedTTs.RemoveAt(0);//remove the NoTTColor (Red)

                GradientStopCollection colorsCollection = new GradientStopCollection();

                double i = 0;
                double inc = 1.0 / SortedTTs.Count;
                foreach (double c in SortedTTs)
                {
                    Color col = (Color)TTValueColor[c];
                    colorsCollection.Add(new GradientStop(col, i));
                    i += inc;
                    Console.WriteLine("**** TT of " + c + " is in " + col + "****");
                }

                LinearGradientBrush colors = new LinearGradientBrush(colorsCollection, 0);

                ProgressBar ColorBar = ControlElements[4] as ProgressBar;
                ColorBar.Background = colors;

            }
        }


        public /*static*/ Dictionary<string, object> GetStaticPropertyBag(Type t)
        {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            var map = new Dictionary<string, object>();
            foreach (var prop in t.GetProperties(flags))
            {
                map[prop.Name] = prop.GetValue(null, null);
            }
            return map;
        }

        public double GetTTifExists(Xbim.Ifc2x3.Kernel.IfcProduct ifcProduct)
        {
            //bool found = false;
            var propertySets = ifcProduct.PropertySets.ToList() as List<Xbim.Ifc2x3.Kernel.IfcPropertySet>;
            int ii = 0;
            while (ii < propertySets.Count)
            {
                var onepropertySet = propertySets[ii].HasProperties.ToList();
                int jj = 0;
                while (jj < onepropertySet.Count)
                {
                    if (onepropertySet[jj].Name == "ThermalTransmittance")
                    {
                        return (double)((Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue)onepropertySet[jj]).NominalValue.Value;
                    }
                    jj++;
                }
                ii++;
            }
            return -1;//TT property was not found in this element...
        }


        public override Node Clone()
        {
            return new IfcParseGeometryNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }

     
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


            var myGeometryModel = new GeometryModel3D
            {
                //Material = new DiffuseMaterial(new SolidColorBrush(Colors.Aqua)),
                //BackMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Red)),
                Material = mat,
                Geometry = meshBuilder.ToMesh(true)
                // In case that you have to rotate the model ... 
                // Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 90))
            };

            var element = new ModelUIElement3D { Model = myGeometryModel };

            // Add the Mesh to the ViewPort
            _viewPort.Children.Add(element);

            return true;
        }
      
        protected void OnElementMouseDown(object sender, MouseButtonEventArgs e, IfcEnergyVisualizeTT ifcParseGeometryNode, IfcProduct itemModel, int indexOfModel)
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
                if (ModelListIFC2x3[indexOfModel].ElementIds.Contains(itemModel.GlobalId))
                {
                    geometryModel3D.Material = geometryModel3D.BackMaterial;
                    ModelListIFC2x3[indexOfModel].ElementIds.Remove(itemModel.GlobalId);
                }
                // If not ... Select!
                else
                {
                    ModelListIFC2x3[indexOfModel].AddElementIds(itemModel.GlobalId);
                    geometryModel3D.Material = _selectionMaterial;
                }
            }

            var button_2 = ControlElements[2] as RadioButton;
            if (button_2 == null) return;
            if ((bool)button_2.IsChecked)
            {
                if (ModelListIFC2x3.Count == 1)
                {
                    OutputPorts[0].Data = ModelListIFC2x3[0];

                }
                // Set selected models to Output ...  
                if (ModelListIFC2x3.Count > 1)
                {
                    OutputPorts[0].Data = ModelListIFC2x3;

                }

            }

            // button_2.Checked += (sender2,e2)=>button_2_Checked(sender2,e2,indexOfModel);

            e.Handled = true;
        }
        private void button_2_Checked(object sender2, RoutedEventArgs e2, int indexOfModel)
        {

        }

        //check here
        public void GetGeometryFromXbimModel_IFC4(MeshGeometry3D m, IPersistEntity item, XbimMatrix3D wcsTransform)
        {
            if (item.Model == null || !(item is Xbim.Ifc4.Kernel.IfcProduct)) return;

            var context = new Xbim3DModelContext(item.Model);

            var productShape = context.ShapeInstancesOf((Xbim.Ifc4.Interfaces.IIfcProduct)item)
                .Where(s => s.RepresentationType != XbimGeometryRepresentationType.OpeningsAndAdditionsExcluded)
                .ToList();
            if (!productShape.Any() && item is Xbim.Ifc4.Interfaces.IIfcFeatureElement)
            {
                productShape = context.ShapeInstancesOf((Xbim.Ifc4.Interfaces.IIfcProduct)item)
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
    
