using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUM.CMS.VplControl.IFC.Utilities.mvdXMLReaderClasses
{
    public class ModelView
    {
        public string uuid { get; set; }
        public string name { get; set; }
        public string version { get; set; }
        public string applicableSchema { get; set; }

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
        public Dictionary<string, ExchangeRequirement> GetAllExchangeRequirements()
        {
            return exchangeRequirements;
        }


        public void addRoots(string uuid, ConceptRoot conceptRoot)
        {
            roots.Add(uuid, conceptRoot);
        }

        public Dictionary<string, ConceptRoot> GetAllConceptRoots()
        {
            return roots;
        }

        public void addDefinition(Definition definition)
        {
            definitiones.Add(definition);
        }

    }
}
