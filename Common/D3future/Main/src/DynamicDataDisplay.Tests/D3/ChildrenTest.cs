﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Research.DynamicDataDisplay;
using System.Reflection;
using Microsoft.Research.DynamicDataDisplay.Charts.Navigation;
using Microsoft.Research.DynamicDataDisplay.Charts.Maps;
using DynamicDataDisplay.Test.Common;
using Microsoft.Research.DynamicDataDisplay.Common;
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay.Common.Auxiliary;
using System.Windows.Threading;

namespace DynamicDataDisplay.Test
{
	[TestClass]
	public class ChildrenTest
	{

		private List<Assembly> testedAssemblies;

		public ChildrenTest()
		{
			testedAssemblies = (from assemblyName in Assembly.GetExecutingAssembly().GetReferencedAssemblies()
								// D3 assemblies currently has version 0.*
								where assemblyName.FullName != typeof(ChildrenTest).Assembly.FullName
								where assemblyName.Name.StartsWith("DynamicDataDisplay")
								where !assemblyName.Name.EndsWith("Accessor")
								select Assembly.Load(assemblyName)).ToList();
		}

		public TestContext TestContext { get; set; }

		[TestMethod]
		public void TestAddingNull()
		{
			ChartPlotter plotter = new ChartPlotter();
			bool thrown = false;
			try
			{
				plotter.Children.Add(null);
			}
			catch (ArgumentNullException)
			{
				thrown = true;
			}

			Assert.IsTrue(thrown);
		}

		[TestMethod]
		public void TestAllElementsAddRemove()
		{
			ChartPlotter plotter = new ChartPlotter();
			plotter.PerformLoad();

			var types = GetAllCharts();

			var withoutCtor = from type in types
							  let ctors = type.GetConstructors()
							  let noParameterlessCtor = (from c in ctors
														 let p = c.GetParameters()
														 select p).All(p => p.Length >= 1)
							  where noParameterlessCtor
							  select type;

			var plotterElements = new List<IPlotterElement>();
			plotter.Children.Clear();
			foreach (var type in types)
			{
				IPlotterElement element = (IPlotterElement)Activator.CreateInstance(type);
				plotterElements.Add(element);
				plotter.Children.Add(element);
			}

			foreach (var item in plotterElements)
			{
				Assert.AreEqual(plotter, item.Plotter);
			}

			plotter.Children.Clear();
			plotter.Wait(DispatcherPriority.Background);

			foreach (var item in plotterElements)
			{
				Assert.IsNull(item.Plotter, item.ToString());
			}
		}

		[TestMethod]
		public void CheckThatSegmentHasProperPlotter()
		{
			ChartPlotter plotter = new ChartPlotter();
			plotter.PerformLoad();
			Segment segment = new Segment();

			plotter.Children.Add(segment);

			Assert.AreEqual(plotter, segment.Plotter);
		}

		[TestMethod]
		public void CheckThatSegmentHasNullPlotterAfterDisconnection()
		{
			ChartPlotter plotter = new ChartPlotter();
			Segment segment = new Segment();

			plotter.Children.Add(segment);
			plotter.PerformLoad();
			plotter.Wait(DispatcherPriority.Background);

			plotter.Children.Remove(segment);
			plotter.Wait(DispatcherPriority.Background);

			Assert.IsNull(segment.Plotter);
		}

		private List<Type> GetAllCharts()
		{
			Type elementType = typeof(IPlotterElement);

			var types = from assembly in testedAssemblies
						where assembly != typeof(ChildrenTest).Assembly
						from type in assembly.GetExportedTypes()
						where !type.IsDefined(typeof(SkipPropertyCheckAttribute), true)
						where elementType.IsAssignableFrom(type) && !type.IsAbstract && type.IsPublic
						where !elementType.Name.EndsWith("Accessor")
						let ctors = type.GetConstructors()
						let hasParameterlessCtor = (from ctor in ctors
													where ctor.GetParameters().Length == 0
													select ctor).Any()
						where hasParameterlessCtor
						where type.Assembly != typeof(ChildrenTest).Assembly
						select type;

			var list = types.ToList();

			Assert.IsTrue(list.Count > 0, "List of IPlotterElement, loaded by reflection from referenced assemblies, should not be empty.");

			return list;
		}

		private List<Type> GetAllExportedClasses()
		{
			var types = from assembly in testedAssemblies
						from type in assembly.GetExportedTypes()
						where !type.IsAbstract && type.IsPublic && !type.ContainsGenericParameters
						let ctor = type.GetConstructor(new Type[] { })
						where ctor != null
						where type.Assembly != typeof(ChildrenTest).Assembly
						select type;

			var list = types.ToList();

			Assert.IsTrue(list.Count > 0, "List of exported classes, loaded by reflection from referenced assemblies, should not be empty.");

			return list;
		}

		[TestMethod]
		public void SettingSameValueAsWasGot()
		{
			var types = GetAllExportedClasses();
			var properties = from type in types
							 let typeProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
							 let ownProperties = from prop in typeProperties
												 where prop.CanWrite && prop.CanRead
												 where testedAssemblies.Contains(prop.DeclaringType.Assembly)
												 select prop
							 select new
							 {
								 Type = type,
								 Properties = ownProperties.ToArray()
							 };

			var propertiesList = properties.ToList();

			var charts = (from item in propertiesList
						  where !item.Type.IsDefined(typeof(SkipPropertyCheckAttribute), false)
						  let instance = Activator.CreateInstance(item.Type)
						  select new { Instance = instance, Properties = item.Properties }).ToList();

			// setting the same value to property as was stored in it
			foreach (var chart in charts)
			{
				var instance = chart.Instance;
				foreach (var property in chart.Properties)
				{
					try
					{
						var value = property.GetValue(instance, null);
						property.SetValue(instance, value, null);
					}
					catch (ArgumentNullException) { }
					catch (TargetInvocationException) { }
					catch (NotImplementedException) { }
				}
			}

			PropertySetSystem system = new PropertySetSystem();
			// setting custom values
			foreach (var chart in charts)
			{
				var instance = chart.Instance;
				foreach (var property in chart.Properties)
				{
					system.TrySetValue(instance, property);
				}
			}
		}

		[TestMethod]
		public void SetLegendPanelTemplate()
		{
			ChartPlotter plotter = new ChartPlotter();

			var template = plotter.LegendPanelTemplate;
			plotter.LegendPanelTemplate = template;
		}

		[TestMethod]
		public void SettingPropertiesBeforeAddingToPlotter()
		{
			var types = GetAllCharts();
			var properties = from type in types
							 let typeProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
							 let ownProperties = from prop in typeProperties
												 where prop.CanWrite && prop.CanRead
												 where testedAssemblies.Contains(prop.DeclaringType.Assembly)
												 select prop
							 select new
							 {
								 Type = type,
								 Properties = ownProperties.ToArray()
							 };

			var propertiesList = properties.ToList();

			var charts = (from item in propertiesList
						  let instance = Activator.CreateInstance(item.Type)
						  select new { Instance = instance, Properties = item.Properties }).ToList();

			PropertySetSystem system = new PropertySetSystem();
			// setting custom values
			foreach (var chart in charts)
			{
				var instance = chart.Instance;
				foreach (var property in chart.Properties)
				{
					system.TrySetValue(instance, property);
				}
			}

			ChartPlotter plotter = new ChartPlotter();
			plotter.Children.Clear();

			foreach (var chart in charts)
			{
				plotter.Children.Add(chart.Instance as IPlotterElement);
			}

			foreach (var chart in charts)
			{
				var instance = chart.Instance;
				foreach (var property in chart.Properties)
				{
					system.TrySetValue(instance, property);
				}
			}
		}

		private Type GetExternalBaseType(Type type)
		{
			Type baseType = type.BaseType;
			while (testedAssemblies.Contains(baseType.Assembly))
			{
				baseType = baseType.BaseType;
			}
			return baseType;
		}

		[TestMethod]
		public void AddingAndRemovalOfChartWithoutLoadingPlotter()
		{
			ChartPlotter plotter = new ChartPlotter();
			LineGraph chart = new LineGraph();

			plotter.Children.Add(chart);

			Assert.IsTrue(plotter.Children.Contains(chart));

			plotter.Children.Remove(chart);

			Assert.IsNull(chart.Plotter);
			Assert.IsFalse(plotter.Children.Contains(chart));

			plotter.Children.Add(chart);

			Assert.IsTrue(plotter.Children.Contains(chart));
		}
	}
}
