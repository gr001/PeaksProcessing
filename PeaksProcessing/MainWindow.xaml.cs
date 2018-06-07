using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay.Charts.Navigation;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay.DirectX11;
using Microsoft.Research.DynamicDataDisplay.PointMarkers;
using Microsoft.WindowsAPICodePack.Dialogs;
using PeaksProcessing.Data;
using PeaksProcessing.Processing;
using PeaksProcessing.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PeaksProcessing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public DataViewModel ViewModel { get; } = DataViewModel.Instance;

        IPlotterElement m_peaksGraph;
        IPlotterElement m_selectedPeakGraph;

        IPlotterElement m_stimuliStartsGraph;
        //IPlotterElement m_selectedArtifactGraph;

        IPlotterElement m_diffSignalGraph;
        IPlotterElement m_signalGraph;
        CursorCoordinateGraph m_cursorGraph;
        IPlotterElement m_peakThresholdGraph;
       // IPlotterElement m_curvaturesGraph;

        public MainWindow()
        {
            InitializeComponent();

            this.ViewModel.CurrentDataItemViewModelChanged += OnCurrentDataItemViewModelChanged;
            Settings.Settings.Instance.ViewSettings.IsDiffSignalVisible_Changed += ViewSettings_IsDiffSignalVisible_Changed;
            Settings.Settings.Instance.ViewSettings.IsSignalVisible_Changed += ViewSettings_IsSignalVisible_Changed;
            this.Loaded += OnLoaded;
        }

        private void OnCurrentPeakIndexChanged()
        {
            RefreshSelectedPeakGraph();
        }

        void RefreshPeaksGraph()
        {
            Part_Plotter.Children.Remove(m_peaksGraph);
            m_peaksGraph = null;

            var data = this.ViewModel.CurrentDataItem?.DataItem;

            if (data != null && data.LocatedPeaks != null)
            {
                EnumerableDataSource<PeakInfo> locatedPeaksDS = new EnumerableDataSource<PeakInfo>(data.LocatedPeaks);

                locatedPeaksDS.SetXMapping((item) => item.Pos.X);
                locatedPeaksDS.SetYMapping((item) => item.Pos.Y);

                m_peaksGraph = new ElementMarkerPointsGraph(locatedPeaksDS)
                {
                    Marker = new PeaksMarker() { Size = 10, Fill = Brushes.Yellow, ContextMenu = this.TryFindResource("PeakMarkerContextMenu") as ContextMenu }
                    //Marker = new CircleElementPointMarker() { Size = 10, Fill = Brushes.Yellow, ToolTipText = "Jouda" }
                };

                locatedPeaksDS.AddMapping(PeaksMarker.ToolTipTextProperty, (peakInfo) => peakInfo.ArtifactPeakSample.ToString());
                locatedPeaksDS.AddMapping(PeaksMarker.DataContextProperty, (peakInfo) => peakInfo);

                Part_Plotter.Children.Add(m_peaksGraph);
            }

            RefreshSelectedPeakGraph();
            RefreshCalculatedThresholdGraph();

            OrderGraphs();
        }

        void RefreshStimuliStartsGraph()
        {
            Part_Plotter.Children.Remove(m_stimuliStartsGraph);
            m_stimuliStartsGraph = null;

            var data = this.ViewModel.CurrentDataItem?.DataItem;

            if (data != null && data.Sweeps != null)
            {
                EnumerableDataSource<Sweep> peaksDS = new EnumerableDataSource<Sweep>(data.Sweeps);

                peaksDS.SetXMapping((item) => item.StimulusInfo.Start.X);
                peaksDS.SetYMapping((item) => item.StimulusInfo.Start.Y);

                m_stimuliStartsGraph = new ElementMarkerPointsGraph(peaksDS)
                {
                    Marker = new RectElementPointMarker() { Size = 10, Pen = new Pen(Brushes.Violet, 1) }
                };

                Part_Plotter.Children.Add(m_stimuliStartsGraph);

                OrderGraphs();
            }
        }

        void RefreshCalculatedThresholdGraph()
        {
            Part_Plotter.Children.Remove(m_peakThresholdGraph);
            m_peakThresholdGraph = null;

            var data = this.ViewModel.CurrentDataItem?.DataItem;
            if (data == null)
                return;

            {
                Point[] signal = new Point[2]
                {
                    new Point(0, data.PeakDetectionSettings.CalculatedThreshold),
                    new Point((data.SamplesCount - 1) * data.Recording.Xscale, data.PeakDetectionSettings.CalculatedThreshold)
                };

                EnumerableDataSource<Point> signalDS = new EnumerableDataSource<Point>(signal);

                signalDS.SetXMapping((item) => item.X);
                signalDS.SetYMapping((item) => item.Y);

                m_peakThresholdGraph = new DXLineGraph() { DataSource = signalDS, LineColor = Colors.Brown, Background = null };
                Part_Plotter.Children.Add(m_peakThresholdGraph);
                OrderGraphs();
            }
        }

        void OrderGraphs()
        {
            bool isSelectedPeakGraph = Part_Plotter.Children.Contains(m_selectedPeakGraph);
            Part_Plotter.Children.Remove(m_selectedPeakGraph);

            bool isPeaksGraph = Part_Plotter.Children.Contains(m_peaksGraph);
            Part_Plotter.Children.Remove(m_peaksGraph);

            bool isDiffSignalGraph = Part_Plotter.Children.Contains(m_diffSignalGraph);
            Part_Plotter.Children.Remove(m_diffSignalGraph);

            bool isSignalGraph = Part_Plotter.Children.Contains(m_signalGraph);
            Part_Plotter.Children.Remove(m_signalGraph);

            bool isCursorGraph = Part_Plotter.Children.Contains(m_cursorGraph);
            Part_Plotter.Children.Remove(m_cursorGraph);

            bool isPeakThresholdGraph = Part_Plotter.Children.Contains(m_peakThresholdGraph);
            Part_Plotter.Children.Remove(m_peakThresholdGraph);

            bool isStimuliStartsGraph = Part_Plotter.Children.Contains(m_stimuliStartsGraph);
            Part_Plotter.Children.Remove(m_stimuliStartsGraph);

            //bool isCurvaturesGraph = Part_Plotter.Children.Contains(m_curvaturesGraph);
            //Part_Plotter.Children.Remove(m_curvaturesGraph);

            if (m_cursorGraph != null && isCursorGraph) Part_Plotter.Children.Add(m_cursorGraph);
            if (m_diffSignalGraph != null && isDiffSignalGraph) Part_Plotter.Children.Add(m_diffSignalGraph);
            if (m_signalGraph != null && isSignalGraph) Part_Plotter.Children.Add(m_signalGraph);
            if (m_peakThresholdGraph != null && isPeakThresholdGraph) Part_Plotter.Children.Add(m_peakThresholdGraph);
            if (m_stimuliStartsGraph != null && isStimuliStartsGraph) Part_Plotter.Children.Add(m_stimuliStartsGraph);
            if (m_peaksGraph != null && isPeaksGraph) Part_Plotter.Children.Add(m_peaksGraph);
            if (m_selectedPeakGraph != null && isSelectedPeakGraph) Part_Plotter.Children.Add(m_selectedPeakGraph);

            //if (m_curvaturesGraph != null && isCurvaturesGraph) Part_Plotter.Children.Add(m_curvaturesGraph);
        }

        void RefreshSelectedPeakGraph()
        {
            Part_Plotter.Children.Remove(m_selectedPeakGraph);
            m_selectedPeakGraph = null;

            var peakInfo = this.ViewModel.CurrentDataItem?.SelectedPeakInfo;

            if (peakInfo != null)
            {
                PeakInfo[] point = new PeakInfo[] { peakInfo.PeakInfo };
                EnumerableDataSource<PeakInfo> locatedPeaksDS = new EnumerableDataSource<PeakInfo>(point);

                locatedPeaksDS.SetXMapping((item) => item.Pos.X);
                locatedPeaksDS.SetYMapping((item) => item.Pos.Y);
                locatedPeaksDS.AddMapping(PeaksMarker.DataContextProperty, (peakInfoItem) => peakInfoItem);

                m_selectedPeakGraph = new ElementMarkerPointsGraph(locatedPeaksDS)
                {
                    Marker = new SelectedPeaksMarker() { Size = 10, Pen = new Pen(Brushes.BlanchedAlmond, 1), Fill = Brushes.Transparent, ContextMenu = this.TryFindResource("PeakMarkerContextMenu") as ContextMenu }
                };

                Part_Plotter.Children.Add(m_selectedPeakGraph);
                OrderGraphs();
            }
        }

        private void ViewSettings_IsSignalVisible_Changed()
        {
            if (m_signalGraph != null)
            {
                if (Settings.Settings.Instance.ViewSettings.IsSignalVisible)
                    Part_Plotter.Children.Add(m_signalGraph);
                else
                    Part_Plotter.Children.Remove(m_signalGraph);

                OrderGraphs();
            }
        }

        private void ViewSettings_IsDiffSignalVisible_Changed()
        {
            if (m_diffSignalGraph != null)
            {
                if (Settings.Settings.Instance.ViewSettings.IsDiffSignalVisible)
                    Part_Plotter.Children.Add(m_diffSignalGraph);
                else
                    Part_Plotter.Children.Remove(m_diffSignalGraph);
                OrderGraphs();
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
#if false
            var directory = @"C:\Projects\Misa\Peaks\Data\latest\PTX-Amplitude";
            var directoryAbfs = @"C:\Projects\Misa\Peaks\Data\Oprava\new\abffiles\abfs";
            var csvFiles = System.IO.Directory.GetFiles(directory, "*.csv", System.IO.SearchOption.AllDirectories);
            var abfFiles = System.IO.Directory.GetFiles(directoryAbfs, "*.abf", System.IO.SearchOption.AllDirectories);

            Bridge.DataReader dataReader = new Bridge.DataReader();

            foreach (var abfFile in abfFiles)
            {
                var abfName = System.IO.Path.GetFileNameWithoutExtension(abfFile).ToUpper();

                var counts = csvFiles.Count(item => System.IO.Path.GetFileNameWithoutExtension(item).ToUpper().Contains(abfName));
                System.Diagnostics.Debug.Assert(counts == 1 || counts == 0);

                var csvFile = csvFiles.FirstOrDefault(item => System.IO.Path.GetFileNameWithoutExtension(item).ToUpper().Contains(abfName));
                bool fileUpdated = false;

                if (!string.IsNullOrEmpty(csvFile))
                {
                    var loadedCsvFile = System.IO.File.ReadAllLines(csvFile);
                    var loadedAbf = dataReader.ReadFile(abfFile);

                    System.Diagnostics.Debug.Assert(loadedAbf.Xscale == 0.05);
                    DataItem di = new DataItem(loadedAbf);
                    var signal = di.CreateContinuousSignal();

                    PeaksDetector.DetectPeaks(di, Settings.PeakDetectionModes.FirstPeak);
                    //var baseLineValue = di.PeakDetectionSettings.BaseLineValue;

                    System.Diagnostics.Debug.Assert(loadedCsvFile[0].Contains("Stimulus position [ms]"));

                    List<Point> areaPoints = new List<Point>();

                    for (int iLine = 1; iLine < loadedCsvFile.Length; iLine++)
                    {
                        var splittedLine = loadedCsvFile[iLine].Split(',');

                        double stimulusPositionMs = double.Parse(splittedLine[0]);
                        int stimulusSample = (int)(Math.Round(stimulusPositionMs / loadedAbf.Xscale));
                        int samplesCount = (int)Math.Round(0.50 / loadedAbf.Xscale);
                        int offsetInSamples = 2;// (int)Math.Round(0.05 / loadedAbf.Xscale);

                        var qq = di.Sweeps.FirstOrDefault((sweep) => sweep.StartSample == stimulusSample);
                        
                        if (qq == null)
                        {
                            int iii = 3;
                        }
                        System.Diagnostics.Debug.Assert(samplesCount > 2);
                        System.Diagnostics.Debug.Assert(offsetInSamples > 0);

                        int startSample = stimulusSample - samplesCount - offsetInSamples;

                        System.Diagnostics.Debug.Assert(startSample >= 0);

                        startSample = Math.Max(0, startSample);

                        areaPoints.Add(new Point(startSample * loadedAbf.Xscale * 1e-3, (signal[startSample] - di.PeakDetectionSettings.BaseLineValue) * 1e-9));

                        //                List<double> baseLine = new List<double>(samplesCount);

                        //                for (int i = startSample; i < startSample + samplesCount; i++)
                        //                    baseLine.Add(signal[i]);

                        //                var localBaseLineValue = baseLine.Average();

                        //                double peakTimeMs;
                        //                if (double.TryParse(splittedLine[1], out peakTimeMs))
                        //                {
                        //                    var peakSample = (int)(Math.Round(peakTimeMs / loadedAbf.Xscale));
                        //                    var relativeAmplitude = (signal[peakSample] - localBaseLineValue);
                        ////                    System.Diagnostics.Debug.Assert(relativeAmplitude > 0);

                        //                    var line = splittedLine[0] + "," + splittedLine[1] + "," + splittedLine[2] + ", " + relativeAmplitude;
                        //                    loadedCsvFile[iLine] = line;
                        //                    fileUpdated = true;
                        //                }
                        //                //else
                        //                  //  System.Diagnostics.Debug.Assert(false, "Why?");
                    }

                    var areaIntegral = Processing.Geometry.PolylineAreaIntegral(areaPoints);
                    double duration = areaPoints[areaPoints.Count - 1].X - areaPoints[0].X;

                    var chargePerSecond = areaIntegral / duration;

                    areaPoints.Insert(0, new Point(areaPoints[0].X, 0));
                    areaPoints.Add(new Point(areaPoints[areaPoints.Count - 1].X, 0));

                    //areaPoints.Insert(0, new Point(areaPoints[0].X, di.PeakDetectionSettings.BaseLineValue * 1e-9));
                    //areaPoints.Add(new Point(areaPoints[areaPoints.Count - 1].X, di.PeakDetectionSettings.BaseLineValue * 1e-9));

                    
                    var area = -Processing.Geometry.PolygonArea(areaPoints);

                    if (Math.Abs(area - areaIntegral) > (area + areaIntegral)*1e-7)
                    {
                        int iii = 3;
                    }

                    //if (fileUpdated)
                    {
//                        loadedCsvFile[0] += ", Residual Charge [nC]: , " + chargePerSecond / 1e-9; // = "Stimulus position [ms], Peak position [ms], Peak offset [ms], Peak amplitude relative to baseline";
  //                      System.IO.File.WriteAllLines(csvFile, loadedCsvFile);
                    }
                }
            }
#endif
            ViewSettingsWindow settingsWindow = new ViewSettingsWindow() { Owner = this };
            settingsWindow.Show();

            FilesView fv = new FilesView() { Owner = this };
            fv.Show();

            //InputBindings.Add(new KeyBinding( Commands.MoveToNextPeakCommand.InputGestures);

            CommandBindings.Add(new CommandBinding(Commands.MoveToNextPeakCommand, OnMoveToNextPeakExecuted));
            CommandBindings.Add(new CommandBinding(Commands.MoveToPrevPeakCommand, OnMoveToPrevPeakExecuted));
            CommandBindings.Add(new CommandBinding(Commands.ZoomToFirstPeakCommand, OnZoomToFirstPeakExecuted));

            CommandBindings.Add(new CommandBinding(Commands.SaveCurrentItemCommand, OnSaveCurrentItemExecuted));
            CommandBindings.Add(new CommandBinding(Commands.FindNextPeakCommand, OnFindNextPeakExecuted));
            CommandBindings.Add(new CommandBinding(Commands.FindPrevPeakCommand, OnFindPrevPeakExecuted));

            Commands.RemovePeakCommand.Executed += RemovePeakCommand_Executed;
            //CommandBindings.Add(new CommandBinding(Commands.RemovePeakCommand, OnRemovePeakExecuted));

        }

        private void OnSaveCurrentItemExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.ViewModel.DataModel.SaveFolder))
            {
                CommonOpenFileDialog dlg = new CommonOpenFileDialog();
                dlg.IsFolderPicker = true;
                if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    this.ViewModel.DataModel.SaveFolder = dlg.FileName;
                }
            }

            if (!string.IsNullOrEmpty(this.ViewModel.DataModel.SaveFolder))
            {
                var currentRecording = this.ViewModel.CurrentDataItem?.DataItem;

                if (currentRecording != null)
                {
                    currentRecording.SaveResults(this.ViewModel.DataModel.SaveFolder);
                }
            }
        }

        private void RemovePeakCommand_Executed(PeakInfo peakInfo)
        {
            this.ViewModel.RemovePeak(peakInfo);
        }

        private void OnFindPrevPeakExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.ViewModel.FindPrevPeak();
            RefreshPeaksGraph();
        }

        private void OnFindNextPeakExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.ViewModel.FindNextPeak();
            RefreshPeaksGraph();
        }

        private void OnMoveToNextPeakExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.ViewModel.MoveToNextPeak();
            MoveGraphToShowCurrentDetectedPeak();
        }

        private void OnMoveToPrevPeakExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.ViewModel.MoveToPrevPeak();
            MoveGraphToShowCurrentDetectedPeak();
        }

        private void OnZoomToFirstPeakExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var currentDataItemViewModel = this.ViewModel.CurrentDataItem;

            if (currentDataItemViewModel == null)
                return;

            var currentDataItem = currentDataItemViewModel.DataItem;

            if (currentDataItem.Sweeps == null || !currentDataItem.IsAnalyzed || currentDataItem.LocatedPeaks == null ||
                currentDataItem.LocatedPeaks.Length == 0)
                return;

            currentDataItemViewModel.CurrentPeakIndex = 0;

            var currentPeak = currentDataItem.LocatedPeaks[currentDataItemViewModel.CurrentPeakIndex];

            int? nearestStimulusStart = currentDataItemViewModel.GetNearestStimulusIndex(currentPeak);

            if (nearestStimulusStart.HasValue)
            {
                var visibleRect = this.Part_Plotter.Viewport.Visible;

                visibleRect.XMin = Math.Max(currentDataItem.Sweeps[nearestStimulusStart.Value].StimulusInfo.Start.X - currentDataItem.Recording.Xscale * 5, 0);
                visibleRect.Width = Math.Abs(currentPeak.Pos.X - currentDataItem.Sweeps[nearestStimulusStart.Value].StimulusInfo.Start.X) * 3;

                this.Part_Plotter.Viewport.Visible = visibleRect;
            }
        }

        DXLineGraph m_filteredSignalGraph;
        void OnCurrentDataItemViewModelChanged(DataItemViewModel oldItem, DataItemViewModel newItem)
        {
            this.Title = string.Empty;

            Part_Plotter.Children.Remove(m_selectedPeakGraph);
            Part_Plotter.Children.Remove(m_peaksGraph);
            Part_Plotter.Children.Remove(m_diffSignalGraph);
            Part_Plotter.Children.Remove(m_signalGraph);
            Part_Plotter.Children.Remove(m_cursorGraph);
            Part_Plotter.Children.Remove(m_peakThresholdGraph);
            Part_Plotter.Children.Remove(m_stimuliStartsGraph);
            Part_Plotter.Children.Remove(m_filteredSignalGraph);
            


            m_selectedPeakGraph = null;
            m_peaksGraph = null;
            m_diffSignalGraph = null;
            m_signalGraph = null;
            m_cursorGraph = null;
            m_peakThresholdGraph = null;
            m_stimuliStartsGraph = null;
            m_filteredSignalGraph = null;

            if (oldItem != null)
            {
                oldItem.CurrentPeakIndexChanged += OnCurrentPeakIndexChanged;
                oldItem.DataItem.LocatedPeaksUpdated -= OnData_LocatedPeaksUpdated;
                oldItem.DataItem.SweepsChanged -= OnSweepsChangedChanged;
                oldItem.DataItem.PeakDetectionSettings.CalculatedThresholdChanged -= OnCalculatedThresholdChanged;
            }

            if (newItem == null)
                return;

            this.Title = System.IO.Path.GetFileNameWithoutExtension(newItem.DataItem.Recording.FilePath);

            newItem.CurrentPeakIndexChanged += OnCurrentPeakIndexChanged;
            newItem.DataItem.LocatedPeaksUpdated += OnData_LocatedPeaksUpdated;
            newItem.DataItem.SweepsChanged += OnSweepsChangedChanged;
            newItem.DataItem.PeakDetectionSettings.CalculatedThresholdChanged += OnCalculatedThresholdChanged;

            var points = newItem.DataItem.CreateContinuousSignal();

            {
                var xs = Enumerable.Range(0, points.Length).Select(item => item * newItem.DataItem.Recording.Xscale);
                CompositeDataSource origSignal = new CompositeDataSource(xs.AsXDataSource(), points.AsYDataSource());
                m_signalGraph = new DXLineGraph() { DataSource = origSignal, LineColor = Colors.Blue };

                if (Settings.Settings.Instance.ViewSettings.IsSignalVisible)
                    Part_Plotter.Children.Add(m_signalGraph);

                double samplingPeriod = 1.0 / (newItem.DataItem.Recording.SamplingFrequency * 1000);
                double cutoffFrequency = 300;
//                bool isLowPass = false;
                var filter = PeaksProcessing.Processing.FirFilter.Create(cutoffFrequency, samplingPeriod, FilterType.HighPass);

                var filteredPoints = newItem.DataItem.CreateContinuousSignal();

                filter.FilterInPlace(filteredPoints);

                //var mad = DataProcessor.MAD(filteredPoints);
                //double sigma = 1.4826 * mad.Value;

                //var curvatures = DataProcessor.CalculateCurvatures(points);
                CompositeDataSource curvaturesSignal = new CompositeDataSource(xs.AsXDataSource(), filteredPoints.AsYDataSource());
                m_filteredSignalGraph = new DXLineGraph() { DataSource = curvaturesSignal, LineColor = Colors.Green };

                Part_Plotter.Children.Add(m_filteredSignalGraph);

                //m_curvaturesGraph = new DXLineGraph() { DataSource = curvaturesSignal, LineColor = Colors.Green };

                //Part_Plotter.Children.Add(m_curvaturesGraph);
            }

            {
                var xsDiff = Enumerable.Range(0, points.Length - 1).Select(item => item * newItem.DataItem.Recording.Xscale);
                CompositeDataSource diffSignal = new CompositeDataSource(xsDiff.AsXDataSource(), DataProcessor.ComputeDiff(points).AsYDataSource());
                m_diffSignalGraph = new DXLineGraph() { DataSource = diffSignal, LineColor = Colors.Red };

                if (Settings.Settings.Instance.ViewSettings.IsDiffSignalVisible)
                    Part_Plotter.Children.Add(m_diffSignalGraph);
            }

            m_cursorGraph = new CursorCoordinateGraph() { CustomXFormat = "{0:F}", CustomYFormat = "{0:F}" };
            Part_Plotter.Children.Add(m_cursorGraph);

            Part_Plotter.FitToView();

            RefreshPeaksGraph();
            RefreshStimuliStartsGraph();
            OrderGraphs();
        }

        private void OnCalculatedThresholdChanged(double obj)
        {
            RefreshCalculatedThresholdGraph();
        }

        private void OnSweepsChangedChanged()
        {
            RefreshStimuliStartsGraph();
        }

        private void OnData_LocatedPeaksUpdated()
        {
            RefreshPeaksGraph();
        }

        private void MoveGraphToShowCurrentDetectedPeak()
        {
            var currentDataItemViewModel = this.ViewModel.CurrentDataItem;

            if (currentDataItemViewModel == null)
                return;

            var currentDataItem = currentDataItemViewModel.DataItem;

            if (!currentDataItem.IsAnalyzed || currentDataItem.LocatedPeaks == null || currentDataItemViewModel.CurrentPeakIndex < 0)
                return;

            var currentPeak = currentDataItem.LocatedPeaks[currentDataItemViewModel.CurrentPeakIndex];

            int? nearestStimulusIndex = currentDataItemViewModel.GetNearestStimulusIndex(currentPeak);

            if (nearestStimulusIndex.HasValue)
            {
                var visibleRect = this.Part_Plotter.Viewport.Visible;
                //visibleRect.XMin = Math.Max(currentDataItem.Xs[currentDataItem.LocatedPeaksArtifactsPeaks[nearestArtefactSampleIndex]], 0);
                visibleRect.XMin = Math.Max(currentDataItem.Sweeps[nearestStimulusIndex.Value].StimulusInfo.Start.X - currentDataItem.Recording.Xscale * 5, 0);

                this.Part_Plotter.Viewport.Visible = visibleRect;
            }
        }

        private void Part_Plotter_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.ViewModel.IsManualPeakSelectionEnabled)
            {
                Point mousePos = m_cursorGraph.Position;
                var transform = Part_Plotter.Viewport.Transform;
                Point mousePosInData = mousePos.ScreenToData(transform);
                double timeMs = mousePosInData.X;

                m_mouseClickedPosition = mousePos.X;
                m_sampleToAddCandidateInMs = timeMs;
            }
        }

        private void Part_Plotter_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.ViewModel.IsManualPeakSelectionEnabled && m_mouseClickedPosition.HasValue)
            {
                Point mousePos = m_cursorGraph.Position;

                if (Math.Abs(mousePos.X - m_mouseClickedPosition.Value) < 4)
                {
                    var transform = Part_Plotter.Viewport.Transform;
                    Point mousePosInData = mousePos.ScreenToData(transform);
                    double timeMs = mousePosInData.X;

                    this.ViewModel.AddPeakAtPosition(m_sampleToAddCandidateInMs.Value);
                }
            }

            m_mouseClickedPosition = null;
            m_sampleToAddCandidateInMs = null;
        }

        private void Part_Plotter_MouseLeave(object sender, MouseEventArgs e)
        {
            m_mouseClickedPosition = null;
            m_sampleToAddCandidateInMs = null;
        }

        double? m_mouseClickedPosition;
        double? m_sampleToAddCandidateInMs;
    }
}
