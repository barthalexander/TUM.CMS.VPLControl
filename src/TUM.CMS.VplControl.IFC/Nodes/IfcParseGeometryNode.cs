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
using System.Windows.Media;
using Xbim.Geometry.Engine.Interop;
using Xbim.Ifc2x3.GeometricModelResource;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcParseGeometryNode : Node
    {
        public XbimModel xModel;

        public IfcParseGeometryNode(Core.VplControl hostCanvas) : base(hostCanvas)
        {
            AddInputPortToNode("Test", typeof(string));

            AddOutputPortToNode("Test", typeof(string));

            var label = new Label
            {
                Content = "IFC File Rading",
                Width = 100,
                FontSize = 15,
                HorizontalContentAlignment = HorizontalAlignment.Center
            };
            var textBlock = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                FontSize = 10,
                Padding = new Thickness(5),
                IsHitTestVisible = false
            };
            AddControlToNode(label);
            AddControlToNode(textBlock);


        }

       
        public override void Calculate()
        {
            var file = InputPorts[0].Data.ToString();
            if(file != null && File.Exists(file))
            {
                Random zufall = new Random();
                int number = zufall.Next(1, 1000);

                var path = Path.GetTempPath();
                xModel = new XbimModel();
                xModel.CreateFrom(file, path + "temp_reader" + number + ".xbim");

                IXbimSolid solid;
                XbimGeometryEngine _xbimGeometryCreator = new XbimGeometryEngine();
                if(xModel.Instances.OfType<IfcExtrudedAreaSolid>() != null)
                {
                    var instance = xModel.Instances.OfType<IfcExtrudedAreaSolid>().FirstOrDefault();
                    solid = _xbimGeometryCreator.CreateSolid(instance);
                }



               // XbimShapeGeometry fbrep = _xbimGeometryCreator.CreateShapeGeometry(solid, 0.01, 10);
               // XbimMeshGeometry3D mesh = new XbimMeshGeometry3D();
               // mesh.Read(fbrep.ShapeData);
               // 
               // foreach (var vertex in mesh.Positions)
               // {
               //     Console.WriteLine(vertex.X + ", " + vertex.Y + ", " + vertex.Z);
               // }
               // 
               // for (int i = 0; i < mesh.TriangleIndices.Count; i += 3)
               // {
               //     Console.WriteLine(mesh.TriangleIndices[i] + ", " + mesh.TriangleIndices[i + 1] + ", " + mesh.TriangleIndices[i + 2]);
               // }
               // 
               // Console.ReadLine();


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



    }
}