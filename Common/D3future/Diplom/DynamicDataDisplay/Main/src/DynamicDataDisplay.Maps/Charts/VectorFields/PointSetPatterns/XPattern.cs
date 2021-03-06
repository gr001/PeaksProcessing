﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Microsoft.Research.DynamicDataDisplay.Maps.Charts.VectorFields.Convolution
{
	public sealed class XPattern : PointSetPattern
	{
		public override IEnumerable<Point> GeneratePoints()
		{
			int halfCount = PointsCount / 2;
			double xDelta = 1.0 / (halfCount + 1);
			double yDelta = 1.0 / (halfCount + 1);

			for (int i = 0; i < halfCount; i++)
			{
				yield return new Point((i + 0.5) * xDelta, (i + 0.5) * yDelta);
			}

			for (int i = 0; i < halfCount; i++)
			{
				yield return new Point((i + 0.5) * xDelta, 1.0 - (i + 0.5) * yDelta);
			}
		}
	}
}
