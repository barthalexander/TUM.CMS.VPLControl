using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUM.CMS.VplControl.IFC.Utilities.mvdXMLReaderClasses
{
    public class Concept
    {
        public string uuid { get; set; }
        public string name { get; set; }
        public string Override { get; set; }
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

        public Dictionary<string, Concept> GetAllSubConcepts()
        {
            return subConcepts;
        }

        public Dictionary<string, Requirement> GetAllRequirements()
        {
            return requirements;
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

}
