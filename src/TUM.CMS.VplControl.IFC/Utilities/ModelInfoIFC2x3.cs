using System;
using System.Collections.Generic;
using Xbim.Ifc2x3.UtilityResource;

namespace TUM.CMS.VplControl.IFC.Utilities
{
    public class ModelInfoIFC2x3
    {
        public string ModelId { get; set; }
        public List<IfcGloballyUniqueId> ElementIds;

        public ModelInfoIFC2x3(String filePath)
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
