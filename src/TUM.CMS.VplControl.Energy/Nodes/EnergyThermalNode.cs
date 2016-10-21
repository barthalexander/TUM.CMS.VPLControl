using System.Windows.Controls;
using System.Xml;
using TUM.CMS.VplControl.Core;
using System.Collections.Generic;
using Xbim.IO;
using Xbim.XbimExtensions;
using TUM.CMS.VplControl.IFC.Utilities;
using System.Linq;
using System.Collections;
using System;
using System.Windows;
using System.Reflection;
using Xbim.Ifc2x3.ProductExtension;
using System.IO;
using Xbim.Ifc;
using Xbim.Ifc2x3.MeasureResource;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcThermalNode : Node
    {
        public IfcStore xModel;
        private bool TT_available;
        private Hashtable TTValues;


        public IfcThermalNode(Core.VplControl hostCanvas) : base(hostCanvas)
        {
            AddInputPortToNode("FilteredProducts", typeof(object));
            //AddOutputPortToNode("", typeof(object)); //no output

            var labelThermal = new Label { Content = "Thermal Node" };
            AddControlToNode(labelThermal);//#0
            var labelChoice = new Label { Content = "Choose element:" };
            AddControlToNode(labelChoice);//#1
            var comboBoxElementsSet = new ComboBox { };//this ckeckBox will have as Options all the elements (of a certain ifcType) filtered by the ParseNode n given in the input
            comboBoxElementsSet.SelectionChanged += comboBoxElementsSet_SelChanged;
            AddControlToNode(comboBoxElementsSet);//#2
            var labelMessage = new Label { Content = "Do not know yet..." };
            AddControlToNode(labelMessage);//#3
            var labelTTValue = new Label { Content = "Value:" };
            AddControlToNode(labelTTValue);//#4

        }

        private void comboBoxElementsSet_SelChanged(object sender, SelectionChangedEventArgs ee)//action handler 4 the comboBoxElementsSet
        {
            var comboBoxElementsSet = ControlElements[2] as ComboBox;
            if (comboBoxElementsSet == null) return;
            var selectedElementGID = (comboBoxElementsSet.SelectedItem); //as Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId;
                                                                         // var selectedElGID = selectedElementGID as Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId;
            if (selectedElementGID == null) return;
            Console.WriteLine("-->ComboBox Action Handler runnin!!<--");

            var labelTTValue = ControlElements[4] as Label;//it is in position #4...

            if (TT_available)
            {
                Console.WriteLine("~~~ TT IS available ~~~");
                var TT = TTValues[selectedElementGID] as IfcValue;
                Console.WriteLine("~~~" + TT.Value + "~~~");

                labelTTValue.Content = "Value: " + TT.Value;
            }
            else
                labelTTValue.Content = "Value: ?";
        }

        public override void Calculate()
        {
            Console.WriteLine("***This be Calculate() function runnin***");

            TTValues = new Hashtable();
            var modelid = ((ModelInfo)(InputPorts[0].Data)).ModelId;
            if (modelid == null) return;
            xModel = DataController.Instance.GetModel(modelid);
            //the Calculate() function re-loads the input ifc-elements, puts them in the checkBox and also checks whether ThermalTrasmittance(TT) is given or not
            //if it is given, all the user has to do is select an ifc-element and its TT will be shown.
            var comboBoxElementsSet = ControlElements[2] as ComboBox;
            if (comboBoxElementsSet == null) return;//
            comboBoxElementsSet.Items.Clear();
            var selectedItemIds = ((ModelInfo)(InputPorts[0].Data)).ElementIds;
            if (selectedItemIds == null) return;
            TT_available = true;//lets suppose TT is available in every element...If it is not in at least one, then TT_available=false
            Console.WriteLine("***" + selectedItemIds.Count + " ifc-Elements in the input***");
            for (int i = 0; i < selectedItemIds.Count; i++)
            {
                comboBoxElementsSet.Items.Add(selectedItemIds[i]);
                Console.WriteLine("--" + selectedItemIds[i] + "--");
                //now check TT-availability
                var selectedProduct = xModel.Instances.OfType<IfcElement>().ToList().Find(x => x.GlobalId == selectedItemIds[i]);

                var propertySets = selectedProduct.PropertySets.ToList() as List<Xbim.Ifc2x3.Kernel.IfcPropertySet>;

                bool found = false; int ii = 0;
                //Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue TTProperty = null;
                IfcValue TTValue = null;
                while (!found && ii < propertySets.Count)
                {
                    var onepropertySet = propertySets[ii].HasProperties.ToList();
                    int jj = 0;
                    while (!found && jj < onepropertySet.Count)
                    {
                        if (onepropertySet[jj].Name == "ThermalTransmittance")
                        {
                            found = true;
                            TTValue = ((Xbim.Ifc2x3.PropertyResource.IfcPropertySingleValue)onepropertySet[jj]).NominalValue;
                        }
                        jj++;
                    }
                    ii++;
                }
                if (!found)
                {
                    TT_available = false;
                    Console.WriteLine("$$ No TT available here $$" + i);
                }
                else
                {
                    Console.WriteLine("$$ TT is available here $$" + i + "$$" + TTValue.Value);
                    TTValues.Add(selectedItemIds[i], TTValue);
                }
            }
            comboBoxElementsSet.SelectedIndex = 0;

            //Write in the label whether TT is available or it has to b calculated
            var labelMessage = ControlElements[3] as Label;
            if (TT_available)
            {
                labelMessage.Content = "ThermalTransmittance available!";
            }
            else
            {
                labelMessage.Content = "ThermalTransmitance NOT available.";
            }

        }



        public class ComboboxItem
        {
            public string Text { get; set; }
            public object Value { get; set; }

            public override string ToString()
            { return Text; }


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
            return new IfcPropertyFilterNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}
