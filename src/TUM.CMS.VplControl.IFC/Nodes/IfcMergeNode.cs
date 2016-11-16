using System;
using System.Collections.Generic;
using System.Windows.Controls;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.IFC.Utilities;
using System.Collections;
using System.IO;
using System.Linq;
using System.Windows;
using Xbim.Common;
using Xbim.Common.Metadata;
using Xbim.Common.Step21;
using Xbim.Ifc;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcMergeNode : Node
    {
        public ModelInfoIFC2x3 OutputIfc2x3;
        public ModelInfoIFC4 OutputIfc4;
        public Type IfcVersionTypeModel1 = null;
        public Type IfcVersionTypeModel2 = null;
        public IfcMergeNode(Core.VplControl hostCanvas) : base(hostCanvas)
        {

            var labelModels = new Label { Content = "Ifc Merge Node", FontWeight = FontWeights.Bold};
            AddControlToNode(labelModels);
            var label1 = new Label {};
            AddControlToNode(label1);

            AddInputPortToNode("Object", typeof(object), false);
            AddInputPortToNode("Object", typeof(object), false);
            AddOutputPortToNode("Object", typeof(object));


        }
        
       
        /// <summary>
        /// Adds all Models which are stored at the DataController to the ComboBox
        /// </summary>
        public override void Calculate()
        {
            if (InputPorts[0].Data == null)
                return;
            if (InputPorts[1].Data == null)
                return;



            IfcVersionTypeModel1 = InputPorts[0].Data.GetType();
            IfcVersionTypeModel2 = InputPorts[1].Data.GetType();

            if (IfcVersionTypeModel1 != IfcVersionTypeModel2)
            {
                MessageBox.Show("The IFC Versions are not the same!", "My Application", MessageBoxButton.OK);
                return;
            }

            PropertyTranformDelegate propTransform = delegate (ExpressMetaProperty prop, object toCopy)
            {
                var value = prop.PropertyInfo.GetValue(toCopy, null);
                return value;
            };

            var newModelIfc2x3 = IfcStore.Create(IfcSchemaVersion.Ifc2X3, XbimStoreType.InMemoryModel);
            var newModelIfc4 = IfcStore.Create(IfcSchemaVersion.Ifc4, XbimStoreType.InMemoryModel);

            var txnIfc2x3 = newModelIfc2x3.BeginTransaction();
            var txnIfc4 = newModelIfc4.BeginTransaction();

            HashSet<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> buildingElementsIFC2x3 = new HashSet<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId>();
            HashSet<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId> buildingElementsIFC4 = new HashSet<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId>();

            IfcSchemaVersion ifcVersion = IfcSchemaVersion.Unsupported;

            XbimInstanceHandleMap copied = null;

            var label1 = ControlElements[1] as Label;
            label1.Content = "";

            for (var i = 0; i < 2; i++)
            {
                if (IfcVersionTypeModel1.Name == "ModelInfoIFC2x3")
                {
                    var modelid = ((ModelInfoIFC2x3)(InputPorts[i].Data)).ModelId;
                    var res = new HashSet<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId>();
                    var elementIdsList = ((ModelInfoIFC2x3)(InputPorts[i].Data)).ElementIds;
                    foreach (var elementID in elementIdsList)
                    {
                        res.Add(elementID);
                    }

                    var xModel = DataController.Instance.GetModel(modelid);
                    ifcVersion = xModel.IfcSchemaVersion;
                    List<Xbim.Ifc2x3.Kernel.IfcProduct> elements = xModel.Instances.OfType<Xbim.Ifc2x3.Kernel.IfcProduct>().ToList();
                    copied = new XbimInstanceHandleMap(xModel, newModelIfc2x3);

                    foreach (var element in elements)
                    {
                        if (res.Contains(element.GlobalId) && !buildingElementsIFC2x3.Contains(element.GlobalId))
                        {
                            newModelIfc2x3.InsertCopy(element, copied, propTransform, false, false);
                            buildingElementsIFC2x3.Add(element.GlobalId);
                        }
                    }
                }
                else if (IfcVersionTypeModel1.Name == "ModelInfoIFC4")
                {
                    var modelid = ((ModelInfoIFC4)(InputPorts[i].Data)).ModelId;
                    var elementIdsList = ((ModelInfoIFC4)(InputPorts[i].Data)).ElementIds;
                    var res = new HashSet<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId>(elementIdsList);

                    var xModel = DataController.Instance.GetModel(modelid);
                    ifcVersion = xModel.IfcSchemaVersion;
                    List<Xbim.Ifc4.Kernel.IfcProduct> elements = xModel.Instances.OfType<Xbim.Ifc4.Kernel.IfcProduct>().ToList();
                    copied = new XbimInstanceHandleMap(xModel, newModelIfc4);

                    foreach (var element in elements)
                    {
                        if (res.Contains(element.GlobalId) && !buildingElementsIFC4.Contains(element.GlobalId))
                        {
                            newModelIfc4.InsertCopy(element, copied, propTransform, false, false);
                            buildingElementsIFC4.Add(element.GlobalId);
                        }
                    }
                }
            }

            Random zufall = new Random();
            int number = zufall.Next(1, 1000);

            string result = Path.GetTempPath();
            string copyFile = result + "copy" + number + ".ifc";
            while (File.Exists(copyFile))
            {
                number = zufall.Next(1, 1000);
                copyFile = result + "copy" + number + ".ifc";
            }


            if (ifcVersion == IfcSchemaVersion.Ifc2X3)
            {
                txnIfc2x3.Commit();
                newModelIfc2x3.SaveAs(copyFile);
                newModelIfc2x3.Close();

                ModelInfoIFC2x3 modelInfo = new ModelInfoIFC2x3(copyFile);
                foreach (var id in buildingElementsIFC2x3)
                {
                    modelInfo.AddElementIds(id);
                }
                OutputIfc2x3 = modelInfo;
            }
            else if (ifcVersion == IfcSchemaVersion.Ifc4)
            {
                txnIfc4.Commit();
                newModelIfc4.SaveAs(copyFile);
                newModelIfc4.Close();

                ModelInfoIFC4 modelInfo = new ModelInfoIFC4(copyFile);
                foreach (var id in buildingElementsIFC4)
                {
                    modelInfo.AddElementIds(id);
                }
                OutputIfc4 = modelInfo;
            }

            if (IfcVersionTypeModel1.Name == "ModelInfoIFC2x3")
            {
                label1.Content = "Merge Complete!";
                OutputPorts[0].Data = OutputIfc2x3;
            }
            else if (IfcVersionTypeModel1.Name == "ModelInfoIFC4")
            {
                label1.Content = "Merge Complete!";
                OutputPorts[0].Data = OutputIfc4;
            }
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