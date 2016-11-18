using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUM.CMS.VplControl.Core;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using TUM.CMS.VplControl.IFC.Utilities;
using Xbim.IO;
using Xbim;
using TUM.CMS.VplControl.IFC.Utilities.mvdXMLReaderClasses;
using System.Collections;
using Xbim.Ifc2x3.UtilityResource;
using Xbim.Common;
using Xbim.Common.Metadata;
using Xbim.Common.Step21;
using Xbim.Ifc;
using Xbim.Ifc2x3.Kernel;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc2x3.ProductExtension;
using Xbim.Presentation;
using System.Reflection;
using TUM.CMS.VplControl.IFC.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class MvdCheckerAndFilter : Node
    {
        public IfcStore xModel;
        public ModelInfoIFC2x3 OutputInfoIfc2x3;
        public ModelInfoIFC4 OutputInfoIfc4;
        public Type IfcVersionType = null;
        public bool match;
        public List<Xbim.Ifc2x3.Kernel.IfcProduct> elements2x3 = new List<Xbim.Ifc2x3.Kernel.IfcProduct> { };
        public List<Xbim.Ifc4.Kernel.IfcProduct> elements4 = new List<Xbim.Ifc4.Kernel.IfcProduct> { };


        public HashSet<String> ChosenEntities;
        

        public MvdCheckerAndFilter(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {
            IsResizeable = true;
            AddInputPortToNode("IfcFile", typeof(object));
            AddInputPortToNode("MvdFile", typeof(object));
            AddOutputPortToNode("FilteredIfc", typeof(object));

           

            MvdCheckerAndFilterControl mvdCheckerAndFilterControl = new MvdCheckerAndFilterControl();

            AddControlToNode(mvdCheckerAndFilterControl);

            


        }

      

        public override void Calculate()
        {
            if (InputPorts[0].Data == null) return;
            if (InputPorts[1].Data == null) return;
            match = true;


            ScrollViewer resultScroll = new ScrollViewer
            {
                Visibility = Visibility.Visible,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                CanContentScroll=true,
                MinWidth=300,
                MaxWidth=400,
                MinHeight=240,
                MaxHeight=240
            

            };
            ScrollViewer errorListScroll = new ScrollViewer {
                Visibility = Visibility.Visible,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                CanContentScroll = true,
                MinWidth = 300,
                MaxWidth = 400,
                MinHeight = 240,
                MaxHeight = 240
            };
            ScrollViewer fullReportScroll = new ScrollViewer {
                Visibility = Visibility.Visible,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                CanContentScroll = true,
                MinWidth = 300,
                MaxWidth = 400,
                MinHeight = 240,
                MaxHeight = 240
            };
            TextBlock resultText = new TextBlock {
                Visibility = Visibility.Visible,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                TextWrapping = TextWrapping.Wrap,
                FontSize=14,
                Padding= new Thickness(5, 10, 5, 10),
                 IsHitTestVisible =false
                 

            };
            resultScroll.Content = resultText;

            TextBlock errorListText = new TextBlock {
                Visibility = Visibility.Visible,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 14,
                Padding = new Thickness(5, 10, 5, 10),
                IsHitTestVisible = false,
                Foreground = Brushes.Red
            };
            errorListScroll.Content = errorListText;
            TextBlock fullReportText = new TextBlock
            {
                Visibility = Visibility.Visible,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 14,
                Padding = new Thickness(5, 10, 5, 10),
                IsHitTestVisible = false,
                Foreground = Brushes.Green
            };
            fullReportScroll.Content = fullReportText;
            fullReportText.Text = "";
            errorListText.Text = "";

            var mvdCheckerAndFilterControl = ControlElements[0] as MvdCheckerAndFilterControl;
            var tabControl = mvdCheckerAndFilterControl.tabControl as TabControl;
            if (tabControl == null) return;

            mvdCheckerAndFilterControl.StackPanel1.Children.Add(resultScroll);
            mvdCheckerAndFilterControl.StackPanel2.Children.Add(errorListScroll);
            mvdCheckerAndFilterControl.StackPanel3.Children.Add(fullReportScroll);

            

            IfcVersionType = InputPorts[0].Data.GetType();
            if (IfcVersionType.Name == "ModelInfoIFC2x3")
            {
                var modelid = ((ModelInfoIFC2x3)(InputPorts[0].Data)).ModelId;
                xModel = DataController.Instance.GetModel(modelid);
                var elementIds = ((ModelInfoIFC2x3)(InputPorts[0].Data)).ElementIds;
                OutputInfoIfc2x3 = (ModelInfoIFC2x3)(InputPorts[0].Data);


                ModelView modelView = (ModelView)(InputPorts[1].Data);
                Dictionary<string, ConceptRoot> allConceptRoots = modelView.GetAllConceptRoots();

                if (allConceptRoots.Count > 0)
                {
                    foreach (var conceptRoot in allConceptRoots)
                    {
                        if (CheckConceptRoot(conceptRoot.Value))
                        {
                            continue;
                        }
                        else
                        {
                            match = false;
                            continue;

                        }

                    }

                }

                if (match == true)
                {
                    OutputInfoIfc2x3.ElementIds.Clear();
                    foreach (var item in elements2x3)
                    {
                        OutputInfoIfc2x3.AddElementIds(item.GlobalId);
                    }
                    resultText.Text = "Ifc file is valid according to this specific MVD!";

                    OutputPorts[0].Data = OutputInfoIfc2x3;


                }
                else
                {
                    resultText.Text = "Ifc file doesn't match with MVD!\r\n Information Missing! \r\nSee details in Error List and Full Report!";
                }
            }
            else if (IfcVersionType.Name == "ModelInfoIFC4")
            {
                var modelid = ((ModelInfoIFC4)(InputPorts[0].Data)).ModelId;
                xModel = DataController.Instance.GetModel(modelid);
                var elementIds = ((ModelInfoIFC4)(InputPorts[0].Data)).ElementIds;
                OutputInfoIfc4 = (ModelInfoIFC4)(InputPorts[0].Data);


                ModelView modelView = (ModelView)(InputPorts[1].Data);
                Dictionary<string, ConceptRoot> allConceptRoots = modelView.GetAllConceptRoots();

                if (allConceptRoots.Count > 0)
                {
                    foreach (var conceptRoot in allConceptRoots)
                    {
                        if (CheckConceptRoot(conceptRoot.Value))
                        {
                            continue;
                        }
                        else
                        {
                            match = false;
                            continue;

                        }

                    }

                }

                if (match == true)
                {
                    OutputInfoIfc4.ElementIds.Clear();
                    foreach (var item in elements4)
                    {
                        OutputInfoIfc4.AddElementIds(item.GlobalId);
                    }
                    resultText.Text = "Ifc file is valid according to this specific MVD!";

                    OutputPorts[0].Data = OutputInfoIfc4;


                }
                else
                {
                    resultText.Text = "Ifc file doesn't match with MVD!\r\n Information Missing! \r\nSee details in Error List and Full Report!";
                }
            }


        }
        
        public bool CheckConceptRoot(ConceptRoot conceptRoot)
        {
            var mvdCheckerAndFilterControl = ControlElements[0] as MvdCheckerAndFilterControl;          
            var tabControl = mvdCheckerAndFilterControl.tabControl as TabControl;

            var tabitem1 = tabControl.Items.GetItemAt(0) as TabItem;
            var resultScroll = mvdCheckerAndFilterControl.StackPanel1.Children[0] as ScrollViewer;            
            var resultText = resultScroll.Content as TextBlock;
            
            var tabitem2 = tabControl.Items.GetItemAt(1) as TabItem;
            var errorListScroll = mvdCheckerAndFilterControl.StackPanel2.Children[0] as ScrollViewer;
            var errorListText = errorListScroll.Content as TextBlock;
            
            var tabitem3 = tabControl.Items.GetItemAt(2) as TabItem;
            var fullReportScroll = mvdCheckerAndFilterControl.StackPanel3.Children[0] as ScrollViewer;
            var fullReportText = fullReportScroll.Content as TextBlock;
            

            bool conceptRootMatch = true;
            string conceptRootString = conceptRoot.name;

            if (IfcVersionType.Name == "ModelInfoIFC2x3")
            {
                switch (conceptRootString)
                {
                    case "Slab":
                        if (xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcSlab>().ToList() == null)
                        {
                            conceptRootMatch = false;
                            errorListText.Text += "missing concept root Slab!\r\n";
                            return conceptRootMatch;
                        }
                        else
                        {
                            elements2x3.AddRange(xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcSlab>().ToList());
                            fullReportText.Text += "Concept Root Slab matches!\r\n";
                        }
                        break;
                    case "Wall":
                        if (xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcWall>().ToList() == null)
                        {
                            conceptRootMatch = false;
                            errorListText.Text += "missing concept root Wall!\r\n";
                            return conceptRootMatch;
                        }
                        else
                        {
                            elements2x3.AddRange(xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcWall>().ToList());
                            fullReportText.Text += "Concept Root Wall matches!\r\n";
                        }
                        break;
                    case "Column":
                        if (xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcColumn>().ToList() == null)
                        {
                            conceptRootMatch = false;
                            errorListText.Text += "missing concept root Column!\r\n";
                            return conceptRootMatch;
                        }
                        else
                        {
                            elements2x3.AddRange(xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcColumn>().ToList());
                            fullReportText.Text += "Concept Root Column matches!\r\n";
                        }
                        break;
                    case "Window":
                        if (xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcWindow>().ToList() == null)
                        {
                            conceptRootMatch = false;
                            errorListText.Text += "missing concept root Window!\r\n";
                            return conceptRootMatch;
                        }
                        else
                        {
                            elements2x3.AddRange(xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcWindow>().ToList());
                            fullReportText.Text += "Concept Root Window matches!\r\n";
                        }
                        break;
                    case "Door":
                        if (xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcDoor>().ToList() == null)
                        {
                            conceptRootMatch = false;
                            errorListText.Text += "missing concept root Door!\r\n";
                            return conceptRootMatch;
                        }
                        else
                        {
                            elements2x3.AddRange(xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcDoor>().ToList());
                            fullReportText.Text += "Concept Root Door matches!\r\n";
                        }
                        break;
                    case "CurtainWall":
                        if (xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcCurtainWall>().ToList() == null)
                        {
                            conceptRootMatch = false;
                            errorListText.Text += "missing concept root CurtainWall!\r\n";
                            return conceptRootMatch;
                        }
                        else
                        {
                            elements2x3.AddRange(xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcCurtainWall>().ToList());
                            fullReportText.Text += "Concept Root CurtainWall matches!\r\n";
                        }
                        break;
                    case "Beam":
                        if (xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcBeam>().ToList() == null)
                        {
                            conceptRootMatch = false;
                            errorListText.Text += "missing concept root Beam!\r\n";
                            return conceptRootMatch;
                        }
                        else
                        {
                            elements2x3.AddRange(xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcBeam>().ToList());
                            fullReportText.Text += "Concept Root Beam matches!\r\n";
                        }
                        break;
                    case "Railing":
                        if (xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcRailing>().ToList() == null)
                        {
                            conceptRootMatch = false;
                            errorListText.Text += "missing concept root Railing!\r\n";
                            return conceptRootMatch;
                        }
                        else
                        {
                            elements2x3.AddRange(xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcRailing>().ToList());
                            fullReportText.Text += "Concept Root Railing matches!\r\n";
                        }
                        break;
                    case "Plate":
                        if (xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcPlate>().ToList() == null)
                        {
                            conceptRootMatch = false;
                            errorListText.Text += "missing concept root Plate!\r\n";
                            return conceptRootMatch;
                        }
                        else
                        {
                            elements2x3.AddRange(xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcPlate>().ToList());
                            fullReportText.Text += "Concept Root Plate matches!\r\n";
                        }
                        break;
                    case "Ramp":
                        if (xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcRamp>().ToList() == null)
                        {
                            conceptRootMatch = false;
                            errorListText.Text += "missing concept root Ramp!\r\n";
                            return conceptRootMatch;
                        }
                        else
                        {
                            elements2x3.AddRange(xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcRamp>().ToList());
                            fullReportText.Text += "Concept Root Ramp matches!\r\n";
                        }
                        break;
                    case "Roof":
                        if (xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcRoof>().ToList() == null)
                        {
                            conceptRootMatch = false;
                            errorListText.Text += "missing concept root Roof!\r\n";
                            return conceptRootMatch;
                        }
                        else
                        {
                            elements2x3.AddRange(xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcRoof>().ToList());
                            fullReportText.Text += "Concept Root Roof matches!\r\n";
                        }
                        break;
                    case "Stair":
                        if (xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcStair>().ToList() == null)
                        {
                            conceptRootMatch = false;
                            errorListText.Text += "missing concept root Stair!\r\n";
                            return conceptRootMatch;
                        }
                        else
                        {
                            elements2x3.AddRange(xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcStair>().ToList());
                            fullReportText.Text += "Concept Root Stair matches!\r\n";
                        }
                        break;
                }
                
            }

            else if (IfcVersionType.Name == "ModelInfoIFC4")
            {
                switch (conceptRootString)
                {
                    case "Slab":
                        if (xModel.Instances.OfType<Xbim.Ifc4.SharedBldgElements.IfcSlab>().ToList() == null)
                        {
                            conceptRootMatch = false;
                            errorListText.Text += "missing concept root Slab!\r\n";
                            return conceptRootMatch;
                        }
                        else
                        {
                            elements4.AddRange(xModel.Instances.OfType<Xbim.Ifc4.SharedBldgElements.IfcSlab>().ToList());
                            fullReportText.Text += "Concept Root Slab matches!\r\n";
                        }
                        break;
                    case "Wall":
                        if (xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcWall>().ToList() == null)
                        {
                            conceptRootMatch = false;
                            errorListText.Text += "missing concept root Wall!\r\n";
                            return conceptRootMatch;
                        }
                        else
                        {
                            elements4.AddRange(xModel.Instances.OfType<Xbim.Ifc4.SharedBldgElements.IfcWall>().ToList());
                            fullReportText.Text += "Concept Root Wall matches!\r\n";
                        }
                        break;
                    case "Column":
                        if (xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcColumn>().ToList() == null)
                        {
                            conceptRootMatch = false;
                            errorListText.Text += "missing concept root Column!\r\n";
                            return conceptRootMatch;
                        }
                        else
                        {
                            elements4.AddRange(xModel.Instances.OfType<Xbim.Ifc4.SharedBldgElements.IfcColumn>().ToList());
                            fullReportText.Text += "Concept Root Column matches!\r\n";
                        }
                        break;
                    case "Window":
                        if (xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcWindow>().ToList() == null)
                        {
                            conceptRootMatch = false;
                            errorListText.Text += "missing concept root Window!\r\n";
                            return conceptRootMatch;
                        }
                        else
                        {
                            elements4.AddRange(xModel.Instances.OfType<Xbim.Ifc4.SharedBldgElements.IfcWindow>().ToList());
                            fullReportText.Text += "Concept Root Window matches!\r\n";
                        }
                        break;
                    case "Door":
                        if (xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcDoor>().ToList() == null)
                        {
                            conceptRootMatch = false;
                            errorListText.Text += "missing concept root Door!\r\n";
                            return conceptRootMatch;
                        }
                        else
                        {
                            elements4.AddRange(xModel.Instances.OfType<Xbim.Ifc4.SharedBldgElements.IfcDoor>().ToList());
                            fullReportText.Text += "Concept Root Door matches!\r\n";
                        }
                        break;
                    case "CurtainWall":
                        if (xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcCurtainWall>().ToList() == null)
                        {
                            conceptRootMatch = false;
                            errorListText.Text += "missing concept root CurtainWall!\r\n";
                            return conceptRootMatch;
                        }
                        else
                        {
                            elements4.AddRange(xModel.Instances.OfType<Xbim.Ifc4.SharedBldgElements.IfcCurtainWall>().ToList());
                            fullReportText.Text += "Concept Root CurtainWall matches!\r\n";
                        }
                        break;
                    case "Beam":
                        if (xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcBeam>().ToList() == null)
                        {
                            conceptRootMatch = false;
                            errorListText.Text += "missing concept root Beam!\r\n";
                            return conceptRootMatch;
                        }
                        else
                        {
                            elements4.AddRange(xModel.Instances.OfType<Xbim.Ifc4.SharedBldgElements.IfcBeam>().ToList());
                            fullReportText.Text += "Concept Root Beam matches!\r\n";
                        }
                        break;
                    case "Railing":
                        if (xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcRailing>().ToList() == null)
                        {
                            conceptRootMatch = false;
                            errorListText.Text += "missing concept root Railing!\r\n";
                            return conceptRootMatch;
                        }
                        else
                        {
                            elements4.AddRange(xModel.Instances.OfType<Xbim.Ifc4.SharedBldgElements.IfcRailing>().ToList());
                            fullReportText.Text += "Concept Root Railing matches!\r\n";
                        }
                        break;
                    case "Plate":
                        if (xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcPlate>().ToList() == null)
                        {
                            conceptRootMatch = false;
                            errorListText.Text += "missing concept root Plate!\r\n";
                            return conceptRootMatch;
                        }
                        else
                        {
                            elements4.AddRange(xModel.Instances.OfType<Xbim.Ifc4.SharedBldgElements.IfcPlate>().ToList());
                            fullReportText.Text += "Concept Root Plate matches!\r\n";
                        }
                        break;
                    case "Ramp":
                        if (xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcRamp>().ToList() == null)
                        {
                            conceptRootMatch = false;
                            errorListText.Text += "missing concept root Ramp!\r\n";
                            return conceptRootMatch;
                        }
                        else
                        {
                            elements4.AddRange(xModel.Instances.OfType<Xbim.Ifc4.SharedBldgElements.IfcRamp>().ToList());
                            fullReportText.Text += "Concept Root Ramp matches!\r\n";
                        }
                        break;
                    case "Roof":
                        if (xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcRoof>().ToList() == null)
                        {
                            conceptRootMatch = false;
                            errorListText.Text += "missing concept root Roof!\r\n";
                            return conceptRootMatch;
                        }
                        else
                        {
                            elements4.AddRange(xModel.Instances.OfType<Xbim.Ifc4.SharedBldgElements.IfcRoof>().ToList());
                            fullReportText.Text += "Concept Root Roof matches!\r\n";
                        }
                        break;
                    case "Stair":
                        if (xModel.Instances.OfType<Xbim.Ifc2x3.SharedBldgElements.IfcStair>().ToList() == null)
                        {
                            conceptRootMatch = false;
                            errorListText.Text += "missing concept root Stair!\r\n";
                            return conceptRootMatch;
                        }
                        else
                        {
                            elements4.AddRange(xModel.Instances.OfType<Xbim.Ifc4.SharedBldgElements.IfcStair>().ToList());
                            fullReportText.Text += "Concept Root Stair matches!\r\n";
                        }
                        break;
                }

            }

                Dictionary<string, Concept> allConcepts = conceptRoot.GetAllConcepts();

            if (allConcepts.Count > 0)
            {
                foreach (var item in allConcepts)
                {
                    if(CheckConcept(item.Value))
                    {
                        continue;
                    }
                    {
                        conceptRootMatch = false;
                        continue;
                    }
                   
                }
            }



            return conceptRootMatch;

        }

        public static Type[] getTypeByNameFromIfc2x3(string className)
        {
            List<Type> returnVal = new List<Type>();
            Assembly ifc2x3=Assembly.LoadWithPartialName("Xbim.Ifc2x3");

            //Assembly ifc4 = Assembly.LoadWithPartialName("Xbim.Ifc4");
            Type[] assemblyTypes = ifc2x3.GetTypes();
            for (int j = 0; j < assemblyTypes.Length; j++)
            {
                if (assemblyTypes[j].Name == className)
                {
                    returnVal.Add(assemblyTypes[j]);
                    
                }
            }

            /*foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] assemblyTypes = a.GetTypes();
                for (int j = 0; j < assemblyTypes.Length; j++)
                {
                    if (assemblyTypes[j].Name == className)
                    {
                        returnVal.Add(assemblyTypes[j]);
                        if(returnVal.Count==2) return returnVal.ToArray();
                    }
                }
            }*/

            return returnVal.ToArray();
            
        }

        public static Type[] getTypeByNameFromIfc4(string className)
        {
            List<Type> returnVal = new List<Type>();
            Assembly ifc4 = Assembly.LoadWithPartialName("Xbim.Ifc4");

           
            Type[] assemblyTypes = ifc4.GetTypes();
            for (int j = 0; j < assemblyTypes.Length; j++)
            {
                if (assemblyTypes[j].Name == className)
                {
                    returnVal.Add(assemblyTypes[j]);

                }
            }

            return returnVal.ToArray();

        }


        public bool CheckConcept(Concept concept)//Material Layer Set Usage
        {

            var mvdCheckerAndFilterControl = ControlElements[0] as MvdCheckerAndFilterControl;
            var tabControl = mvdCheckerAndFilterControl.tabControl as TabControl;

            var tabitem1 = tabControl.Items.GetItemAt(0) as TabItem;
            var resultScroll = mvdCheckerAndFilterControl.StackPanel1.Children[0] as ScrollViewer;
            var resultText = resultScroll.Content as TextBlock;

            var tabitem2 = tabControl.Items.GetItemAt(1) as TabItem;
            var errorListScroll = mvdCheckerAndFilterControl.StackPanel2.Children[0] as ScrollViewer;
            var errorListText = errorListScroll.Content as TextBlock;

            var tabitem3 = tabControl.Items.GetItemAt(2) as TabItem;
            var fullReportScroll = mvdCheckerAndFilterControl.StackPanel3.Children[0] as ScrollViewer;
            var fullReportText = fullReportScroll.Content as TextBlock;

            string conceptName = concept.name;                                                    
            bool conceptMatch = true;
            ConceptTemplate conceptTemplate = concept.getConceptTemplate();
            
            if(CheckConceptTemplate(conceptTemplate))
            {
                fullReportText.Text += "Concept " + conceptName + " matches!\r\n";

            }
            else
            {
                errorListText.Text += "Concept " + conceptName + " is missing!\r\n";
                conceptMatch = false;

            }
            

            Dictionary<string, Concept> allSubConcepts = concept.GetAllSubConcepts();
            if (allSubConcepts.Count > 0)
            {
                foreach (var item in allSubConcepts)
                {
                    if(CheckConcept(item.Value))
                    {
                        continue;
                    }
                    else
                    {
                        conceptMatch = false;
                        continue;
                    }

                }
            }
            return conceptMatch;
        }

        public bool CheckConceptTemplate(ConceptTemplate conceptTemplate)
        {
           
            bool conceptTemplateMatch = true;

            Dictionary<string, ConceptTemplate> subConceptTemplates = conceptTemplate.getConceptTemplateses();
            if (subConceptTemplates.Count > 0)
            {

                foreach (var item in subConceptTemplates)
                {
                    if(CheckConceptTemplate(item.Value))
                    {
                        continue;
                    }
                    else
                    {
                        conceptTemplateMatch = false;
                        continue;
                    }
                   
                }
            }

            List<Templates> subTemplates = conceptTemplate.getAllSubTemplates();
            if (subTemplates.Any())
            {

                foreach (var item in subTemplates)
                {
                    if(CheckTemplate(item))
                    {
                        continue;
                    }
                    else
                    {
                        conceptTemplateMatch = false;
                        continue;
                    }
                    
                }
            }

            List<AttributeRule> attributeRules = conceptTemplate.getAttributeRules();
            if (attributeRules.Any())
            {

                foreach (var item in attributeRules)
                {
                    if(CheckAttributeRuleBegin(item))
                    {
                        continue;
                    }
                    else
                    {
                        conceptTemplateMatch = false;
                        continue;
                    }
                    
                }
            }
            return conceptTemplateMatch;
        }


        public bool CheckTemplate(Templates template)
        {

            bool templateMatch = true;
            Dictionary<string, ConceptTemplate> allConceptTemplates = template.getConceptTemplates();
            if (allConceptTemplates.Count > 0)
            {
                foreach (var item in allConceptTemplates)
                {
                    if(CheckConceptTemplate(item.Value))
                    {
                        continue;
                    }
                    else
                    {
                        templateMatch = false;
                        continue;
                    }
                    

                }
            }
            return templateMatch;
        }

        public bool CheckAttributeRuleBegin(AttributeRule attributeRule)
        {
            var mvdCheckerAndFilterControl = ControlElements[0] as MvdCheckerAndFilterControl;
            var tabControl = mvdCheckerAndFilterControl.tabControl as TabControl;

            var tabitem1 = tabControl.Items.GetItemAt(0) as TabItem;
            var resultScroll = mvdCheckerAndFilterControl.StackPanel1.Children[0] as ScrollViewer;
            var resultText = resultScroll.Content as TextBlock;

            var tabitem2 = tabControl.Items.GetItemAt(1) as TabItem;
            var errorListScroll = mvdCheckerAndFilterControl.StackPanel2.Children[0] as ScrollViewer;
            var errorListText = errorListScroll.Content as TextBlock;

            var tabitem3 = tabControl.Items.GetItemAt(2) as TabItem;
            var fullReportScroll = mvdCheckerAndFilterControl.StackPanel3.Children[0] as ScrollViewer;
            var fullReportText = fullReportScroll.Content as TextBlock;

            object transportingEntity;

            bool attributeRuleMatch = true;
            string attributeRuleName = attributeRule.attributeName;//HasAssociations
            if (IfcVersionType.Name == "ModelInfoIFC2x3")
            {
                foreach (Xbim.Ifc2x3.Kernel.IfcProduct element in elements2x3)
                {
                    string elementName = element.Name;
                    errorListText.Text += "=======" + elementName + "=======\r\n";
                    fullReportText.Text += "=======" + elementName + "=======\r\n";
                    if (element.GetType().GetProperty(attributeRuleName) == null || element.GetType().GetProperty(attributeRuleName).GetValue(element) == null)
                    {

                        errorListText.Text += "Attribute " + attributeRuleName + " is missing!\r\n";
                        attributeRuleMatch = false;
                        continue;
                    }
                    else
                    {

                        fullReportText.Text += "Attribute Rule " + attributeRuleName + " matches!" + "\r\n";

                        List<EntityRule> allEntityRules = attributeRule.getEntityRules();
                        foreach (EntityRule entityRule in allEntityRules)//IfcRelAssociatesMaterial
                        {

                            string entityName = entityRule.entityName;
                            Type TypeOfEntityRuleInMvd = getTypeByNameFromIfc2x3(entityName)[0];

                            // foreach (IfcProduct element in elements)
                            //{

                            var IfcProperties = element.GetType().GetProperties();


                            Type baseType = TypeOfEntityRuleInMvd.BaseType;

                            foreach (PropertyInfo property in IfcProperties)
                            {
                                if (property.PropertyType.IsGenericType)
                                {
                                    if (property.PropertyType.GetGenericArguments()[0] == baseType || property.PropertyType.GetGenericArguments()[0] == TypeOfEntityRuleInMvd)
                                    {
                                        PropertyInfo IfcPropertyNeeded = property;
                                        var obj = IfcPropertyNeeded.GetValue(element) as IEnumerable;
                                        if (obj != null)
                                        {
                                            foreach (var item in obj)
                                            {
                                                transportingEntity = item;//IfcRelAssociatesMaterial
                                                if (item.GetType() == TypeOfEntityRuleInMvd)
                                                {
                                                    fullReportText.Text += "Entity Rule " + entityName + " matches!\r\n";
                                                    if (CheckEntityRule(entityRule, transportingEntity))//IfcRelAssociatesMaterial
                                                    {
                                                        continue;
                                                    }
                                                    else
                                                    {
                                                        attributeRuleMatch = false;
                                                        continue;
                                                    }

                                                }
                                            }

                                        }

                                    }

                                }
                                /* else
                                 {
                                     if (property.PropertyType == baseType)
                                     {
                                         PropertyInfo IfcPropertyNeeded = property;
                                         var obj = IfcPropertyNeeded.GetValue(element);

                                     }
                                 }*/

                            }
                            //}


                        }

                    }
                }

                if (attributeRuleMatch == true)
                {
                    fullReportText.Text += "All Elements' Attribute Rule " + attributeRuleName + " matches!" + "\r\n";

                }

                
            }
            else if(IfcVersionType.Name == "ModelInfoIFC4")
            {
                foreach (Xbim.Ifc4.Kernel.IfcProduct element in elements4)
                {
                    string elementName = element.Name;
                    errorListText.Text += "=======" + elementName + "=======\r\n";
                    fullReportText.Text += "=======" + elementName + "=======\r\n";
                    if (element.GetType().GetProperty(attributeRuleName) == null || element.GetType().GetProperty(attributeRuleName).GetValue(element) == null)
                    {

                        errorListText.Text += "Attribute " + attributeRuleName + " is missing!\r\n";
                        attributeRuleMatch = false;
                        continue;
                    }
                    else
                    {

                        fullReportText.Text += "Attribute Rule " + attributeRuleName + " matches!" + "\r\n";

                        List<EntityRule> allEntityRules = attributeRule.getEntityRules();
                        foreach (EntityRule entityRule in allEntityRules)//IfcRelAssociatesMaterial
                        {

                            string entityName = entityRule.entityName;
                            Type TypeOfEntityRuleInMvd = getTypeByNameFromIfc4(entityName)[0];

                          

                            var IfcProperties = element.GetType().GetProperties();


                            Type baseType = TypeOfEntityRuleInMvd.BaseType;

                            foreach (PropertyInfo property in IfcProperties)
                            {
                                if (property.PropertyType.IsGenericType)
                                {
                                    if (property.PropertyType.GetGenericArguments()[0] == baseType || property.PropertyType.GetGenericArguments()[0] == TypeOfEntityRuleInMvd)
                                    {
                                        PropertyInfo IfcPropertyNeeded = property;
                                        var obj = IfcPropertyNeeded.GetValue(element) as IEnumerable;
                                        if (obj != null)
                                        {
                                            foreach (var item in obj)
                                            {
                                                transportingEntity = item;//IfcRelAssociatesMaterial
                                                if (item.GetType() == TypeOfEntityRuleInMvd)
                                                {
                                                    fullReportText.Text += "Entity Rule " + entityName + " matches!\r\n";
                                                    if (CheckEntityRule(entityRule, transportingEntity))//IfcRelAssociatesMaterial
                                                    {
                                                        continue;
                                                    }
                                                    else
                                                    {
                                                        attributeRuleMatch = false;
                                                        continue;
                                                    }

                                                }
                                            }

                                        }

                                    }

                                }
                                /* else
                                 {
                                     if (property.PropertyType == baseType)
                                     {
                                         PropertyInfo IfcPropertyNeeded = property;
                                         var obj = IfcPropertyNeeded.GetValue(element);

                                     }
                                 }*/

                            }
                            //}


                        }

                    }
                }

                if (attributeRuleMatch == true)
                {
                    fullReportText.Text += "All Elements' Attribute Rule " + attributeRuleName + " matches!" + "\r\n";

                }

                
            }
            return attributeRuleMatch;


        }

        public bool CheckAttributeRule(AttributeRule attributeRule,object transportingEntity)
        {
            var mvdCheckerAndFilterControl = ControlElements[0] as MvdCheckerAndFilterControl;
            var tabControl = mvdCheckerAndFilterControl.tabControl as TabControl;

            var tabitem1 = tabControl.Items.GetItemAt(0) as TabItem;
            var resultScroll = mvdCheckerAndFilterControl.StackPanel1.Children[0] as ScrollViewer;
            var resultText = resultScroll.Content as TextBlock;

            var tabitem2 = tabControl.Items.GetItemAt(1) as TabItem;
            var errorListScroll = mvdCheckerAndFilterControl.StackPanel2.Children[0] as ScrollViewer;
            var errorListText = errorListScroll.Content as TextBlock;

            var tabitem3 = tabControl.Items.GetItemAt(2) as TabItem;
            var fullReportScroll = mvdCheckerAndFilterControl.StackPanel3.Children[0] as ScrollViewer;
            var fullReportText = fullReportScroll.Content as TextBlock;


            bool attributeRuleMatch = true;
            string attributeRuleName = attributeRule.attributeName;//Error: attribute = RelatingElement

            if (transportingEntity.GetType().GetProperty(attributeRuleName) == null
                || transportingEntity.GetType().GetProperty(attributeRuleName).GetValue(transportingEntity) == null)
            {
                errorListText.Text += "Attribute " + attributeRuleName + " is missing!\r\n";
                attributeRuleMatch = false;
            }
            else
            {
                fullReportText.Text+= "AttributeRule " + attributeRuleName + " matches!\r\n";

            }
           

            List<EntityRule> allEntityRules = attributeRule.getEntityRules();

            if (allEntityRules.Any()&& attributeRuleMatch)
            {
                foreach (EntityRule entityRule in allEntityRules)
                {
                    string entityName = entityRule.entityName;//IfcMaterialLayerSetUsage,IfcMaterialLayerSet
                    Type TypeOfEntityRuleInMvd;

                    if (IfcVersionType.Name == "ModelInfoIFC2x3")
                    {
                        TypeOfEntityRuleInMvd = getTypeByNameFromIfc2x3(entityName)[0];//IfcMaterialLayerSetUsage,IfcMaterialLayerSet
                    }
                    else
                    {
                        TypeOfEntityRuleInMvd = getTypeByNameFromIfc4(entityName)[0];
                    }
                  



                    var IfcProperties = transportingEntity.GetType().GetProperties();

                    //loop over attribute rule types


                    Type[] interfaces = TypeOfEntityRuleInMvd.GetInterfaces();

                    if (interfaces != null)
                    {
                        foreach (Type item in interfaces)
                        {
                            if (IfcProperties.FirstOrDefault(a => a.PropertyType == item) != null)
                            {
                                PropertyInfo IfcPropertyNeeded = IfcProperties.FirstOrDefault(a => a.PropertyType == item);
                                var obj = IfcPropertyNeeded.GetValue(transportingEntity);
                                
                                if (obj.GetType() == TypeOfEntityRuleInMvd)
                                {
                                    fullReportText.Text += "EntityRule " + entityName + " matches!\r\n";
                                    if(CheckEntityRule(entityRule, obj))
                                    {
                                        
                                    }
                                    else
                                    {
                                        attributeRuleMatch = false;
                                        
                                    }
                                    
                                   
                                }
                                break;
                            }
                        }
                    }

                     if (IfcProperties.FirstOrDefault(a => a.PropertyType == TypeOfEntityRuleInMvd) != null)
                    {
                        PropertyInfo IfcPropertyNeeded = IfcProperties.FirstOrDefault(a => a.PropertyType == TypeOfEntityRuleInMvd);//relating building element
                        var obj = IfcPropertyNeeded.GetValue(transportingEntity);

                        if (obj.GetType() == TypeOfEntityRuleInMvd)
                        {
                            fullReportText.Text += "EntityRule " + entityName + " matches!\r\n";
                            if (CheckEntityRule(entityRule, obj))
                            {
                                
                            }
                            else
                            {
                                attributeRuleMatch = false;
                                

                            }

                        }
                        else if (transportingEntity.GetType().GetRuntimeProperties().FirstOrDefault(a => a.Name == attributeRuleName) != null)
                        {
                            PropertyInfo IfcPropertyNeeded2 = transportingEntity.GetType().GetRuntimeProperties().FirstOrDefault(a => a.Name == attributeRuleName);
                            var obj2 = IfcPropertyNeeded2.GetValue(transportingEntity);
                            if (obj2 != null)
                            {
                                fullReportText.Text += "EntityRule " + entityName + " matches!\r\n";
                                if (CheckEntityRule(entityRule, obj))
                                {

                                }
                                else
                                {
                                    attributeRuleMatch = false;

                                }

                            }
                        }
                        else
                        {
                            attributeRuleMatch = false;
                            errorListText.Text += "EntityRule " + entityName + " is missing!\r\n";
                            continue;
                        }


                    }

   

                }
            }

            return attributeRuleMatch;
        }

        public bool CheckEntityRule(EntityRule entityRule,object transportingEntity)
        {
            bool entityRuleMatch = true;
            string entityName = entityRule.entityName;

            List<AttributeRule> allAttributeRules = entityRule.getAttributeRules();
            if (allAttributeRules.Any())
            {
                foreach (var item in allAttributeRules)
                {
                    if(CheckAttributeRule(item, transportingEntity))
                    {
                        continue;
                    }
                    else
                    {
                        entityRuleMatch = false;
                        continue;
                    }
                   
                }
            }
            return entityRuleMatch;
        }
        public override Node Clone()
        {
            return new MvdCheckerAndFilter(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}


