using Signals.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Signals.ViewModels
{
    public class AddWaveViewModel
    {
        public AddWaveModel WaveModel { get; set; }
        public ICommand RandomWaveCommand { get; set; }
        public ICommand AddWaveCommand { get; set; }
    }

    public class AddWaveCommand : ICommand
    {
        private readonly Action _action;

        public AddWaveCommand(Action actionToExecute)
        {
            _action = actionToExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _action();
        }
    }

    public class GenerateRandomWaveCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
           
            return true;
        }

        public void Execute(object parameter)
        {
            if(parameter is AddWaveModel model)
            {
                Random random = new Random();
                model.Frequency = random.Next(0, 100);
                model.Amplitude = random.Next(0, 100);
                model.PointCount = random.Next(100, 10000);
                model.SampleRate= random.Next(100, 10000);
                model.Phase = random.Next(0, 10);
            }
        }
    }
}
