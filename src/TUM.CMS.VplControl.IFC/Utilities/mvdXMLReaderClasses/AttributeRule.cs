using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUM.CMS.VplControl.IFC.Utilities.mvdXMLReaderClasses
{
    public class AttributeRule
    {
        public string attributeName { get; set; }
        public string cardinality { get; set; }
        List<EntityRule> entityRules = new List<EntityRule>();

        public void addEntityRule(EntityRule entityRule)
        {
            entityRules.Add(entityRule);
        }

        public List<EntityRule> getEntityRules()
        {
            return entityRules;
        }
    }
}
