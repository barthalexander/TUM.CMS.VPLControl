using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.IFC.Controls;
using TUM.CMS.VplControl.IFC.Utilities.mvdXMLReaderClasses;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class MvdTreeNode : Node
    {

        public MvdTreeNode(Core.VplControl hostCanvas) : base(hostCanvas)
        {
            IsResizeable = true;

            MVDTreeControle mvdTreeControle = new MVDTreeControle();

            mvdTreeControle.ScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            mvdTreeControle.ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            mvdTreeControle.ScrollViewer.CanContentScroll = true;

            mvdTreeControle.Title.Content = "MVD Tree Node";
            AddInputPortToNode("Object", typeof(object));

            AddControlToNode(mvdTreeControle);
        }


        /// <summary>
        /// Visualize the mvdXML file in a tree
        /// 
        /// Using classes in the utilities folder
        /// </summary>
        public override void Calculate()
        {
            if (InputPorts[0].Data != null)
            {
                var modelView = (ModelView)InputPorts[0].Data;
                var mvdTreeControle = ControlElements[0] as MVDTreeControle;
                TreeView treeView = new TreeView();
                mvdTreeControle.MinHeight = 100;
                mvdTreeControle.MinWidth = 400;
                mvdTreeControle.ScrollViewer.MinWidth = 400;
                mvdTreeControle.ScrollViewer.MinHeight = 100;
                mvdTreeControle.ScrollViewer.MaxWidth = 600;
                mvdTreeControle.ScrollViewer.MaxHeight = 600;

                mvdTreeControle.ScrollViewer.Content = treeView;
                if (treeView == null) return;

                TreeViewItem treeViewItem = new TreeViewItem();
                treeViewItem = PrintModelView(modelView);

                treeView.Items.Add(treeViewItem);
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
        
        /// <summary>
        /// Prints the ModelView
        /// 
        /// </summary>
        /// <param name="modelView"></param>
        /// <returns></returns>
        public TreeViewItem PrintModelView(ModelView modelView)
        {
            TreeViewItem treeViewItem = new TreeViewItem();
            treeViewItem.Header = "ModelView - " + modelView.name;
            if (modelView.uuid != "")
                treeViewItem.Items.Add("UUID: " + modelView.uuid);
            if (modelView.version != "")
                treeViewItem.Items.Add("Version: " + modelView.version);
            if (modelView.applicableSchema != "")
                treeViewItem.Items.Add("applicableSchema: " + modelView.applicableSchema);

            Dictionary<string, ConceptRoot> allConceptRoots = modelView.GetAllConceptRoots();
            if (allConceptRoots.Count > 0)
            {
                TreeViewItem mvdRootTreeViewItem = new TreeViewItem();
                mvdRootTreeViewItem.Header = "ConceptRoots";

                foreach (var conceptRoot in allConceptRoots)
                {
                    TreeViewItem conceptRootItem = new TreeViewItem();
                    conceptRootItem = PrintConceptRoot(conceptRoot.Value);
                    mvdRootTreeViewItem.Items.Add(conceptRootItem);
                }

                treeViewItem.Items.Add(mvdRootTreeViewItem);
            }

            Dictionary<string, ExchangeRequirement> allExchangeRequirements = modelView.GetAllExchangeRequirements();
            if (allExchangeRequirements.Count > 0)
            {
                TreeViewItem mvdExchangeTreeViewItem = new TreeViewItem();
                mvdExchangeTreeViewItem.Header = "ExchangeRequirements";
                foreach (var exchangeRequirement in allExchangeRequirements)
                {
                    TreeViewItem exchangeRequirementItem = new TreeViewItem();
                    exchangeRequirementItem = PrintExchangeRequirement(exchangeRequirement.Value);
                    mvdExchangeTreeViewItem.Items.Add(exchangeRequirementItem);
                }
                treeViewItem.Items.Add(mvdExchangeTreeViewItem);
            }
            return treeViewItem;
        }

        /// <summary>
        /// Print the exchange requirements
        /// 
        /// </summary>
        /// <param name="exchangeRequirement"></param>
        /// <returns></returns>
        private TreeViewItem PrintExchangeRequirement(ExchangeRequirement exchangeRequirement)
        {
            TreeViewItem treeViewItem = new TreeViewItem();
            treeViewItem.Header = "ExchangeRequirement - " + exchangeRequirement.name;
            if (exchangeRequirement.uuid != "") treeViewItem.Items.Add("UUID: " + exchangeRequirement.uuid);
            if (exchangeRequirement.version != "") treeViewItem.Items.Add("Version: " + exchangeRequirement.version);

            return treeViewItem;

        }

        /// <summary>
        /// Print the concept root
        /// 
        /// </summary>
        /// <param name="conceptRoot"></param>
        /// <returns></returns>
        public TreeViewItem PrintConceptRoot(ConceptRoot conceptRoot)
        {
            TreeViewItem treeViewItem = new TreeViewItem();
            treeViewItem.Header = "ConceptRoot - " + conceptRoot.name;
            if (conceptRoot.uuid != "") treeViewItem.Items.Add("UUID: " + conceptRoot.uuid);
            if (conceptRoot.applicableRootEntity != "")
                treeViewItem.Items.Add("applicableRootEntity: " + conceptRoot.applicableRootEntity);

            Dictionary<string, Concept> allConcepts = conceptRoot.GetAllConcepts();
            if (allConcepts.Count > 0)
            {
                TreeViewItem conceptItem = new TreeViewItem();
                conceptItem.Header = "Concepts";
                foreach (var item in allConcepts)
                {
                    var treeViewItem2 = PrintConcept(item.Value);
                    conceptItem.Items.Add(treeViewItem2);
                }
                treeViewItem.Items.Add(conceptItem);
            }
            return treeViewItem;
        }

        /// <summary>
        /// Print the concept
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private TreeViewItem PrintConcept(Concept value)
        {
            TreeViewItem treeViewItem = new TreeViewItem();
            treeViewItem.Header = "Concept - " + value.name;
            if (value.uuid != "") treeViewItem.Items.Add("UUID: " + value.uuid);
            if (value.Override != "") treeViewItem.Items.Add("Override: " + value.Override);

            ConceptTemplate conceptTemplate = value.getConceptTemplate();
            if (conceptTemplate.name != "")
                treeViewItem.Items.Add(PrintConceptTemplate(conceptTemplate));

            Dictionary<string, Requirement> allRequirements = value.GetAllRequirements();
            if (allRequirements.Count > 0)
            {
                TreeViewItem requirementsItems = new TreeViewItem();
                requirementsItems.Header = "Requirements";

                foreach (var item in allRequirements)
                {
                    TreeViewItem treeViewItem1 = new TreeViewItem();
                    treeViewItem1 = PrintRequirement(item.Value);
                    requirementsItems.Items.Add(treeViewItem1);
                }
                treeViewItem.Items.Add(requirementsItems);
            }

            Dictionary<string, Concept> allSubConcepts = value.GetAllSubConcepts();
            if (allSubConcepts.Count > 0)
            {
                TreeViewItem subConceptsItems = new TreeViewItem();
                subConceptsItems.Header = "SubConcepts";
                foreach (var item in allSubConcepts)
                {
                    TreeViewItem treeViewItem1 = new TreeViewItem();
                    treeViewItem1 = PrintConcept(item.Value);
                    subConceptsItems.Items.Add(treeViewItem1);
                }
                treeViewItem.Items.Add(subConceptsItems);
            }
            return treeViewItem;
        }

        /// <summary>
        /// Print the requirements
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private TreeViewItem PrintRequirement(Requirement value)
        {
            TreeViewItem treeViewItem = new TreeViewItem();
            treeViewItem.Header = "Requirement";
            if (value.applicability != "") treeViewItem.Items.Add("applicability: " + value.applicability);
            if (value.requirement != "") treeViewItem.Items.Add("requirement: " + value.requirement);
            if (value.exchangeRequirement.name != "")
            {
                treeViewItem.Items.Add(PrintExchangeRequirement(value.exchangeRequirement));
            }

            return treeViewItem;
        }

        /// <summary>
        /// Print the concept template
        /// 
        /// </summary>
        /// <param name="conceptTemplate"></param>
        /// <returns></returns>
        private TreeViewItem PrintConceptTemplate(ConceptTemplate conceptTemplate)
        {
            TreeViewItem treeViewItem = new TreeViewItem();
            treeViewItem.Header = "ConceptTemplate - " + conceptTemplate.name;
            if (conceptTemplate.applicableEntity != "")
                treeViewItem.Items.Add("applicableEntity: " + conceptTemplate.applicableEntity);
            if (conceptTemplate.applicableSchema != "")
                treeViewItem.Items.Add("applicableSchema: " + conceptTemplate.applicableSchema);
            if (conceptTemplate.code != "") treeViewItem.Items.Add("code: " + conceptTemplate.code);


            Dictionary<string, ConceptTemplate> allConceptTemplates = conceptTemplate.getConceptTemplateses();
            if (allConceptTemplates.Count > 0)
            {
                TreeViewItem subConceptItems = new TreeViewItem();
                subConceptItems.Header = "SubConceptTemplates";

                foreach (var item in allConceptTemplates)
                {
                    TreeViewItem treeViewItem1 = new TreeViewItem();
                    treeViewItem1 = PrintConceptTemplate(item.Value);
                    subConceptItems.Items.Add(treeViewItem1);
                }
                treeViewItem.Items.Add(subConceptItems);
            }


            List<Templates> allSubTemplates = conceptTemplate.getAllSubTemplates();
            if (allSubTemplates.Any())
            {
                TreeViewItem subTemplatesItems = new TreeViewItem();
                subTemplatesItems.Header = "SubTemplates";

                foreach (var item in allSubTemplates)
                {
                    TreeViewItem treeViewItem1 = new TreeViewItem();
                    treeViewItem1 = PrintTemplate(item);
                    subTemplatesItems.Items.Add(treeViewItem1);
                }
                treeViewItem.Items.Add(subTemplatesItems);
            }

            List<AttributeRule> allAttributeRules = conceptTemplate.getAttributeRules();
            if (allAttributeRules.Any())
            {
                TreeViewItem attributeRulesItems = new TreeViewItem();
                attributeRulesItems.Header = "AttributeRules";


                foreach (var item in allAttributeRules)
                {
                    TreeViewItem treeViewItem1 = new TreeViewItem();
                    treeViewItem1 = PrintAttribute(item);
                    attributeRulesItems.Items.Add(treeViewItem1);
                }
                treeViewItem.Items.Add(attributeRulesItems);
            }
            
            return treeViewItem;
        }

        /// <summary>
        /// Print the attribute
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private TreeViewItem PrintAttribute(AttributeRule item)
        {
            TreeViewItem attributeItem = new TreeViewItem();
            attributeItem.Header = "Attribute - " + item.attributeName;
            if (item.cardinality != "") attributeItem.Items.Add("Cardinality: " + item.cardinality);

            List<EntityRule> allEntityRules = item.getEntityRules();
            if (allEntityRules.Any())
            {
                TreeViewItem entityRulesItems = new TreeViewItem();
                entityRulesItems.Header = "EntityRules";
                foreach (var item1 in allEntityRules)
                {
                    TreeViewItem treeViewItem1 = new TreeViewItem();
                    treeViewItem1 = PrintEntityRule(item1);
                    entityRulesItems.Items.Add(treeViewItem1);
                }
                attributeItem.Items.Add(entityRulesItems);
            }
            return attributeItem;
        }

        /// <summary>
        /// Print the entity rule
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private TreeViewItem PrintEntityRule(EntityRule item)
        {
            TreeViewItem entityRuleItem = new TreeViewItem();
            entityRuleItem.Header = "EntityRule- " + item.entityName;
            if (item.cardinality != "") entityRuleItem.Items.Add("Cardinality: " + item.cardinality);

            List<AttributeRule> allAttributeRules = item.getAttributeRules();
            if (allAttributeRules.Any())
            {
                TreeViewItem AttributesItems = new TreeViewItem();
                AttributesItems.Header = "AttributesRules";

                foreach (var item1 in allAttributeRules)
                {
                    TreeViewItem treeViewItem1 = new TreeViewItem();
                    treeViewItem1 = PrintAttribute(item1);
                    AttributesItems.Items.Add(treeViewItem1);
                }
                entityRuleItem.Items.Add(AttributesItems);
            }
            return entityRuleItem;
        }

        /// <summary>
        /// Print the template
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private TreeViewItem PrintTemplate(Templates item)
        {
            TreeViewItem subConceptItems = new TreeViewItem();
            subConceptItems.Header = "SubConceptTemplates";

            Dictionary<string, ConceptTemplate> allConceptTemplates = item.getConceptTemplates();
            if (allConceptTemplates.Count > 0)
            {
                foreach (var item1 in allConceptTemplates)
                {
                    TreeViewItem treeViewItem1 = new TreeViewItem();
                    treeViewItem1 = PrintConceptTemplate(item1.Value);
                    subConceptItems.Items.Add(treeViewItem1);
                }
            }
            return subConceptItems;
        }
    }
}