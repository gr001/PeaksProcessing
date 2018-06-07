using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeaksProcessing.Settings
{
    public class ViewSettings : Notifiable
    {
        public event Action IsSignalVisible_Changed;
        public event Action IsDiffSignalVisible_Changed;

        public bool IsSignalVisible
        {
            get { return m_isSignalVisible; }
            set
            {
                if (m_isSignalVisible != value)
                {
                    m_isSignalVisible = value;

                    var handler = IsSignalVisible_Changed;
                    if (handler != null)
                        handler();

                    NotifyPropertyChanged();
                }
            }
        }
        bool m_isSignalVisible = true;

        public bool IsDiffSignalVisible
        {
            get { return m_isDiffSignalVisible; }
            set
            {
                if (m_isDiffSignalVisible != value)
                {
                    m_isDiffSignalVisible = value;

                    var handler = IsDiffSignalVisible_Changed;
                    if (handler != null)
                        handler();

                    NotifyPropertyChanged();
                }
            }
        }
        bool m_isDiffSignalVisible = true;
    }
}
