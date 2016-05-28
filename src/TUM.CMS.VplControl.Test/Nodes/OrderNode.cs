using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using TUM.CMS.VplControl.Core;

namespace TUM.CMS.VplControl.Test.Nodes
{
    public class OrderNode : Node
    {
        public OrderNode(Core.VplControl hostCanvas) : base(hostCanvas)
        {
            AddInputPortToNode("Object", typeof(double));
            AddInputPortToNode("Object", typeof(double));
            AddInputPortToNode("Object", typeof(double));


            var textBlock = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                FontSize = 14,
                Padding = new Thickness(5),
                IsHitTestVisible = false
            };

            var scrollViewer = new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                MinWidth = 120,
                MinHeight = 120,
                MaxWidth = 200,
                MaxHeight = 400,
                CanContentScroll = true,
                Content = textBlock
                //IsHitTestVisible = false
            };


            AddControlToNode(scrollViewer);

        }


        public override void Calculate()
        {
            if (InputPorts[0] == null || ControlElements[0] == null) return;

            var scrollViewer = ControlElements[0] as ScrollViewer;
            if (scrollViewer == null) return;

            var textBlock = scrollViewer.Content as TextBlock;
            if (textBlock == null) return;

            if (InputPorts[0].Data == null || InputPorts[1].Data == null || InputPorts[2].Data == null)
                textBlock.Text = "null";
            else
            {
                textBlock.Text = "";

                int[] array = new int[3];

                array[0] = int.Parse(InputPorts[0].Data.ToString());
                array[1] = int.Parse(InputPorts[1].Data.ToString());
                array[2] = int.Parse(InputPorts[2].Data.ToString());

                Array.Sort(array);

                foreach (int i in array) textBlock.Text += i + "; ";
            }
        }



        public override Node Clone()
        {
            return new OrderNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}