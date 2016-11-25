using System;
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
using Xbim.Common.Step21;
using Xbim.Ifc;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcWriterNode : Node
    {
        private readonly TextBox _textBox;
        public IfcStore xModel;
        public Type IfcVersionType = null;

        public IfcWriterNode(Core.VplControl hostCanvas) : base(hostCanvas)
        {
            AddInputPortToNode("Object", typeof(object), false);

            IfcWriterControl ifcWriterControl = new IfcWriterControl();
            ifcWriterControl.Title.Content = "IFC Writer";
            ifcWriterControl.Button.Content = "Save File";
            ifcWriterControl.Button.Click += button_Click;

            AddControlToNode(ifcWriterControl);
        }

        /// <summary>
        /// Writes the Input to an IFC file
        /// 
        /// The Input can be one model
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (InputPorts[0].Data != null)
            {
                PropertyTranformDelegate semanticFilterIFC2x3 = (property, parentObject) =>
                {
                    return property.PropertyInfo.GetValue(parentObject, null);
                };
                PropertyTranformDelegate semanticFilterIFC4 = (property, parentObject) =>
                {
                    return property.PropertyInfo.GetValue(parentObject, null);
                };

                var newModelIfc2x3 = IfcStore.Create(IfcSchemaVersion.Ifc2X3, XbimStoreType.InMemoryModel);

                var newModelIfc4 = IfcStore.Create(IfcSchemaVersion.Ifc4, XbimStoreType.InMemoryModel);

                IfcSchemaVersion ifcVersion = IfcSchemaVersion.Unsupported;

                // Check the IFC Version
                IfcVersionType = InputPorts[0].Data.GetType();
                if (IfcVersionType.Name == "ModelInfoIFC2x3")
                {
                    var modelid = ((ModelInfoIFC2x3)(InputPorts[0].Data)).ModelId;
                    var elementIdsList = ((ModelInfoIFC2x3)(InputPorts[0].Data)).ElementIds;
                    var res = new HashSet<Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId>(elementIdsList);

                    xModel = DataController.Instance.GetModel(modelid);
                    ifcVersion = xModel.IfcSchemaVersion;

                    List<Xbim.Ifc2x3.Kernel.IfcProduct> elements = xModel.Instances.OfType<Xbim.Ifc2x3.Kernel.IfcProduct>().ToList();

                    using (var txn = newModelIfc2x3.BeginTransaction())
                    {
                        var copied = new XbimInstanceHandleMap(xModel, newModelIfc2x3);
                        foreach (var element in elements)
                        {
                            if (res.Contains(element.GlobalId))
                            {
                                newModelIfc2x3.InsertCopy(element, copied, semanticFilterIFC2x3, false, false);
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
                    ifcVersion = xModel.IfcSchemaVersion;

                    List<Xbim.Ifc4.Kernel.IfcProduct> elements = xModel.Instances.OfType<Xbim.Ifc4.Kernel.IfcProduct>().ToList();

                    using (var txn = newModelIfc4.BeginTransaction())
                    {
                        var copied = new XbimInstanceHandleMap(xModel, newModelIfc4);
                        foreach (var element in elements)
                        {
                            if (res.Contains(element.GlobalId))
                            {
                                newModelIfc4.InsertCopy(element, copied, semanticFilterIFC4, false, false);
                            }
                        }
                        txn.Commit();
                    }
                }

                // Write the new IFC file to the selected Path
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.Filter = "IfcFile |*.ifc";
                saveFileDialog1.Title = "Save an IFC File";
                saveFileDialog1.ShowDialog();
                if (saveFileDialog1.FileName != "")
                {
                    if (ifcVersion == IfcSchemaVersion.Ifc2X3)
                    {
                        newModelIfc2x3.SaveAs(saveFileDialog1.FileName);
                        newModelIfc2x3.Close();
                        if (File.Exists(saveFileDialog1.FileName))
                        {
                            MessageBox.Show("File saved", "My Application", MessageBoxButton.OK);
                        }
                        else
                        {
                            MessageBox.Show("There was an Error \n Please Try again", "My Application", MessageBoxButton.OK);
                        }
                    }
                    else if (ifcVersion == IfcSchemaVersion.Ifc4)
                    {
                        newModelIfc4.SaveAs(saveFileDialog1.FileName);
                        newModelIfc4.Close();
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