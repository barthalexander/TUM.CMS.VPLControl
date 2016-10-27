using System;
using System.Collections.Generic;
using Xbim.Ifc2x3.UtilityResource;

namespace TUM.CMS.VplControl.IFC.Utilities
{
    public class Templates
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

    public class ConceptTemplate
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


    public class Definition
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

    public class ModelView
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

        public Dictionary<string, ConceptRoot> GetRoots()
        {
            return roots;
        }

        public Dictionary<string, ExchangeRequirement> GetAllER()
        {
            return exchangeRequirements;
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

    public class ConceptRoot
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

    public class ExchangeRequirement
    {
        public string uuid { get; set; }
        public string name { get; set; }
        List<Definition> definitiones = new List<Definition>();
        public void addDefinition(Definition definition)
        {
            definitiones.Add(definition);
        }
    }

    public class Concept
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

    public class Requirement
    {
        public string applicability;
        public ExchangeRequirement exchangeRequirement;
        public string requirement;
    }
}
