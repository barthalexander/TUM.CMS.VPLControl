using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.IFC.Utilities;
using Xbim.Presentation;
using System.Collections;
using System.Windows.Data;
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

            TitleListControl titleListControl = new TitleListControl();

            titleListControl.Title.Content = "IFC Element Liste";

            AddControlToNode(titleListControl);

            AddInputPortToNode("ModelInfo", typeof(object), true);
           

        }



        public override void Calculate()
        {
            if(InputPorts[0].Data == null)
                return;

            var titleListControl = ControlElements[0] as TitleListControl;
            StackPanel stackPanel = new StackPanel();
            stackPanel = titleListControl.StackPanel;
            stackPanel.Visibility = Visibility.Visible;
            if (stackPanel.Children.Count > 0)
            {
                stackPanel.Children.Clear();
            }
            Type t = InputPorts[0].Data.GetType();
            if (t.IsGenericType)
            {
                var collection = InputPorts[0].Data as ICollection;
                int i = 1;
                if (collection != null)
                    foreach (var model in collection)
                    {
                        if(model == null)
                            return;

                        Label label = new Label();
                        label.Content = "Model" + i;

                        ListView listView = new ListView();
                        listView.Name = "lvElementList" + i;
                        listView.MaxHeight = 200;
                        GridView gridView = new GridView();

                        gridView.Columns.Add(new GridViewColumn
                        {
                            Header = "Name",
                            DisplayMemberBinding = new Binding("Name")
                        });

                        gridView.Columns.Add(new GridViewColumn
                        {
                            Header = "GUID",
                            DisplayMemberBinding = new Binding("GUID")
                        });

                        listView.View = gridView;

                        Type ifcVersion = model.GetType();

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
                                        listView.Items.Add(new ElementListClass() { Name = "n.N.", GUID = element.GlobalId });
                                    }
                                    else
                                    {
                                        listView.Items.Add(new ElementListClass() { Name = element.Name, GUID = element.GlobalId });
                                    }
                                }
                            }
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
                                        listView.Items.Add(new ElementListClass() { Name = "n.N.", GUID = element.GlobalId });
                                    }
                                    else
                                    {
                                        listView.Items.Add(new ElementListClass() { Name = element.Name, GUID = element.GlobalId });
                                    }

                                }
                            }
                           
                        }
                        i++;
                        stackPanel.Children.Add(label);
                        stackPanel.Children.Add(listView);
                    }
            }
            else
            {
                Label label = new Label();
                label.Content = "Model 1" ;

                ListView listView = new ListView();
                listView.Name = "lvElementList";
                listView.MaxHeight = 500;
                GridView gridView = new GridView();

                gridView.Columns.Add(new GridViewColumn
                {
                    Header = "Name",
                    DisplayMemberBinding = new Binding("Name")
                });

                gridView.Columns.Add(new GridViewColumn
                {
                    Header = "GUID",
                    DisplayMemberBinding = new Binding("GUID")
                });

                listView.View = gridView;
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
                                listView.Items.Add(new ElementListClass() { Name = "n.N.", GUID = element.GlobalId });
                            }
                            else
                            {
                                listView.Items.Add(new ElementListClass() { Name = element.Name, GUID = element.GlobalId });
                            }
                        }
                    }
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
                                listView.Items.Add(new ElementListClass() { Name = "n.N.", GUID = element.GlobalId });
                            }
                            else
                            {
                                listView.Items.Add(new ElementListClass() { Name = element.Name, GUID = element.GlobalId });
                            }
                        }
                    }
                }
                stackPanel.Children.Add(label);
                stackPanel.Children.Add(listView);
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