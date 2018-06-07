using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using PeaksProcessing.ViewModel;
using System;
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
    /// Interaction logic for FilesView.xaml
    /// </summary>
    public partial class FilesView : Window
    {
        public DataModel DataModel {  get { return DataModel.Instance; } }
        public DataViewModel DataViewModel {  get { return DataViewModel.Instance; } }

        public FilesView()
        {
            InitializeComponent();

            CommandBindings.Add(new CommandBinding(Commands.CloseRecordingCommand, OnCloseRecordingExecuted));
        }

        private void OnCloseRecordingExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            System.Diagnostics.Debug.Assert(e.Parameter is Bridge.RecordingNet);
            this.DataModel.CloseRecording(e.Parameter as Bridge.RecordingNet);
        }

        private void OpenFiles_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = "abf";
            dlg.Filter = "Abf files (*.abf;*.abf)|*.abf;*.abf|All files (*.*)|*.*";
            dlg.CheckFileExists = true;
            dlg.Multiselect = true;

            if (dlg.ShowDialog() == true)
            {
                this.DataModel.LoadFiles(dlg.FileNames);
            }
        }

        private void CloseAllRecordings_Click(object sender, RoutedEventArgs e)
        {
            this.DataModel.CloseAllRecordings();
        }
    }
}
