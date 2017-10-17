using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Rico.ViewModels;
using Rico.Models;
using System.Windows;
using System.Collections.ObjectModel;

namespace Rico.ViewModels.Commands
{
	public class CollectValuesCommand : ICommand
	{
		// Instace of the base ViewModel
		public ParametersViewModel ViewModel { get; set; }
		// Class constructor
		public CollectValuesCommand(ParametersViewModel viewModel)
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
			var collection = parameter as ObservableCollection<Parameter>;
			if (collection.Count <= 0) return false;
			return true;
		}

		public void Execute(object parameter)
		{
			var duplicatedParameters = "";
			var amountOfDuplicates = 0;
			var collection = parameter as ObservableCollection<Parameter>;
			foreach (var item in collection) {
				if (ViewModel.FindDuplicateParametersInFile(item.ParameterName) == true) {
					amountOfDuplicates++;
					duplicatedParameters += ("->" + item.ParameterName + "\n");
				}
			}
			if (amountOfDuplicates > 0) {
				MessageBox.Show("Encontrou mais do que 1 ocorrência do(s) seguinte(s) parâmetro(s):\n" +
					duplicatedParameters + "Por favor especifique o código do parâmetro (ex: 'parâm,cód')");
				return;
			}
			else {
				foreach (var item in collection)
					ViewModel.CollectParametersValues(item.ParameterName);
			}
		}
	}
}