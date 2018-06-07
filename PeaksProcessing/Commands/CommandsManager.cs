using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Tango.Share.WPF.Commands
{
    public class CommandsManager
    {
        class CommandItem
        {
            public WeakReference<IDelegateCommand> Command;
            public bool? CanExecuteLastValue;
        }

        System.Windows.Threading.DispatcherTimer m_canExecuteTimer = new System.Windows.Threading.DispatcherTimer(System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        List<WeakReference<IDelegateCommand>> m_commandsWithParameter = new List<WeakReference<IDelegateCommand>>(128);
        List<CommandItem> m_commandsWithoutParameter = new List<CommandItem>(128);
        int m_cleanCommandsCollectionCounter = 0;
        bool m_garbageCollectedCommandWithParameterFound = false;
        bool m_garbageCollectedCommandWithoutParameterFound = false;

        static CommandsManager s_commandManager = new CommandsManager();

        public static CommandsManager Current
        {
            get
            {
                return s_commandManager;
            }
        }

        private CommandsManager()
        {
            m_canExecuteTimer.Interval = TimeSpan.FromMilliseconds(100);
            m_canExecuteTimer.Tick += OnCanExecuteTimer_Tick;
        }

        internal void RegisterCommand(IDelegateCommand command)
        {
            if (command != null)
            {
                DelegateCommand delegateCommand = command as DelegateCommand;//this command does not have any parameter for CanExecute
#if DEBUG

                if (delegateCommand != null)
                {
                    //check if the command has been already registered
                    foreach (var item in m_commandsWithoutParameter)
                    {
                        IDelegateCommand existingCommand;
                        if (item.Command.TryGetTarget(out existingCommand))
                            System.Diagnostics.Debug.Assert(existingCommand != command);
                    }
                }
                else
                {
                    //check if the command has been already registered
                    foreach (var item in m_commandsWithParameter)
                    {
                        IDelegateCommand existingCommand;
                        if (item.TryGetTarget(out existingCommand))
                            System.Diagnostics.Debug.Assert(existingCommand != command);
                    }
                }
#endif
                if (delegateCommand != null)
                    m_commandsWithoutParameter.Add(new CommandItem() { Command = new WeakReference<IDelegateCommand>(command) });
                else
                    m_commandsWithParameter.Add(new WeakReference<IDelegateCommand>(command));

                if (!m_canExecuteTimer.IsEnabled)
                    m_canExecuteTimer.Start();
            }
        }

        internal void UnregisterCommand(IDelegateCommand command)
        {
            if (command != null)
            {
                DelegateCommand delegateCommand = command as DelegateCommand;
                IDelegateCommand existingCommand;

                if (delegateCommand != null)
                    m_commandsWithoutParameter.RemoveAll((item) => !item.Command.TryGetTarget(out existingCommand) || existingCommand == command);
                else
                    m_commandsWithParameter.RemoveAll((item) => !item.TryGetTarget(out existingCommand) || existingCommand == command);
            }
        }

        private void OnCanExecuteTimer_Tick(object sender, EventArgs e)
        {
            m_cleanCommandsCollectionCounter++;
            IDelegateCommand command;

            foreach (var item in m_commandsWithParameter)
            {
                if (item.TryGetTarget(out command))
                    command.RaiseCanExecuteChanged();
                else
                    m_garbageCollectedCommandWithParameterFound = true;
            }

            foreach (var item in m_commandsWithoutParameter)
            {
                if (item.Command.TryGetTarget(out command))
                {
                    bool canExecuteValue = command.CanExecute(null);

                    if (!item.CanExecuteLastValue.HasValue || canExecuteValue != item.CanExecuteLastValue.Value)
                    {
                        item.CanExecuteLastValue = canExecuteValue;
                        command.RaiseCanExecuteChanged();
                    }
                }
                else
                    m_garbageCollectedCommandWithoutParameterFound = true;
            }

            if (m_cleanCommandsCollectionCounter >= 100)
            {

                if (m_garbageCollectedCommandWithParameterFound)
                {
                    m_commandsWithParameter.RemoveAll((item) => !item.TryGetTarget(out command));
                    m_garbageCollectedCommandWithParameterFound = false;
                }

                if (m_garbageCollectedCommandWithoutParameterFound)
                {
                    m_commandsWithoutParameter.RemoveAll((item) => !item.Command.TryGetTarget(out command));
                    m_garbageCollectedCommandWithoutParameterFound = false;
                }

                m_cleanCommandsCollectionCounter = 0;
            }
        }
    }
}
