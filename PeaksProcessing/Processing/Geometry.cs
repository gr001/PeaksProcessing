using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PeaksProcessing.Processing
{
    public sealed class Geometry
    {
        public static double PolygonArea(IList<Point> polygonVertices)
        {
            if (polygonVertices == null || polygonVertices.Count < 2)
                return 0;

            int verticesCount = polygonVertices.Count;
            double area = 0;
            int i = 0;
            Point point, nextPoint;
            while (i < verticesCount - 1)
            {
                point = polygonVertices[i++];
                nextPoint = polygonVertices[i];
                area += point.X * nextPoint.Y - nextPoint.X * point.Y;
            }

            point = polygonVertices[i++];
            nextPoint = polygonVertices[0];
            area += point.X * nextPoint.Y - nextPoint.X * point.Y;

            return area * 0.5;
        }

        public static double PolylineAreaIntegral(IList<Point> polygonVertices)
        {
            if (polygonVertices == null || polygonVertices.Count < 2)
                return 0;

            double area = 0;
            for(int i=1; i<polygonVertices.Count; i++)
            {
                var p0 = polygonVertices[i-1];
                var p1 = polygonVertices[i];

                area += (p1.X - p0.X) * (p0.Y + p1.Y) * 0.5;
            }

            return area;
        }
    }
}
