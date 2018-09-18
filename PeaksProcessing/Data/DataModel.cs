using PeaksProcessing.Processing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bridge;
using System.Windows;
using PeaksProcessing.Settings;

namespace PeaksProcessing
{
    public sealed class DataModel : Notifiable
    {
        internal event Action DataItemsLoaded;
        internal event Action<DataItem> DataItemClosed;

        internal event Action DataItemsClosed;

        internal void CloseAllRecordings()
        {
            m_recordings.Clear();
            Fire_DataItemsClosed();
        }

        public IEnumerable<DataItem> Recordings { get { return m_recordings; } }
        ObservableCollection<DataItem> m_recordings = new ObservableCollection<DataItem>();

        static DataModel s_model;

        private DataModel()
        {
        }

        public static DataModel Instance
        {
            get
            {
                if (s_model == null)
                    s_model = new DataModel();
                return s_model;
            }
        }

        internal string SaveFolder;

        public void LoadFiles(IEnumerable<string> filePaths)
        {
            try
            {
                this.SaveFolder = null;
                m_recordings.Clear();
                Fire_DataItemsClosed();

                if (filePaths == null)
                    return;

                Bridge.DataReader dataReader = new Bridge.DataReader();
                ObservableCollection<Bridge.RecordingNet> recordings = new ObservableCollection<Bridge.RecordingNet>();

                foreach (var file in filePaths)
                {
                    var readFile = dataReader.ReadFile(file);

                    if (readFile != null)
                    {
                        lock (m_recordings)
                        {
                            recordings.Add(readFile);
                        }
                    }
                }

                m_recordings.Clear();
                Fire_DataItemsClosed();

                var detectionMode = Settings.Settings.Instance.GlobalDetectionSettings.PeakDetectionMode;

                foreach (var item in recordings)
                    m_recordings.Add(new DataItem(item, detectionMode));

                Parallel.ForEach(m_recordings, (item) => Processing.PeaksDetectorFactory.DetectPeaks(item));

                var handler = DataItemsLoaded;
                if (handler != null)
                    handler();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Exception when loading files:\n" + ex.ToString());
            }
        }

        void Fire_DataItemsClosed()
        {
            var handler = DataItemsClosed;
            if (handler != null)
                handler();
        }

        void Fire_DataItemClosed(DataItem item)
        {
            var handler = DataItemClosed;
            if (handler != null)
                handler(item);
        }

        internal void CloseRecording(RecordingNet recording)
        {
            if (recording != null)
            {
                foreach(var dataItem in m_recordings)
                {
                    if (dataItem.Recording == recording)
                    {
                        m_recordings.Remove(dataItem);
                        var handler = DataItemClosed;
                        if (handler != null)
                            handler(dataItem);
                        break;
                    }
                }
            }
        }
    }
}
