﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Microsoft.Research.DynamicDataDisplay.Charts.NewLine;
using Microsoft.Research.DynamicDataDisplay.Markers.DataSources.DataSourceFactories;

namespace DynamicDataDisplay.Markers.DataSources.DataSourceFactories
{
	public sealed class DataSourceFactoryStore
	{
		private DataSourceFactoryStore()
		{
			RegisterFactory(new StringFactory());
			RegisterFactory(new DoubleLambdaDataSourceFactory());
			RegisterFactory(new DataSourceDataSourceFactory());
			RegisterFactory(new PointArrayFactory());
			RegisterFactory(new IEnumerablePointFactory());
			RegisterFactory(new GenericIListFactory());
			RegisterFactory(new GenericIEnumerableFactory());
			RegisterFactory(new GenericIDataSource2DFactory());
			RegisterFactory(new XmlElementFactory());
			RegisterFactory(new EnumerableDataSourceFactory());
		}

		private static DataSourceFactoryStore current = new DataSourceFactoryStore();
		public static DataSourceFactoryStore Current { get { return current; } }

		private readonly ObservableCollection<DataSourceFactory> factories = new ObservableCollection<DataSourceFactory>();
		public ObservableCollection<DataSourceFactory> Factories
		{
			get { return factories; }
		} 

		public void RegisterFactory(DataSourceFactory factory)
		{
			if (factory == null)
				throw new ArgumentNullException("factory");

			factories.Add(factory);
		}

		public PointDataSourceBase BuildDataSource(object data)
		{
			if (data == null)
				throw new ArgumentNullException("data");

			foreach (var factory in factories)
			{
				var result = factory.TryBuild(data);
				if (result != null)
					return result;
			}

			return null;
		}
	}
}
