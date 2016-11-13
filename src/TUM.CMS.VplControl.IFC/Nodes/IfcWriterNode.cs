using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.IFC.Controls;
using TUM.CMS.VplControl.IFC.Utilities;
using Xbim.Common;
using Xbim.Common.Metadata;
using Xbim.Common.Step21;
using Xbim.Ifc;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcWriterNode : Node
    {
        private readonly TextBox _textBox;
        public IfcStore xModel;

        public Type IfcVersionType = null;


        // private DynamicProductSelectionControl productSelectionControl;
        public IfcWriterNode(Core.VplControl hostCanvas) : base(hostCanvas)
        {
            AddInputPortToNode("Object", typeof(object), true);

            IfcWriterControl ifcWriterControl = new IfcWriterControl();
            ifcWriterControl.Title.Content = "IFC Writer";
            ifcWriterControl.Button.Content = "Save File";
            ifcWriterControl.Button.Click += button_Click;

            AddControlToNode(ifcWriterControl);


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
                    HashSet<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId> buildingElementsIFC2x3 = new HashSet<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId>();
                    HashSet<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId> buildingElementsIFC4 = new HashSet<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId>();

                    int i = 1;
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
                                var modelid = ((ModelInfoIFC2x3)(model)).ModelId;
                                var elementIdsList = ((ModelInfoIFC2x3)(model)).ElementIds;
                                var res = new HashSet<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId>(elementIdsList);


                                xModel = DataController.Instance.GetModel(modelid);
                                List<Xbim.Ifc2x3.Kernel.IfcProduct> elements = xModel.Instances.OfType<Xbim.Ifc2x3.Kernel.IfcProduct>().ToList();

                                copied = new XbimInstanceHandleMap(xModel, newModel);

                                foreach (var element in elements)
                                {
                                    if (res.Contains(element.GlobalId) && !buildingElementsIFC2x3.Contains(element.GlobalId))
                                    {
                                        newModel.InsertCopy(element, copied, propTransform, false, false);
                                        buildingElementsIFC2x3.Add(element.GlobalId);
                                    }
                                }

                            }
                            else if (IfcVersionType.Name == "ModelInfoIFC4")
                            {
                                var modelid = ((ModelInfoIFC4)(model)).ModelId;
                                var elementIdsList = ((ModelInfoIFC4)(model)).ElementIds;
                                var res = new HashSet<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId>(elementIdsList);


                                xModel = DataController.Instance.GetModel(modelid);
                                List<Xbim.Ifc4.Kernel.IfcProduct> elements = xModel.Instances.OfType<Xbim.Ifc4.Kernel.IfcProduct>().ToList();

                                copied = new XbimInstanceHandleMap(xModel, newModel);

                                foreach (var element in elements)
                                {
                                    if (res.Contains(element.GlobalId) && !buildingElementsIFC4.Contains(element.GlobalId))
                                    {
                                        newModel.InsertCopy(element, copied, propTransform, false, false);
                                        buildingElementsIFC4.Add(element.GlobalId);
                                    }
                                }

                            }
                        }

                    }
                    txn.Commit();
                }
                else
                {
                    IfcVersionType = InputPorts[0].Data.GetType();
                    if (IfcVersionType.Name == "ModelInfoIFC2x3")
                    {
                        var modelid = ((ModelInfoIFC2x3)(InputPorts[0].Data)).ModelId;
                        var elementIdsList = ((ModelInfoIFC2x3)(InputPorts[0].Data)).ElementIds;
                        var res = new HashSet<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId>(elementIdsList);

                        xModel = DataController.Instance.GetModel(modelid);
                        List<Xbim.Ifc2x3.Kernel.IfcProduct> elements = xModel.Instances.OfType<Xbim.Ifc2x3.Kernel.IfcProduct>().ToList();



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
                    else if (IfcVersionType.Name == "ModelInfoIFC4")
                    {
                        var modelid = ((ModelInfoIFC4)(InputPorts[0].Data)).ModelId;
                        var elementIdsList = ((ModelInfoIFC4)(InputPorts[0].Data)).ElementIds;
                        var res = new HashSet<Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId>(elementIdsList);

                        xModel = DataController.Instance.GetModel(modelid);
                        List<Xbim.Ifc4.Kernel.IfcProduct> elements = xModel.Instances.OfType<Xbim.Ifc4.Kernel.IfcProduct>().ToList();



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
            if (InputPorts[0].Data == null)
            {
                return;
            }

            var ifcWriterControl = ControlElements[0] as IfcWriterControl;
            ifcWriterControl.Button.Visibility = Visibility.Visible;

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