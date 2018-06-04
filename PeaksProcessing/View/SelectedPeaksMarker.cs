using Microsoft.Research.DynamicDataDisplay.PointMarkers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PeaksProcessing
{
    public class SelectedPeaksMarker : CircleElementPointMarker
    {
        public ContextMenu ContextMenu { get; internal set; }

        public override UIElement CreateMarker()
        {
            var retElement = base.CreateMarker() as FrameworkElement;

            retElement.DataContext = this.DataContext;
            retElement.ContextMenu = this.ContextMenu;
            retElement.MouseRightButtonDown += RetElement_MouseRightButtonDown;
            return retElement;
        }

        private void RetElement_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var element = sender as FrameworkElement;

            if (element != null)
            {
                this.ContextMenu.DataContext = element.DataContext;
                this.ContextMenu.IsOpen = true;
            }
        }

        public override void SetMarkerProperties(UIElement marker)
        {
            base.SetMarkerProperties(marker);

            var element = marker as FrameworkElement;

            element.DataContext = this.DataContext;
            element.ContextMenu = this.ContextMenu;

            if (element.ContextMenu != null)
                element.ContextMenu.DataContext = this.DataContext;
        }
    }
}
