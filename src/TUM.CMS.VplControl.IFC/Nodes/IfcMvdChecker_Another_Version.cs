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
using System.Reflection;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcMvdChecker_Another_Version : Node
    {
        public IfcStore xModel;
        public ModelInfoIFC2x3 OutputInfoIfc2x3;
        public ModelInfoIFC4 OutputInfoIfc4;
        public Type IfcVersionType = null;
        public bool match;

        public HashSet<String> ChosenEntities;


        public IfcMvdChecker_Another_Version(Core.VplControl hostCanvas)
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
            match = true;

            ModelView modelView = (ModelView)(InputPorts[1].Data);
            Dictionary<string, ConceptRoot> allConceptRoots = modelView.GetAllConceptRoots();
            List<string> mvdRootString = new List<string>();
            if (allConceptRoots.Count > 0)
            {
                foreach (var conceptRoot in allConceptRoots)
                {
                    match = CheckConceptRoot(conceptRoot.Value);
                    mvdRootString.Add(conceptRoot.Value.ToString());
                }



            }
        }

        public bool CheckConceptRoot(ConceptRoot conceptRoot)
        {
            bool conceptRootMatch = true;
            string conceptRootString = conceptRoot.name;
            var modelid = ((ModelInfoIFC2x3)(InputPorts[0].Data)).ModelId;
            xModel = DataController.Instance.GetModel(modelid);
            switch (conceptRootString)
            {
                case "Slab":
                    if (xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcSlab>().ToList() == null) { conceptRootMatch = false; Console.WriteLine("missing concept root Slab"); return conceptRootMatch; }
                    break;
                case "Wall":
                    if (xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcWall>().ToList() == null) { conceptRootMatch = false; Console.WriteLine("missing concept root Wall"); return conceptRootMatch; }
                    break;
                case "Column":
                    if (xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcColumn>().ToList() == null) { conceptRootMatch = false; Console.WriteLine("missing concept root Column"); return conceptRootMatch; }
                    break;
                case "Window":
                    if (xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcWindow>().ToList() == null) { conceptRootMatch = false; Console.WriteLine("missing concept root Window"); return conceptRootMatch; }
                    break;
                case "Door":
                    if (xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcDoor>().ToList() == null) { conceptRootMatch = false; Console.WriteLine("missing concept root Door"); return conceptRootMatch; }
                    break;
                case "CurtainWall":
                    if (xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcCurtainWall>().ToList() == null) { conceptRootMatch = false; Console.WriteLine("missing concept root CurtainWall"); return conceptRootMatch; }
                    break;
                case "Beam":
                    if (xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcBeam>().ToList() == null) { conceptRootMatch = false; Console.WriteLine("missing concept root Beam"); return conceptRootMatch; }
                    break;
                case "Railing":
                    if (xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcRailing>().ToList() == null) { conceptRootMatch = false; Console.WriteLine("missing concept root Railing"); return conceptRootMatch; }
                    break;
                case "Plate":
                    if (xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcPlate>().ToList() == null) { conceptRootMatch = false; Console.WriteLine("missing concept root Plate"); return conceptRootMatch; }
                    break;
                case "Ramp":
                    if (xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcRamp>().ToList() == null) { conceptRootMatch = false; Console.WriteLine("missing concept root Ramp"); return conceptRootMatch; }
                    break;
                case "Roof":
                    if (xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcRoof>().ToList() == null) { conceptRootMatch = false; Console.WriteLine("missing concept root Roof"); return conceptRootMatch; }
                    break;
                case "Stair":
                    if (xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcStair>().ToList() == null) { conceptRootMatch = false; Console.WriteLine("missing concept root Stair"); return conceptRootMatch; }
                    break;
            }

            Dictionary<string, Concept> allConcepts = conceptRoot.GetAllConcepts();            

            if (allConcepts.Count > 0)
            {
                foreach (var item in allConcepts)
                {
                    conceptRootMatch = CheckConcept(item.Value);
                    if (conceptRootMatch == false) { return conceptRootMatch;}
                }
            }

            return true;
            
        }



        public bool CheckConcept(Concept concept)
        {
            bool conceptMatch = true;
            ConceptTemplate conceptTemplate = concept.getConceptTemplate();
            conceptMatch=CheckConceptTemplate(conceptTemplate);
            if(conceptMatch==false) { return conceptMatch; }

            Dictionary<string, Concept> allSubConcepts = concept.GetAllSubConcepts();
            if (allSubConcepts.Count > 0)
            {
                foreach (var item in allSubConcepts)
                {
                    conceptMatch=CheckConcept(item.Value);

                }
            }
            return true;
        }

        public bool CheckConceptTemplate(ConceptTemplate conceptTemplate)
        {
            Dictionary<string, ConceptTemplate> subConceptTemplates = conceptTemplate.getConceptTemplateses();
            if (subConceptTemplates.Count > 0)
            {

                foreach (var item in subConceptTemplates)
                {
                    CheckConceptTemplate(item.Value);

                }
            }

            List<Templates> subTemplates = conceptTemplate.getAllSubTemplates();
            if (subTemplates.Any())
            {

                foreach (var item in subTemplates)
                {
                    CheckTemplate(item);

                }
            }

            List<AttributeRule> attributeRules = conceptTemplate.getAttributeRules();
            if (attributeRules.Any())
            {

                foreach (var item in attributeRules)
                {
                    CheckAttributeRule(item);
                }
            }
            return true;
        }


        public bool CheckTemplate(Templates template)
        {
            Dictionary<string, ConceptTemplate> allConceptTemplates = template.getConceptTemplates();
            if (allConceptTemplates.Count > 0)
            {
                foreach (var item in allConceptTemplates)
                {

                    CheckConceptTemplate(item.Value);

                }
            }
            return true;
        }

        public bool CheckAttributeRule(AttributeRule attributeRule)
        {
            List<EntityRule> allEntityRules = attributeRule.getEntityRules();
            if (allEntityRules.Any())
            {
                foreach (var item in allEntityRules)
                {
                    CheckEntityRule(item);
                }
            }
            return true;
        }

        public bool CheckEntityRule(EntityRule entityRule)
        {
            List<AttributeRule> allAttributeRules = entityRule.getAttributeRules();
            if (allAttributeRules.Any())
            {
                foreach (var item in allAttributeRules)
                {
                    CheckAttributeRule(item);
                }
            }
            return true;
        }
        public override Node Clone()
        {
            return new IfcMvdChecker_Another_Version(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}


