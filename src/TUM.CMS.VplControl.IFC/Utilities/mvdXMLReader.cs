using System;
using System.Xml;
using TUM.CMS.VplControl.IFC.Utilities.mvdXMLReaderClasses;

namespace TUM.CMS.VplControl.IFC.Utilities
{
    public class MvdXMLReader
    {
        public string filepath { get; set; }
        public Templates templates { get; set; }
        private ModelView modelView;
        public MvdXMLReader(string filepath)
        {
            this.filepath = filepath;
        }

        public void readXML()
        {
            XmlDocument xmlFile = new XmlDocument();
            xmlFile.Load(filepath);
            XmlNode rootNode = xmlFile.DocumentElement;
            Templates templateClass = new Templates();
            ModelView modelViewClass = new ModelView();

            templates = DisplayTemplates(rootNode, null, templateClass, false, false, null, null);
            modelView = DisplayModelView(rootNode, templates, modelViewClass, null, null, null);
        }

        public ModelView GetModelView()
        {
            return modelView;
        }
        private static Templates DisplayTemplates(XmlNode node, ConceptTemplate conceptTemplate, Templates templates, bool subTemplates, bool rules, AttributeRule attributeRule, EntityRule entityRule)
        {
            //Print the node type, node name and node value of the node
            if (node.Name == "ConceptTemplate")
            {
                conceptTemplate = new ConceptTemplate();
                if (node.Attributes != null)
                {
                    XmlAttributeCollection attrs = node.Attributes;
                    foreach (XmlAttribute attr in attrs)
                    {
                        switch (attr.Name)
                        {
                            case "uuid":
                                conceptTemplate.setUUID(attr.Value);
                                break;
                            case "name":
                                conceptTemplate.name = attr.Value;
                                break;
                            case "code":
                                conceptTemplate.code = attr.Value;
                                break;
                            case "status":
                                break;
                            case "applicableSchema":
                                conceptTemplate.applicableSchema = attr.Value;
                                break;
                            case "applicableEntity":
                                conceptTemplate.applicableEntity = attr.Value;
                                break;
                        }
                    }
                }
                templates.addConceptTemplate(conceptTemplate.getUUID(), conceptTemplate);
            }
            else if (node.Name == "Definition")
            {
                foreach (XmlNode childNode in node.ChildNodes)
                {
                    Definition definition = new Definition();
                    definition.setBody(childNode.InnerText);
                    conceptTemplate.addDefinition(definition);
                }
            }
            else if (node.Name == "SubTemplates")
            {
                templates = new Templates();
                conceptTemplate.addTemplates(templates);
            }
            else if (node.Name == "Rules")
            {
                rules = true;
            }

            else if (node.Name == "AttributeRule")
            {

                attributeRule = new AttributeRule();

                if (node.Attributes != null)
                {
                    XmlAttributeCollection attrs = node.Attributes;
                    foreach (XmlAttribute attr in attrs)
                    {
                        if (attr.Name == "AttributeName")
                        {
                            attributeRule.attributeName = attr.Value;
                        }
                        else if (attr.Name == "Cardinality")
                        {
                            attributeRule.cardinality = attr.Value;
                        }
                    }
                }
                if (rules == true)
                {
                    conceptTemplate.addAttributeRule(attributeRule);
                }
                else
                {
                    entityRule.addAttributeRule(attributeRule);
                }


            }
            else if (node.Name == "EntityRule")
            {
                rules = false;
                entityRule = new EntityRule();
                string attributeName = attributeRule.attributeName;
                if (!String.IsNullOrEmpty(attributeName))
                {
                    if (node.Attributes != null)
                    {
                        XmlAttributeCollection attrs = node.Attributes;
                        foreach (XmlAttribute attr in attrs)
                        {
                            if (attr.Name == "EntityName")
                            {
                                entityRule.entityName = attr.Value;
                            }
                            else if (attr.Name == "Cardinality")
                            {
                                entityRule.cardinality = attr.Value;
                            }
                        }
                    }
                    attributeRule.addEntityRule(entityRule);

                }

            }
            else
            {
                Console.WriteLine("Type = [" + node.NodeType + "] Name = " + node.Name);
            }

            //Print attributes of the node
            if (node.Attributes != null)
            {
                XmlAttributeCollection attrs = node.Attributes;
                foreach (XmlAttribute attr in attrs)
                {
                    Console.WriteLine("Attribute Name = " + attr.Name + "; Attribute Value = " + attr.Value);
                }
            }

            //Print individual children of the node, gets only direct children of the node
            XmlNodeList children = node.ChildNodes;
            foreach (XmlNode child in children)
            {

                DisplayTemplates(child, conceptTemplate, templates, subTemplates, rules, attributeRule, entityRule);


            }
            return templates;
        }
        private static ModelView DisplayModelView(XmlNode node, Templates templates, ModelView modelView, ConceptRoot conceptRoot, Concept concept, Requirement requirement)
        {
            if (node.Name == "ModelView")
            {
                if (node.Attributes != null)
                {
                    XmlAttributeCollection attrs = node.Attributes;
                    foreach (XmlAttribute attr in attrs)
                    {
                        switch (attr.Name)
                        {
                            case "uuid":
                                modelView.uuid = attr.Value;
                                break;
                            case "name":
                                modelView.name = attr.Value;
                                break;
                            case "version":
                                modelView.version = attr.Value;
                                break;
                            case "applicableSchema":
                                modelView.applicableSchema = attr.Value;
                                break;
                        }
                    }
                }
            }

            else if (node.Name == "ExchangeRequirement")
            {
                ExchangeRequirement exchangeRequirement = new ExchangeRequirement();
                if (node.Attributes != null)
                {
                    XmlAttributeCollection attrs = node.Attributes;
                    foreach (XmlAttribute attr in attrs)
                    {
                        switch (attr.Name)
                        {
                            case "uuid":
                                exchangeRequirement.uuid = attr.Value;
                                break;
                            case "name":
                                exchangeRequirement.name = attr.Value;
                                break;
                            case "applicability":
                                exchangeRequirement.version = attr.Value;
                                break;
                        }
                    }
                }
                modelView.addExchangeRequirements(exchangeRequirement.uuid, exchangeRequirement);
            }
            else if (node.Name == "ConceptRoot")
            {
                conceptRoot = new ConceptRoot();
                if (node.Attributes != null)
                {
                    XmlAttributeCollection attrs = node.Attributes;
                    foreach (XmlAttribute attr in attrs)
                    {
                        switch (attr.Name)
                        {
                            case "uuid":
                                conceptRoot.uuid = attr.Value;
                                break;
                            case "name":
                                conceptRoot.name = attr.Value;
                                break;
                            case "applicableRootEntity":
                                conceptRoot.applicableRootEntity = attr.Value;
                                break;
                        }
                    }
                }
                modelView.addRoots(conceptRoot.uuid, conceptRoot);
            }
            else if (node.Name == "Concept")
            {
                concept = new Concept();
                if (node.Attributes != null)
                {
                    XmlAttributeCollection attrs = node.Attributes;
                    foreach (XmlAttribute attr in attrs)
                    {
                        switch (attr.Name)
                        {
                            case "uuid":
                                concept.uuid = attr.Value;
                                break;
                            case "name":
                                concept.name = attr.Value;
                                break;
                            case "Override":
                                concept.Override = attr.Value;
                                break;
                        }
                    }
                }


                // concept.setConceptTemplate(FindTemplate(templates, concept.uuid));
                conceptRoot.addConcept(concept.uuid, concept);
            }
            else if (node.Name == "Template")
            {
                if (node.Attributes != null)
                {
                    XmlAttributeCollection attrs = node.Attributes;
                    foreach (XmlAttribute attr in attrs)
                    {
                        switch (attr.Name)
                        {
                            case "ref":
                                var uuid = attr.Value;
                                foreach (var entry in templates.getConceptTemplates())
                                {
                                    ConceptTemplate conceptTemplate = entry.Value;
                                    concept.setConceptTemplate(FindTemplate(conceptTemplate, uuid));
                                }
                                break;
                        }
                    }
                }

            }
            XmlNodeList children = node.ChildNodes;
            foreach (XmlNode child in children)
            {

                DisplayModelView(child, templates, modelView, conceptRoot, concept, requirement);


            }
            return modelView;
        }
        private static ConceptTemplate FindTemplate(ConceptTemplate conceptTemplate, string uuid)
        {

            Console.WriteLine(conceptTemplate);
            var conceptUUID = conceptTemplate.getUUID();
            if (conceptUUID == uuid)
            {
            }
            else
            {
                foreach (var item in conceptTemplate.getTemplate())
                {
                    Console.WriteLine(item);
                    foreach (var entry in item.getConceptTemplates())
                    {
                        ConceptTemplate conceptTemplate1 = entry.Value;
                        FindTemplate(conceptTemplate1, uuid);
                    }
                }
            }
            return conceptTemplate;
        }
    }
}
