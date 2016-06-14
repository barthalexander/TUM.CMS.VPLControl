using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using TUM.CMS.VplControl.Core;
using Xbim.IO;
using Xbim.ModelGeometry.Scene;
using Xbim.Presentation;
using Xbim.XbimExtensions;
using XbimGeometry.Interfaces;
using System.Linq;
using System.Diagnostics;
using TUM.CMS.VplControl.IFC.Utilities;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcMapsNode : Node
    {
        private WebBrowser maps;
        public XbimModel xModel;
        public IfcMapsNode(Core.VplControl hostCanvas) : base(hostCanvas)
        {
           
            IsResizeable = true;
            var textBlock = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                FontSize = 14,
                Padding = new Thickness(5),
                IsHitTestVisible = false
            };

            AddInputPortToNode("Object", typeof(string));

            var appName = Process.GetCurrentProcess().ProcessName + ".exe";
            SetIE9KeyforWebBrowserControl(appName);


            maps = new WebBrowser
            {
                MinWidth = 600,
                MinHeight = 450

            };

            

            AddControlToNode(maps);
            AddControlToNode(textBlock);


        }

       
        public override void Calculate()
        {
            var modelid = ((ModelInfo)(InputPorts[0].Data)).ModelId;

            if (modelid != null && File.Exists(modelid))
            {
                xModel = DataController.Instance.GetModel(modelid);
               
                try
                {
                    var ifcsite = xModel.IfcProducts.OfType<Xbim.Ifc2x3.ProductExtension.IfcSite>().ToList();
                    var ifcRefLong = ifcsite[0].RefLongitude.ToString();
                    var ifcRefLat = ifcsite[0].RefLatitude.ToString();
                    string[] separators = { ",", ".", "!", "?", ";", ":", " " };
                    string[] ifcRefLat_temp = ifcRefLat.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    string[] ifcRefLong_temp = ifcRefLong.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    if (ifcRefLong != "" && ifcRefLat != "")
                    {
                        maps.Source = null;
                        maps.Source = new Uri("https://www.google.de/maps/@" + ifcRefLat_temp[0] + "." + ifcRefLat_temp[1] + ifcRefLat_temp[2] + "," + ifcRefLong_temp[0] + "." + ifcRefLong_temp[1] + ifcRefLong_temp[2] + ",15z");
                        var textBlock = ControlElements[1] as TextBlock;
                        textBlock.Text = "Geo Coordinates: " + ifcRefLat_temp[0] + "." + ifcRefLat_temp[1] + ifcRefLat_temp[2] + "," + ifcRefLong_temp[0] + "." + ifcRefLong_temp[1] + ifcRefLong_temp[2];
                    }
                    else
                    {
                        var textBlock = ControlElements[1] as TextBlock;
                        textBlock.Text = "No Geo Coordinates were found on the IFC File";
                    }


                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
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