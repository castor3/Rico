using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using Rico.Models;
using SupportFiles;

namespace Rico.ViewModels
{
	public class ParametersViewModel : INotifyPropertyChanged
	{
		public ParametersViewModel()
		{
			ParametersCollection = new ObservableCollection<Parameter>() {
				new Parameter { ParameterName = "219" },	// P-ganho
				new Parameter { ParameterName = "118" },	// I-ganho
				new Parameter { ParameterName = "TR" },		// Correção Referência ferram.
				new Parameter { ParameterName = "374" },
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
		readonly string _baseMachineParameters = "MachineParameters.txt";
		readonly string _parametersFilesPaths = "machinepaths.txt";
		readonly string _CSVFilePath = "parameters_values.csv";

		string _machineParametersFilePath = string.Empty;
		IList<string> _listOfParameters = new List<string>();
		#endregion

		#region Properties
		public ICommand AddParameterCommand { get; }
		public ICommand RemoveParameterCommand { get; }
		public ICommand CollectValuesCommand { get; }

		private string _initialPathBoxContent;
		public string InitialPathBoxContent {
			get { return _initialPathBoxContent; }
			set {
				if (_initialPathBoxContent == value) return;
				_initialPathBoxContent = value;
				RaisePropertyChanged(nameof(InitialPathBoxContent));
			}
		}
		private string _parameterBoxContent;
		public string ParameterBoxContent {
			get { return _parameterBoxContent; }
			set {
				if (_parameterBoxContent == value) return;
				_parameterBoxContent = value;
				RaisePropertyChanged(nameof(ParameterBoxContent));
			}
		}
		private ObservableCollection<Parameter> _parametersCollection;
		public ObservableCollection<Parameter> ParametersCollection {
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
		public Parameter ParametersCollectionSelectedItem {
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
		public string StatusBarContent {
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
					ParameterBoxContent = string.Empty;
					return;
				}
			}
			ParametersCollection.Add(new Parameter { ParameterName = ParameterBoxContent });
			ParameterBoxContent = string.Empty;
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
			foreach (var item in ParametersCollection) {
				_listOfParameters.Add(item.ParameterName + " =");
			}
			GetPathsOfParametersFiles();
			foreach (var parameter in _listOfParameters) {
				var paramProperties = new ParameterProperties();
				foreach (var file in Document.YieldReturnLinesFromFile(_parametersFilesPaths)) {
					_machineParametersFilePath = file;
					var validationProperties = ValidateListedParameters();
					if (validationProperties.amountOfParametersNotFound > 0 || validationProperties.amountOfDuplicates > 0) {
						DisplayParametersErrorMessages(validationProperties);
						StatusBarContent = "Error collecting values";
						return;
					}
					CollectValidParameters(parameter, paramProperties);
					MessageBox.Show(paramProperties.parameterName + "=" + paramProperties.parameterAverage);
				}
				SaveParameterToCSV(paramProperties.parameterName, paramProperties.parameterAverage.ToString());
			}
			StatusBarContent = "Collected successfuly";
		}
		private void CollectValidParameters(string parameterFromList, ParameterProperties paramProperties)
		{
			var parameterLine = GetParameterFromFile(parameterFromList);

			if (string.IsNullOrWhiteSpace(parameterLine) || !parameterLine.Contains('=')) return;

			paramProperties.numberOfParametersFound++;
			paramProperties.foundParameter = true;
			var parameterNameAndValue = GetParameterNameAndValue(parameterLine);
			var parameterValueAsDouble = 0.0;
			var tryParseSuccessful = double.TryParse(parameterNameAndValue.Item2, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out parameterValueAsDouble);

			if (paramProperties.foundParameter == false || !tryParseSuccessful) {
				StatusBarContent = $"Error collecting values on parameter {parameterNameAndValue.Item1}";
				return;
			}
			else {
				paramProperties.parameterAverage += parameterValueAsDouble;
				paramProperties.parameterAverage /= paramProperties.numberOfParametersFound;
			}
		}
		public void GetPathsOfParametersFiles()
		{
			if (InitialPathBoxContent == null) {
				StatusBarContent = "Please input initial path";
				return;
			}
			var paths = Directory.GetFiles(InitialPathBoxContent, "machineparameters.txt", SearchOption.AllDirectories);
			Document.WriteToFile(_parametersFilesPaths, paths);
		}
		private ParameterValidation ValidateListedParameters()
		{
			var paramValidation = new ParameterValidation();
			foreach (var parameter in _listOfParameters) {
				if (FoundOccurrenceOfParameter(parameter) == false) {
					paramValidation.amountOfParametersNotFound++;
					paramValidation.parametersNotFound += ("->" + parameter + "\n");
				}
				if (FindDuplicateParametersInFile(parameter) == true) {
					paramValidation.amountOfDuplicates++;
					paramValidation.duplicatedParameters += ("->" + parameter + "\n");
				}
			}
			return paramValidation;
		}
		private bool FoundOccurrenceOfParameter(string parameter)
		{// Returns TRUE if it finds the parameter in the file, returns false if it doesn't find
			var array = parameter.Split(',');
			var arrayNotNullOrEmpty = (array.Count() < 1);
			foreach (var item in Document.YieldReturnLinesFromFile(_baseMachineParameters)) {
				if (arrayNotNullOrEmpty) {
					if ((item.Contains(array[0]) && item.Contains(array[array.Length - 1])))
						return true;
				}
				else {
					if (item.Contains(parameter))
						return true;
				}
			}
			return false;
		}
		private bool FindDuplicateParametersInFile(string parameter)
		{// Returns TRUE if finds duplicates of the parameter passed
			var found = 0;
			var array = parameter.Split(',');
			var arrayNotNullOrEmpty = (array.Count() < 1);
			foreach (var item in Document.YieldReturnLinesFromFile(_baseMachineParameters)) {
				if (arrayNotNullOrEmpty) {
					if ((item.Contains(array[0]) && item.Contains(array[array.Length - 1])))
						if (++found > 1) return true;
				}
				else {
					if (item.Contains(parameter))
						if (++found > 1) return true;
				}
			}
			return false;
		}
		private void DisplayParametersErrorMessages(ParameterValidation paramValidation)
		{
			if (paramValidation.amountOfParametersNotFound > 0) {
				MessageBox.Show("O(s) seguinte(s) parâmetro(s) não foi/foram encontrado(s):\n" +
					paramValidation.parametersNotFound + "Por favor verifique o texto inserido");
			}
			if (paramValidation.amountOfDuplicates > 0) {
				MessageBox.Show("Encontrou mais do que 1 ocorrência do(s) seguinte(s) parâmetro(s):\n" +
					paramValidation.duplicatedParameters + "Por favor verifique o parâmetro introduzido");
			}
		}
		private string GetParameterFromFile(string originalParameter)
		{
			var array = originalParameter.Split(',');
			var arrayNotNullOrEmpty = (array.Count() < 1);
			foreach (var item in Document.YieldReturnLinesFromFile(_machineParametersFilePath)) {
				if (arrayNotNullOrEmpty) {
					if ((item.Contains(array[0]) && item.Contains(array[array.Length - 1])))
						return item;
				}
				else {
					if (item.Contains(originalParameter))
						return item;
				}
			}
			return string.Empty;
		}
		private Tuple<string, string> GetParameterNameAndValue(string parameterLine)
		{// Receives the entire line of the parameter and returns a tuple with the name and the value
			int index = parameterLine.IndexOf('=');
			var parameterName = parameterLine.Remove(index);
			parameterLine = parameterLine.Substring(index + 1).Trim();
			var parameterValue = Regex.Split(parameterLine, @"[^0-9\.]+")
										.Where(c => c != "." && c.Trim() != "")
										.First();
			return new Tuple<string, string>(parameterName, parameterValue);
		}
		private void SaveParameterToCSV(string parameterName, string parameterValue)
		{
			Document.AppendToFile(_CSVFilePath, parameterName + ";" + parameterValue + "\n");
		}
		#endregion
	}
}
