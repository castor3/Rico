﻿using System;
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
using System.Windows.Threading;

namespace Rico.ViewModels
{
	public class ParametersViewModel : INotifyPropertyChanged
	{
		public ParametersViewModel()
		{
			ParametersCollection = new ObservableCollection<Parameter>() {
				new Parameter { Code = "3895" },			// P-ganho
				new Parameter { Code = "3915" },			// Ganho de paralelismo
				new Parameter { Code = "1496" },			// Maximum Y1Y2 difference
				new Parameter { Code = "4960" },			// Pressão (subindo)
			};
			AddParameterCommand = new RelayCommand(CanAddParameter, AddParameter);
			RemoveParameterCommand = new RelayCommand(CanRemoveParameter, RemoveParameter);
			CollectValuesCommand = new RelayCommand(CanCollectValues, CollectValues);
		}

		#region Fields
		readonly string _CSVFilePath = "parameters_values.csv";

		string _machineParametersFilePath = string.Empty;
		IList<string> _listOfParametersCode = new List<string>();
		IEnumerable<string> _listOfValidParameters;
		bool _isFirstCycle = true;
		#endregion

		#region Properties
		public ICommand AddParameterCommand { get; }
		public ICommand RemoveParameterCommand { get; }
		public ICommand CollectValuesCommand { get; }

		private string _initialPathBoxContent = Directory.GetCurrentDirectory();
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
		private string _statusBarContent = "Pronto";
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
		readonly string _baseMachineParameters = "Parameters.txt";
		public string BaseMachineParameters => _baseMachineParameters;
		readonly string _parametersFilesPaths = "machinepaths.txt";
		public string ParametersFilesPaths => _parametersFilesPaths;
		private string _nameOfFileToSearch;
		public string NameOfFileToSearch => _nameOfFileToSearch;
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
					UpdateStatusBar("Parâmetro já foi adicionado à lista");
					ParameterBoxContent = string.Empty;
					return;
				}
			}
			ParametersCollection.Add(new Parameter { Name = ParameterBoxContent });
			ParameterBoxContent = string.Empty;
			UpdateStatusBar("Added successfully");
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
					UpdateStatusBar("Removed successfully");
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
			if (!File.Exists(BaseMachineParameters)) {
				UpdateStatusBar("There's a file missing");
				return;
			}

			if (_listOfParametersCode.Any())
				_listOfParametersCode.Clear();

			foreach (var item in ParametersCollection) {
				_listOfParametersCode.Add(item.Code + " =");
			}

			if (string.IsNullOrWhiteSpace(InitialPathBoxContent))
				InitialPathBoxContent = Directory.GetCurrentDirectory();

			if (!GetPathsOfParametersFiles()) {
				UpdateStatusBar("Failed to find parameters files");
				return;
			}

			var validationProperties = ValidateListedParameters();
			if (validationProperties.NumberOfParametersNotFound > 0/* || validationProperties.NumberOfDuplicates > 0*/) {
				DisplayParametersErrorMessages(validationProperties);
				return;
			}

			_listOfValidParameters = _listOfParametersCode.Except(validationProperties.DuplicatedParameters);

			foreach (var parameterFromList in _listOfValidParameters) {
				var parameter = new Parameter();
				_isFirstCycle = true;
				foreach (var file in Document.YieldReturnLinesFromFile(ParametersFilesPaths)) {
					_machineParametersFilePath = file;
					if (!CollectValidParameter(parameterFromList, parameter)) {
						UpdateStatusBar($"Error collecting values, the parameter '{parameterFromList.Trim('=').Trim()}' doesn't have a value to collect");
						return;
					}
				}
				parameter.Average /= parameter.NumberOfOcurrences;
				parameter.Name = Text.RemoveDiacritics(parameter.Name);
				if (parameter.Name == string.Empty) {
					UpdateStatusBar("Error collecting values");
					return;
				}
				SaveParameterToCSV(parameter.Name, parameter.Average.ToString());
			}
			Document.AppendToFile(_CSVFilePath, "\n");
			UpdateStatusBar("Collected successfully");
		}
		private bool CollectValidParameter(string parameterFromList, Parameter parameter)
		{// Receives a "valid parameter" and gets its name and value from the "machineparameters.txt" file
			parameter.ParameterLine = GetParameterFromFile(parameterFromList);

			// If the "ParameterLine" is empty is probably because it searched in an incompatible file
			// Anyway, it will proceed ('return true;') to the next file without throwing an error
			if (string.IsNullOrWhiteSpace(parameter.ParameterLine)/* || !parameter.ParameterLine.Contains('=')*/) return true;

			parameter.NumberOfOcurrences++;

			if (_isFirstCycle) {
				parameter.GetParameterName();
				_isFirstCycle = false;
			}

			parameter.GetParameterValue();
			var parameterValue = parameter.Value;

			if (string.IsNullOrWhiteSpace(parameterValue)) return false;

			var parameterValueAsDouble = 0.0d;
			if (!double.TryParse(parameterValue, out parameterValueAsDouble)) {
				UpdateStatusBar($"Error collecting value of parameter: {parameter.Name}");
				return false;
			}
			else {
				parameter.Average += parameterValueAsDouble;
				return true;
			}
		}
		private bool GetPathsOfParametersFiles()
		{// What: Gets all paths for the parameters files, recursively, starting on the "InitialPathBox" path
		 // Why: To have a list with the paths of all the "machineparameters.txt" from which we will retrieve the values
			var paths = Directory.GetFiles(InitialPathBoxContent, "machineparameters.txt", SearchOption.AllDirectories);
			if (!paths.Any()) return false;
			Document.WriteToFile(ParametersFilesPaths, paths);
			return true;
		}
		private ParameterValidation ValidateListedParameters()
		{// What: Check if the parameter exists in the file and if it does not have duplicates
		 // Why: To know if it's OK to proceed with retrieving the parameter name and value from the file
			var paramValidation = new ParameterValidation();
			foreach (var parameterCode in _listOfParametersCode) {
				if (!CheckIfParameterExistsInFile(parameterCode)) {
					paramValidation.NumberOfParametersNotFound++;
					paramValidation.ParametersNotFound += (parameterCode + "\n");
				}
				if (SearchForDuplicatedParameterInFile(parameterCode)) {
					paramValidation.NumberOfDuplicates++;
					paramValidation.DuplicatedParameters.Add(parameterCode);
				}
			}
			return paramValidation;
		}
		private bool CheckIfParameterExistsInFile(string parameter)
		{// What: Returns TRUE if it finds the parameter in the file, returns false if it doesn't find
		 // Why: Simply to know if the parameter exists in the file
			var array = parameter.Split(',');
			var arrayIsNotNullOrEmpty = array.Any();
			foreach (var item in Document.YieldReturnLinesFromFile(BaseMachineParameters)) {
				if (arrayIsNotNullOrEmpty) {
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
			foreach (var item in Document.YieldReturnLinesFromFile(BaseMachineParameters)) {
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
		{// !!! Messages will override each other if they happen to be written to the screen at the same time
			if (paramValidation.NumberOfParametersNotFound > 0) {
				MessageBox.Show("O(s) seguinte(s) parâmetro(s) não foi/foram encontrado(s):\n" +
					paramValidation.ParametersNotFound + "Por favor verifique o texto inserido");
			}
			//if (paramValidation.NumberOfDuplicates > 0) {
			//	MessageBox.Show("Encontrou mais do que 1 ocorrência do(s) seguinte(s) parâmetro(s):\n" +
			//		paramValidation.DuplicatedParameters + "Por favor verifique o parâmetro introduzido");
			//}
		}
		private string GetParameterFromFile(string parameterFromList)
		{// Retrieves, from the parameters file, the full line of the parameter passed
			var array = parameterFromList.Split(',');
			var hasSplited = array.Count() > 1;
			foreach (var item in Document.YieldReturnLinesFromFile(_machineParametersFilePath)) {
				if (hasSplited) {
					if ((item.Contains(array[0]) && item.Contains(array[array.Length - 1])))
						return item;
				}
				else {
					if (item.Contains(parameterFromList))
						return item;
				}
			}
			return string.Empty;
		}
		private void SaveParameterToCSV(string parameterName, string parameterValue)
		{
			var valueToSave = parameterName + "," + parameterValue + "\n";
			Document.AppendToFile(_CSVFilePath, valueToSave);
		}
		#region StatusBar
		// Status bar update
		private void SetStatusBarTimer()
		{//  DispatcherTimer setup
			DispatcherTimer timer = new DispatcherTimer();
			timer.Tick += new EventHandler(StatusBarTimer_Tick);
			timer.Interval = new TimeSpan(0, 0, 0, 2, 500);
			timer.Stop();
			timer.Start();
		}
		private void StatusBarTimer_Tick(object sender, EventArgs e)
		{
			DispatcherTimer timer = (DispatcherTimer)sender;
			timer.Stop();
			StatusBarContent = "Pronto";
		}
		private void UpdateStatusBar(string msg)
		{
			StatusBarContent = msg;
			SetStatusBarTimer();
		}
		#endregion
		#endregion
	}
}
