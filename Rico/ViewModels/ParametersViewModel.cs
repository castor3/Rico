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
using System.Text;

namespace Rico.ViewModels
{
	public class ParametersViewModel : INotifyPropertyChanged
	{
		public ParametersViewModel()
		{
			ParametersCollection = new ObservableCollection<Parameter>() {
				new Parameter { Name = "219" },				// P-ganho
				//new Parameter { ParameterName = "118" },	// I-ganho
				new Parameter { Name = "TR" },				// Correção Referência ferram.
				new Parameter { Name = "374" },				// Ganho de paralelismo
				new Parameter { Name = "1496" },			// Maximum Y1Y2 difference
				//new Parameter { ParameterName = "Cursos" },
				//new Parameter { ParameterName = "Coisas" },
				//new Parameter { ParameterName = "Parametro" },
				//new Parameter { ParameterName = "Parametro1" },
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
		bool _firstCycle = true;
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
				if (item.Name == ParameterBoxContent) {
					StatusBarContent = "Parâmetro já foi adicionado à lista";
					ParameterBoxContent = string.Empty;
					return;
				}
			}
			ParametersCollection.Add(new Parameter { Name = ParameterBoxContent });
			ParameterBoxContent = string.Empty;
			StatusBarContent = "Added successfuly";
		}
		private bool CanRemoveParameter()
		{
			if (_parametersCollectionSelectedItem == null) return false;
			return !string.IsNullOrEmpty(_parametersCollectionSelectedItem.Name);
		}
		public void RemoveParameter()
		{
			foreach (var item in ParametersCollection) {
				if (item.Name == ParametersCollectionSelectedItem.Name) {
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
			_listOfParameters.Clear();
			foreach (var item in ParametersCollection) {
				_listOfParameters.Add(item.Name + " =");
			}
			GetPathsOfParametersFiles();
			foreach (var parameterFromList in _listOfParameters) {
				var parameter = new Parameter();
				_firstCycle = true;
				foreach (var file in Document.YieldReturnLinesFromFile(_parametersFilesPaths)) {
					//MessageBox.Show(file);
					_machineParametersFilePath = file;
					var validationProperties = ValidateListedParameters();
					if (validationProperties.NumberOfParametersNotFound > 0 || validationProperties.NumberOfDuplicates > 0) {
						DisplayParametersErrorMessages(validationProperties);
						StatusBarContent = "Error collecting values";
						return;
					}
					CollectValidParameter(parameterFromList, parameter);
					//MessageBox.Show(parameter.Name + " = " + parameter.Average);
				}
				parameter.Name = RemoveDiacritics(parameter.Name);
				SaveParameterToCSV(parameter.Name, parameter.Average.ToString());
			}
			Document.AppendToFile(_CSVFilePath, "\n");
			StatusBarContent = "Collected successfuly";
		}
		private void CollectValidParameter(string parameterFromList, Parameter parameter)
		{// Receives a "valid parameter" and gets its name and value from the "machineparameters.txt" file
			var parameterValueAsDouble = 0.0d;
			var tryParseSuccessful = false;
			var parameterLine = GetParameterFromFile(parameterFromList);
			if (string.IsNullOrWhiteSpace(parameterLine) || !parameterLine.Contains('=')) return;

			parameter.NumberOfOcurrencesFound++;
			parameter.DidFindParameter = true;

			if (_firstCycle) {
				parameter.Name = GetParameterName(parameterLine);
				_firstCycle = false;
			}

			var parameterValue = GetParameterValue(parameterLine);

			if (parameterValue == null)
				parameter.DidFindParameter = false;
			else {
				tryParseSuccessful = double.TryParse(parameterValue, out parameterValueAsDouble);
			}

			if (!parameter.DidFindParameter || !tryParseSuccessful) {
				StatusBarContent = $"Error collecting values on parameter {parameter.Name}";
				return;
			}
			else {
				parameter.Average += parameterValueAsDouble;
				parameter.Average /= parameter.NumberOfOcurrencesFound;
			}
		}
		private string GetParameterName(string parameterLine)
		{
			var regexResult = Regex.Match(parameterLine, @" {2}(([\w\-]+ ?)+) +(\w+)");
			if (!regexResult.Success) return string.Empty;

			var parameterName = string.Empty;
			for (int i = 0; i < regexResult.Groups.Count; i++) {
				parameterName = regexResult.Groups[i].Value;
			}
			parameterName += "(" + regexResult.Groups[regexResult.Groups.Count - 1].Value + ")";
			return parameterName;
		}
		private void GetPathsOfParametersFiles()
		{// What: Gets all paths for the parameters files, recursively, starting on the "InitialPathBox" path
		 // Why: To have a list with the paths of all the "machineparameters.txt" from which we will retrieve the values
			if (InitialPathBoxContent == null) {
				StatusBarContent = "Please input initial path";
				return;
			}
			var paths = Directory.GetFiles(InitialPathBoxContent, "machineparameters.txt", SearchOption.AllDirectories);
			Document.WriteToFile(_parametersFilesPaths, paths);
		}
		private ParameterValidation ValidateListedParameters()
		{// What: Check if the parameter exists in the file and if it does not have duplicates
		 // Why: To know if it's OK to proceed with retrieving the parameter name and value from the file
			var paramValidation = new ParameterValidation();
			foreach (var parameter in _listOfParameters) {
				if (CheckIfParameterExistsInFile(parameter) == false) {
					paramValidation.NumberOfParametersNotFound++;
					paramValidation.ParametersNotFound += ("->" + parameter + "\n");
				}
				if (SearchForDuplicatedParameterInFile(parameter) == true) {
					paramValidation.NumberOfDuplicates++;
					paramValidation.DuplicatedParameters += ("->" + parameter + "\n");
				}
			}
			return paramValidation;
		}
		private bool CheckIfParameterExistsInFile(string parameter)
		{// What: Returns TRUE if it finds the parameter in the file, returns false if it doesn't find
		 // Why: To know if the parameter exists in the file
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
		private bool SearchForDuplicatedParameterInFile(string parameter)
		{// Returns TRUE if finds duplicates of the parameter passed
		 // Why: We can only get the value of each parameter, once from each file. If it finds the same parameter
		 //		more than once, there's something wrong, and so the user will have to rechecked what he typed
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
		{// !!! Messages will override eachother if they happen to be written to the screen at the same time
			if (paramValidation.NumberOfParametersNotFound > 0) {
				MessageBox.Show("O(s) seguinte(s) parâmetro(s) não foi/foram encontrado(s):\n" +
					paramValidation.ParametersNotFound + "Por favor verifique o texto inserido");
			}
			if (paramValidation.NumberOfDuplicates > 0) {
				MessageBox.Show("Encontrou mais do que 1 ocorrência do(s) seguinte(s) parâmetro(s):\n" +
					paramValidation.DuplicatedParameters + "Por favor verifique o parâmetro introduzido");
			}
		}
		private string GetParameterFromFile(string originalParameter)
		{// Retrieves, from the parameters file, the full line of the parameter passed
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
		private string GetParameterValue(string parameterLine)
		{// Receives the entire line of the parameter and returns a tuple with the name and the value
			byte index = (byte)parameterLine.IndexOf('=');
			parameterLine = parameterLine.Substring(index + 1).Trim();
			var parameterValue = Regex.Split(parameterLine, @"[^0-9\.]+")
										.Where(c => c != "." && c.Trim() != "")
										.First();
			return parameterValue;
		}
		private string RemoveDiacritics(string text)
		{// Replaces accented letters with equivalent ones (normalizes the string)
			var normalizedString = text.Normalize(NormalizationForm.FormD);
			var stringBuilder = new StringBuilder();

			foreach (var character in normalizedString) {
				var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(character);
				if (unicodeCategory != UnicodeCategory.NonSpacingMark) {
					stringBuilder.Append(character);
				}
			}
			return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
		}
		private void SaveParameterToCSV(string parameterName, string parameterValue)
		{
			var valueToSave = parameterName + "," + parameterValue + "\n";
			Document.AppendToFile(_CSVFilePath, valueToSave);
		}
		#endregion
	}
}
