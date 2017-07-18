using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Rico.ViewModels;
using System.Windows;

namespace Rico.ViewModels.Commands
{
	public class AddParameterCommand : ICommand
	{
		// Instace of the base ViewModel
		public ParametersViewModel ViewModel { get; set; }
		// Class constructor
		public AddParameterCommand(ParametersViewModel viewModel)
		{
			ViewModel = viewModel;
		}

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public bool CanExecute(object parameter)
		{
			if (parameter == null) return false;
			var parameterValue = parameter as string;
			parameterValue = parameterValue.Trim();
			if (string.IsNullOrEmpty(parameterValue)) return false;
			return true;
		}

		public void Execute(object parameter)
		{
			ViewModel.AddParameter((string)parameter);
		}
	}
}
