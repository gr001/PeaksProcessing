using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeaksProcessing.Settings
{
    public class Settings : Notifiable
    {
        static Settings s_instance = new Settings();

        Settings() { }

        public static Settings Instance { get { return s_instance; } }

        public ViewSettings ViewSettings { get; } = new ViewSettings();
        public GlobalDetectionSettings GlobalDetectionSettings { get; } = new GlobalDetectionSettings();
    }
}
