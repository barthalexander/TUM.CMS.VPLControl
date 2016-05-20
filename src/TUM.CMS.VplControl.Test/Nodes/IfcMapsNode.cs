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

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcMapsNode : Node
    {
        private Frame maps;
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

           

            maps = new Frame
            {
                MinWidth = 600,
                MinHeight = 450
            };
            AddControlToNode(maps);
            AddControlToNode(textBlock);


           
        }

       
        public override void Calculate()
        {
            var file = InputPorts[0].Data.ToString();
            if (file != null && File.Exists(file))
            {
                xModel = new XbimModel();
                var res = xModel.Open(file, XbimDBAccess.Read);

                if (res == false)
                {
                    var err = xModel.Validate(TextWriter.Null, ValidationFlags.All);
                    MessageBox.Show("ERROR in reading process!");
                }

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

    }
}