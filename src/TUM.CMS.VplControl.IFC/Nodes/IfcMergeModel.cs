using System;
using System.Collections.Generic;
using System.Windows.Controls;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.IFC.Utilities;
using System.Collections;
using Xbim.Ifc2x3.UtilityResource;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcMergeModel : Node
    {
        public ModelInfo output;

        public IfcMergeModel(Core.VplControl hostCanvas) : base(hostCanvas)
        {

            var labelModels = new Label { Content = "Merge Models" };
            AddControlToNode(labelModels);
            var label1 = new Label {};
            AddControlToNode(label1);

            AddInputPortToNode("Object", typeof(object), true);
            AddOutputPortToNode("Object", typeof(object));


        }
        
       
        /// <summary>
        /// Adds all Models which are stored at the DataController to the ComboBox
        /// </summary>
        public override void Calculate()
        {
            Type t = InputPorts[0].Data.GetType();
            var label1 = ControlElements[1] as Label;
            label1.Content = "";
            if (t.IsGenericType)
            {

                var collection = InputPorts[0].Data as ICollection;
                var modelid = "";
                if (collection != null)
                {
                    foreach (var model in collection)
                    {
                        if (modelid != "" && modelid == ((ModelInfo)(model)).ModelId)
                        {
                            var elementIdsList = ((ModelInfo)(model)).ElementIds;
                            var res = new HashSet<IfcGloballyUniqueId>(elementIdsList);
                            foreach (var item in res)
                            {
                                output.AddElementIds(item);
                            }
                        }
                        else if (modelid == "")
                        {
                            modelid = ((ModelInfo) (model)).ModelId;
                            output = new ModelInfo(modelid);

                            var elementIdsList = ((ModelInfo) (model)).ElementIds;
                            var res = new HashSet<IfcGloballyUniqueId>(elementIdsList);
                            foreach (var item in res)
                            {
                                output.AddElementIds(item);
                            }
                        }
                        else
                        {
                            label1.Content = "Your Models have not the same ModelId!";
                        }
                    }
                    label1.Content = "Merge Complete!";
                    OutputPorts[0].Data = output;
                }    
            }
            else
            {
                label1.Content = "Please connect to Models !";
            }



        }

        public override Node Clone()
        {
            return new IfcTreeNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }

    }
}