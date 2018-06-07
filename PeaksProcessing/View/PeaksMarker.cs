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
    public class PeaksMarker : ShapeElementPointMarker
    {
        public ContextMenu ContextMenu { get; internal set; }

        public override UIElement CreateMarker()
        {
            var center = Size * 0.5;
            PathFigure myPathFigure = new PathFigure();
            myPathFigure.StartPoint = new Point(0, center);
            myPathFigure.Segments.Add(new LineSegment(new Point(Size, center), true /* IsStroked */ ));

            /// Create a PathGeometry to contain the figure.
            PathGeometry myPathGeometry = new PathGeometry();
            myPathGeometry.Figures.Add(myPathFigure);


            myPathFigure = new PathFigure();
            myPathFigure.StartPoint = new Point(center, 0);
            myPathFigure.Segments.Add(new LineSegment(new Point(center, Size), true /* IsStroked */ ));
            myPathGeometry.Figures.Add(myPathFigure);
            //myPathGeometry.AddGeometry(new RectangleGeometry() { Rect = new Rect(0, 0, Size, Size) });


            // Display the PathGeometry. 
            Path myPath = new Path();
            myPath.Stroke = Brush;
            myPath.StrokeThickness = 1;
            myPath.Data = myPathGeometry;

            Border retElement = new Border() { Child = myPath, Background = Brushes.Transparent };

            if (!String.IsNullOrEmpty(ToolTipText))
            {
                //ToolTip tt = new ToolTip();
                //tt.Content = ToolTipText;
                retElement.ToolTip = ToolTipText;
            }

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
            //(marker as Border).ToolTip = new ToolTip() { Content = ToolTipText };
            //return;
            //Path myPath = marker as Path;

            //PathFigure myPathFigure = new PathFigure();
            //myPathFigure.StartPoint = new Point(-10, 0);
            //myPathFigure.Segments.Add(new LineSegment(new Point(10, 0), false /* IsStroked */ ));

            ///// Create a PathGeometry to contain the figure.
            //PathGeometry myPathGeometry = new PathGeometry();
            //myPathGeometry.Figures.Add(myPathFigure);


            //myPathFigure = new PathFigure();
            //myPathFigure.StartPoint = new Point(0, -10);
            //myPathFigure.Segments.Add(new LineSegment(new Point(0, 10), false /* IsStroked */ ));

            //myPath.Stroke = Brush;
            //myPath.StrokeThickness = 3;
            //myPath.Data = myPathGeometry;

            //System.Windows.Shapes.Path path = new Path();
            ////path.
            //Ellipse ellipse = (Ellipse)marker;

            //ellipse.Width = Size;
            //ellipse.Height = Size;
            //ellipse.Stroke = Brush;
            //ellipse.Fill = Fill;

            if (!String.IsNullOrEmpty(ToolTipText))
            {
                //ToolTip tt = new ToolTip();
                //tt.Content = ToolTipText;
                (marker as Border).ToolTip = ToolTipText;
            }

            (marker as Border).DataContext = this.DataContext;
            (marker as Border).ContextMenu = this.ContextMenu;

            if ((marker as Border).ContextMenu != null)
                (marker as Border).ContextMenu.DataContext = this.DataContext;
        }

        public override void SetPosition(UIElement marker, Point screenPoint)
        {
            Canvas.SetLeft(marker, screenPoint.X - Size / 2);
            Canvas.SetTop(marker, screenPoint.Y - Size / 2);
        }
    }
}
