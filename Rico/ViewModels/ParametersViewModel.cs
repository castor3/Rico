using Rico.Models;
using SupportFiles;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Rico.ViewModels
{
	public class ParametersViewModel : INotifyPropertyChanged
	{
		public ParametersViewModel()
		{
			ParametersCollection = new ObservableCollection<Parameter>() {
				new Parameter { ParameterName = "P-ganho,3895" },
				new Parameter { ParameterName = "I-ganho,4797" },
				new Parameter { ParameterName = "Correção Referência ferram." },
				new Parameter { ParameterName = "Horas" },
				new Parameter { ParameterName = "Cursos" },
				new Parameter { ParameterName = "Coisas" },
				new Parameter { ParameterName = "Parametro" },
				new Parameter { ParameterName = "Parametro1" },
			};
			AddParameterCommand = new RelayCommand(CanAddParameter, AddParameter);
			RemoveParameterCommand = new RelayCommand(CanRemoveParameter, RemoveParameter);
			CollectValuesCommand = new RelayCommand(CanCollectValues, CollectValues);
			StatusBarContent = "Inicio";
		}

		#region Fields
		readonly Parameter _model = new Parameter();
		readonly string _machineParametersFilePath = @"machineparameters_original.txt";
		readonly string _CSVFilePath = @"parameters_values.csv";
		#endregion

		#region Properties
		public ICommand AddParameterCommand { get; }
		public ICommand RemoveParameterCommand { get; }
		public ICommand CollectValuesCommand { get; }

		private string _initialPathBoxContent;
		public string InitialPathBoxContent
		{
			get { return _initialPathBoxContent; }
			set {
				if (_initialPathBoxContent == value) return;
				_initialPathBoxContent = value;
				RaisePropertyChanged(nameof(InitialPathBoxContent));
			}
		}
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
				if (_parametersCollection == value) return;
				_parametersCollection = value;
				RaisePropertyChanged(nameof(ParametersCollection));
			}
		}
		private Parameter _parametersCollectionSelectedItem;
		public Parameter ParametersCollectionSelectedItem
		{
			get {
				return _parametersCollectionSelectedItem;
			}
			set {
				if (_parametersCollectionSelectedItem == value) return;
				_parametersCollectionSelectedItem = value;
				RaisePropertyChanged(nameof(ParametersCollectionSelectedItem));
			}
		}
		private string _statusBarContent;
		public string StatusBarContent
		{
			get {
				return _statusBarContent;
			}
			set {
				if (_statusBarContent == value) return;
				_statusBarContent = value;
				RaisePropertyChanged(nameof(StatusBarContent));
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
		private bool CanAddParameter()
		{
			return !string.IsNullOrEmpty(ParameterBoxContent);
		}
		public void AddParameter()
		{
			foreach (var item in ParametersCollection) {
				if (item.ParameterName == ParameterBoxContent) {
					StatusBarContent = "Parâmetro já foi adicionado à lista";
					ParameterBoxContent = "";
					return;
				}
			}
			ParametersCollection.Add(new Parameter { ParameterName = ParameterBoxContent });
			ParameterBoxContent = "";
			StatusBarContent = "Added successfuly";
		}
		private bool CanRemoveParameter()
		{
			if (_parametersCollectionSelectedItem == null) return false;
			return !string.IsNullOrEmpty(_parametersCollectionSelectedItem.ParameterName);
		}
		public void RemoveParameter()
		{
			foreach (var item in ParametersCollection) {
				if (item.ParameterName == ParametersCollectionSelectedItem.ParameterName) {
					ParametersCollection.Remove(item);
					StatusBarContent = "Removed successfuly";
					return;
				}
			}
		}
		private bool CanCollectValues()
		{
			return ParametersCollection.Count > 0 ? true : false;
		}
		public void CollectValues()
		{
			var parametersNotFound = "";
			var duplicatedParameters = "";
			var amountOfParametersNotFound = 0;
			var amountOfDuplicates = 0;
			var parameterWithNotEnoughChars = false;
			ValidateListedParameters(ref parametersNotFound, ref duplicatedParameters, ref amountOfParametersNotFound, ref amountOfDuplicates, ref parameterWithNotEnoughChars);
			if (amountOfParametersNotFound > 0 || amountOfDuplicates > 0 || parameterWithNotEnoughChars == true) {
				DisplayParametersErrorMessages(parametersNotFound, duplicatedParameters, amountOfParametersNotFound, amountOfDuplicates, parameterWithNotEnoughChars);
			}
			else {
				foreach (var item in ParametersCollection) {
					var parameterLine = GetParameterFromFile(item.ParameterName);
					var parameterNameAndValue = HandleParameterValue(parameterLine);
					SaveParameterToCSV(parameterNameAndValue);
				}
				StatusBarContent = "Collected successfuly";
			}
		}
		private void ValidateListedParameters(ref string parametersNotFound, ref string duplicatedParameters, ref int amountOfParametersNotFound, ref int amountOfDuplicates, ref bool parameterWithNotEnoughChars)
		{
			foreach (var item in ParametersCollection) {
				if (CheckMinimumAmountOfCharacters(item.ParameterName) == false) parameterWithNotEnoughChars = true;
				if (FoundOccurrenceOfParameter(item.ParameterName) == false) {
					amountOfParametersNotFound++;
					parametersNotFound += ("->" + item.ParameterName + "\n");
				}
				if (FindDuplicateParametersInFile(item.ParameterName) == true) {
					amountOfDuplicates++;
					duplicatedParameters += ("->" + item.ParameterName + "\n");
				}
			}
		}
		private bool CheckMinimumAmountOfCharacters(string parameter)
		{// Return true for enough chars
			var array = parameter.Split(',');
			if (array[0].Length > 2) return true;
			return false;
		}
		private bool FoundOccurrenceOfParameter(string parameter)
		{// Returns TRUE if it finds the parameter in the file, returns false if it doesn't find
			var array = parameter.Split(',');
			foreach (var item in Document.YieldReturnLinesFromFile(_machineParametersFilePath)) {
				if (item.Contains(array[0]) && item.Contains(array[array.Length - 1])) return true;
			}
			return false;
		}
		private bool FindDuplicateParametersInFile(string parameter)
		{// Returns TRUE if finds duplicates of the parameter passed
			var found = 0;
			var array = parameter.Split(',');
			foreach (var item in Document.YieldReturnLinesFromFile(_machineParametersFilePath)) {
				if (item.Contains(array[0]) && item.Contains(array[array.Length - 1])) {
					if (++found > 1) return true;
				}
			}
			return false;
		}
		private void DisplayParametersErrorMessages(string parametersNotFound, string duplicatedParameters, int amountOfParametersNotFound, int amountOfDuplicates, bool parameterWithNotEnoughChars)
		{
			if (parameterWithNotEnoughChars == true)
				MessageBox.Show("Parameters needs to have more than 3 characters");
			if (amountOfParametersNotFound > 0) {
				MessageBox.Show("O(s) seguinte(s) parâmetro(s) não foi/foram encontrado(s):\n" +
					parametersNotFound + "Por favor verifique o texto inserido");
			}
			if (amountOfDuplicates > 0) {
				MessageBox.Show("Encontrou mais do que 1 ocorrência do(s) seguinte(s) parâmetro(s):\n" +
					duplicatedParameters + "Por favor especifique o código do parâmetro (ex: 'parâm,cód')");
			}
		}
		private string GetParameterFromFile(string originalParameter)
		{
			var array = originalParameter.Split(',');
			foreach (var item in Document.YieldReturnLinesFromFile(_machineParametersFilePath)) {
				if (item.Contains(array[0]) && item.Contains(array[array.Length - 1])) return item;
			}
			return "";
		}
		private Tuple<string, string> HandleParameterValue(string parameterLine)
		{// Receives the entire line of the parameter and returns a tuple with the name and the value
			if (string.IsNullOrEmpty(parameterLine)) return new Tuple<string, string>("", "");

			if (!parameterLine.Contains('=')) return new Tuple<string, string>("", "");

			int index = parameterLine.IndexOf('=');
			var parameterName = parameterLine.Remove(index);
			parameterLine = parameterLine.Substring(index + 1).Trim();
			var parameterValue = Regex.Split(parameterLine, @"[^0-9\.]+").Where(c => c != "." && c.Trim() != "").First();
			return new Tuple<string, string>(parameterName, parameterValue);
		}
		private void SaveParameterToCSV(Tuple<string, string> parameterNameAndValue)
		{
			Document.AppendToFile(_CSVFilePath, parameterNameAndValue.Item1 + ";" + parameterNameAndValue.Item2 + "\n");
		}
		#endregion
	}
}
