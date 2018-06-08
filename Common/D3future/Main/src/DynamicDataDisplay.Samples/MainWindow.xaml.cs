﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Research.DynamicDataDisplay.Samples.Internals.Models;
using Microsoft.Research.DynamicDataDisplay.Samples.Internals.Views;
using System.Diagnostics;
using System.Threading;
using System.Globalization;

namespace Microsoft.Research.DynamicDataDisplay
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
			InitializeComponent();
		}
	}
}
