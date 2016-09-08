using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using TUM.CMS.VplControl.Core;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class mvdXMLReaderNode : Node
    {
        public mvdXMLReaderNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {
            AddInputPortToNode("file", typeof (string));

            AddOutputPortToNode("object", typeof (object));

            var label = new Label
            {
                Content = "Reading mvdXML",
                Width = 120,
                FontSize = 15,
                HorizontalContentAlignment = HorizontalAlignment.Center
            };

            AddControlToNode(label);
        }
        private BackgroundWorker _worker;
        public override void Calculate()
        {
            var file = InputPorts[0].Data.ToString();
            if (file != "" && File.Exists(file))
            {
                _worker = new BackgroundWorker();
                _worker.DoWork += new DoWorkEventHandler(readMvdXML);
                _worker.RunWorkerAsync(file);
                _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(readMvdXMLCompleted);
            }

        }

        private void readMvdXML(object sender, DoWorkEventArgs e)
        {
            var file = e.Argument.ToString();
            Templates template = new Templates();
            ModelView modelView = new ModelView();
            XmlReaderSettings readerSettings = new XmlReaderSettings();
            using (XmlReader reader = XmlReader.Create(file, readerSettings))
            {

                ConceptTemplate conceptTemplate = null;
                Definition definition = null;
                ConceptTemplate subConceptTemplate = null;

                bool body = false;
                bool subConcept = false;
                bool templateClass = false;

                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.XmlDeclaration:
                            Console.WriteLine("{0,-20}<{1}>", "DEKLARATION", reader.Value);
                            break;
                        case XmlNodeType.CDATA:
                            if (body == true && templateClass == true)
                            {
                                definition.setBody(reader.Value);
                            }
                            Console.WriteLine("{0,-20}{1}", "CDATA", reader.Value);
                            break;

                        case XmlNodeType.Element:
                            switch (reader.Name)
                            {
                                case "mvd:Templates":
                                    templateClass = true;
                                    break;
                                case "mvd:ConceptTemplate":
                                    if (templateClass == true)
                                    {
                                        if (subConcept == false)
                                        {
                                            conceptTemplate = new ConceptTemplate();

                                            if (reader.HasAttributes && subConcept == false)
                                            {
                                                // Durch die Attribute navigieren
                                                while (reader.MoveToNextAttribute())
                                                {
                                                    if (reader.Name == "uuid")
                                                    {
                                                        conceptTemplate.setUUID(reader.Value);
                                                    }
                                                    if (reader.Name == "code")
                                                    {
                                                        conceptTemplate.code = reader.Value;
                                                    }
                                                    if (reader.Name == "name")
                                                    {
                                                        conceptTemplate.name = reader.Value;
                                                    }
                                                    Console.WriteLine("{0,-20}{1}",
                                                        "ATTRIBUT", reader.Name + "=" + reader.Value);
                                                }
                                            }

                                            template.addConceptTemplate(conceptTemplate.getUUID(), conceptTemplate);
                                        }
                                        else if (subConcept == true)
                                        {
                                            subConceptTemplate = new ConceptTemplate();

                                            if (reader.HasAttributes && subConcept == true)
                                            {
                                                // Durch die Attribute navigieren
                                                while (reader.MoveToNextAttribute())
                                                {
                                                    if (reader.Name == "uuid")
                                                    {
                                                        subConceptTemplate.setUUID(reader.Value);
                                                    }
                                                    if (reader.Name == "code")
                                                    {
                                                        subConceptTemplate.code = reader.Value;
                                                    }
                                                    if (reader.Name == "name")
                                                    {
                                                        subConceptTemplate.name = reader.Value;
                                                    }
                                                    Console.WriteLine("{0,-20}{1}",
                                                        "ATTRIBUT", reader.Name + "=" + reader.Value);
                                                }
                                            }
                                            conceptTemplate.addConceptTemplates(subConceptTemplate.getUUID(),
                                                subConceptTemplate);
                                        }
                                        Console.WriteLine("{0,-20}<{1}>", "Template", reader.Name);
                                    }
                                    break;
                                case "mvd:Definition":
                                    if (templateClass == true && subConcept == false)
                                    {
                                        definition = new Definition();
                                        conceptTemplate.addDefinition(definition);
                                    }
                                    if (templateClass == true && subConcept == true)
                                    {
                                        definition = new Definition();
                                        subConceptTemplate.addDefinition(definition);
                                    }
                                    break;
                                case "mvd:Body":
                                    if (templateClass == true)
                                    {
                                        body = true;
                                    }
                                    break;
                                case "mvd:Link":
                                    if (templateClass == true)
                                    {
                                        if (reader.HasAttributes)
                                        {
                                            while (reader.MoveToNextAttribute())
                                            {
                                                if (reader.Name == "href")
                                                {
                                                    definition.addLink(reader.Value);
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case "mvd:SubTemplates":
                                    if (templateClass == true)
                                    {
                                        subConceptTemplate = new ConceptTemplate();
                                        subConcept = true;
                                    }
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case XmlNodeType.EndElement:
                            if (reader.Name == "mvd:Body")
                            {
                                body = false;
                            }
                            if (reader.Name == "mvd:Templates")
                            {
                                templateClass = false;
                                break;
                            }
                            if (reader.Name == "mvd:SubTemplates")
                            {
                                subConcept = false;
                            }
                            Console.WriteLine("{0,-20}</{1}>", "END_ELEMENT", reader.Name);
                            break;
                        case XmlNodeType.Text:
                            Console.WriteLine("{0,-20}{1}", "TEXT", reader.Value);
                            break;
                    }
                }
            }
            using (XmlReader reader = XmlReader.Create(file, readerSettings))
            {
                Definition definition = null;
                ExchangeRequirement exchangeRequirement = null;
                ConceptRoot conceptRoot = null;
                Concept concept = null;
                Concept subConcept = null;
                bool body = false;
                bool viewClass = false;
                bool modelViewDef = false;
                bool exchangeRequirementsDef = false;
                bool conceptRootsDef = false;
                bool conceptDef = false;
                bool root = false;
                bool subconceptClass = false;
                bool exit = false;
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.XmlDeclaration:
                            Console.WriteLine("{0,-20}<{1}>", "DEKLARATION", reader.Value);
                            break;
                        case XmlNodeType.CDATA:
                            if (body == true && viewClass == true)
                            {
                                definition.setBody(reader.Value);
                            }
                            Console.WriteLine("{0,-20}{1}", "CDATA", reader.Value);
                            break;

                        case XmlNodeType.Element:
                            switch (reader.Name)
                            {
                                case "mvd:Views":
                                    viewClass = true;
                                    break;
                                case "mvd:ModelView":
                                    modelViewDef = true;
                                    if (reader.HasAttributes && viewClass == true)
                                    {
                                        while (reader.MoveToNextAttribute())
                                        {
                                            if (reader.Name == "uuid")
                                            {
                                                modelView.uuid = reader.Value;
                                            }
                                            if (reader.Name == "name")
                                            {
                                                modelView.name = reader.Value;
                                            }
                                        }
                                    }
                                    break;


                                case "mvd:Definition":
                                    if (viewClass == true)
                                    {
                                        if (modelViewDef == true)
                                        {
                                            definition = new Definition();
                                            modelView.addDefinition(definition);
                                        }
                                        if (exchangeRequirementsDef == true)
                                        {
                                            definition = new Definition();
                                            exchangeRequirement.addDefinition(definition);
                                        }
                                        if (conceptRootsDef == true && conceptDef == false)
                                        {
                                            definition = new Definition();
                                            conceptRoot.addDefinition(definition);
                                        }
                                        if (conceptDef == true && subconceptClass == false)
                                        {
                                            definition = new Definition();
                                            concept.addDefinition(definition);
                                        }
                                        if (conceptDef == true && subconceptClass == true)
                                        {
                                            definition = new Definition();
                                            subConcept.addDefinition(definition);
                                        }


                                    }

                                    break;
                                case "mvd:Body":
                                    if (viewClass == true)
                                    {
                                        body = true;
                                    }
                                    break;
                                case "mvd:ExchangeRequirement":
                                    if (viewClass == true)
                                    {
                                        exchangeRequirement = new ExchangeRequirement();
                                        exchangeRequirementsDef = true;
                                        if (reader.HasAttributes)
                                        {
                                            while (reader.MoveToNextAttribute())
                                            {
                                                if (reader.Name == "uuid")
                                                {
                                                    exchangeRequirement.uuid = reader.Value;
                                                }
                                                if (reader.Name == "name")
                                                {
                                                    exchangeRequirement.name = reader.Value;
                                                }
                                            }
                                        }
                                        modelView.addExchangeRequirements(exchangeRequirement.uuid, exchangeRequirement);

                                    }
                                    break;
                                case "mvd:Roots":
                                    root = true;
                                    break;
                                case "mvd:ConceptRoot":
                                    if (viewClass == true && root == true)
                                    {
                                        conceptRoot = new ConceptRoot();
                                        conceptRootsDef = true;
                                        if (reader.HasAttributes)
                                        {
                                            while (reader.MoveToNextAttribute())
                                            {
                                                if (reader.Name == "uuid")
                                                {
                                                    conceptRoot.uuid = reader.Value;
                                                }
                                                if (reader.Name == "name")
                                                {
                                                    conceptRoot.name = reader.Value;
                                                }
                                                if (reader.Name == "applicableRootEntity")
                                                {
                                                    conceptRoot.applicableRootEntity = reader.Value;
                                                }
                                            }
                                        }
                                        modelView.addRoots(conceptRoot.uuid, conceptRoot);
                                    }
                                    break;
                                case "mvd:Concept":
                                    if (viewClass == true && root == true)
                                    {
                                        if (subconceptClass == false)
                                        {
                                            concept = new Concept();
                                            conceptDef = true;
                                            if (reader.HasAttributes)
                                            {
                                                while (reader.MoveToNextAttribute())
                                                {
                                                    if (reader.Name == "uuid")
                                                    {
                                                        concept.uuid = reader.Value;
                                                    }
                                                    if (reader.Name == "name")
                                                    {
                                                        concept.name = reader.Value;
                                                    }
                                                }
                                            }
                                            conceptRoot.addConcept(concept.uuid, concept);
                                        }
                                        if (subconceptClass == true)
                                        {
                                            subConcept = new Concept();
                                            conceptDef = true;
                                            if (reader.HasAttributes)
                                            {
                                                while (reader.MoveToNextAttribute())
                                                {
                                                    if (reader.Name == "uuid")
                                                    {
                                                        subConcept.uuid = reader.Value;
                                                    }
                                                    if (reader.Name == "name")
                                                    {
                                                        subConcept.name = reader.Value;
                                                    }
                                                }
                                            }
                                            concept.addSubConcept(subConcept.uuid, subConcept);
                                        }

                                    }
                                    break;
                                case "mvd:Template":
                                    if (reader.HasAttributes && conceptDef == true)
                                    {
                                        while (reader.MoveToNextAttribute())
                                        {
                                            if (reader.Name == "ref" && subconceptClass == false)
                                            {
                                                ConceptTemplate tempConcept = template.getConceptTemplate(reader.Value);
                                                concept.setConceptTemplate(tempConcept);
                                            }
                                            if (reader.Name == "ref" && subconceptClass == true)
                                            {
                                                ConceptTemplate tempConcept = concept.getConceptTemplate().getConceptTemplate(reader.Value);
                                                subConcept.setConceptTemplate(tempConcept);
                                            }
                                        }
                                    }
                                    break;
                                case "mvd:Requirement":
                                    if (reader.HasAttributes)
                                    {
                                        Requirement requirement = new Requirement();

                                        string uuid = null;
                                        while (reader.MoveToNextAttribute())
                                        {
                                            if (reader.Name == "applicability")
                                            {
                                                requirement.applicability = reader.Value;
                                            }
                                            if (reader.Name == "exchangeRequirement")
                                            {
                                                uuid = reader.Value;
                                                var tempExchangeRequirement = modelView.GetExchangeRequirement(uuid);
                                                requirement.exchangeRequirement = tempExchangeRequirement;
                                            }
                                            if (reader.Name == "requirement")
                                            {
                                                requirement.requirement = reader.Value;
                                            }
                                        }


                                        if (uuid == null) uuid = requirement.applicability;
                                        if (subconceptClass == false)
                                        {
                                            concept.addRequirements(uuid, requirement);
                                        }
                                        else if (subconceptClass == true)
                                        {
                                            subConcept.addRequirements(uuid, requirement);
                                        }
                                    }
                                    break;
                                case "mvd:SubConcepts":
                                    subconceptClass = true;
                                    break;

                            }
                            break;
                        case XmlNodeType.EndElement:
                            if (reader.Name == "mvd:Body")
                            {
                                body = false;
                            }
                            if (reader.Name == "mvd:Views")
                            {
                                viewClass = false;
                                break;
                            }
                            if (reader.Name == "mvd:Definitions")
                            {
                                if (modelViewDef == true)
                                {
                                    modelViewDef = false;
                                }
                                if (exchangeRequirementsDef == true)
                                {
                                    exchangeRequirementsDef = false;
                                }
                                if (conceptRootsDef == true)
                                {
                                    conceptRootsDef = false;
                                }
                                if (conceptDef == true)
                                {
                                    conceptDef = false;
                                }
                            }
                            if (reader.Name == "mvd:SubConcepts")
                            {
                                subconceptClass = false;
                            }
                            Console.WriteLine("{0,-20}</{1}>", "END_ELEMENT", reader.Name);
                            break;
                        case XmlNodeType.Text:
                            Console.WriteLine("{0,-20}{1}", "TEXT", reader.Value);
                            break;
                    }
                }


            }
            e.Result = modelView;
        }

        private void readMvdXMLCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            OutputPorts[0].Data = e.Result;
        }

        private class Templates
        {
            Dictionary<string, ConceptTemplate> conceptTemplates = new Dictionary<string, ConceptTemplate>();

            public void addConceptTemplate(string uuid, ConceptTemplate conceptTemplate)
            {
                conceptTemplates.Add(uuid, conceptTemplate);
            }

            public ConceptTemplate getConceptTemplate(string uuid)
            {
                return conceptTemplates[uuid];
            }
        }

        private class ConceptTemplate
        {
            private string uuid;
            public string name { get; set; }
            public string code { get; set; }

            List<Definition> definitiones = new List<Definition>();
            Dictionary<string, ConceptTemplate> subConceptTemplateses = new Dictionary<string, ConceptTemplate>();

            public void addDefinition(Definition definition)
            {
                definitiones.Add(definition);
            }
            public void addConceptTemplates(string uuid, ConceptTemplate conceptTemplate)
            {
                subConceptTemplateses.Add(uuid, conceptTemplate);
            }

            public void setUUID(string uuid)
            {
                this.uuid = uuid;
            }

            public string getUUID()
            {
                return uuid;
            }
            public ConceptTemplate getConceptTemplate(string uuid)
            {
                return subConceptTemplateses[uuid];
            }
        }


        private class Definition
        {
            string Body;
            List<string> Link = new List<string>();

            public void addLink(string link)
            {
                Link.Add(link);
            }

            public void setBody(string body)
            {
                this.Body = body;
            }


        }

        private class ModelView
        {
            public string uuid { get; set; }
            public string name { get; set; }

            List<Definition> definitiones = new List<Definition>();
            Dictionary<string, ExchangeRequirement> exchangeRequirements = new Dictionary<string, ExchangeRequirement>();
            Dictionary<string, ConceptRoot> roots = new Dictionary<string, ConceptRoot>();

            public void addExchangeRequirements(string uuid, ExchangeRequirement exchangeRequirement)
            {
                exchangeRequirements.Add(uuid, exchangeRequirement);
            }

            public ExchangeRequirement GetExchangeRequirement(string uuid)
            {
                return exchangeRequirements[uuid];
            }

            public void addRoots(string uuid, ConceptRoot conceptRoot)
            {
                roots.Add(uuid, conceptRoot);
            }

            public void addDefinition(Definition definition)
            {
                definitiones.Add(definition);
            }

        }

        private class ConceptRoot
        {
            public string uuid { get; set; }
            public string name { get; set; }
            public string applicableRootEntity { get; set; }
            List<Definition> definitiones = new List<Definition>();
            Dictionary<string, Concept> concepts = new Dictionary<string, Concept>();

            public void addDefinition(Definition definition)
            {
                definitiones.Add(definition);
            }
            public void addConcept(string uuid, Concept concept)
            {
                concepts.Add(uuid, concept);
            }

        }

        private class ExchangeRequirement
        {
            public string uuid { get; set; }
            public string name { get; set; }
            List<Definition> definitiones = new List<Definition>();
            public void addDefinition(Definition definition)
            {
                definitiones.Add(definition);
            }
        }

        private class Concept
        {
            public string uuid { get; set; }
            public string name { get; set; }
            ConceptTemplate conceptTemplate = new ConceptTemplate();
            List<Definition> definitiones = new List<Definition>();
            Dictionary<string, Requirement> requirements = new Dictionary<string, Requirement>();
            Dictionary<string, Concept> subConcepts = new Dictionary<string, Concept>();

            public void addDefinition(Definition definition)
            {
                definitiones.Add(definition);
            }

            public void addRequirements(string value, Requirement requirement)
            {
                requirements.Add(value, requirement);
            }

            public void addSubConcept(string uuid, Concept concept)
            {
                subConcepts.Add(uuid, concept);
            }

            public void setConceptTemplate(ConceptTemplate conceptTemplate)
            {
                this.conceptTemplate = conceptTemplate;
            }

            public ConceptTemplate getConceptTemplate()
            {
                return conceptTemplate;
            }
        }

        private class Requirement
        {
            public string applicability;
            public ExchangeRequirement exchangeRequirement;
            public string requirement;
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
            return new mvdXMLReaderNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}