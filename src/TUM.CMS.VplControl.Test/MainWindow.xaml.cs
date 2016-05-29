using System.Linq;
using System.Reflection;
using System.Windows;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.Utilities;
using TUM.CMS.VplControl.Watch3D.Nodes;
using TUM.CMS.VPL.Scripting.Nodes;
using TUM.CMS.VplControl.Energy.Nodes;
using System.Diagnostics;
using System.Windows.Controls;

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

            
            VplControl.ExternalNodeTypes.Add(typeof(ScriptingNode));
            // VplControl.ExternalNodeTypes.Add(typeof(Watch3DNode));

            VplControl.NodeTypeMode = NodeTypeModes.All;


            VplPropertyGrid.SelectedObject = VplControl;
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

       
    }
}