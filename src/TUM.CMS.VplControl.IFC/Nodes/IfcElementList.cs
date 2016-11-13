using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.IFC.Utilities;
using Xbim.Presentation;
using System.Collections;
using TUM.CMS.VplControl.IFC.Controls;
using Xbim.Ifc;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcElementListNode : Node
    {
        private readonly TextBox _textBox;
        public IfcStore xModel;

        private XbimTreeview treeview;
        // private DynamicProductSelectionControl productSelectionControl;
        public IfcElementListNode(Core.VplControl hostCanvas) : base(hostCanvas)
        {

            IsResizeable = true;

            MVDTreeControle mvdTreeControle = new MVDTreeControle();

            mvdTreeControle.ScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            mvdTreeControle.ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            mvdTreeControle.ScrollViewer.CanContentScroll = true;

            mvdTreeControle.Title.Content = "IFC Element Liste";

            AddControlToNode(mvdTreeControle);

            AddInputPortToNode("ModelInfo", typeof(object), true);
           

        }



        public override void Calculate()
        {
            var mvdTreeControle = ControlElements[0] as MVDTreeControle;

            TextBlock textBlock = new TextBlock();
            mvdTreeControle.ScrollViewer.Content = textBlock;

            mvdTreeControle.MinHeight = 100;
            mvdTreeControle.MinWidth = 400;

            mvdTreeControle.ScrollViewer.MinWidth = 400;
            mvdTreeControle.ScrollViewer.MinHeight = 100;
            mvdTreeControle.ScrollViewer.MaxWidth = 600;
            mvdTreeControle.ScrollViewer.MaxHeight = 600;

            if (textBlock == null) return;
            textBlock.Text = "";

            Type t = InputPorts[0].Data.GetType();
            if (t.IsGenericType)
            {
                var collection = InputPorts[0].Data as ICollection;
                int i = 1;
                if (collection != null)
                    foreach (var model in collection)
                    {
                        textBlock.Text += "Model " + i + "\n\n";
                        Type ifcVersion = InputPorts[0].Data.GetType();
                        if (ifcVersion.Name == "ModelInfoIFC2x3")
                        {
                            var modelid = ((ModelInfoIFC2x3)(model)).ModelId;
                            var elementIdsList = ((ModelInfoIFC2x3)(model)).ElementIds;
                            var res = new HashSet<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId>(elementIdsList);


                            xModel = DataController.Instance.GetModel(modelid);
                            List<Xbim.Ifc2x3.Kernel.IfcProduct> elements = xModel.Instances.OfType<Xbim.Ifc2x3.Kernel.IfcProduct>().ToList();
                            foreach (var element in elements)
                            {
                                if (res.Contains(element.GlobalId))
                                {
                                    var text = "";
                                    if (element.Name == "")
                                    {
                                        text = "n.N" + " (" + element.GlobalId + ")";
                                    }
                                    else
                                    {
                                        text = element.Name + " (" + element.GlobalId + ")";
                                    }

                                    textBlock.Text += text + "\n";
                                }
                            }
                            textBlock.Text += "\n\n";
                            i++;
                        }
                        else if (ifcVersion.Name == "ModelInfoIFC4")
                        {
                            var modelid = ((ModelInfoIFC4)(model)).ModelId;
                            var elementIdsList = ((ModelInfoIFC4)(model)).ElementIds;
                            var res = new HashSet<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId>(elementIdsList);


                            xModel = DataController.Instance.GetModel(modelid);
                            List<Xbim.Ifc4.Kernel.IfcProduct> elements = xModel.Instances.OfType<Xbim.Ifc4.Kernel.IfcProduct>().ToList();
                            foreach (var element in elements)
                            {
                                if (res.Contains(element.GlobalId))
                                {
                                    var text = "";
                                    if (element.Name == "")
                                    {
                                        text = "n.N" + " (" + element.GlobalId + ")";
                                    }
                                    else
                                    {
                                        text = element.Name + " (" + element.GlobalId + ")";
                                    }

                                    textBlock.Text += text + "\n";
                                }
                            }
                            textBlock.Text += "\n\n";
                            i++;
                        }

                    }
            }
            else
            {
                textBlock.Text += "Model 1 \n\n";

                Type ifcVersion = InputPorts[0].Data.GetType();
                if (ifcVersion.Name == "ModelInfoIFC2x3")
                {
                    var modelid = ((ModelInfoIFC2x3)(InputPorts[0].Data)).ModelId;
                    var elementIdsList = ((ModelInfoIFC2x3)(InputPorts[0].Data)).ElementIds;
                    var res = new HashSet<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId>(elementIdsList);


                    xModel = DataController.Instance.GetModel(modelid);
                    List<Xbim.Ifc2x3.Kernel.IfcProduct> elements = xModel.Instances.OfType<Xbim.Ifc2x3.Kernel.IfcProduct>().ToList();
                    foreach (var element in elements)
                    {
                        if (res.Contains(element.GlobalId))
                        {
                            var text = "";
                            if (element.Name == "")
                            {
                                text = "n.N" + " (" + element.GlobalId + ")";
                            }
                            else
                            {
                                text = element.Name + " (" + element.GlobalId + ")";
                            }

                            textBlock.Text += text + "\n";
                        }
                    }
                    textBlock.Text += "\n\n";
                }
                else if(ifcVersion.Name == "ModelInfoIFC4")
                {
                    var modelid = ((ModelInfoIFC4)(InputPorts[0].Data)).ModelId;
                    var elementIdsList = ((ModelInfoIFC4)(InputPorts[0].Data)).ElementIds;
                    var res = new HashSet<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId>(elementIdsList);


                    xModel = DataController.Instance.GetModel(modelid);
                    List<Xbim.Ifc4.Kernel.IfcProduct> elements = xModel.Instances.OfType<Xbim.Ifc4.Kernel.IfcProduct>().ToList();
                    foreach (var element in elements)
                    {
                        if (res.Contains(element.GlobalId))
                        {
                            var text = "";
                            if (element.Name == "")
                            {
                                text = "n.N" + " (" + element.GlobalId + ")";
                            }
                            else
                            {
                                text = element.Name + " (" + element.GlobalId + ")";
                            }

                            textBlock.Text += text + "\n";
                        }
                    }
                    textBlock.Text += "\n\n";

                }

                
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