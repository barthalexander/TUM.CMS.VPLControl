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
//using Xbim.Ifc2x3.GeometricConstraintResource;


 namespace TUM.CMS.VplControl.IFC.Nodes
 {
    public class IfcFilterWithMvd : Node
    {
        public XbimModel xModel;
        public ModelInfo outputInfo;
        public HashSet<String> ChosenEntities;

        public IfcFilterWithMvd(Core.VplControl hostCanvas)
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
            Console.WriteLine(" 1 BEGIN");

            //get xbim-model of the ifc-file from the IfcParseNode
            Console.WriteLine(InputPorts[0].Data.GetType().ToString());

            string modelid = ((ModelInfo)(InputPorts[0].Data)).ModelId;
            List<IfcGloballyUniqueId> elementsids = ((ModelInfo)(InputPorts[0].Data)).ElementIds;

            if (modelid == null) return;
            Console.WriteLine("modelid={0}", modelid);

            xModel = DataController.Instance.GetModel(modelid); 
           /*                   
           foreach (IfcGloballyUniqueId uid in elementsids)
            {
                Console.WriteLine(uid);
            }
            */
            Console.WriteLine("elementsids.Count()={0}",elementsids.Count());  //-->1076

            var comboBox = ControlElements[2] as ComboBox;
            if (comboBox != null && comboBox.Items.Count > 0)
            {
                comboBox.SelectedItem = -1;
                comboBox.Items.Clear();
            }




            //Console.WriteLine("xModel.IfcSite.Count()={0}", IfcCount);
            /*
            foreach (IfcGloballyUniqueId el in elementsids)
            {
                Console.WriteLine(el.ToString());
            }
            */

            //outputInfo = new ModelInfo(modelid);
            //xModel = DataController.Instance.GetModel(modelid);

            Console.WriteLine(" 2 ");

            //get the mvd-file from the IfcMvdXMLReader
            MvdXMLReader mvd = (MvdXMLReader)InputPorts[1].Data;

            //We find all applicableEntities from all ConceptTemplates and all EntityNames from EntiryRules
            //The ifc-elements of these entity-types are going to be the output
            ChosenEntities = new HashSet<String>();

            Dictionary<string, ConceptTemplate> dict = mvd.templates.getConceptTemplates();

            Console.WriteLine("BEGIN");
            FindEntityNames(dict);
            Console.WriteLine("END");
            foreach (string s in ChosenEntities)
            {
                Console.WriteLine(s);     //--> 
            }


            // var ifcwindow = xModel.IfcProducts.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcWindow>().ToList();
            var ifcLocalPlacement = xModel.IfcProducts.OfType<Xbim.Ifc2x3.GeometricConstraintResource.IfcLocalPlacement>().ToList();
            var ifcAxis2Placement3D = xModel.IfcProducts.OfType<Xbim.Ifc2x3.GeometryResource.IfcAxis2Placement3D>().ToList();
            var ifcCartesianPoint = xModel.IfcProducts.OfType<Xbim.Ifc2x3.GeometryResource.IfcCartesianPoint>().ToList();
            var ifcDirection = xModel.IfcProducts.OfType<Xbim.Ifc2x3.GeometryResource.IfcDirection>().ToList();
            //there are Not in the IFC file 
            var ifcRelNests = xModel.IfcProducts.OfType<Xbim.Ifc2x3.Kernel.IfcRelNests>().ToList();
            var ifcFlowDirectionEnum = xModel.IfcProducts.OfType<Xbim.Ifc2x3.SharedBldgServiceElements.IfcFlowDirectionEnum>().ToList();
            // IfcDistributionSystemEnum --> NOT FOUND

            List<IfcGloballyUniqueId> IfcLocalPlacementFiltered = new List<IfcGloballyUniqueId> { };
            List<IfcGloballyUniqueId> IfcAxis2Placement3DFiltered = new List<IfcGloballyUniqueId> { };
            List<IfcGloballyUniqueId> IfcCartesianPointFiltered = new List<IfcGloballyUniqueId> { };
            List<IfcGloballyUniqueId> IfcDirectionFiltered = new List<IfcGloballyUniqueId> { };
            List<IfcGloballyUniqueId> IfcRelNestsFiltered = new List<IfcGloballyUniqueId> { };
            List<IfcGloballyUniqueId> IfcFlowDirectionEnumFiltered = new List<IfcGloballyUniqueId> { };

            foreach (var item in ifcLocalPlacement)
            {

                IfcLocalPlacementFiltered.Add(item.GlobalId);
                
            }

        }




        //Every template has one CONCEPTTEMPLATE
        //dict : current ConceptTemplate
        private void FindEntityNames(Dictionary<string, ConceptTemplate> dict)
        {
            foreach (KeyValuePair<string, ConceptTemplate> pair in dict)
            {
                //Current ConceptTemplate
                Console.WriteLine("uuid of current ConceptTemplate: {0}", pair.Key);

                //Look for Rules in current ConceptTemplate
                //Console.WriteLine("Lets look for AttributeRule count={0}", pair.Value.getAttributeRules().Count);
                List<AttributeRule> AttRulesList = pair.Value.getAttributeRules();
                foreach (AttributeRule ar in AttRulesList)
                    FindEntityRules(ar);



                //Look for SubTemplates and new ConceptTemplates in current ConceptTemplate
                List<Templates> t1 = pair.Value.getTemplate();     //t1 : List of SubTemplates of current ConceptTemplate
                for (int i = 0; i < pair.Value.getTemplate().Count; i++)
                {
                    //new ConceptTemplate : t[i].getConceptTemplates() --> only one (?)
                    Dictionary<string, ConceptTemplate> dict1 = new Dictionary<string, ConceptTemplate>();
                    dict1 = t1[0].getConceptTemplates();   //dict1 --> new current ConceptTemplate
                    FindEntityNames(dict1);

                }
            }

        }



        private void FindEntityRules(AttributeRule AttRule)
        {
            //Console.WriteLine("AttributeName: {0}, EntityRules count: {1}", AttRule.attributeName, AttRule.getEntityRules().Count);
            List<EntityRule> listEnRules = AttRule.getEntityRules();
            foreach (EntityRule en in listEnRules)
            {
                //Console.WriteLine("EntityName={0}", en.entityName);
                ChosenEntities.Add(en.entityName);
                FindAttributeRules(en);
            }

        }


        private void FindAttributeRules(EntityRule EnRule)
        {
            //Console.WriteLine("EntityName: {0}, AttributeRules count: {1}", EnRule.entityName, EnRule.getAttributeRules().Count);
            List<AttributeRule> listAttRules = EnRule.getAttributeRules();
            foreach (AttributeRule at in listAttRules)
            {
                FindEntityRules(at);
            }
        }


        private void getConceptTemplates(ConceptTemplate conceptTemplate)
        {
            ChosenEntities.Add(conceptTemplate.applicableEntity);
            foreach (ConceptTemplate ct in conceptTemplate.getConceptTemplateses().Values)
            {
                getConceptTemplates(ct);

            }
        }

        private void getEntityRules1(ConceptTemplate conceptTemplate)
        {
            foreach (AttributeRule ar in conceptTemplate.getAttributeRules())
            {
                getEntityRules2(ar);
            }
        }

        private void getEntityRules2(AttributeRule attributeRule)
        {
            foreach (EntityRule er in attributeRule.getEntityRules())
            {
                ChosenEntities.Add(er.entityName);
                foreach (AttributeRule ar in er.getAttributeRules())
                {
                    getEntityRules2(ar);
                }
            }

        }

        public override Node Clone()
        {
            return new mvdXMLReaderNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
 }
