using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUM.CMS.VplControl.IFC.Utilities.mvdXMLReaderClasses
{
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
}
