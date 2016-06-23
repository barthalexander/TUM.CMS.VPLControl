using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.IFC.Utilities;
using Xbim.Ifc2x3.Kernel;
using Xbim.Ifc2x3.UtilityResource;
using Xbim.IO;
using Xbim.ModelGeometry.Scene;
using Xbim.Presentation;
using Xbim.XbimExtensions;
using XbimGeometry.Interfaces;
using System.Collections;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcElementListNode : Node
    {
        private readonly TextBox _textBox;
        public XbimModel xModel;

        private XbimTreeview treeview;
        // private DynamicProductSelectionControl productSelectionControl;
        public IfcElementListNode(Core.VplControl hostCanvas) : base(hostCanvas)
        {
            IsResizeable = true;

            var textBlock = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                FontSize = 16,
                Padding = new Thickness(5),
                IsHitTestVisible = false
            };
            var scrollViewer = new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                MinWidth = 400,
                MinHeight = 30,
                MaxWidth = 1000,
                MaxHeight = 400,
                CanContentScroll = true,
                Content = textBlock
                //IsHitTestVisible = false
            };
            AddControlToNode(new Label { Content = "IFC Element Liste" });
            AddControlToNode(scrollViewer);
            AddInputPortToNode("ModelInfo", typeof(object), true);
           

        }



        public override void Calculate()
        {
            var scrollViewer = ControlElements[1] as ScrollViewer;
            if (scrollViewer == null) return;
            var textBlock = scrollViewer.Content as TextBlock;
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
                        var modelid = ((ModelInfo) (model)).ModelId;
                        var elementIdsList = ((ModelInfo) (model)).ElementIds;
                        var res = new HashSet<IfcGloballyUniqueId>(elementIdsList);


                        xModel = DataController.Instance.GetModel(modelid);
                        List<IfcProduct> elements = xModel.IfcProducts.OfType<IfcProduct>().ToList();
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
            else
            {
                textBlock.Text += "Model 1 \n\n";
                var modelid = ((ModelInfo)(InputPorts[0].Data)).ModelId;
                var elementIdsList = ((ModelInfo)(InputPorts[0].Data)).ElementIds;
                var res = new HashSet<IfcGloballyUniqueId>(elementIdsList);


                xModel = DataController.Instance.GetModel(modelid);
                List<IfcProduct> elements = xModel.IfcProducts.OfType<IfcProduct>().ToList();
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
                            text = element.Name +" ("+ element.GlobalId + ")";
                        }

                        textBlock.Text += text + "\n";
                    }
                }
                textBlock.Text += "\n\n";
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