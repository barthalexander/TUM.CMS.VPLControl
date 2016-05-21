using System;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using TUM.CMS.VplControl.Core;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;
using Microsoft.Win32;
using Xbim.IO;
using Xbim.ModelGeometry.Scene;
using Xbim.Presentation;
using Xbim.XbimExtensions;
using XbimGeometry.Interfaces;
using Xbim.Ifc;

using System.ComponentModel;
using System.Linq;

namespace TUM.CMS.VplControl.Test.Nodes
{
    public class ListIfcNode : Node
    {
        public ListIfcNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {

            AddInputPortToNode("IfcFile", typeof(string));

            AddOutputPortToNode("FilteredProducts", typeof(object));


            var textBlock = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                FontSize = 14,
                Padding = new Thickness(5),
                IsHitTestVisible = false
            };
            var scrollViewer = new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                MinWidth = 600,
                MinHeight = 30,
                MaxWidth = 1000,
                MaxHeight = 400,
                CanContentScroll = true,
                Content = textBlock
                //IsHitTestVisible = false
            };


            AddControlToNode(scrollViewer);
            AddControlToNode(new Label { Content = "IfcProductsNode" });
        }
        //private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    Calculate();
        //}
        public override void Calculate()
        {
           
            XbimModel xModel = new XbimModel();
           
            var res = xModel.Open(InputPorts[0].Data.ToString(), XbimDBAccess.ReadWrite);

            var scrollViewer = ControlElements[0] as ScrollViewer;
            if (scrollViewer == null) return;
            var textBlock = scrollViewer.Content as TextBlock;
            if (textBlock == null) return;
            var ifcwall = xModel.IfcProducts.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcWall>().ToList();
            // Xbim.XbimExtensions.Interfaces.IModel ifcWall = ifcwall;
            //xModel.InsertCopy(ifcwall,XbimReadWriteTransaction);

            var ifcbeam = xModel.IfcProducts.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcBeam>().ToList();
            var ifccolumn = xModel.IfcProducts.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcColumn>().ToList();
            var ifcslab = xModel.IfcProducts.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcSlab>().ToList();
            var ifcwindow = xModel.IfcProducts.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcWindow>().ToList();
            var ifcstair = xModel.IfcProducts.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcStair>().ToList();
            var ifcroof = xModel.IfcProducts.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcRoof>().ToList();
            var ifcramp = xModel.IfcProducts.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcRamp>().ToList();
            var ifcplate = xModel.IfcProducts.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcPlate>().ToList();
            var ifcdoor = xModel.IfcProducts.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcDoor>().ToList();
            var ifccurtainwall = xModel.IfcProducts.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcCurtainWall>().ToList();
            textBlock.Text += ifcwall.Count().ToString() + " walls," + ifcbeam.Count().ToString() + " beams,"
                             + ifccolumn.Count().ToString() + " columns," + ifcslab.Count().ToString() + " slabs,"
                             + ifcwindow.Count().ToString() + " windows," + ifcstair.Count().ToString() + " stairs,"
                             + ifcroof.Count().ToString() + " roofs," + ifcramp.Count().ToString() + " ramps,"
                             + ifcplate.Count().ToString() + " plates," + ifcdoor.Count().ToString() + " doors,"
                             + ifccurtainwall.Count().ToString() + " curtainwalls.";

            var ifcproducts = xModel.IfcProducts;
            //Xbim.XbimExtensions.Interfaces.IPersistIfcEntity
            //ifcwall[0].Representation.
            // xModel.Delete((ifcproducts.Except(ifcproducts.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcWall>));
            // xModel.SaveAs("filteredxModel");
            //OutputPorts[0].Data = xModel;



            // DrawingControl3D dc3d=new DrawingControl3D();



            var geometry = ifcwall[0].GeometryData(XbimGeometryType.TriangulatedMesh).ToList();
            foreach (var geom in geometry)
            {
                var test = geom.ShapeData;
            }




            //var textBox = ControlElements[0] as TextBox;
            /* foreach (var ifcproduct in ifcproducts)
                 {
                 textBlock.Text += ifcproduct.ToString()+"  ";
                     }
                     */
            //OutputPorts[0].Data = geometry;
            int count = 0;
            IEnumerator<Xbim.XbimExtensions.Interfaces.IPersistIfcEntity> ifcenum = ifcproducts.GetEnumerator();

            while (ifcenum.MoveNext())
            {
                count++;
            }

            textBlock.Text += "  totally " + count.ToString() + " IFCproducts.";








        }

        public override void SerializeNetwork(XmlWriter xmlWriter)
        {
            base.SerializeNetwork(xmlWriter);

            // add your xml serialization methods here
        }

        public override void DeserializeNetwork(XmlReader xmlReader)
        {
            base.DeserializeNetwork(xmlReader);

            // add your xml deserialization methods here
        }

        public override Node Clone()
        {
            return new ListIfcNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}