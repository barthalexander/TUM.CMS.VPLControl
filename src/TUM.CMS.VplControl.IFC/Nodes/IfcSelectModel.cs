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
using Xbim.Ifc;
using Xbim.IO;
using Xbim.ModelGeometry.Scene;
using Xbim.Presentation;
using Xbim.XbimExtensions;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcSelectModel : Node
    {
        private IfcStore xModel;

        // private DynamicProductSelectionControl productSelectionControl;
        public IfcSelectModel(Core.VplControl hostCanvas) : base(hostCanvas)
        {

            var labelModels = new Label { Content = "Select Model" };
            AddControlToNode(labelModels);

            var comboBoxModels = new ComboBox { };
            comboBoxModels.SelectionChanged += comboBoxModels_SelectionChanged;
            AddControlToNode(comboBoxModels);

            
           

            AddInputPortToNode("Object", typeof(object));
            AddOutputPortToNode("Object", typeof(object));


        }
        public class ComboboxItem
        {
            public string Text { get; set; }
            public object Value { get; set; }

            public override string ToString()
            { return Text; }

        }
        /// <summary>
        /// On Change you can work with another Model on the Output
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxModels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBoxModels = ControlElements[1] as ComboBox;
            var selectedItem = (ComboboxItem)(comboBoxModels.SelectedItem);

            var fileString = selectedItem.Value.ToString();

            ModelInfo modelInfo = new ModelInfo(fileString);
            xModel = DataController.Instance.GetModel(fileString);
            List<Xbim.Ifc2x3.ProductExtension.IfcBuildingElement> elements = xModel.Instances.OfType<Xbim.Ifc2x3.ProductExtension.IfcBuildingElement>().ToList();
            foreach (var element in elements)
            {
                modelInfo.AddElementIds(element.GlobalId);
            }
            xModel.Close();
            OutputPorts[0].Data = modelInfo;
        }
        /// <summary>
        /// Adds all Models which are stored at the DataController to the ComboBox
        /// </summary>
        public override void Calculate()
        {
            var models = DataController.Instance.ModelStorage.ToList();
            var comboBoxModels = ControlElements[1] as ComboBox;
            if (comboBoxModels != null)
            {
                comboBoxModels.SelectedItem = -1;
                comboBoxModels.Items.Clear();

                for (int i = 0; i < models.Count; i++)
                {
                    var model = models[i].Key;
                    string modelName = i + " " + models[i].Value.Header.Name;
                    ComboboxItem modelItem = new ComboboxItem() { Text = modelName, Value = model };

                    comboBoxModels.Items.Add(modelItem);
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