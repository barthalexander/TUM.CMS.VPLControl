using System;
using System.Collections.Generic;
using Xbim.Ifc2x3.UtilityResource;

namespace TUM.CMS.VplControl.IFC.Utilities
{
    public class ModelInfo
    {
        public string ModelId { get; set; }
        public List<IfcGloballyUniqueId> ElementIds;

        public ModelInfo(String filePath)
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
