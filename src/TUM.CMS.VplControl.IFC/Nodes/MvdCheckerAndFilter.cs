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
using TUM.CMS.VplControl.IFC.Controls;


namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class MvdCheckerAndFilter : Node
    {
        public IfcStore xModel;
        public ModelInfoIFC2x3 OutputInfoIfc2x3;
        public ModelInfoIFC4 OutputInfoIfc4;
        public Type IfcVersionType = null;
        public bool match;
        public List<IfcProduct> elements = new List<IfcProduct> { };

        public HashSet<String> ChosenEntities;
        

        public MvdCheckerAndFilter(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {
            IsResizeable = true;
            AddInputPortToNode("IfcFile", typeof(object));
            AddInputPortToNode("MvdFile", typeof(object));
            AddOutputPortToNode("FilteredIfc", typeof(object));

           

            MvdCheckerAndFilterControl mvdCheckerAndFilterControl = new MvdCheckerAndFilterControl();

            AddControlToNode(mvdCheckerAndFilterControl);

        

        }

        

        public override void Calculate()
        {
            if (InputPorts[0].Data == null) return;
            if (InputPorts[1].Data == null) return;
            match = true;

            var mvdCheckerAndFilterControl = ControlElements[0] as MvdCheckerAndFilterControl;
            if (mvdCheckerAndFilterControl == null) return;
            var scrollViewer = mvdCheckerAndFilterControl.scrollViewer as ScrollViewer;
            if (scrollViewer == null) return;
            var textBlock = scrollViewer.Content as TextBlock;
            if (textBlock == null) return;
            textBlock.Text = "";

            var modelid = ((ModelInfoIFC2x3)(InputPorts[0].Data)).ModelId;
             xModel = DataController.Instance.GetModel(modelid);            
            var elementIds = ((ModelInfoIFC2x3)(InputPorts[0].Data)).ElementIds;
            OutputInfoIfc2x3 = (ModelInfoIFC2x3)(InputPorts[0].Data);


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

            if (match == true)
            {                
                OutputInfoIfc2x3.ElementIds.Clear();
                foreach (var item in elements)
                {                  
                    OutputInfoIfc2x3.AddElementIds(item.GlobalId);
                }
                OutputPorts[0].Data = OutputInfoIfc2x3;
                MessageBox.Show("Ifc file matches with MVD! Ifc file is filtered according to MVD!", "My Application", MessageBoxButton.OK);

            }
            else
            {
                MessageBox.Show("Ifc file doesn't match with MVD! Information Missing!", "My Application", MessageBoxButton.OK);
            }
        }

        public bool CheckConceptRoot(ConceptRoot conceptRoot)
        {
            var mvdCheckerAndFilterControl = ControlElements[0] as MvdCheckerAndFilterControl;
            if (mvdCheckerAndFilterControl == null) return true;
            var scrollViewer = mvdCheckerAndFilterControl.scrollViewer as ScrollViewer;
            if (scrollViewer == null) return true;
            var textBlock = scrollViewer.Content as TextBlock;
            if (textBlock == null) return true;
            

            bool conceptRootMatch = true;
            string conceptRootString = conceptRoot.name;
         
           
            switch (conceptRootString)
            {
                case "Slab":
                    if (xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcSlab>().ToList() == null)
                    {
                        conceptRootMatch = false;
                        textBlock.Text += "missing concept root Slab\r\n!";                                                                      
                        return conceptRootMatch;
                    }
                    else
                    {
                        elements.AddRange(xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcSlab>().ToList());
                        textBlock.Text += "Concept Root Slab matches!\r\n";                        
                    }
                    break;
                case "Wall":
                    if (xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcWall>().ToList() == null) { conceptRootMatch = false; Console.WriteLine("missing concept root Wall"); return conceptRootMatch; }
                    break;
                case "Column":
                    if (xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcColumn>().ToList() == null) { conceptRootMatch = false; Console.WriteLine("missing concept root Column"); return conceptRootMatch; }
                    break;
                case "Window":
                    if (xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcWindow>().ToList() == null) { conceptRootMatch = false; Console.WriteLine("missing concept root Window"); return conceptRootMatch; }
                    else { elements.AddRange(xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcWindow>().ToList()); textBlock.Text += "Concept Root Window matches!\r\n"; }
                    break;
                case "Door":
                    if (xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcDoor>().ToList() == null) { conceptRootMatch = false; Console.WriteLine("missing concept root Door"); return conceptRootMatch; }
                    else { elements.AddRange(xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcDoor>().ToList()); textBlock.Text += "Concept Root Door matches!\r\n"; }
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
                    if (conceptRootMatch == false) { return conceptRootMatch; }
                }
            }



            return true;

        }

        public static Type[] getTypeByNameFromIfc2x3(string className)
        {
            List<Type> returnVal = new List<Type>();
            Assembly ifc2x3=Assembly.LoadWithPartialName("Xbim.Ifc2x3");

            //Assembly ifc4 = Assembly.LoadWithPartialName("Xbim.Ifc4");
            Type[] assemblyTypes = ifc2x3.GetTypes();
            for (int j = 0; j < assemblyTypes.Length; j++)
            {
                if (assemblyTypes[j].Name == className)
                {
                    returnVal.Add(assemblyTypes[j]);
                    
                }
            }

            /*foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] assemblyTypes = a.GetTypes();
                for (int j = 0; j < assemblyTypes.Length; j++)
                {
                    if (assemblyTypes[j].Name == className)
                    {
                        returnVal.Add(assemblyTypes[j]);
                        if(returnVal.Count==2) return returnVal.ToArray();
                    }
                }
            }*/

            return returnVal.ToArray();
            
        }

        public T Cast<T>(object input)
        {
            return (T)input;
        }
        public bool CheckConcept(Concept concept)//Material Layer Set Usage
        {

            var mvdCheckerAndFilterControl = ControlElements[0] as MvdCheckerAndFilterControl;
            if (mvdCheckerAndFilterControl == null) return true;
            var scrollViewer = mvdCheckerAndFilterControl.scrollViewer as ScrollViewer;
            if (scrollViewer == null) return true;
            var textBlock = scrollViewer.Content as TextBlock;
            if (textBlock == null) return true;

            string conceptName = concept.name;                                                    
            bool conceptMatch = true;
            ConceptTemplate conceptTemplate = concept.getConceptTemplate();
            conceptMatch = CheckConceptTemplate(conceptTemplate);
            if (conceptMatch == false) { return conceptMatch; } else { textBlock.Text += "Concept " + conceptName + " matches!\r\n"; }

            Dictionary<string, Concept> allSubConcepts = concept.GetAllSubConcepts();
            if (allSubConcepts.Count > 0)
            {
                foreach (var item in allSubConcepts)
                {
                    conceptMatch = CheckConcept(item.Value);

                }
            }
            return true;
        }

        public bool CheckConceptTemplate(ConceptTemplate conceptTemplate)
        {
           
            bool conceptTemplateMatch = true;

            Dictionary<string, ConceptTemplate> subConceptTemplates = conceptTemplate.getConceptTemplateses();
            if (subConceptTemplates.Count > 0)
            {

                foreach (var item in subConceptTemplates)
                {
                    conceptTemplateMatch=CheckConceptTemplate(item.Value);
                    if (conceptTemplateMatch == false) { return conceptTemplateMatch; } 
                }
            }

            List<Templates> subTemplates = conceptTemplate.getAllSubTemplates();
            if (subTemplates.Any())
            {

                foreach (var item in subTemplates)
                {
                    conceptTemplateMatch= CheckTemplate(item);
                    if (conceptTemplateMatch == false) { return conceptTemplateMatch; } 
                }
            }

            List<AttributeRule> attributeRules = conceptTemplate.getAttributeRules();
            if (attributeRules.Any())
            {

                foreach (var item in attributeRules)
                {

                    conceptTemplateMatch = CheckAttributeRuleBegin(item);
                    if (conceptTemplateMatch == false) { return conceptTemplateMatch; }
                }
            }
            return true;
        }


        public bool CheckTemplate(Templates template)
        {

            bool templateMatch = true;
            Dictionary<string, ConceptTemplate> allConceptTemplates = template.getConceptTemplates();
            if (allConceptTemplates.Count > 0)
            {
                foreach (var item in allConceptTemplates)
                {

                    templateMatch=CheckConceptTemplate(item.Value);
                    if (templateMatch == false) { return templateMatch; }

                }
            }
            return true;
        }

        public bool CheckAttributeRuleBegin(AttributeRule attributeRule)
        {
            var mvdCheckerAndFilterControl = ControlElements[0] as MvdCheckerAndFilterControl;
            if (mvdCheckerAndFilterControl == null) return true;
            var scrollViewer = mvdCheckerAndFilterControl.scrollViewer as ScrollViewer;
            if (scrollViewer == null) return true;
            var textBlock = scrollViewer.Content as TextBlock;
            if (textBlock == null) return true;

            object transportingEntity;

            bool attributeRuleMatch = true;
            string attributeRuleName = attributeRule.attributeName;//HasAssociations
            foreach (IfcProduct element in elements)
            {
                if (element.GetType().GetProperty(attributeRuleName).GetValue(element) == null)
                {
                    attributeRuleMatch = false;
                    textBlock.Text += "missing Attribute " + attributeRuleName+"!\r\n";
                    return attributeRuleMatch;
                }
            }
            if (attributeRuleMatch == true)
            {
                textBlock.Text += "Attribute Rule " + attributeRuleName + " matches!" + "\r\n";
                
            }

            List<EntityRule> allEntityRules = attributeRule.getEntityRules();

            foreach (EntityRule entityRule in allEntityRules)//IfcRelAssociatesMaterial
            {

                string entityName = entityRule.entityName;
                Type TypeOfEntityRuleInMvd = getTypeByNameFromIfc2x3(entityName)[0];

                // foreach (IfcProduct element in elements)
                //{

                var IfcProperties = elements[0].GetType().GetProperties();

                
                Type baseType = TypeOfEntityRuleInMvd.BaseType;
                
                foreach (PropertyInfo property in IfcProperties)
                {
                    if (property.PropertyType.IsGenericType)
                    {
                        if (property.PropertyType.GetGenericArguments()[0] == baseType || property.PropertyType.GetGenericArguments()[0]== TypeOfEntityRuleInMvd)
                        {
                            PropertyInfo IfcPropertyNeeded = property;
                            var obj = IfcPropertyNeeded.GetValue(elements[0]) as IEnumerable;
                            if (obj != null)
                            {
                                foreach (var item in obj)
                                {
                                    transportingEntity = item;//IfcRelAssociatesMaterial
                                    if (item.GetType() == TypeOfEntityRuleInMvd)
                                    {
                                        textBlock.Text += "Entity Rule " + entityName + " matches!\r\n";
                                        attributeRuleMatch = CheckEntityRule(entityRule,transportingEntity); //IfcRelAssociatesMaterial
                                        if (attributeRuleMatch == false) { return attributeRuleMatch; }
                                    }
                                }

                            }

                        }
                       
                    }
                    /* else
                     {
                         if (property.PropertyType == baseType)
                         {
                             PropertyInfo IfcPropertyNeeded = property;
                             var obj = IfcPropertyNeeded.GetValue(element);

                         }
                     }*/

                }
                //}

            }

            return true;
        }

        public bool CheckAttributeRule(AttributeRule attributeRule,object transportingEntity)
        {
            var mvdCheckerAndFilterControl = ControlElements[0] as MvdCheckerAndFilterControl;
            if (mvdCheckerAndFilterControl == null) return true;
            var scrollViewer = mvdCheckerAndFilterControl.scrollViewer as ScrollViewer;
            if (scrollViewer == null) return true;
            var textBlock = scrollViewer.Content as TextBlock;
            if (textBlock == null) return true;


            bool attributeRuleMatch = true;
            string attributeRuleName = attributeRule.attributeName;//Error: attribute = RelatingElement

            if (transportingEntity.GetType().GetProperty(attributeRuleName) == null
                || transportingEntity.GetType().GetProperty(attributeRuleName).GetValue(transportingEntity) ==null)
            {                           
                attributeRuleMatch = false;
                textBlock.Text +="missing Attribute " + attributeRuleName+ "\r\n";
                return attributeRuleMatch;
            }
            if (attributeRuleMatch == true) textBlock.Text += "AttributeRule " + attributeRuleName + " matches!\r\n";

            List<EntityRule> allEntityRules = attributeRule.getEntityRules();

            if (allEntityRules.Any())
            {
                foreach (EntityRule entityRule in allEntityRules)
                {
                    string entityName = entityRule.entityName;//IfcMaterialLayerSetUsage,IfcMaterialLayerSet
                    Type TypeOfEntityRuleInMvd = getTypeByNameFromIfc2x3(entityName)[0];//IfcMaterialLayerSetUsage,IfcMaterialLayerSet



                    var IfcProperties = transportingEntity.GetType().GetProperties();

                    //loop over attribute rule types


                    Type[] interfaces = TypeOfEntityRuleInMvd.GetInterfaces();

                    if (interfaces != null)
                    {
                        foreach (Type item in interfaces)
                        {
                            if (IfcProperties.FirstOrDefault(a => a.PropertyType == item) != null)
                            {
                                PropertyInfo IfcPropertyNeeded = IfcProperties.FirstOrDefault(a => a.PropertyType == item);
                                var obj = IfcPropertyNeeded.GetValue(transportingEntity);
                                
                                if (obj.GetType() == TypeOfEntityRuleInMvd)
                                {
                                    textBlock.Text += "Entity Rule " + entityName + " matches!\r\n";
                                    attributeRuleMatch = CheckEntityRule(entityRule,obj);
                                    if (attributeRuleMatch == false) { return attributeRuleMatch; }
                                    return true;
                                }
                            }
                        }
                    }

                    if (IfcProperties.FirstOrDefault(a => a.PropertyType == TypeOfEntityRuleInMvd) != null)
                    {
                        PropertyInfo IfcPropertyNeeded = IfcProperties.FirstOrDefault(a => a.PropertyType == TypeOfEntityRuleInMvd);
                        var obj = IfcPropertyNeeded.GetValue(transportingEntity);

                        if (obj.GetType() == TypeOfEntityRuleInMvd)
                        {
                            textBlock.Text += "Entity Rule " + entityName + " matches!\r\n";
                            attributeRuleMatch = CheckEntityRule(entityRule, obj);
                            if (attributeRuleMatch == false) { return attributeRuleMatch; }
                            return true;
                        }
                    }

                    else
                    {
                        PropertyInfo IfcPropertyNeeded = transportingEntity.GetType().GetRuntimeProperties().FirstOrDefault(a => a.Name == attributeRuleName);
                        var obj = IfcPropertyNeeded.GetValue(transportingEntity);
                        if (obj != null)
                        {
                            textBlock.Text += "EntityRule " + entityName + " matches!\r\n";
                            attributeRuleMatch = CheckEntityRule(entityRule,obj);
                            if (attributeRuleMatch == false) { return attributeRuleMatch; }
                            return true;
                        }
                    }
                    


                }
            }

            return true;
        }

        public bool CheckEntityRule(EntityRule entityRule,object transportingEntity)
        {
            bool entityRuleMatch = true;
            string entityName = entityRule.entityName;

            List<AttributeRule> allAttributeRules = entityRule.getAttributeRules();
            if (allAttributeRules.Any())
            {
                foreach (var item in allAttributeRules)
                {
                    entityRuleMatch=CheckAttributeRule(item, transportingEntity);
                    if (entityRuleMatch == false) return entityRuleMatch;
                }
            }
            return true;
        }
        public override Node Clone()
        {
            return new MvdCheckerAndFilter(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}


