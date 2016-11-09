using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using TUM.CMS.VplControl.Core;
using Xbim.Common.Geometry;
using Xbim.ModelGeometry.Scene;
using Xbim.Presentation;
using System.Reflection;
using System.Collections;
using System.ComponentModel;
using TUM.CMS.VplControl.IFC.Utilities;
using Xbim.Common;
using Xbim.Ifc;
using System.Windows.Controls;
using Xbim.Ifc2x3.UtilityResource;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class EnergyVisualisationNode : Node
    {
        public const double Rsi = 0.13;
        public const double Rse_1 = 0.04;
        public const double Rse_2 = 0.08;
        public const double Rse_3 = 0.00;

        //CHOSEN VALUES
        public const double l = 1;
        public const double Rse = Rse_1;

        private readonly HelixViewport3D _viewPort;
        private IfcStore _xModel;
        private BackgroundWorker worker;

        private Hashtable TTValueColorAll;
        private Hashtable TTValueColorExistin;
        private Hashtable TTValueColorCalculated;
        private List<KeyValuePair<String, object>> _Colors;
        private Color TTCannotBeCalculated = Colors.Black;//when TT is not available NOR can be calculated we use Black
                                                          /*   private Color TTSmaller = Colors.Green;
                                                               private Color TTBigger = Colors.Red;
                                                          */
        public EnergyVisualisationNode(Core.VplControl hostCanvas) : base(hostCanvas)
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

            //create a bar
            ProgressBar ColorBarAll = new ProgressBar();//creates a new progress bar, it can be any other control, but this will work

            ColorBarAll.Height = 20;//defines the height
            ColorBarAll.Width = 400;//defines the width
            //ColorBar.Background=new Color(Color)
            ColorBarAll.Value = 0;//just keeps he progress bar empty
            _Colors = GetStaticPropertyBag(typeof(Colors)).ToList();
            /*
                 ProgressBar ColorBarAvailable = new ProgressBar();//creates a new progress bar, it can be any other control, but this will work

                 ColorBarAvailable.Height = 20;//defines the height
                 ColorBarAvailable.Width = 400;//defines the width
                 //ColorBar.Background=new Color(Color)
                 ColorBarAvailable.Value = 0;//just keeps he progress bar empty
                 _Colors = GetStaticPropertyBag(typeof(Colors)).ToList();

                 ProgressBar ColorBarCalculated = new ProgressBar();//creates a new progress bar, it can be any other control, but this will work

                 ColorBarCalculated.Height = 20;//defines the height
                 ColorBarCalculated.Width = 400;//defines the width
                 //ColorBar.Background=new Color(Color)
                 ColorBarCalculated.Value = 0;//just keeps he progress bar empty
                 _Colors = GetStaticPropertyBag(typeof(Colors)).ToList();
            */
            AddControlToNode(_viewPort);//#0
            AddControlToNode(ColorBarAll);//#1

            /*     Label TTAvailable = new Label { Content = "Colors for available TTs" };
                 AddControlToNode(TTAvailable);//#2
                 AddControlToNode(ColorBarAvailable);//#3
                 Label TTCalculated = new Label { Content = "Colors for calculated TTs" };
                 AddControlToNode(TTCalculated);//#4
                 AddControlToNode(ColorBarCalculated);//#5
           */
        }

        public override void Calculate()
        {
            //in TTValueColor hashTable
            TTValueColorAll = new Hashtable();
            TTValueColorExistin = new Hashtable();
            TTValueColorCalculated = new Hashtable();
            //get all colors available in a list
            Console.WriteLine("Total of " + _Colors.Count + " Colors available!!");

            OutputPorts[0].Data = null;

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
            //in TTValueColor hashTable we will have <TT, Color> key-value pairs

            //get all colors available in a list
            var _Colors = GetStaticPropertyBag(typeof(Colors)).ToList();
            Console.WriteLine("Total of " + _Colors.Count + " Colors available!!");

            // DiffuseMaterial NoTTMaterial = new DiffuseMaterial(new SolidColorBrush((Color)_Colors[_Colors.Count - 1].Value));//in case of no TT being available the Color used will be the last one in the list

            // Loop through Entities and visualize them in the viewport
            Console.WriteLine("The elements r in total " + _xModel.Instances.OfType<Xbim.Ifc2x3.Kernel.IfcProduct>().Count() + ".");

            TTValueColorAll.Add(-1, new TTColorAvailable2(-1, -1, TTCannotBeCalculated, false));//black for the problematic ones
            foreach (var item in _xModel.Instances.OfType<Xbim.Ifc2x3.Kernel.IfcProduct>())
            {

                var m = new MeshGeometry3D();
                GetGeometryFromXbimModel_IFC2x3(m, item, XbimMatrix3D.Identity);
                TTColorAvailable2 ttColorAv = GetTTifExistsCalculateifNot_IFC2x3(item);//it TT-property does not exist, it calculates it
                // Console.WriteLine("TT = "+TT);
                ttColorAv.divedTT = Math.Truncate(ttColorAv.TT);

                DiffuseMaterial Material;
                Color thisColor;
                if (TTValueColorAll.ContainsKey(ttColorAv.divedTT))
                    thisColor = ((TTColorAvailable2)TTValueColorAll[ttColorAv.divedTT]).col;
                else  //new TT - Material(Color) pair
                {
                    thisColor = (Color)_Colors[TTValueColorAll.Count].Value;
                    int thisC = TTValueColorAll.Count;
                    while (thisColor.Equals(Colors.Black))// || thisColor.Equals(Colors.Green) || thisColor.Equals(Colors.Red))
                        thisColor = (Color)_Colors[++thisC].Value;//black, green n red r reserved, get the next one, sorry
                    ttColorAv.col = thisColor;
                    TTValueColorAll.Add(ttColorAv.divedTT, ttColorAv);
                }
                Material = new DiffuseMaterial(new SolidColorBrush(thisColor));

                var mb = new MeshBuilder(false, false);
                VisualizeMesh(mb, m, Material);
            }
            Console.WriteLine("The colors that are actually been used are " + TTValueColorAll.Keys.Count + " (distinct TT-Values).");
            List<double> SortedTTs = TTValueColorAll.Keys.OfType<double>().ToList();
            SortedTTs.Sort();
            SortedTTs.RemoveAt(0);//remove the NoTTColor (Black)
                                  //Create a new SortedTT-List with values' ranges of TT. Going +1 at each group


            GradientStopCollection colorsCollection = new GradientStopCollection();
            GradientStopCollection colorsCollectionAvailable = new GradientStopCollection();
            GradientStopCollection colorsCollectionCalculated = new GradientStopCollection();

            double i = 0;
            double inc = 1.0 / SortedTTs.Count;
            for (int k = 0; k < SortedTTs.Count; k++)
            {
                // Color col = ((TTColorAvailable2)TTValueColorAll[TTRanges[k]]).col;
                Color col = (Color)_Colors[k].Value;
                if (SortedTTs[k].Equals(-1))
                    col = TTCannotBeCalculated;
                /*  if (k == 0)
                      col = TTSmaller;
                  if (k == (SortedTTs.Count - 1))
                      col = TTBigger;  */
                colorsCollection.Add(new GradientStop(col, i));

                /*  if (((TTColorAvailable)TTValueColorAll[TTRanges[k]]).available)
                      colorsCollectionAvailable.Add(new GradientStop(col, i));
                  else
                      colorsCollectionCalculated.Add(new GradientStop(col, i));
                  */
                i += inc;
                Console.WriteLine("**** TT of " + SortedTTs[k] + " is now in " + col + " ****");
            }

            LinearGradientBrush colors = new LinearGradientBrush(colorsCollection, 0);
            ProgressBar ColorBarAll = ControlElements[1] as ProgressBar;
            ColorBarAll.Background = colors;

            /*          LinearGradientBrush colorsAvailable = new LinearGradientBrush(colorsCollectionAvailable, 0);
                      ProgressBar ColorBarAvailable = ControlElements[3] as ProgressBar;
                      ColorBarAvailable.Background = colorsAvailable;

                      LinearGradientBrush colorsCalculated = new LinearGradientBrush(colorsCollectionCalculated, 0);
                      ProgressBar ColorBarCalculated = ControlElements[5] as ProgressBar;
                      ColorBarCalculated.Background = colorsCalculated;
          */
        }

        private void worker_DoWork_IFC4(IfcStore xModel)
        {
            //in TTValueColor hashTable we will have <TT, Color> key-value pairs

            //get all colors available in a list
            var _Colors = GetStaticPropertyBag(typeof(Colors)).ToList();
            Console.WriteLine("Total of " + _Colors.Count + " Colors available!!");

            // DiffuseMaterial NoTTMaterial = new DiffuseMaterial(new SolidColorBrush((Color)_Colors[_Colors.Count - 1].Value));//in case of no TT being available the Color used will be the last one in the list

            // Loop through Entities and visualize them in the viewport
            Console.WriteLine("The elements r in total " + _xModel.Instances.OfType<Xbim.Ifc4.Kernel.IfcProduct>().Count() + ".");
            TTValueColorAll.Add(-1, new TTColorAvailable2(-1, -1, TTCannotBeCalculated, false));//black for the problematic ones
            foreach (var item in _xModel.Instances.OfType<Xbim.Ifc2x3.Kernel.IfcProduct>())
            {

                var m = new MeshGeometry3D();
                GetGeometryFromXbimModel_IFC2x3(m, item, XbimMatrix3D.Identity);
                TTColorAvailable2 ttColorAv = GetTTifExistsCalculateifNot_IFC2x3(item);//it TT-property does not exist, it calculates it
                // Console.WriteLine("TT = "+TT);
                ttColorAv.divedTT = Math.Truncate(ttColorAv.TT);

                DiffuseMaterial Material;
                Color thisColor;
                if (TTValueColorAll.ContainsKey(ttColorAv.divedTT))
                    thisColor = ((TTColorAvailable2)TTValueColorAll[ttColorAv.divedTT]).col;
                else  //new TT - Material(Color) pair
                {
                    thisColor = (Color)_Colors[TTValueColorAll.Count].Value;
                    int thisC = TTValueColorAll.Count;
                    while (thisColor.Equals(Colors.Black))// || thisColor.Equals(Colors.Green) || thisColor.Equals(Colors.Red))
                        thisColor = (Color)_Colors[++thisC].Value;//black, green n red r reserved, get the next one, sorry
                    ttColorAv.col = thisColor;
                    TTValueColorAll.Add(ttColorAv.divedTT, ttColorAv);
                }
                Material = new DiffuseMaterial(new SolidColorBrush(thisColor));

                var mb = new MeshBuilder(false, false);
                VisualizeMesh(mb, m, Material);
            }
            Console.WriteLine("The colors that are actually been used are " + TTValueColorAll.Keys.Count + " (distinct TT-Values).");
            List<double> SortedTTs = TTValueColorAll.Keys.OfType<double>().ToList();
            SortedTTs.Sort();
            SortedTTs.RemoveAt(0);//remove the NoTTColor (Black)
                                  //Create a new SortedTT-List with values' ranges of TT. Going +1 at each group


            GradientStopCollection colorsCollection = new GradientStopCollection();
            GradientStopCollection colorsCollectionAvailable = new GradientStopCollection();
            GradientStopCollection colorsCollectionCalculated = new GradientStopCollection();

            double i = 0;
            double inc = 1.0 / SortedTTs.Count;
            for (int k = 0; k < SortedTTs.Count; k++)
            {
                // Color col = ((TTColorAvailable2)TTValueColorAll[TTRanges[k]]).col;
                Color col = (Color)_Colors[k].Value;
                if (SortedTTs[k].Equals(-1))
                    col = TTCannotBeCalculated;
                /*  if (k == 0)
                      col = TTSmaller;
                  if (k == (SortedTTs.Count - 1))
                      col = TTBigger;  */
                colorsCollection.Add(new GradientStop(col, i));

                /*  if (((TTColorAvailable)TTValueColorAll[TTRanges[k]]).available)
                      colorsCollectionAvailable.Add(new GradientStop(col, i));
                  else
                      colorsCollectionCalculated.Add(new GradientStop(col, i));
                  */
                i += inc;
                Console.WriteLine("**** TT of " + SortedTTs[k] + " is now in " + col + " ****");
            }

            LinearGradientBrush colors = new LinearGradientBrush(colorsCollection, 0);
            ProgressBar ColorBarAll = ControlElements[1] as ProgressBar;
            ColorBarAll.Background = colors;

            /*   LinearGradientBrush colorsAvailable = new LinearGradientBrush(colorsCollectionAvailable, 0);
               ProgressBar ColorBarAvailable = ControlElements[3] as ProgressBar;
               ColorBarAvailable.Background = colorsAvailable;

               LinearGradientBrush colorsCalculated = new LinearGradientBrush(colorsCollectionCalculated, 0);
               ProgressBar ColorBarCalculated = ControlElements[5] as ProgressBar;
               ColorBarCalculated.Background = colorsCalculated;
           */
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

        public TTColorAvailable2 GetTTifExistsCalculateifNot_IFC2x3(Xbim.Ifc2x3.Kernel.IfcProduct ifcProduct)
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
                        double thisTT = (double)((Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue)onepropertySet[jj]).NominalValue.Value;

                        return new TTColorAvailable2(thisTT, true);
                    }
                    jj++;
                }
                ii++;
            }
            //if it comes up 2 here, no TT was found... we r going 2 calculate it...
            if (ifcProduct.GetType() == typeof(Xbim.Ifc2x3.SharedBldgElements.IfcColumn))
            {
                double thisTT = CalculateTT_IFC2x3((Xbim.Ifc2x3.SharedBldgElements.IfcColumn)ifcProduct);
                return new TTColorAvailable2(thisTT, false);
            }
            else if (ifcProduct.GetType() == typeof(Xbim.Ifc2x3.SharedBldgElements.IfcDoor))
            {
                double thisTT = CalculateTT_IFC2x3((Xbim.Ifc2x3.SharedBldgElements.IfcDoor)ifcProduct);
                return new TTColorAvailable2(thisTT, false);
            }
            else if (ifcProduct.GetType() == typeof(Xbim.Ifc2x3.SharedBldgElements.IfcCurtainWall))
            {
                double thisTT = CalculateTT_IFC2x3((Xbim.Ifc2x3.SharedBldgElements.IfcCurtainWall)ifcProduct);
                return new TTColorAvailable2(thisTT, false);
            }

            return new TTColorAvailable2(-1, true);//smth may be wrong...
        }
        public TTColorAvailable2 GetTTifExistsCalculateifNot_IFC4(Xbim.Ifc4.Kernel.IfcProduct ifcProduct)
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
                        double thisTT = (double)((Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue)onepropertySet[jj]).NominalValue.Value;


                        return new TTColorAvailable2(thisTT, true);
                    }
                    jj++;
                }
                ii++;
            }
            //if it comes up 2 here, no TT was found... we r going 2 calculate it...
            if (ifcProduct.GetType() == typeof(Xbim.Ifc4.SharedBldgElements.IfcColumn))
            {
                double thisTT = CalculateTT_IFC4((Xbim.Ifc4.SharedBldgElements.IfcColumn)ifcProduct);
                return new TTColorAvailable2(thisTT, false);
            }
            else if (ifcProduct.GetType() == typeof(Xbim.Ifc4.SharedBldgElements.IfcDoor))
            {
                double thisTT = CalculateTT_IFC4((Xbim.Ifc4.SharedBldgElements.IfcDoor)ifcProduct);
                return new TTColorAvailable2(thisTT, false);
            }
            else if (ifcProduct.GetType() == typeof(Xbim.Ifc4.SharedBldgElements.IfcCurtainWall))
            {
                double thisTT = CalculateTT_IFC4((Xbim.Ifc4.SharedBldgElements.IfcCurtainWall)ifcProduct);
                return new TTColorAvailable2(thisTT, false);
            }

            return new TTColorAvailable2(-1, true);//smth may be wrong...
        }


        public double CalculateTT_IFC2x3(Xbim.Ifc2x3.SharedBldgElements.IfcColumn ifcProduct)
        {
            try
            {
                Console.WriteLine("** This is a Column **");
                var ifcVolume = ifcProduct.PropertySets.ToList()[2].HasProperties.ToList()[1];//Volumen
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
                double thickness = (volumeVal / areaVal);

                double denominator = Rsi + thickness / l + Rse;
                double thermo = 1 / denominator;



                return thermo;
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
                var ifcVolume = ifcProduct.PropertySets.ToList()[2].HasProperties.ToList()[1];//Volumen
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
                double thickness = (volumeVal / areaVal);

                double denominator = Rsi + thickness / l + Rse;
                double thermo = 1 / denominator;



                return thermo;
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
                var ifcVolume = ifcProduct.PropertySets.ToList()[2].HasProperties.ToList()[1];//is it the right property..?-->Yes (Volumen)
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
                double thickness = (volumeVal / areaVal);

                double denominator = Rsi + thickness / l + Rse;
                double thermo = 1 / denominator;



                return thermo;
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
                var ifcVolume = ifcProduct.PropertySets.ToList()[2].HasProperties.ToList()[1];//Volumen
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
                double thickness = (volumeVal / areaVal);

                double denominator = Rsi + thickness / l + Rse;
                double thermo = 1 / denominator;



                return thermo;
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
                var ifcVolume = ifcProduct.PropertySets.ToList()[2].HasProperties.ToList()[1];//Volumen
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
                double thickness = (volumeVal / areaVal);

                double denominator = Rsi + thickness / l + Rse;
                double thermo = 1 / denominator;



                return thermo;
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
                var ifcVolume = ifcProduct.PropertySets.ToList()[2].HasProperties.ToList()[1];//is it the right property..?-->Yes (Volumen)
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
                double thickness = (volumeVal / areaVal);

                double denominator = Rsi + thickness / l + Rse;
                double thermo = 1 / denominator;



                return thermo;
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
            return new IfcViewerNode(HostCanvas)
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


    public class TTColorAvailable2
    {
        public double TT { get; set; }
        public Color col { get; set; }
        public bool available { get; set; }//true if TT is available, false if it got 2 be calculated
        public double divedTT { get; set; }

        public TTColorAvailable2(double tt, bool av)
        {
            this.TT = tt;
            this.available = av;
        }

        public TTColorAvailable2(double tt, double ttt, Color c, bool av)
        {
            this.TT = tt;
            this.col = c;
            this.available = av;
            this.divedTT = ttt;
        }
    }
}
