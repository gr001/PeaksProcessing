using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Tango.Share.WPF
{
    public interface IDelegateCommand : System.Windows.Input.ICommand
    {
        void RaiseCanExecuteChanged();//sends notification to UI that CanExecute should be called
    }

    public abstract class DelegateCommandBase
    {
        internal DelegateCommandBase(string debugName)
        {
            m_debugName = debugName;
        }

        public static bool StopEvaluationCanExecute { get; set; }
        protected readonly string m_debugName;
    }

    public class DelegateCommand<T> : DelegateCommandBase, IDelegateCommand
    {
        private readonly Predicate<T> m_canExecute;
        public event Action<T> Executed
        {
            add
            {
                bool isEmpty = m_executedEvent == null || m_executedEvent.GetInvocationList().Length == 0;

                m_executedEvent += value;

                if (isEmpty)
                    Commands.CommandsManager.Current.RegisterCommand(this);
            }
            remove
            {
                m_executedEvent -= value;

                bool isEmpty = m_executedEvent == null || m_executedEvent.GetInvocationList().Length == 0;

                if (isEmpty)
                    Commands.CommandsManager.Current.UnregisterCommand(this);
            }
        }
        Action<T> m_executedEvent;

        public DelegateCommand(string debugName)
            : this(null, debugName)
        {
        }

        public DelegateCommand(Predicate<T> canExecute, string debugName)
            : base(debugName)
        {
            m_canExecute = canExecute;
        }

        bool ICommand.CanExecute(object parameter)
        {
            if (StopEvaluationCanExecute || this.m_executedEvent == null)
                return false;

            if (m_canExecute == null)
                return true;

            return m_canExecute((parameter == null) ? default(T) : (T)parameter);
        }

        void ICommand.Execute(object parameter)
        {
            this.Execute((parameter == null) ? default(T) : (T)parameter);
        }

        public void Execute(T parameter)
        {
            if (!((ICommand)this).CanExecute(parameter))
                return;

            var handler = m_executedEvent;
            if (handler != null)
                handler(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                m_internalCanExecuteChanged += value;
                //CommandManager.RequerySuggested += value;
            }
            remove
            {
                m_internalCanExecuteChanged -= value;
                //CommandManager.RequerySuggested -= value;
            }
        }
        EventHandler m_internalCanExecuteChanged;

        void IDelegateCommand.RaiseCanExecuteChanged()
        {
            var handler = m_internalCanExecuteChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }

    public class DelegateCommand : DelegateCommandBase, IDelegateCommand
    {
        private readonly Func<bool> m_canExecute;
        public event Action Executed
        {
            add 
            {
                bool isEmpty = m_executedEvent == null || m_executedEvent.GetInvocationList().Length == 0;

                m_executedEvent += value;

                if (isEmpty)
                    Commands.CommandsManager.Current.RegisterCommand(this);
            }
            remove
            {
                m_executedEvent -= value;

                bool isEmpty = m_executedEvent == null || m_executedEvent.GetInvocationList().Length == 0;

                if (isEmpty)
                    Commands.CommandsManager.Current.UnregisterCommand(this);
            }
        }
        Action m_executedEvent;

        public DelegateCommand(string debugName) : this(null, debugName)
        {
        }

        public DelegateCommand(Func<bool> canExecute, string debugName)
            : base(debugName)
        {
            m_canExecute = canExecute;
        }

        public bool CanExecute()
        {
            return ((System.Windows.Input.ICommand)this).CanExecute(null);
        }

        bool System.Windows.Input.ICommand.CanExecute(object parameter)
        {
            System.Diagnostics.Debug.Assert(parameter == null);

            if (StopEvaluationCanExecute || m_executedEvent == null)
                return false;

            if (m_canExecute == null)
                return true;

            return m_canExecute();
        }

        public void Execute()
        {
            ((System.Windows.Input.ICommand)this).Execute(null);
        }

        void System.Windows.Input.ICommand.Execute(object parameter)
        {
            if (!CanExecute())
                return;

            System.Diagnostics.Debug.Assert(parameter == null);

            var handler = m_executedEvent;
            if (handler != null)
                handler();
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                m_internalCanExecuteChanged += value;
                //CommandManager.RequerySuggested += value;
            }
            remove
            {
                m_internalCanExecuteChanged -= value;
                //CommandManager.RequerySuggested -= value;
            }
        }
        EventHandler m_internalCanExecuteChanged;

        void IDelegateCommand.RaiseCanExecuteChanged()
        {
            var handler = m_internalCanExecuteChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }
}
