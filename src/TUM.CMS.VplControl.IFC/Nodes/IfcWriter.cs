using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.IFC.Utilities;
using Xbim.Common;
using Xbim.Common.Metadata;
using Xbim.Common.Step21;
using Xbim.Ifc;
using Xbim.Ifc2x3.Kernel;
using Xbim.Ifc2x3.ProductExtension;
using Xbim.Ifc2x3.UtilityResource;
using Xbim.Presentation;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcWriter : Node
    {
        private readonly TextBox _textBox;
        public IfcStore xModel;

        // private DynamicProductSelectionControl productSelectionControl;
        public IfcWriter(Core.VplControl hostCanvas) : base(hostCanvas)
        {
            AddInputPortToNode("Object", typeof(object), true);

            var label = new Label
            {
                Content = "IFC File Reading",
                Width = 130,
                FontSize = 15,
                HorizontalContentAlignment = HorizontalAlignment.Center
            };
            var button = new Button { Content = "Save File" };
            button.Click += button_Click;
            AddControlToNode(label);
            AddControlToNode(button);


        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (InputPorts[0].Data != null)
            {
                PropertyTranformDelegate propTransform = delegate (ExpressMetaProperty prop, object toCopy)
                {
                    var value = prop.PropertyInfo.GetValue(toCopy, null);
                    return value;
                };

                var newModel = IfcStore.Create(IfcSchemaVersion.Ifc2X3, XbimStoreType.InMemoryModel);

                Type t = InputPorts[0].Data.GetType();
                XbimInstanceHandleMap copied = null;

                if (t.IsGenericType)
                {

                    var collection = InputPorts[0].Data as ICollection;
                    var txn = newModel.BeginTransaction();
                    HashSet<IfcGloballyUniqueId> buildingElements = new HashSet<IfcGloballyUniqueId>();

                    int i = 1;
                    if (collection != null)
                    {
                        foreach (var model in collection)
                        {
                            var modelid = ((ModelInfo)(model)).ModelId;
                            var elementIdsList = ((ModelInfo)(model)).ElementIds;
                            var res = new HashSet<IfcGloballyUniqueId>(elementIdsList);


                            xModel = DataController.Instance.GetModel(modelid);
                            List<IfcProduct> elements = xModel.Instances.OfType<IfcProduct>().ToList();

                            copied = new XbimInstanceHandleMap(xModel, newModel);

                            foreach (var element in elements)
                            {
                                if (res.Contains(element.GlobalId) && !buildingElements.Contains(element.GlobalId))
                                {
                                    newModel.InsertCopy(element, copied, propTransform, false, false);
                                    buildingElements.Add(element.GlobalId);
                                }
                            }
                        }

                    }
                    txn.Commit();
                }
                else
                {
                    var modelid = ((ModelInfo)(InputPorts[0].Data)).ModelId;
                    var elementIdsList = ((ModelInfo)(InputPorts[0].Data)).ElementIds;
                    var res = new HashSet<IfcGloballyUniqueId>(elementIdsList);

                    xModel = DataController.Instance.GetModel(modelid);
                    List<IfcProduct> elements = xModel.Instances.OfType<IfcProduct>().ToList();



                    using (var txn = newModel.BeginTransaction())
                    {
                        copied = new XbimInstanceHandleMap(xModel, newModel);
                        foreach (var element in elements)
                        {
                            if (res.Contains(element.GlobalId))
                            {
                                newModel.InsertCopy(element, copied, propTransform, false, false);
                            }
                        }
                        txn.Commit();
                    }

                }
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.Filter = "IfcFile |*.ifc";
                saveFileDialog1.Title = "Save an IFC File";
                saveFileDialog1.ShowDialog();
                if (saveFileDialog1.FileName != "")
                {
                    newModel.SaveAs(saveFileDialog1.FileName);
                    newModel.Close();
                    if (File.Exists(saveFileDialog1.FileName))
                    {
                        MessageBox.Show("File saved", "My Application", MessageBoxButton.OK);
                    }
                    else
                    {
                        MessageBox.Show("There was an Error \n Please Try again", "My Application", MessageBoxButton.OK);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please Connect a Model", "My Application", MessageBoxButton.OK);
            }


        }

        public override void Calculate()
        {
            
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