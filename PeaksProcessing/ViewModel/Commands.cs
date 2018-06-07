using PeaksProcessing.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Tango.Share.WPF;

namespace PeaksProcessing.ViewModel
{
    public static class Commands
    {
        public static DelegateCommand<PeakInfo> RemovePeakCommand { get; private set; }

        public static RoutedUICommand CloseRecordingCommand { get; private set; }
        

        public static RoutedUICommand MoveToNextPeakCommand { get; private set; }
        public static RoutedUICommand MoveToPrevPeakCommand { get; private set; }
        public static RoutedUICommand ZoomToFirstPeakCommand { get; private set; }

        public static RoutedUICommand SaveCurrentItemCommand { get; private set; }
        
        public static RoutedUICommand FindNextPeakCommand { get; private set; }
        public static RoutedUICommand FindPrevPeakCommand { get; private set; }

        static Commands()
        {
            CloseRecordingCommand = new RoutedUICommand("Close recording", "CloseRecording", typeof(Commands));

            InputGestureCollection gestures = new InputGestureCollection();
            gestures.Add(new KeyGesture(Key.Right, ModifierKeys.Alt));
            MoveToNextPeakCommand = new RoutedUICommand("Move to next peak (Alt ->)", "MoveToNextPeak", typeof(Commands), gestures);

            gestures = new InputGestureCollection();
            gestures.Add(new KeyGesture(Key.Left, ModifierKeys.Alt));
            MoveToPrevPeakCommand = new RoutedUICommand("Move to previous peak (Alt <-)", "MoveToPrevPeak", typeof(Commands), gestures);

            gestures = new InputGestureCollection();
            gestures.Add(new KeyGesture(Key.Z, ModifierKeys.Alt));
            ZoomToFirstPeakCommand = new RoutedUICommand("Zoom to first peak (Alt Z)", "ZoomToFirstPeak", typeof(Commands), gestures);

            gestures = new InputGestureCollection();
            gestures.Add(new KeyGesture(Key.Right, ModifierKeys.Control | ModifierKeys.Shift));
            FindNextPeakCommand = new RoutedUICommand("Find next peak (Ctrl Shift ->)", "FindNextPeakCommand", typeof(Commands), gestures);

            gestures = new InputGestureCollection();
            gestures.Add(new KeyGesture(Key.Left, ModifierKeys.Control | ModifierKeys.Shift));
            FindPrevPeakCommand = new RoutedUICommand("Find next peak (Ctrl Shift <-)", "FindPrevPeakCommand", typeof(Commands), gestures);

            gestures = new InputGestureCollection();
            gestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
            SaveCurrentItemCommand = new RoutedUICommand("Save current item (Ctrl S)", "SaveCurrentItemCommand", typeof(Commands), gestures);
            

            //gestures = new InputGestureCollection();
            //gestures.Add(new KeyGesture(Key.Right, ModifierKeys.Control | ModifierKeys.Shift));
            RemovePeakCommand = new Tango.Share.WPF.DelegateCommand<PeakInfo>("Remove peak");
        }

    }
}
