using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUM.CMS.VplControl.IFC.Utilities.mvdXMLReaderClasses
{
    public class ConceptTemplate
    {
        private string uuid;
        public string name { get; set; }
        public string code { get; set; }
        public string applicableEntity { get; set; }
        public string applicableSchema { get; set; }

        List<Definition> definitiones = new List<Definition>();
        List<Templates> subTemplates = new List<Templates>();
        Dictionary<string, ConceptTemplate> subConceptTemplateses = new Dictionary<string, ConceptTemplate>();
        List<AttributeRule> attributeRules = new List<AttributeRule>();

        public void addDefinition(Definition definition)
        {
            definitiones.Add(definition);
        }
        public void addConceptTemplates(string uuid, ConceptTemplate conceptTemplate)
        {
            subConceptTemplateses.Add(uuid, conceptTemplate);
        }

        public void addTemplates(Templates template)
        {
            subTemplates.Add(template);
        }
        public void setUUID(string uuid)
        {
            this.uuid = uuid;
        }

        public string getUUID()
        {
            return uuid;
        }
        public ConceptTemplate findConceptTemplate(string uuid)
        {
            return subConceptTemplateses[uuid];
        }
        public void addAttributeRule(AttributeRule attributeRule)
        {
            attributeRules.Add(attributeRule);
        }

        public List<Templates> getTemplate()
        {
            return subTemplates;
        }
    }
}
