using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.Utilities;
using TUM.CMS.VplControl.Watch3D.Nodes;
using TUM.CMS.VPL.Scripting.Nodes;
using TUM.CMS.VplControl.Energy.Nodes;
using TUM.CMS.VplControl.IFC.Nodes;
using System.Diagnostics;
using System.Windows.Controls;
using PropertyTools.Wpf;

namespace TUM.CMS.VplControl.Test
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            KeyDown += VplControl.VplControl_KeyDown;
            KeyUp += VplControl.VplControl_KeyUp;

            VplControl.ExternalNodeTypes.AddRange(
                ClassUtility.GetTypesInNamespace(Assembly.GetExecutingAssembly(), "TUM.CMS.VplControl.Test.Nodes")
                    .ToList());

            VplControl.ExternalNodeTypes.AddRange(
             ClassUtility.GetTypesInNamespace(Assembly.GetAssembly(typeof(Watch3DNode)), "TUM.CMS.VplControl.Watch3D.Nodes")
                     .ToList());

            VplControl.ExternalNodeTypes.AddRange(
                ClassUtility.GetTypesInNamespace(Assembly.GetAssembly(typeof(EnergyNode)), "TUM.CMS.VplControl.Energy.Nodes")
                       .ToList());
            VplControl.ExternalNodeTypes.AddRange(
                ClassUtility.GetTypesInNamespace(Assembly.GetAssembly(typeof(IfcMapsNode)), "TUM.CMS.VplControl.IFC.Nodes")
                       .ToList());
            

            VplControl.ExternalNodeTypes.Add(typeof(ScriptingNode));
            // VplControl.ExternalNodeTypes.Add(typeof(Watch3DNode));

            VplControl.NodeTypeMode = NodeTypeModes.All;

            var group = VplControl.ExternalNodeTypes.GroupBy(u => u.Namespace).Select(grp => grp.ToList()).ToList();
            
            foreach (var item in group)
            {
                var namespaceNameList = item[0].Namespace.Split('.');
                var namespaceName = namespaceNameList[3];
                if (namespaceNameList[3] == "")
                    namespaceName = namespaceNameList[2];

                MenuItem tempItem = new MenuItem();
                tempItem.Header = namespaceName;
                tempItem.Name = namespaceName;
                Nodes.Items.Add(tempItem);
                foreach (var item1 in item)
                {
                    if (!item1.Name.Contains("<"))
                    {
                        MenuItem tempItem1 = new MenuItem();
                        tempItem1.Header = item1.Name;
                        tempItem1.Click += NodeItem_Click;
                        tempItem.Items.Add(tempItem1);
                    }
                   
                }
            }
        }
        public TUM.CMS.VplControl.Core.VplControl HostCanvas { get; private set; }
        private void MenuItem_New_Click(object sender, RoutedEventArgs e)
        {
            VplControl.NewFile();

        }

        private void MenuItem_Open_Click(object sender, RoutedEventArgs e)
        {
            VplControl.OpenFile();

        }

        private void MenuItem_SaveAS_Click(object sender, RoutedEventArgs e)
        {
            VplControl.SaveFile();

        }
        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Do you want to Save your Changes, before Closing?", "Warning", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                VplControl.SaveFile();
                Application.Current.Shutdown();

            }
            else if (result == MessageBoxResult.No)
            {
                Application.Current.Shutdown();
            }
            
            

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/tumcms/TUM.CMS.VPLControl/");
        }

        private void MenuItem_Hide_Click(object sender, RoutedEventArgs e)
        {
            VplPropertyGrid.Visibility = System.Windows.Visibility.Hidden;
            GridSplitter.Visibility = System.Windows.Visibility.Hidden;
            VplControl.Margin = new Thickness(0, 18, -0.4, -0.4);
            colOne.Width    = new GridLength(0);
            colTwo.Width    = new GridLength(0);
            colThree.Width  = new GridLength(0);
            colFour.Width   = new GridLength(0);
        }
        private void MenuItem_Show_Click(object sender, RoutedEventArgs e)
        {
            VplPropertyGrid.Visibility = System.Windows.Visibility.Visible;
            GridSplitter.Visibility = System.Windows.Visibility.Visible;
            VplControl.Margin = new Thickness(4.4, 18, -0.4, -0.4);
            colOne.Width = new GridLength(172.045);
            colTwo.Width = new GridLength(0.045);
            colThree.Width = new GridLength(77.555);
            colFour.Width = new GridLength(5);
        }
        private void NodeItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem obMenuItem = e.OriginalSource as MenuItem;
            // var result = VplControl.ExternalNodeTypes.Find(x => x.Name == obMenuItem.Header.ToString());
            MessageBox.Show(string.Format("{0} just said Hi!", obMenuItem.Header));
        }

    }
}