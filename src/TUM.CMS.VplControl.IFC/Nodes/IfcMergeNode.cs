using System;
using System.Collections.Generic;
using System.Windows.Controls;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.IFC.Utilities;
using System.Collections;
using System.Windows;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcMergeNode : Node
    {
        public ModelInfoIFC2x3 OutputIfc2x3;
        public ModelInfoIFC4 OutputIfc4;
        public Type IfcVersionType = null;
        public IfcMergeNode(Core.VplControl hostCanvas) : base(hostCanvas)
        {

            var labelModels = new Label { Content = "Ifc Merge Node", FontWeight = FontWeights.Bold};
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
                    string ifcMergeVersionType = "";
                    foreach (var model in collection)
                    {
                        
                        IfcVersionType = model.GetType();
                        if (ifcMergeVersionType == "")
                        {
                            ifcMergeVersionType = IfcVersionType.Name;
                        }

                        if (IfcVersionType.Name != ifcMergeVersionType)
                        {
                            MessageBox.Show("The IFC Versions are not the same!", "My Application", MessageBoxButton.OK);
                            return;
                        }

                        if (IfcVersionType.Name == "ModelInfoIFC2x3")
                        {
                            if (modelid != "" && modelid == ((ModelInfoIFC2x3)(model)).ModelId)
                            {
                                var elementIdsList = ((ModelInfoIFC2x3)(model)).ElementIds;
                                var res = new HashSet<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId>(elementIdsList);
                                foreach (var item in res)
                                {
                                    OutputIfc2x3.AddElementIds(item);
                                }
                            }
                            else if (modelid == "")
                            {
                                modelid = ((ModelInfoIFC2x3)(model)).ModelId;
                                OutputIfc2x3 = new ModelInfoIFC2x3(modelid);

                                var elementIdsList = ((ModelInfoIFC2x3)(model)).ElementIds;
                                var res = new HashSet<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId>(elementIdsList);
                                foreach (var item in res)
                                {
                                    OutputIfc2x3.AddElementIds(item);
                                }
                            }
                            else
                            {
                                label1.Content = "Your Models have not the same ModelId!";
                            }
                        }
                        else if (IfcVersionType.Name == "ModelInfoIFC4")
                        {
                            if (modelid != "" && modelid == ((ModelInfoIFC4)(model)).ModelId)
                            {
                                var elementIdsList = ((ModelInfoIFC4)(model)).ElementIds;
                                var res = new HashSet<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId>(elementIdsList);
                                foreach (var item in res)
                                {
                                    OutputIfc4.AddElementIds(item);
                                }
                            }
                            else if (modelid == "")
                            {
                                modelid = ((ModelInfoIFC4)(model)).ModelId;
                                OutputIfc4 = new ModelInfoIFC4(modelid);

                                var elementIdsList = ((ModelInfoIFC4)(model)).ElementIds;
                                var res = new HashSet<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId>(elementIdsList);
                                foreach (var item in res)
                                {
                                    OutputIfc4.AddElementIds(item);
                                }
                            }
                            else
                            {
                                label1.Content = "Your Models have not the same ModelId!";
                            }
                        }
                        
                    }
                    if (IfcVersionType.Name == "ModelInfoIFC2x3")
                    {
                        label1.Content = "Merge Complete!";
                        OutputPorts[0].Data = OutputIfc2x3;
                    }
                    else if (IfcVersionType.Name == "ModelInfoIFC4")
                    {
                        label1.Content = "Merge Complete!";
                        OutputPorts[0].Data = OutputIfc4;
                    }
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