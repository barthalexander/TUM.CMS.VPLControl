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
using Xbim.IO;
using Xbim.ModelGeometry.Scene;
using Xbim.Presentation;
using Xbim.XbimExtensions;
using System.Windows.Input;
using System.Reflection;
using System.Collections;
using System.ComponentModel;
using TUM.CMS.VplControl.IFC.Utilities;
using Xbim.Common;
using Xbim.Common.Enumerations;
using Xbim.Ifc;
using Xbim.IO.Esent;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcEnergyCalculateVisualizeTT : Node
    {
        private readonly HelixViewport3D _viewPort;
        private IfcStore _xModel;
        private BackgroundWorker worker;



        public IfcEnergyCalculateVisualizeTT(Core.VplControl hostCanvas) : base(hostCanvas)
        {
            // Init UI
            IsResizeable = true;

            AddInputPortToNode("Model", typeof(string));
            _viewPort = new HelixViewport3D
            {
                MinWidth = 520,
                MinHeight = 520
            };

            AddControlToNode(_viewPort);
        }

        public override void Calculate()
        {
            // Init the viewport
            _viewPort.Children.Clear();
            _viewPort.Children.Add(new SunLight());

            var IfcVersionType = InputPorts[0].Data.GetType();
            if (IfcVersionType.Name == "ModelInfoIFC2x3")
            {
                var modelId = ((ModelInfoIFC2x3)(InputPorts[0].Data)).ModelId;
                if (modelId == null) return;

                _xModel = DataController.Instance.GetModel(modelId, true);

                var context = new Xbim3DModelContext(_xModel);
                //upgrade to new geometry representation, use the default 3D model
                context.CreateContext();

                worker_DoWork_IFC2x3(_xModel);
                worker = new BackgroundWorker();
            }
            else
            {
                var modelId = ((ModelInfoIFC4)(InputPorts[0].Data)).ModelId;
                if (modelId == null) return;

                _xModel = DataController.Instance.GetModel(modelId, true);

                var context = new Xbim3DModelContext(_xModel);
                //upgrade to new geometry representation, use the default 3D model
                context.CreateContext();

                worker_DoWork_IFC4(_xModel);
                worker = new BackgroundWorker();
            }

            

        }

        private void worker_DoWork_IFC2x3(IfcStore xModel)
        {
            //in TTValueColor hashTable we will have <TT,Material(Color)> key-value pairs
            Hashtable TTValueColor = new Hashtable();
            
            //get all colors available in a list
            var _Colors = GetStaticPropertyBag(typeof(Colors)).ToList();
            Console.WriteLine("Total of " + _Colors.Count + " Colors available!!");

            DiffuseMaterial NoTTMaterial = new DiffuseMaterial(new SolidColorBrush((Color)_Colors[_Colors.Count - 1].Value));//in case of no TT being available the Color used will be the last one in the list
            
            // Loop through Entities and visualize them in the viewport
            int count = 0;
            Console.WriteLine("The elements r in total " + _xModel.Instances.OfType<Xbim.Ifc2x3.Kernel.IfcProduct>().Count() + ".");
            foreach (var item in _xModel.Instances.OfType<Xbim.Ifc2x3.Kernel.IfcProduct>())
            {
                // Console.WriteLine(count++);

                var m = new MeshGeometry3D();
                GetGeometryFromXbimModel_IFC2x3(m, item, XbimMatrix3D.Identity);
                // string itemtype = item.GetType().ToString();//...

                double TT = GetTTifExistsCalculateifNot_IFC2x3(item);//it TT-property does not exist, it calculates it
                                                              // Console.WriteLine("TT = "+TT);
                DiffuseMaterial Material = null;
                if (TTValueColor.ContainsKey(TT))
                    Material = TTValueColor[TT] as DiffuseMaterial;
                else  //new TT - Material(Color) pair
                {
                    if (TT == -1)//the TT-Material pair when TT is not available...
                    {
                        Material = NoTTMaterial;//

                    }
                    else
                    {
                        Material = new DiffuseMaterial(new SolidColorBrush((Color)_Colors[TTValueColor.Count].Value));
                    }
                    TTValueColor.Add(TT, Material);
                }

                var mb = new MeshBuilder(false, false);
                VisualizeMesh_IFC2x3(mb, m, Material, item);

                Console.WriteLine("The colors that have actually been used r " + TTValueColor.Keys.Count + " (distinct TT-Values).");
            }
        }

        private void worker_DoWork_IFC4(IfcStore xModel)
        {
            //in TTValueColor hashTable we will have <TT,Material(Color)> key-value pairs
            Hashtable TTValueColor = new Hashtable();

            //get all colors available in a list
            var _Colors = GetStaticPropertyBag(typeof(Colors)).ToList();
            Console.WriteLine("Total of " + _Colors.Count + " Colors available!!");

            DiffuseMaterial NoTTMaterial = new DiffuseMaterial(new SolidColorBrush((Color)_Colors[_Colors.Count - 1].Value));//in case of no TT being available the Color used will be the last one in the list

            // Loop through Entities and visualize them in the viewport
            int count = 0;
            Console.WriteLine("The elements r in total " + _xModel.Instances.OfType<Xbim.Ifc4.Kernel.IfcProduct>().Count() + ".");
            foreach (var item in _xModel.Instances.OfType<Xbim.Ifc4.Kernel.IfcProduct>())
            {
                // Console.WriteLine(count++);

                var m = new MeshGeometry3D();
                GetGeometryFromXbimModel_IFC4(m, item, XbimMatrix3D.Identity);
                // string itemtype = item.GetType().ToString();//...

                double TT = GetTTifExistsCalculateifNot_IFC4(item);//it TT-property does not exist, it calculates it
                                                                     // Console.WriteLine("TT = "+TT);
                DiffuseMaterial Material = null;
                if (TTValueColor.ContainsKey(TT))
                    Material = TTValueColor[TT] as DiffuseMaterial;
                else  //new TT - Material(Color) pair
                {
                    if (TT == -1)//the TT-Material pair when TT is not available...
                    {
                        Material = NoTTMaterial;//

                    }
                    else
                    {
                        Material = new DiffuseMaterial(new SolidColorBrush((Color)_Colors[TTValueColor.Count].Value));
                    }
                    TTValueColor.Add(TT, Material);
                }

                var mb = new MeshBuilder(false, false);
                VisualizeMesh_IFC4(mb, m, Material, item);

                Console.WriteLine("The colors that have actually been used r " + TTValueColor.Keys.Count + " (distinct TT-Values).");
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

        public double GetTTifExistsCalculateifNot_IFC2x3(Xbim.Ifc2x3.Kernel.IfcProduct ifcProduct)
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
            //if it comes up 2 here, no TT was found... we r going 2 calculate it...
            if (ifcProduct.GetType() == typeof(Xbim.Ifc2x3.SharedBldgElements.IfcColumn))
                return CalculateTT_IFC2x3((Xbim.Ifc2x3.SharedBldgElements.IfcColumn)ifcProduct);
            else if (ifcProduct.GetType() == typeof(Xbim.Ifc2x3.SharedBldgElements.IfcDoor))
                return CalculateTT_IFC2x3((Xbim.Ifc2x3.SharedBldgElements.IfcDoor)ifcProduct);
            else if (ifcProduct.GetType() == typeof(Xbim.Ifc2x3.SharedBldgElements.IfcCurtainWall))
                return CalculateTT_IFC2x3((Xbim.Ifc2x3.SharedBldgElements.IfcCurtainWall)ifcProduct);

            return -1;//TT property was not found in this element...
        }
        public double GetTTifExistsCalculateifNot_IFC4(Xbim.Ifc4.Kernel.IfcProduct ifcProduct)
        {
            //bool found = false;
            var propertySets = ifcProduct.PropertySets.ToList();
            int ii = 0;
            while (ii < propertySets.Count)
            {
                var onepropertySet = propertySets[ii].HasProperties.ToList();
                int jj = 0;
                while (jj < onepropertySet.Count)
                {
                    if (onepropertySet[jj].Name == "ThermalTransmittance")
                    {
                        return (double)((Xbim.Ifc4.PropertyResource.IfcPropertySingleValue)onepropertySet[jj]).NominalValue.Value;
                    }
                    jj++;
                }
                ii++;
            }
            //if it comes up 2 here, no TT was found... we r going 2 calculate it...
            if (ifcProduct.GetType() == typeof(Xbim.Ifc4.SharedBldgElements.IfcColumn))
                return CalculateTT_IFC4((Xbim.Ifc4.SharedBldgElements.IfcColumn)ifcProduct);
            else if (ifcProduct.GetType() == typeof(Xbim.Ifc4.SharedBldgElements.IfcDoor))
                return CalculateTT_IFC4((Xbim.Ifc4.SharedBldgElements.IfcDoor)ifcProduct);
            else if (ifcProduct.GetType() == typeof(Xbim.Ifc4.SharedBldgElements.IfcCurtainWall))
                return CalculateTT_IFC4((Xbim.Ifc4.SharedBldgElements.IfcCurtainWall)ifcProduct);

            return -1;//TT property was not found in this element...
        }


        public double CalculateTT_IFC2x3(Xbim.Ifc2x3.SharedBldgElements.IfcColumn ifcProduct)
        {
            try
            {
                Console.WriteLine("** This is a Column **");
                var ifcVolume = ifcProduct.PropertySets.ToList()[2].HasProperties.ToList()[2];//Volumen
                var ifcArea = ifcProduct.PropertySets.ToList()[2].HasProperties.ToList()[0];//Flache
                var volume = ifcVolume as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                var volumeValue = volume.NominalValue;
                object volumeValueTrue = volume.NominalValue.Value;
                double volumeVal = (double)volumeValueTrue;
                var area = ifcArea as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                var areaValue = area.NominalValue;
                Console.WriteLine("##Flache:" + areaValue + " -- Volumen:" + volumeVal);
                object areaValueTrue = area.NominalValue.Value;
                double areaVal = (double)areaValueTrue;//
                return (volumeVal / areaVal);
            }
            catch (System.InvalidCastException castwrong)
            {
                Console.WriteLine(ifcProduct.Tag + " although being " + ifcProduct.GetType() + ", it does not have the Flache and Volumen Properties where it should... Therefore we choose to ignore it...");
            }
            catch (System.ArgumentOutOfRangeException notenough)
            {
                Console.WriteLine(ifcProduct.Tag + " although being " + ifcProduct.GetType() + ", it does not have its Properties where it should... Therefore we choose to ignore it...");
            }
            return -1;//in case smth goes wrong...
        }
        public double CalculateTT_IFC2x3(Xbim.Ifc2x3.SharedBldgElements.IfcDoor ifcProduct)
        {
            try
            {
                Console.WriteLine("** This is a Door **");
                var ifcVolume = ifcProduct.PropertySets.ToList()[2].HasProperties.ToList()[2];//Volumen
                var ifcArea = ifcProduct.PropertySets.ToList()[2].HasProperties.ToList()[0];//Flache
                var volume = ifcVolume as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                var volumeValue = volume.NominalValue;
                object volumeValueTrue = volume.NominalValue.Value;
                double volumeVal = (double)volumeValueTrue;
                var area = ifcArea as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                var areaValue = area.NominalValue;
                Console.WriteLine("##Flache:" + areaValue + " -- Volumen:" + volumeVal);
                object areaValueTrue = area.NominalValue.Value;
                double areaVal = (double)areaValueTrue;//
                return (volumeVal / areaVal);
            }
            catch (System.InvalidCastException castwrong)
            {
                Console.WriteLine(ifcProduct.Tag + " although being " + ifcProduct.GetType() + ", it does not have the Flache and Volumen Properties where it should... Therefore we choose to ignore it...");
            }
            catch (System.ArgumentOutOfRangeException notenough)
            {
                Console.WriteLine(ifcProduct.Tag + " although being " + ifcProduct.GetType() + ", it does not have its Properties where it should... Therefore we choose to ignore it...");
            }
            return -1;//in case smth goes wrong...
        }
        public double CalculateTT_IFC2x3(Xbim.Ifc2x3.SharedBldgElements.IfcCurtainWall ifcProduct)
        {
            try
            {
                Console.WriteLine("** This is a CurtainWall **");
                var ifcVolume = ifcProduct.PropertySets.ToList()[2].HasProperties.ToList()[2];//is it the right property..?-->Yes (Volumen)
                var ifcArea = ifcProduct.PropertySets.ToList()[2].HasProperties.ToList()[0];//is it the right property..?-->Yes (Flache)
                var volume = ifcVolume as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                var volumeValue = volume.NominalValue;
                object volumeValueTrue = volume.NominalValue.Value;
                double volumeVal = (double)volumeValueTrue;
                var area = ifcArea as Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue;
                var areaValue = area.NominalValue;
                Console.WriteLine("##Flache:" + areaValue + " -- Volumen:" + volumeVal);
                object areaValueTrue = area.NominalValue.Value;
                double areaVal = (double)areaValueTrue;//
                return (volumeVal / areaVal);
            }
            catch (System.InvalidCastException castwrong)
            {
                Console.WriteLine(ifcProduct.Tag + " although being " + ifcProduct.GetType() + ", it does not have the Flache and Volumen Properties where it should... Therefore we choose to ignore it...");
            }
            catch (System.ArgumentOutOfRangeException notenough)
            {
                Console.WriteLine(ifcProduct.Tag + " although being " + ifcProduct.GetType() + ", it does not have its Properties where it should... Therefore we choose to ignore it...");
            }
            return -1;//in case smth goes wrong...
        }

        public double CalculateTT_IFC4(Xbim.Ifc4.SharedBldgElements.IfcColumn ifcProduct)
        {
            try
            {
                Console.WriteLine("** This is a Column **");
                var ifcVolume = ifcProduct.PropertySets.ToList()[2].HasProperties.ToList()[2];//Volumen
                var ifcArea = ifcProduct.PropertySets.ToList()[2].HasProperties.ToList()[0];//Flache
                var volume = ifcVolume as Xbim.Ifc4.PropertyResource.IfcPropertySingleValue;
                var volumeValue = volume.NominalValue;
                object volumeValueTrue = volume.NominalValue.Value;
                double volumeVal = (double)volumeValueTrue;
                var area = ifcArea as Xbim.Ifc4.PropertyResource.IfcPropertySingleValue;
                var areaValue = area.NominalValue;
                Console.WriteLine("##Flache:" + areaValue + " -- Volumen:" + volumeVal);
                object areaValueTrue = area.NominalValue.Value;
                double areaVal = (double)areaValueTrue;//
                return (volumeVal / areaVal);
            }
            catch (System.InvalidCastException castwrong)
            {
                Console.WriteLine(ifcProduct.Tag + " although being " + ifcProduct.GetType() + ", it does not have the Flache and Volumen Properties where it should... Therefore we choose to ignore it...");
            }
            catch (System.ArgumentOutOfRangeException notenough)
            {
                Console.WriteLine(ifcProduct.Tag + " although being " + ifcProduct.GetType() + ", it does not have its Properties where it should... Therefore we choose to ignore it...");
            }
            return -1;//in case smth goes wrong...
        }
        public double CalculateTT_IFC4(Xbim.Ifc4.SharedBldgElements.IfcDoor ifcProduct)
        {
            try
            {
                Console.WriteLine("** This is a Door **");
                var ifcVolume = ifcProduct.PropertySets.ToList()[2].HasProperties.ToList()[2];//Volumen
                var ifcArea = ifcProduct.PropertySets.ToList()[2].HasProperties.ToList()[0];//Flache
                var volume = ifcVolume as Xbim.Ifc4.PropertyResource.IfcPropertySingleValue;
                var volumeValue = volume.NominalValue;
                object volumeValueTrue = volume.NominalValue.Value;
                double volumeVal = (double)volumeValueTrue;
                var area = ifcArea as Xbim.Ifc4.PropertyResource.IfcPropertySingleValue;
                var areaValue = area.NominalValue;
                Console.WriteLine("##Flache:" + areaValue + " -- Volumen:" + volumeVal);
                object areaValueTrue = area.NominalValue.Value;
                double areaVal = (double)areaValueTrue;//
                return (volumeVal / areaVal);
            }
            catch (System.InvalidCastException castwrong)
            {
                Console.WriteLine(ifcProduct.Tag + " although being " + ifcProduct.GetType() + ", it does not have the Flache and Volumen Properties where it should... Therefore we choose to ignore it...");
            }
            catch (System.ArgumentOutOfRangeException notenough)
            {
                Console.WriteLine(ifcProduct.Tag + " although being " + ifcProduct.GetType() + ", it does not have its Properties where it should... Therefore we choose to ignore it...");
            }
            return -1;//in case smth goes wrong...
        }
        public double CalculateTT_IFC4(Xbim.Ifc4.SharedBldgElements.IfcCurtainWall ifcProduct)
        {
            try
            {
                Console.WriteLine("** This is a CurtainWall **");
                var ifcVolume = ifcProduct.PropertySets.ToList()[2].HasProperties.ToList()[2];//is it the right property..?-->Yes (Volumen)
                var ifcArea = ifcProduct.PropertySets.ToList()[2].HasProperties.ToList()[0];//is it the right property..?-->Yes (Flache)
                var volume = ifcVolume as Xbim.Ifc4.PropertyResource.IfcPropertySingleValue;
                var volumeValue = volume.NominalValue;
                object volumeValueTrue = volume.NominalValue.Value;
                double volumeVal = (double)volumeValueTrue;
                var area = ifcArea as Xbim.Ifc4.PropertyResource.IfcPropertySingleValue;
                var areaValue = area.NominalValue;
                Console.WriteLine("##Flache:" + areaValue + " -- Volumen:" + volumeVal);
                object areaValueTrue = area.NominalValue.Value;
                double areaVal = (double)areaValueTrue;//
                return (volumeVal / areaVal);
            }
            catch (System.InvalidCastException castwrong)
            {
                Console.WriteLine(ifcProduct.Tag + " although being " + ifcProduct.GetType() + ", it does not have the Flache and Volumen Properties where it should... Therefore we choose to ignore it...");
            }
            catch (System.ArgumentOutOfRangeException notenough)
            {
                Console.WriteLine(ifcProduct.Tag + " although being " + ifcProduct.GetType() + ", it does not have its Properties where it should... Therefore we choose to ignore it...");
            }
            return -1;//in case smth goes wrong...
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
        
        public bool VisualizeMesh_IFC2x3(MeshBuilder meshBuilder, MeshGeometry3D mesh, Material mat, Xbim.Ifc2x3.Kernel.IfcProduct itemModel)
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


            // Add the Mesh to the ViewPort

            _viewPort.Children.Add(element);

            // Do all UI related work here... }






            return true;
        }
        public bool VisualizeMesh_IFC4(MeshBuilder meshBuilder, MeshGeometry3D mesh, Material mat, Xbim.Ifc4.Kernel.IfcProduct itemModel)
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


            // Add the Mesh to the ViewPort

            _viewPort.Children.Add(element);

            // Do all UI related work here... }






            return true;
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

            var productShape = context.ShapeInstancesOf((Xbim.Ifc4.Interfaces.IIfcProduct)item)
                .Where(s => s.RepresentationType != XbimGeometryRepresentationType.OpeningsAndAdditionsExcluded)
                .ToList();
            if (!productShape.Any() && item is Xbim.Ifc2x3.Interfaces.IIfcFeatureElement)
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
