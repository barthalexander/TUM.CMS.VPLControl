using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUM.CMS.VplControl.IFC.Utilities.mvdXMLReaderClasses
{
    public class Templates
    {
        Dictionary<string, ConceptTemplate> conceptTemplates = new Dictionary<string, ConceptTemplate>();

        public void addConceptTemplate(string uuid, ConceptTemplate conceptTemplate)
        {
            conceptTemplates.Add(uuid, conceptTemplate);
        }

        public ConceptTemplate findConceptTemplate(string uuid)
        {
            return conceptTemplates[uuid];
        }

        public Dictionary<string, ConceptTemplate> getConceptTemplates()
        {
            return conceptTemplates;
        }
    }
}
