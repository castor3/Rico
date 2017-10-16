using Aux;
using Rico.Models;
using Rico.ViewModels.Commands;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace Rico.ViewModels
{
	public class ParametersViewModel : INotifyPropertyChanged
	{
		#region Class constructor
		public ParametersViewModel()
		{
			ParametersCollection = new ObservableCollection<Parameter>() {
				new Parameter { ParameterName = "P-ganho,3895" },
				new Parameter { ParameterName = "I-ganho" },
				//new Parameter { ParameterName = "Correção Referência ferram." },
				//new Parameter { ParameterName = "Horas" },
				//new Parameter { ParameterName = "Cursos" },
			};
			AddParameterCommand = new AddParameterCommand(this);
			CollectValuesCommand = new CollectValuesCommand(this);
		}
		#endregion

		#region Fields
		readonly Parameter _model = new Parameter();
		readonly string _machineParametersFilePath = @"machineparameters_original.txt";
		readonly string _CSVFilePath = @"parameters_values.csv";
		#endregion

		#region Properties
		public AddParameterCommand AddParameterCommand { get; }
		public CollectValuesCommand CollectValuesCommand { get; }

		private string _parameterBoxContent;
		public string ParameterBoxContent
		{
			get { return _parameterBoxContent; }
			set {
				if (_parameterBoxContent == value) return;
				_parameterBoxContent = value;
				RaisePropertyChanged(nameof(ParameterBoxContent));
			}
		}
		private ObservableCollection<Parameter> _parametersCollection;
		public ObservableCollection<Parameter> ParametersCollection
		{
			get {
				return _parametersCollection;
			}
			set {
				_parametersCollection = value;
				RaisePropertyChanged(nameof(ParametersCollection));
			}
		}
		#endregion

		#region Property changed Event
		public event PropertyChangedEventHandler PropertyChanged;

		private void RaisePropertyChanged(string property)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
		}
		#endregion

		#region Methods
		public void LoadParameters()
		{
			//ParametersCollection = _model.ParametersCollection;
		}
		public void AddParameter(string parameter)
		{
			foreach (var item in ParametersCollection) {
				if (item.ParameterName == parameter) {
					MessageBox.Show("Parâmetro já foi adicionado à lista");
					ParameterBoxContent = "";
					return;
				}
			}
			ParametersCollection.Add(new Parameter { ParameterName = parameter });
			ParameterBoxContent = "";
		}
		public void RemoveParameter(string parameter)
		{
			foreach (var item in ParametersCollection) {
				if (item.ParameterName == parameter) {
					ParametersCollection.Remove(item);
					return;
				}
			}
		}
		public void CollectParametersValues(string originalParameter)
		{
			var parameterLine = SearchParameterInFile(originalParameter);
			if (parameterLine == null || string.IsNullOrEmpty(parameterLine) || string.IsNullOrWhiteSpace(parameterLine)) return;
			var parameterNameAndValue = HandleParameterValue(parameterLine);
			Document.AppendToFile(_CSVFilePath, parameterNameAndValue.Item1 + ";" + parameterNameAndValue.Item2 + "\n");
		}
		private string SearchParameterInFile(string originalParameter)
		{
			var line = "";
			var found = 0;
			var array = originalParameter.Split(',');
			foreach (var item in Document.YieldReturnLinesFromFile(_machineParametersFilePath)) {
				if (item.Contains(array[0]) && item.Contains(array[array.Length-1])) {
					found++;
					line = item;
				}
			}
			if (found > 1) {
				MessageBox.Show("Encontrou mais do que 1 ocorrência do parâmetro:\n" + originalParameter +
					"\nPor favor especifique o código do parâmetro");
				return null;
			}
			else
				return line;
		}
		private Tuple<string, string> HandleParameterValue(string parameterLine)
		{// Receives the entire line of the parameter and returns a tuple with the name and the value
			if (parameterLine == null || !parameterLine.Contains('=')) return new Tuple<string, string>("", "");

			int index = parameterLine.IndexOf('=');

			var parameterName = parameterLine.Remove(index);
			//MessageBox.Show("<parameterName>:\n" + parameterName);

			parameterLine = parameterLine.Substring(index + 1).Trim();
			//MessageBox.Show("<parameterLine.'substringed'>:\n" + parameterLine);

			var parameterValue = Regex.Split(parameterLine, @"[^0-9\.]+").Where(c => c != "." && c.Trim() != "").First();
			//MessageBox.Show("<Value>:\n" + parameterValue);

			return new Tuple<string, string>(parameterName, parameterValue);
		}
		#endregion
	}
}
