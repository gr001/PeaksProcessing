﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PeaksProcessing
{
    /// <summary>
    /// Interaction logic for ViewSettings.xaml
    /// </summary>
    public partial class ViewSettingsWindow : Window
    {
        public PeaksProcessing.Settings.GlobalDetectionSettings GlobalDetectionSettings { get { return Settings.Settings.Instance.GlobalDetectionSettings; } }
        public PeaksProcessing.Settings.ViewSettings ViewSettings {  get { return Settings.Settings.Instance.ViewSettings; } }
        public DataViewModel DataViewModel {  get { return DataViewModel.Instance; } }
        public ViewSettingsWindow()
        {
            InitializeComponent();
        }
    }
}
