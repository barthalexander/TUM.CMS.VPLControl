using System;
using System.Windows.Controls;
using System.Xml;
using TUM.CMS.VplControl.Core;
using System.Collections.Generic;
using TUM.CMS.VplControl.IFC.Utilities;
using System.Linq;
using System.Windows;
using TUM.CMS.VplControl.IFC.Controls;
using Xbim.Ifc;
using Xceed.Wpf.AvalonDock.Controls;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcElementPropertyNode : Node
    {
        public IfcStore xModel;
        public string modelID;
        public ModelInfoIFC2x3 OutputInfoIfc2x3; 
        public ModelInfoIFC4 OutputInfoIfc4;
        public Type IfcVersionType = null;
        public IfcElementPropertyNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {

            AddInputPortToNode("IfcFile", typeof(object));

            AddOutputPortToNode("FilteredProducts", typeof(object));

            UserControl usercontrol = new UserControl();
            Grid grid = new Grid();
            usercontrol.Content = grid;

            IfcElementPropertyControl ifcElementPropertyControl = new IfcElementPropertyControl();

            ifcElementPropertyControl.ComboBox_IfcProducts.SelectionChanged += comboBox_IfcProduct_SelectionChanged;
            ifcElementPropertyControl.ComboBox_IfcPropertySets.SelectionChanged += comboBox_PropertySets_SelectionChanged;

            AddControlToNode(ifcElementPropertyControl);
        }

        /// <summary>
        /// Important for Combobox IfcProduct
        /// 
        /// </summary>
        public class ComboboxItem_IfcProduct
        {
            public string Text { get; set; }
            public Xbim.Ifc2x3.Kernel.IfcProduct IfcProduct_IFC2x3 { get; set; }
            public Xbim.Ifc4.Kernel.IfcProduct IfcProduct_IFC4 { get; set; }

            public override string ToString()
            { return Text; }

           
        }

        /// <summary>
        /// Important for Combobox of PropertySets
        /// 
        /// </summary>
        public class ComboboxItem_PropertySet
        {
            public string Text { get; set; }
            public Xbim.Ifc2x3.Kernel.IfcPropertySet PropertySet_IFC2x3 { get; set; }
            public Xbim.Ifc4.Interfaces.IIfcPropertySet PropertySet_IFC4 { get; set; }

            public override string ToString()
            { return Text; }


        }
        /// <summary>
        /// Create a list of elements for the first combobox
        /// </summary>
        public override void Calculate()
        {
            var ifcElementPropertyControl = ControlElements[0] as IfcElementPropertyControl;
            var comboBox = ifcElementPropertyControl.ComboBox_IfcProducts;

            ifcElementPropertyControl.ComboBox_IfcPropertySets.Visibility = Visibility.Hidden;
            ifcElementPropertyControl.StackPanel.Visibility = Visibility.Hidden;
            
            if (InputPorts[0].Data == null)
            {
                return;
            }
            
            // Check the used IFC Version
            IfcVersionType = InputPorts[0].Data.GetType();

            if (comboBox != null && comboBox.Items.Count > 0)
            {
                comboBox.SelectedItem = -1;
                comboBox.Items.Clear();

                ComboboxItem_IfcProduct preselectedItem = new ComboboxItem_IfcProduct()
                {
                    Text = "Select Element",
                    IfcProduct_IFC2x3 = null
                };
                comboBox.Items.Add(preselectedItem);

                comboBox.SelectedItem = preselectedItem;
            }
            else
            {
                ComboboxItem_IfcProduct preselectedItem = new ComboboxItem_IfcProduct()
                {
                    Text = "Select Element",
                    IfcProduct_IFC2x3 = null
                };
                comboBox.Items.Add(preselectedItem);

                comboBox.SelectedItem = preselectedItem;
            }

            Type ifcVersion = InputPorts[0].Data.GetType();
            if (ifcVersion.Name == "ModelInfoIFC2x3")
            {
                comboBox.Visibility = Visibility.Visible;
                var modelid = ((ModelInfoIFC2x3)(InputPorts[0].Data)).ModelId;
                modelID = modelid;
                var elementIdsList = ((ModelInfoIFC2x3)(InputPorts[0].Data)).ElementIds;
                var res = new HashSet<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId>(elementIdsList);


                xModel = DataController.Instance.GetModel(modelid);

                OutputInfoIfc2x3 = new ModelInfoIFC2x3(modelid);

                // Filter for selected ElementIDs
                List<Xbim.Ifc2x3.Kernel.IfcProduct> elements = xModel.Instances.OfType<Xbim.Ifc2x3.Kernel.IfcProduct>().ToList();
                foreach (var element in elements)
                {
                    if (res.Contains(element.GlobalId))
                    {
                        OutputInfoIfc2x3.AddElementIds(element.GlobalId);
                        var elementName = element.Name;
                        if (element.Name == "")
                        {
                            elementName = "n.N.";
                        }
                        ComboboxItem_IfcProduct item = new ComboboxItem_IfcProduct() { Text = elementName, IfcProduct_IFC2x3 = element };
                        comboBox.Items.Add(item);

                    }
                }
                OutputPorts[0].Data = OutputInfoIfc2x3;


            }
            else if (ifcVersion.Name == "ModelInfoIFC4")
            {
                comboBox.Visibility = Visibility.Visible;
                var modelid = ((ModelInfoIFC4)(InputPorts[0].Data)).ModelId;
                modelID = modelid;
                var elementIdsList = ((ModelInfoIFC4)(InputPorts[0].Data)).ElementIds;
                var res = new HashSet<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId>(elementIdsList);

                xModel = DataController.Instance.GetModel(modelid);

                OutputInfoIfc4 = new ModelInfoIFC4(modelid);
               
                // Filter for selected ElementIDs
                List<Xbim.Ifc4.Kernel.IfcProduct> elements = xModel.Instances.OfType<Xbim.Ifc4.Kernel.IfcProduct>().ToList();
                foreach (var element in elements)
                {
                    if (res.Contains(element.GlobalId))
                    {
                        OutputInfoIfc4.AddElementIds(element.GlobalId);

                        var elementName = element.Name;
                        if (element.Name == "")
                        {
                            elementName = "n.N.";
                        }
                        ComboboxItem_IfcProduct item = new ComboboxItem_IfcProduct() { Text = elementName, IfcProduct_IFC4 = element };
                        comboBox.Items.Add(item);
                    }
                }
                OutputPorts[0].Data = OutputInfoIfc4;
            }
        }
        /// <summary>
        /// Function for selection change of combobox IfcProducts
        /// 
        /// Creates the combobox for Proberty Sets
        /// Show Property Set Combobox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox_IfcProduct_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var ifcElementPropertyControl = ControlElements[0] as IfcElementPropertyControl;
            var comboBox_IfcProducts = ifcElementPropertyControl.ComboBox_IfcProducts;

            if (comboBox_IfcProducts == null) return;

            // Important for refreshing the node
            var comboBox_PropertySets = ifcElementPropertyControl.ComboBox_IfcPropertySets;
            if (comboBox_PropertySets != null && comboBox_PropertySets.Items.Count > 0)
            {
                comboBox_PropertySets.SelectedItem = -1;

                comboBox_PropertySets.Items.Clear();
                ComboboxItem_PropertySet preselectedItem = new ComboboxItem_PropertySet()
                {
                    Text = "Select Property Set",
                    PropertySet_IFC2x3 = null
                };
                comboBox_PropertySets.Items.Add(preselectedItem);
                comboBox_PropertySets.SelectedItem = preselectedItem;
            }
            else
            {
                ComboboxItem_PropertySet preselectedItem = new ComboboxItem_PropertySet()
                {
                    Text = "Select Property Set",
                    PropertySet_IFC2x3 = null
                };
                comboBox_PropertySets.Items.Add(preselectedItem);
                comboBox_PropertySets.SelectedItem = preselectedItem;
            }

            // Differs between choose IFC Version
            if (IfcVersionType.Name == "ModelInfoIFC2x3" && comboBox_IfcProducts.SelectedItem != null)
            {
                var element = ((ComboboxItem_IfcProduct)(comboBox_IfcProducts.SelectedItem)).IfcProduct_IFC2x3;
                if (element != null)
                {
                    comboBox_PropertySets.Visibility = Visibility.Visible;

                    var propertySets = element.PropertySets;
                    foreach (var propertySet in propertySets)
                    {
                        ComboboxItem_PropertySet item = new ComboboxItem_PropertySet() { Text = propertySet.Name, PropertySet_IFC2x3 = propertySet };

                        comboBox_PropertySets.Items.Add(item);
                    }
                }
                

            }
            else if (IfcVersionType.Name == "ModelInfoIFC4" & comboBox_IfcProducts.SelectedItem != null)
            {
                var element = ((ComboboxItem_IfcProduct)(comboBox_IfcProducts.SelectedItem)).IfcProduct_IFC4;
                if (element != null)
                {
                    comboBox_PropertySets.Visibility = Visibility.Visible;

                    var propertySets = element.PropertySets;
                    foreach (var propertySet in propertySets)
                    {
                        ComboboxItem_PropertySet item = new ComboboxItem_PropertySet() { Text = propertySet.Name, PropertySet_IFC4 = propertySet };

                        comboBox_PropertySets.Items.Add(item);
                    }
                }
                    
            }



        }
        /// <summary>
        /// Function for selection change of combobox Property Sets
        /// 
        /// Creates the list of proberties
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox_PropertySets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var ifcElementPropertyControl = ControlElements[0] as IfcElementPropertyControl;
            StackPanel stackPanel = new StackPanel();
            stackPanel = ifcElementPropertyControl.StackPanel;
            stackPanel.Orientation = Orientation.Vertical;

            // Important for refreshing of the node
            if (stackPanel.Children.Count > 0)
            {
                stackPanel.Children.Clear();
            }

            var comboBoxPropertySet = ifcElementPropertyControl.ComboBox_IfcPropertySets;
            if (comboBoxPropertySet == null || comboBoxPropertySet.Items.Count == 0)
            {
                return;
            }

            
            // Differs the choosen IFC Version
            if (IfcVersionType.Name == "ModelInfoIFC2x3" & comboBoxPropertySet.SelectedItem != null)
            {
                var element = ((ComboboxItem_PropertySet) (comboBoxPropertySet.SelectedItem)).PropertySet_IFC2x3;
                if (element != null)
                {
                    stackPanel.Visibility = Visibility.Visible;

                    foreach (var prop in element.HasProperties)
                    {
                        StackPanel subStackPanel = new StackPanel();
                        subStackPanel.Orientation = Orientation.Horizontal;
                        Label label = new Label();
                        label.MinWidth = 60;
                        label.Content = prop.Name;

                        TextBox textBox = new TextBox();
                        textBox.MinWidth = 100;
                        textBox.HorizontalAlignment = HorizontalAlignment.Right;
                        textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                        textBox.Text = ((Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue)prop).NominalValue.ToString();
                        subStackPanel.Children.Add(label);
                        subStackPanel.Children.Add(textBox);

                        stackPanel.Children.Add(subStackPanel);
                        
                        // Safe changes after key up
                        textBox.KeyUp += (o, args) =>
                        {
                            using (var txn = xModel.BeginTransaction("Properties"))
                            {

                                Xbim.Ifc2x3.MeasureResource.IfcLabel temp = new Xbim.Ifc2x3.MeasureResource.IfcLabel(textBox.Text);
                                ((Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue)prop).NominalValue = temp;
                                txn.Commit();
                            }
                            OutputPorts[0].Data = OutputInfoIfc2x3;
                            xModel.SaveAs(modelID);
                        };
                    }
                }
                
            }
            else if (IfcVersionType.Name == "ModelInfoIFC4" & comboBoxPropertySet.SelectedItem != null)
            {
                var element = ((ComboboxItem_PropertySet)(comboBoxPropertySet.SelectedItem)).PropertySet_IFC4;
                if (element != null)
                {
                    stackPanel.Visibility = Visibility.Visible;

                    foreach (var prop in element.HasProperties)
                    {
                        StackPanel subStackPanel = new StackPanel();
                        subStackPanel.Orientation = Orientation.Horizontal;
                        Label label = new Label();
                        label.MinWidth = 60;
                        label.Content = prop.Name;

                        TextBox textBox = new TextBox();
                        textBox.MinWidth = 100;
                        textBox.HorizontalAlignment = HorizontalAlignment.Right;
                        textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                        textBox.Text = ((Xbim.Ifc4.PropertyResource.IfcPropertySingleValue)prop).NominalValue.ToString();
                        subStackPanel.Children.Add(label);
                        subStackPanel.Children.Add(textBox);

                        stackPanel.Children.Add(subStackPanel);
                       
                        // Safe changes after key up
                        textBox.KeyUp += (o, args) =>
                        {
                            using (var txn = xModel.BeginTransaction("Properties"))
                            {

                                Xbim.Ifc4.MeasureResource.IfcLabel temp = new Xbim.Ifc4.MeasureResource.IfcLabel(textBox.Text);
                                ((Xbim.Ifc4.PropertyResource.IfcPropertySingleValue)prop).NominalValue = temp;
                                txn.Commit();
                            }
                            OutputPorts[0].Data = OutputInfoIfc4;
                            xModel.SaveAs(modelID);
                        };
                    }
                }
            }
        }




        public override void SerializeNetwork(XmlWriter xmlWriter)
        {
            base.SerializeNetwork(xmlWriter);

            // add your xml serialization methods here
        }

        public override void DeserializeNetwork(XmlReader xmlReader)
        {
            base.DeserializeNetwork(xmlReader);

            // add your xml deserialization methods here
        }

        public override Node Clone()
        {
            return new IfcElementPropertyNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}