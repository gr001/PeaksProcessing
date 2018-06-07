using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Charts;
using PeaksProcessing.Processing;
using PeaksProcessing.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using PeaksProcessing.Data;
using System.Collections.ObjectModel;

namespace PeaksProcessing
{
    public class DataViewModel : Notifiable
    {
        static DataViewModel s_instance;

        public static DataViewModel Instance
        {
            get
            {
                if (s_instance == null)
                    s_instance = new DataViewModel();
                return s_instance;
            }
        }


        public ObservableCollection<DataItemViewModel> DataItems { get; } = new ObservableCollection<DataItemViewModel>();
        public DataModel DataModel { get; } = DataModel.Instance;

        internal delegate void CurrentDataItemViewModelChangedDel(DataItemViewModel oldItem, DataItemViewModel newItem);
        internal event CurrentDataItemViewModelChangedDel CurrentDataItemViewModelChanged;
        public DataItemViewModel CurrentDataItem
        {
            get { return m_currentDataItem; }
            set
            {
                if (value != m_currentDataItem)
                {
                    var oldItem = m_currentDataItem;
                    m_currentDataItem = value;

                    if (value != null && value.DataItem.LocatedPeaks == null)
                        PeaksDetector.DetectPeaks(value.DataItem, Settings.Settings.Instance.GlobalDetectionSettings.PeakDetectionMode);

                    var handler = CurrentDataItemViewModelChanged;
                    if (handler != null)
                        handler(oldItem, m_currentDataItem);

                    NotifyPropertyChanged();
                }
            }
        }
        DataItemViewModel m_currentDataItem;

        public bool IsManualPeakSelectionEnabled { get; set; }
        
        private DataViewModel()
        {
            this.DataModel.DataItemClosed += OnDataItemClosed;
            this.DataModel.DataItemsClosed += OnDataItemsClosed;
            this.DataModel.DataItemsLoaded += OnDataItemsLoaded;
        }

        void OnDataItemsLoaded()
        {
            foreach (var item in this.DataModel.Recordings)
                this.DataItems.Add(new DataItemViewModel(item));

            this.CurrentDataItem = this.DataItems.FirstOrDefault();
        }

        private void OnDataItemsClosed()
        {
            this.DataItems.Clear();
            this.CurrentDataItem = null;
        }

        private void OnDataItemClosed(DataItem itemClosed)
        {
            DataItemViewModel existingViewModel = null;

            DataItemViewModel nextSelectedItem = null;

            for (int i = 0; i < this.DataItems.Count; i++)
            {
                existingViewModel = this.DataItems[i];
                if (existingViewModel.DataItem == itemClosed)
                {
                    if (i + 1 < this.DataItems.Count)
                        nextSelectedItem = this.DataItems[i + 1];
                    else if (i - 1 >= 0)
                        nextSelectedItem = this.DataItems[i - 1];
                    break;
                }
            }

            if (existingViewModel != null)
            {
                bool isRecordingSelected = this.CurrentDataItem == existingViewModel;
                this.DataItems.Remove(existingViewModel);

                this.DataItems.Remove(existingViewModel);

                if (isRecordingSelected)
                    this.CurrentDataItem = nextSelectedItem;
            }
        }

        internal void MoveToPrevPeak()
        {
            this.CurrentDataItem?.MoveToPrevPeak();
        }

        internal void MoveToNextPeak()
        {
            this.CurrentDataItem?.MoveToNextPeak();
        }

        internal void FindPrevPeak()
        {
            this.CurrentDataItem?.FindPrevPeak();
        }

        internal void FindNextPeak()
        {
            this.CurrentDataItem?.FindNextPeak();
        }

        internal void AddPeakAtPosition(double timeMs)
        {
            this.CurrentDataItem?.AddPeakAtPosition(timeMs);
        }

        internal void RemovePeak(PeakInfo peakInfo)
        {
            this.CurrentDataItem?.RemovePeak(peakInfo);
        }
    }
}
