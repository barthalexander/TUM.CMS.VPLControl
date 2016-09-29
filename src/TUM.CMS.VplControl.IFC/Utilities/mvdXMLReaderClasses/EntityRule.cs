using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUM.CMS.VplControl.IFC.Utilities.mvdXMLReaderClasses
{
    public class EntityRule
    {
        public string entityName { get; set; }
        public string cardinality { get; set; }
        List<AttributeRule> attributeRules = new List<AttributeRule>();

        public void addAttributeRule(AttributeRule attributeRule)
        {
            attributeRules.Add(attributeRule);
        }
    }
}
