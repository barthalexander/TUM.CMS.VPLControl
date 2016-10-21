using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.IFC.Utilities;
using Xbim.Ifc;
using Xbim.IO;
using Xbim.ModelGeometry.Scene;
using Xbim.Presentation;
using Xbim.XbimExtensions;
using Xbim.Ifc2x3.ProductExtension;
using Xbim.Ifc2x3.SharedBldgElements;
using Xbim.Ifc2x3.UtilityResource;
using Xbim.Ifc2x3.Kernel;
using Xbim.Ifc.Extensions;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcPartialModel : Node
    {
        private IfcStore xModel;
        

        // private DynamicProductSelectionControl productSelectionControl;
        public IfcPartialModel(Core.VplControl hostCanvas) : base(hostCanvas)
        {
           
            var labelModels = new Label { Content = "" };
            AddControlToNode(labelModels);
            
            DrawingControl3D drawingControl3D=new DrawingControl3D();
        
            
            
            AddControlToNode(drawingControl3D);



            AddInputPortToNode("Object", typeof(object));
            AddOutputPortToNode("Object", typeof(object));


        }


        public override void Calculate()
        {
            /*           
                      using (var transaction = outputModel.BeginTransaction("add to representation"))
                      {
                          outputModel.InsertCopy(xModel.IfcProject, transaction);
                          outputModel.Validate(transaction.Modified(), Console.Out);
                          transaction.Commit();
                      }
                      */
//            var drawingControl3D = ControlElements[1] as DrawingControl3D;
//            if (drawingControl3D == null) return;
//
//            var modelid = ((ModelInfo)(InputPorts[0].Data)).ModelId;
//            if (modelid == null) return;
//
//            xModel = DataController.Instance.GetModel(modelid);
//
//            var selectedItemIds = ((ModelInfo)(InputPorts[0].Data)).ElementIds;
//            if (selectedItemIds == null) return;
//
//            var wall = xModel.IfcProducts.OfType<IfcElement>().ToList().Find(x => x.GlobalId == selectedItemIds[0]);
//            
//            //create temporary model
//            XbimModel outputModel = XbimModel.CreateTemporaryModel();
//            
//            //initialise model
//            using (XbimReadWriteTransaction txn = outputModel.BeginTransaction("Initialise Model"))
//            {
//                outputModel.DefaultOwningUser.ThePerson.FamilyName = "Bloggs";
//                outputModel.DefaultOwningUser.TheOrganization.Name = "Model Author";
//                outputModel.DefaultOwningApplication.ApplicationIdentifier = "Dodgy Software inc.";
//                outputModel.DefaultOwningApplication.ApplicationDeveloper.Name = "Dodgy Programmers Ltd.";
//                outputModel.DefaultOwningApplication.ApplicationFullName = "Ifc sample programme";
//                outputModel.DefaultOwningApplication.Version = "2.0.1";
//
//                IfcProject project = outputModel.Instances.New<IfcProject>();
//                project.Initialize(ProjectUnits.SIUnitsUK);
//                project.Name = "A Project";
//                project.OwnerHistory.OwningUser = outputModel.DefaultOwningUser;
//                project.OwnerHistory.OwningApplication = outputModel.DefaultOwningApplication;
//                txn.Commit();
//            }
//
//            //create ifcBuilding and add it to IfcProject
//            using (XbimReadWriteTransaction txn = outputModel.BeginTransaction("Create Building"))
//            {
//                IfcOwnerHistory ifcOwnerHistory = outputModel.IfcProject.OwnerHistory;
//                IfcBuilding ifcBuilding = outputModel.Instances.New<IfcBuilding>(building =>
//                 {
//                     building.Name = "building 1";
//                     building.ElevationOfRefHeight = 10000;
//                     building.CompositionType = IfcElementCompositionEnum.ELEMENT;
//                 });
//                outputModel.IfcProject.AddBuilding(ifcBuilding);
//                outputModel.IfcProject.AddDecomposingObjectToFirstAggregation(outputModel, wall);
//                if (outputModel.Validate(Console.Out) == 0)
//                {
//                    txn.Commit();
//                }
//                
//            }
//           
//            //create ifcWall
//            /*using (XbimReadWriteTransaction txn = outputModel.BeginTransaction("Create Wall"))
//            {
//                IfcOwnerHistory ifcOwnerHistory = outputModel.IfcProject.OwnerHistory;
//                outputModel.Instances.New<IfcWall>();
//            }*/
//
//
//            
//            using (XbimReadWriteTransaction txn = outputModel.BeginTransaction("Create IfcBuildingStorey and add to rel"))
//            {
//                
//                IfcOwnerHistory ifcOwnerHistory = outputModel.IfcProject.OwnerHistory;
//                IfcBuildingStorey ifcBuildingStorey = outputModel.Instances.New<IfcBuildingStorey>(storey =>
//                {
//                    storey.Name = "storey 1";
//                    storey.CompositionType = IfcElementCompositionEnum.ELEMENT;
//                });
//
//                //need to create the relationship between etage and storey
//                outputModel.Instances.New<IfcRelAggregates>(rdbp =>
//                {
//                    rdbp.OwnerHistory = ifcOwnerHistory;
//                    rdbp.Name = "Object Association";
//                    rdbp.Description = "wall associated to etage";
//                    rdbp.RelatedObjects.Add(ifcBuildingStorey);
//                    // rdbp.RelatingObject = etage;
//                });
//
//                //need to create the relationship between building and storey
//                outputModel.Instances.New<IfcRelAggregates>(rdbp =>
//                {
//                    rdbp.OwnerHistory = ifcOwnerHistory;
//                    rdbp.Name = "Object Association";
//                    rdbp.Description = "wall associated to building";
//                    rdbp.RelatedObjects.Add(ifcBuildingStorey);
//                    rdbp.RelatingObject = outputModel.IfcProject.GetBuildings().ToList()[0];
//                });
//
//                //need to create the relationship between storey and wall
//                IfcRelContainedInSpatialStructure relcss = outputModel.Instances.New<IfcRelContainedInSpatialStructure>(rdbp =>
//                 {
//                     rdbp.OwnerHistory = ifcOwnerHistory;
//                     rdbp.Name = "Element Association";
//                     rdbp.Description = "wall associated to structure";
//                     rdbp.RelatedElements.Add(wall);
//                     rdbp.RelatingStructure.AddDecomposingObjectToFirstAggregation(outputModel, outputModel.IfcProject.GetBuildings().ToList()[0].GetBuildingStoreys().ToList ()[0]);
//                 });
//
//
//                if (outputModel.Validate(Console.Out) == 0)
//                {
//                    txn.Commit();
//                }
//            }
//
//            var a=outputModel.IfcProducts.OfType<IfcElement>().ToList();
//            
//            drawingControl3D.Model = outputModel;
//            drawingControl3D.ShowAll();
//            drawingControl3D.ReloadModel();
//            drawingControl3D.LoadGeometry(outputModel);
//            OutputPorts[0].Data = outputModel;
        }

        public override Node Clone()
        {
            return new IfcTreeNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }

    }
}
