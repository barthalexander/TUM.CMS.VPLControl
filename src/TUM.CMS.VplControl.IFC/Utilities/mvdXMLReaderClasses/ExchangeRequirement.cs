using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUM.CMS.VplControl.IFC.Utilities.mvdXMLReaderClasses
{
    public class ExchangeRequirement
    {
        public string uuid { get; set; }
        public string name { get; set; }
        public string version { get; set; }
        List<Definition> definitiones = new List<Definition>();
        public void addDefinition(Definition definition)
        {
            definitiones.Add(definition);
        }
    }

}
