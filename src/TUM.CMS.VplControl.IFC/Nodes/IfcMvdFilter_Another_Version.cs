using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUM.CMS.VplControl.Core;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using TUM.CMS.VplControl.IFC.Utilities;
using Xbim.IO;
using Xbim;
using TUM.CMS.VplControl.IFC.Utilities.mvdXMLReaderClasses;
using System.Collections;
using Xbim.Ifc2x3.UtilityResource;
using Xbim.Common;
using Xbim.Common.Metadata;
using Xbim.Common.Step21;
using Xbim.Ifc;
using Xbim.Ifc2x3.Kernel;
using Xbim.Ifc2x3.ProductExtension;
using Xbim.Presentation;



namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcMvdFilter_Another_Version : Node
    {
        public IfcStore xModel;
        public ModelInfoIFC2x3 OutputInfoIfc2x3;
        public ModelInfoIFC4 OutputInfoIfc4;
        public Type IfcVersionType = null;

        public HashSet<String> ChosenEntities;


        public IfcMvdFilter_Another_Version(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {
            AddInputPortToNode("IfcFile", typeof(object));
            AddInputPortToNode("MvdFile", typeof(object));

            AddOutputPortToNode("FilteredIfc", typeof(object));

            var label = new Label
            {
                Content = "Filtering ifc with mvdXML",
                Width = 135,
                FontSize = 14,
                HorizontalContentAlignment = HorizontalAlignment.Center
            };

            AddControlToNode(label);
        }



        public override void Calculate()
        {
            //Find all ConceptRoots (example Slabs) in Model View
            ModelView modelView = (ModelView)(InputPorts[1].Data);
            Dictionary<string, ConceptRoot> conceptRoot = modelView.GetAllConceptRoots();
            List<string> mvdRoots = new List<string>();
            foreach (var root in conceptRoot)
            {
                mvdRoots.Add(root.Value.name.ToString());
            }

            //Filter IFC for ConceptRoots
            IfcVersionType = InputPorts[0].Data.GetType();
            if (IfcVersionType.Name == "ModelInfoIFC2x3")
            {
                var modelid = ((ModelInfoIFC2x3)(InputPorts[0].Data)).ModelId;
                xModel = DataController.Instance.GetModel(modelid);
                var elementIdsList = ((ModelInfoIFC2x3)(InputPorts[0].Data)).ElementIds;
                var res = new HashSet<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId>(elementIdsList);

                List<Xbim.Ifc2x3.Kernel.IfcProduct> elements = xModel.Instances.OfType<Xbim.Ifc2x3.Kernel.IfcProduct>().ToList();
                List<Xbim.Ifc2x3.Kernel.IfcProduct> elementsFilterd = new List<IfcProduct>();
                foreach (string mvdRoot in mvdRoots)
                {

                    switch (mvdRoot)
                    {
                        case "Slab":
                            elementsFilterd.AddRange(elements.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcSlab>().ToList());
                            break;
                        case "Wall":
                            elementsFilterd.AddRange(elements.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcWall>().ToList());
                            break;
                        case "Column":
                            elementsFilterd.AddRange(elements.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcColumn>().ToList());
                            break;
                        case "Window":
                            elementsFilterd.AddRange(elements.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcWindow>().ToList());
                            break;
                        case "Door":
                            elementsFilterd.AddRange(elements.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcDoor>().ToList());
                            break;
                        case "CurtainWall":
                            elementsFilterd.AddRange(elements.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcCurtainWall>().ToList());
                            break;

                    }
                }

                PropertyTranformDelegate propTransform = delegate (ExpressMetaProperty prop, object toCopy)
                {
                    var value = prop.PropertyInfo.GetValue(toCopy, null);
                    return value;
                };

                var newModel = IfcStore.Create(IfcSchemaVersion.Ifc2X3, XbimStoreType.InMemoryModel);
                XbimInstanceHandleMap copied = null;
                using (var txn = newModel.BeginTransaction())
                {
                    copied = new XbimInstanceHandleMap(xModel, newModel);
                    foreach (var element in elementsFilterd)
                    {
                        if (res.Contains(element.GlobalId))
                        {
                            newModel.InsertCopy(element, copied, propTransform, false, false);
                        }
                    }
                    txn.Commit();
                }

                


                //List<string> hierarchy =new List<string> { "HasAssociations", "RelatingMaterial", "ForLayerSet", "DirectionSense", "LayerSetDirection" };

                List<string> hierarchy = new List<string> { "HasAssociations", "RelatingMaterial" };

                //normal way to get "material" value
                foreach (var item in elementsFilterd)
                {
                    if (item.HasAssociations == null)
                        continue;

                    foreach (var asso in item.HasAssociations.ToList())
                    {
                        if (asso == null) //asso is of type "IfcRelAssociates",which is a base class of "IfcRelAssociatesMaterial"
                            continue;
                        var material = (asso as IfcRelAssociatesMaterial).RelatingMaterial; //asso is converted to its subtype "IfcRelAssociatesMaterial"
                                                                                            //to access the property "RelatingMaterial"

                        if (material == null)//material gets the value of type "IfcMaterialLayerSetUsage"
                            continue;
                    }

                }

                //in our case, property "HasAssociations" and "RelatingMaterial" has to be read in from mvd,
                //and we don't know we have to convert type in some cases
                foreach (var item in elementsFilterd)
                {
                    if (item.GetType().GetProperty("HasAssociations").GetValue(item) == null)
                        continue;

                    foreach (var asso in item.HasAssociations.ToList())
                    {

                        if (asso == null)
                            continue;
                        var assoConverted = asso as IfcRelAssociatesMaterial;
                        var material = assoConverted.GetType().GetProperty("RelatingMaterial").GetValue(assoConverted);

                        if (material == null)//material gets the value of type "IfcMaterialLayerSetUsage"
                            continue;
                    }

                }

                var a = elementsFilterd[0].HasAssociations;
                var c = ((IfcRelAssociatesMaterial)(a.ToList()[0])).RelatingMaterial;
                // 
                var x = elementsFilterd[0].GetType().GetProperty(hierarchy[0]).GetValue(elementsFilterd[0]);
                // var y = x.GetType().GetProperty(hierarchy[1]).GetValue(x);
                // var z1 = y.GetType().GetProperty(hierarchy[3]).GetValue(y);
                // var z2 = y.GetType().GetProperty(hierarchy[4]).GetValue(y);

            }

            else if (IfcVersionType.Name == "ModelInfoIFC4")
            {
                var modelid = ((ModelInfoIFC4)(InputPorts[0].Data)).ModelId;
                xModel = DataController.Instance.GetModel(modelid);
                List<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId> elementsids = ((ModelInfoIFC4)(InputPorts[0].Data)).ElementIds;

            }
        }





        public override Node Clone()
        {
            return new IfcMvdFilter_Another_Version(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }

}
