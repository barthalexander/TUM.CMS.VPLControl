using System;
using System.Collections.Generic;
using Xbim.Ifc2x3.UtilityResource;

namespace TUM.CMS.VplControl.IFC.Utilities
{
    class ModelInfo
    {
        public Guid ModelId;
        public List<IfcGloballyUniqueId> ElementIds;

        public ModelInfo()
        {
            ElementIds = new List<IfcGloballyUniqueId>();
            ModelId = Guid.NewGuid();
        }
    }
}
