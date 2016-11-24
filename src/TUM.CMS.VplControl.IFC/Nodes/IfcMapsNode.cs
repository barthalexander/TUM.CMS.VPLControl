using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using TUM.CMS.VplControl.Core;
using System.Linq;
using System.Diagnostics;
using TUM.CMS.VplControl.IFC.Controls;
using TUM.CMS.VplControl.IFC.Utilities;
using Xbim.Ifc;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcMapsNode : Node
    {
        public IfcStore xModel;
        public IfcMapsNode(Core.VplControl hostCanvas) : base(hostCanvas)
        {
           
            IsResizeable = true;

            AddInputPortToNode("Object", typeof(string));
            
            // Change Version if Internet Explorer
            var appName = Process.GetCurrentProcess().ProcessName + ".exe";
            SetIE9KeyforWebBrowserControl(appName);


            IfcMapsControl ifcMapsControl = new IfcMapsControl();

            AddControlToNode(ifcMapsControl);
        }

        /// <summary>
        /// Read in the coordinates of the IFC file and print it in google maps
        /// </summary>
        public override void Calculate()
        {
            if (InputPorts[0].Data == null)
                return;

            var ifcMapsControl = ControlElements[0] as IfcMapsControl;

            WebBrowser maps = new WebBrowser();

            maps = ifcMapsControl.Browser;
            maps.Height = 450;
            maps.Width = 600;

            // Check for IFC Version
            Type IfcVersionType = InputPorts[0].Data.GetType();
            if (IfcVersionType.Name == "ModelInfoIFC2x3")
            {
                var modelid = ((ModelInfoIFC2x3)(InputPorts[0].Data)).ModelId;

                if (modelid != null && File.Exists(modelid))
                {
                    xModel = DataController.Instance.GetModel(modelid);

                    try
                    {
                        var ifcsite = xModel.Instances.OfType<Xbim.Ifc2x3.ProductExtension.IfcSite>().ToList();

                        // Split the coordinates of the IFC file
                        List<long> ifcRefLong = new List<long>();
                        List<long> ifcRefLat = new List<long>();
                        
                        ifcRefLong = ifcsite[0].RefLongitude;
                        ifcRefLat = ifcsite[0].RefLatitude;


                        if (ifcRefLong != null && ifcRefLat != null)
                        {
                            maps.Source = null;
                            maps.Source = new Uri("https://www.google.de/maps/@" + ifcRefLat[0] + "." + ifcRefLat[1] + ifcRefLat[2] + ifcRefLat[3] + "," + ifcRefLong[0] + "." + ifcRefLong[1] + ifcRefLong[2] + ifcRefLong[3] + ",15z");
                            TextBlock textBlock = new TextBlock();
                            textBlock = ifcMapsControl.TextBlock;
                            textBlock.Text = "Geo Coordinates: " + ifcsite[0].RefLatitude.Value.AsDouble + "," + ifcsite[0].RefLongitude.Value.AsDouble;
                        }
                        else
                        {
                            TextBlock textBlock = new TextBlock();
                            textBlock = ifcMapsControl.TextBlock;
                            textBlock.Text = "No Geo Coordinates were found on the IFC File";
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }
                }
            }
            else if (IfcVersionType.Name == "ModelInfoIFC4")
            {
                var modelid = ((ModelInfoIFC4)(InputPorts[0].Data)).ModelId;

                if (modelid != null && File.Exists(modelid))
                {
                    xModel = DataController.Instance.GetModel(modelid);

                    try
                    {
                        var ifcsite = xModel.Instances.OfType<Xbim.Ifc4.ProductExtension.IfcSite>().ToList();

                        // Split the coordinates of the IFC file
                        List<long> ifcRefLong = new List<long>();
                        List<long> ifcRefLat = new List<long>();

                        ifcRefLong = ifcsite[0].RefLongitude;
                        ifcRefLat = ifcsite[0].RefLatitude;


                        if (ifcRefLong != null && ifcRefLat != null)
                        {
                            maps.Source = null;
                            maps.Source = new Uri("https://www.google.de/maps/@" + ifcRefLat[0] + "." + ifcRefLat[1] + ifcRefLat[2] + ifcRefLat[3] + "," + ifcRefLong[0] + "." + ifcRefLong[1] + ifcRefLong[2] + ifcRefLong[3] + ",15z");
                            TextBlock textBlock = new TextBlock();
                            textBlock = ifcMapsControl.TextBlock;
                            textBlock.Text = "Geo Coordinates: " + ifcsite[0].RefLatitude.Value.AsDouble + "," + ifcsite[0].RefLongitude.Value.AsDouble;
                        }
                        else
                        {
                            TextBlock textBlock = new TextBlock();
                            textBlock = ifcMapsControl.TextBlock;
                            textBlock.Text = "No Geo Coordinates were found on the IFC File";
                        }


                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }
                }

            }
        }

        public override Node Clone()
        {
            return new IfcMapsNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }

        /// <summary>
        /// Change the Internet Explorer Version in the registry
        /// 
        /// Important: Application must be run in admin mode
        /// </summary>
        /// <param name="appName"></param>
        private void SetIE9KeyforWebBrowserControl(string appName)
        {
            RegistryKey Regkey = null;
            try
            {

                //For 64 bit Machine 
                if (Environment.Is64BitOperatingSystem)
                    Regkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\\Wow6432Node\\Microsoft\\Internet Explorer\\MAIN\\FeatureControl\\FEATURE_BROWSER_EMULATION", true);
                else  //For 32 bit Machine 
                    Regkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION", true);

                //If the path is not correct or 
                //If user't have priviledges to access registry 
                if (Regkey == null)
                {
                    MessageBox.Show("Application Settings Failed - Address Not found");
                    return;
                }

                string FindAppkey = Convert.ToString(Regkey.GetValue(appName));

                //Check if key is already present 
                if (FindAppkey == "9999")
                {
                    Console.WriteLine("Required Application Settings Present");
                    Regkey.Close();
                    return;
                }

                //If key is not present add the key , Kev value 9999-Decimal 
                if (string.IsNullOrEmpty(FindAppkey) || FindAppkey != "9999")
                {
                    Regkey.SetValue(appName, unchecked((int)0x270F), RegistryValueKind.DWord);
                }
                
                //check for the key after adding 
                FindAppkey = Convert.ToString(Regkey.GetValue(appName));

                if (FindAppkey == "9999")
                    Console.WriteLine("Application Settings Applied Successfully");
                else
                    MessageBox.Show("Application Settings for IFC Maps Node Failed \n Please check your Browser Version");


            }
            catch (Exception ex)
            {
                MessageBox.Show("Application Settings Failed");
                MessageBox.Show(ex.Message);
            }
            finally
            {
                //Close the Registry 
                if (Regkey != null)
                    Regkey.Close();
            }
        }
    }
}