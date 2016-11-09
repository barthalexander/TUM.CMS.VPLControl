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
        public List<IfcProduct> elements = new List<IfcProduct> { };

        public HashSet<String> ChosenEntities;
        public object transporting;

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
                    else { elements.AddRange(xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcSlab>().ToList()); }
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
                    if (conceptRootMatch == false) { return conceptRootMatch; }
                }
            }



            return true;

        }

        public static Type[] getTypeByName(string className)
        {
            List<Type> returnVal = new List<Type>();
            
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
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
            }

            return returnVal.ToArray();
        }

        public T Cast<T>(object input)
        {
            return (T)input;
        }
        public bool CheckConcept(Concept concept)
        {
            //Get ConceptTemplate Name from MVD
            string conceptName = concept.name;
            conceptName = string.Join("", conceptName.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
            conceptName = "Ifc" + conceptName;

            //convert ConceptTemplate MVD into corresponding Types in IfcModel
            Type[] TypesOfConceptInMvd= getTypeByName(conceptName);
            //Xbim.Ifc2x3.MaterialResource.IfcMaterialLayerSetUsage
            
            foreach (Type TypeOfConceptInMvd in TypesOfConceptInMvd)
            {
                var IfcProperties = elements[0].GetType().GetProperties();

                Type[] interfaces=TypeOfConceptInMvd.GetInterfaces();
                                        

                foreach (Type item in interfaces)
                {
                    
                    if(IfcProperties.FirstOrDefault(a => a.PropertyType == item)!=null)
                    {
                        PropertyInfo propertyneeded = IfcProperties.FirstOrDefault(a => a.PropertyType == item);
                        //var obj = Activator.CreateInstance(types[1]);
                        var obj=propertyneeded.GetValue(elements[0]);
                        

                        PropertyInfo propertyagain=obj.GetType().GetRuntimeProperties().FirstOrDefault(a=>a.Name=="DirectionSense");
                        var obj2 = propertyagain.GetValue(obj);
                        
                        //if(obj2==POSITIVE)
                    }
                    
                }



                

                //Xbim.Ifc4.Interfaces.IIfcMaterialSelect
                  //  IfcObjectDefinition.
               // PropertyInfo propertyneeded= properties.FirstOrDefault(a => a.PropertyType==types[0]);

            }

            //Assembly resultAssembly = assemblies.FirstOrDefault(a => a.GetType("IfcMaterialLayerSetUsage", false) != null);

            bool conceptMatch = true;
            ConceptTemplate conceptTemplate = concept.getConceptTemplate();
            conceptMatch = CheckConceptTemplate(conceptTemplate);
            if (conceptMatch == false) { return conceptMatch; }

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

                    conceptTemplateMatch = CheckAttributeRuleBegin(item);
                    if (conceptTemplateMatch == false) { return conceptTemplateMatch; }
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

        public bool CheckAttributeRuleBegin(AttributeRule attributeRule)
        {
            bool attributeRuleMatch = true;
            string attributeRuleName = attributeRule.attributeName;
            foreach (IfcProduct element in elements)
            {
                if (element.GetType().GetProperty(attributeRuleName).GetValue(element) == null)
                {
                    attributeRuleMatch = false;
                    Console.WriteLine("missing Attribute " + attributeRuleName);
                    return attributeRuleMatch;
                }
            }

            List<EntityRule> allEntityRules = attributeRule.getEntityRules();

            foreach (EntityRule entityRule in allEntityRules)
            {
               // attributeRuleMatch = CheckEntityRule(entityRule);
               // if (attributeRuleMatch == false) { return attributeRuleMatch; }

                string entityName = entityRule.entityName;
                Type[] TypesOfEntityRuleInMvd = getTypeByName("IfcRelAssociatesMaterial");

                foreach (IfcProduct element in elements)
                {
                    //get all properties in this IfcProduct
                    var IfcProperties = element.GetType().GetProperties();

                    //loop over attribute rule types
                    foreach (Type TypeOfEntityRuleInMvd in TypesOfEntityRuleInMvd)
                    {

                        Type baseType = TypeOfEntityRuleInMvd.BaseType;

                        foreach (PropertyInfo property in IfcProperties)
                        {
                            if (property.PropertyType.IsGenericType)
                            {
                                if (property.PropertyType.GetGenericArguments()[0] == baseType)
                                {
                                    PropertyInfo IfcPropertyNeeded = property;
                                    var obj = IfcPropertyNeeded.GetValue(element) as IEnumerable;
                                    if (obj != null)
                                    {
                                        foreach (var item in obj)
                                        {
                                            transporting = item;//IfcRelAssociatesMaterial
                                            if (item.GetType() == TypeOfEntityRuleInMvd)
                                            {                                             
                                                attributeRuleMatch = CheckEntityRule(entityRule); //IfcRelAssociatesMaterial
                                                if (attributeRuleMatch == false) { return attributeRuleMatch; }
                                            }
                                        }

                                    }



                                }
                            }
                            else
                            {
                                if (property.PropertyType == baseType)
                                {
                                    PropertyInfo IfcPropertyNeeded = property;
                                    var obj = IfcPropertyNeeded.GetValue(element);

                                }
                            }

                        }

                        Type[] interfaces = TypeOfEntityRuleInMvd.GetInterfaces();

                        foreach (Type item in interfaces)
                        {
                            if (IfcProperties.FirstOrDefault(a => a.PropertyType == item) != null)
                            {
                                PropertyInfo IfcPropertyNeeded = IfcProperties.FirstOrDefault(a => a.PropertyType == item);
                                var obj = IfcPropertyNeeded.GetValue(element);
                            }
                        }


                    }
                }

            }

    
                          
                   
            
            
            return true;
        }

        public bool CheckAttributeRule(AttributeRule attributeRule)
        {
            bool attributeRuleMatch = true;
            string attributeRuleName = attributeRule.attributeName;//RelatingMaterial

            if (transporting.GetType().GetProperty(attributeRuleName).GetValue(transporting) == null)
            {
                attributeRuleMatch = false;
                Console.WriteLine("missing Attribute " + attributeRuleName);
                return attributeRuleMatch;
            }

            List<EntityRule> allEntityRules = attributeRule.getEntityRules();

            if (allEntityRules.Any())
            {
                foreach (EntityRule entityRule in allEntityRules)
                {                   
                    string entityName = entityRule.entityName;//IfcMaterialLayerSetUsage
                    Type[] TypesOfEntityRuleInMvd = getTypeByName(entityName);//IfcMaterialLayerSetUsage


                    //get all properties in this IfcProduct
                    var IfcProperties = transporting.GetType().GetProperties();

                    //loop over attribute rule types
                    foreach (Type TypeOfEntityRuleInMvd in TypesOfEntityRuleInMvd)
                    {

                        Type baseType = TypeOfEntityRuleInMvd.BaseType;

                        foreach (PropertyInfo property in IfcProperties)
                        {
                            if (property.PropertyType.IsGenericType)
                            {
                                if (property.PropertyType.GetGenericArguments()[0] == baseType)
                                {
                                    PropertyInfo IfcPropertyNeeded = property;
                                    var obj = IfcPropertyNeeded.GetValue(transporting) as IEnumerable;
                                    if (obj != null)
                                    {
                                        foreach (var item in obj)
                                        {
                                            transporting = item;
                                            if (item.GetType() == TypeOfEntityRuleInMvd)
                                            {
                                                CheckEntityRule(entityRule);
                                            }
                                        }

                                    }



                                }
                            }
                            else
                            {
                                if (property.PropertyType == baseType)
                                {
                                    PropertyInfo IfcPropertyNeeded = property;
                                    var obj = IfcPropertyNeeded.GetValue(transporting);

                                }
                            }

                        }

                        Type[] interfaces = TypeOfEntityRuleInMvd.GetInterfaces();

                        if (interfaces != null)
                        {
                            foreach (Type item in interfaces)
                            {
                                if (IfcProperties.FirstOrDefault(a => a.PropertyType == item) != null)
                                {
                                    PropertyInfo IfcPropertyNeeded = IfcProperties.FirstOrDefault(a => a.PropertyType == item);
                                    var obj = IfcPropertyNeeded.GetValue(transporting);
                                    transporting = obj;
                                    if (obj.GetType() == TypeOfEntityRuleInMvd)
                                    {
                                        attributeRuleMatch = CheckEntityRule(entityRule);
                                        if (attributeRuleMatch == false) { return attributeRuleMatch; }

                                    }
                                }
                            }
                        }
 
                        
                            PropertyInfo IfcPropertyNeeded2 = transporting.GetType().GetRuntimeProperties().FirstOrDefault(a => a.Name == attributeRuleName);
                            var obj2 = IfcPropertyNeeded2.GetValue(transporting);
                        transporting = obj2;

                        if (obj2.GetType() == TypeOfEntityRuleInMvd)
                        {
                            attributeRuleMatch = CheckEntityRule(entityRule);
                            if (attributeRuleMatch == false) { return attributeRuleMatch; }
                        }




                    }


                }
            }

            return true;
        }

        public bool CheckEntityRule(EntityRule entityRule)
        {
            bool entityRuleMatch = true;
            string entityName = entityRule.entityName;

            List<AttributeRule> allAttributeRules = entityRule.getAttributeRules();
            if (allAttributeRules.Any())
            {
                foreach (var item in allAttributeRules)
                {
                    entityRuleMatch=CheckAttributeRule(item);
                    if (entityRuleMatch == false) return entityRuleMatch;
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


