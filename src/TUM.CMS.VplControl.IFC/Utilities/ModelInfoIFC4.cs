using System;
using System.Collections.Generic;
using Xbim.Ifc4.UtilityResource;

namespace TUM.CMS.VplControl.IFC.Utilities
{
    public class ModelInfoIFC4
    {
        public string ModelId { get; set; }
        public List<IfcGloballyUniqueId> ElementIds;

        public ModelInfoIFC4(String filePath)
        {
            ElementIds = new List<IfcGloballyUniqueId>();
            ModelId = filePath;
        }

        public void AddElementIds(string id)
        {
            ElementIds.Add(id);
        }
    }
}
