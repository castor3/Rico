using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Rico.Models;
using SupportFiles;

namespace Rico.ViewModels
{
	public class ParametersViewModel : INotifyPropertyChanged
	{
		public ParametersViewModel()
		{
			ParametersCollection = new ObservableCollection<Parameter>() {
				new Parameter { Code = "3895" },	// P-ganho
				new Parameter { Code = "3915" },	// Ganho de paralelismo
				new Parameter { Code = "1496" },	// Maximum Y1Y2 difference
				new Parameter { Code = "4960" },	// Pressão (subindo)
			};
			NameOfFileToSearch = "Untitled folder";
			AddParameterCommand = new RelayCommand(CanAddParameter, AddParameter);
			RemoveParameterCommand = new RelayCommand(CanRemoveParameter, RemoveParameter);
			CollectValuesCommand = new RelayCommand(CanCollectValues, ExecuteCollectValues);
		}

		#region Fields
		string _currentMachineParametersFile = string.Empty;
		IList<string> _listOfParametersCode = new List<string>();
		IEnumerable<string> _listOfValidParameters;
		bool _isFirstCycle = true;
		#endregion

		#region Properties
		public ICommand AddParameterCommand { get; }
		public ICommand RemoveParameterCommand { get; }
		public ICommand CollectValuesCommand { get; }

		private string _initialPathBoxContent = Directory.GetCurrentDirectory();
		private string _parameterBoxContent;
		private ObservableCollection<Parameter> _parametersCollection;
		private Parameter _parametersCollectionSelectedItem;
		private string _statusBarContent = "Pronto";
		private string _nameOfFileToSearch;

		public string InitialPathBoxContent
		{
			get { return _initialPathBoxContent; }
			set {
				if (_initialPathBoxContent == value || value == null) return;
				_initialPathBoxContent = value;
				RaisePropertyChanged(nameof(InitialPathBoxContent));
			}
		}
		public string ParameterBoxContent
		{
			get { return _parameterBoxContent; }
			set {
				if (_parameterBoxContent == value || value == null) return;
				_parameterBoxContent = value;
				RaisePropertyChanged(nameof(ParameterBoxContent));
			}
		}
		public ObservableCollection<Parameter> ParametersCollection
		{
			get {
				return _parametersCollection;
			}
			set {
				if (_parametersCollection == value || value == null) return;
				_parametersCollection = value;
				RaisePropertyChanged(nameof(ParametersCollection));
			}
		}
		public Parameter ParametersCollectionSelectedItem
		{
			get {
				return _parametersCollectionSelectedItem;
			}
			set {
				if (_parametersCollectionSelectedItem == value || value == null) return;
				_parametersCollectionSelectedItem = value;
				RaisePropertyChanged(nameof(ParametersCollectionSelectedItem));
			}
		}
		public string StatusBarContent
		{
			get {
				return _statusBarContent;
			}
			set {
				if (_statusBarContent == value || value == null) return;
				_statusBarContent = value;
				RaisePropertyChanged(nameof(StatusBarContent));
			}
		}
		public string CSVFilePath => "parameters_values.csv";
		public string BaseMachineParameters => "Parameters.txt";
		public string ParametersFilesPaths => "machinepaths.txt";
		public static string LogFilePath => "output_log.txt";
		public string NameOfFileToSearch
		{
			get { return _nameOfFileToSearch; }
			set {
				if (value != _nameOfFileToSearch && value != null)
					_nameOfFileToSearch = value;
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
					UpdateStatusBar("Parâmetro já foi adicionado à lista");
					ParameterBoxContent = string.Empty;
					return;
				}
			}
			ParametersCollection.Add(new Parameter { Name = ParameterBoxContent });
			ParameterBoxContent = string.Empty;
			UpdateStatusBar("Added successfully");
			return;
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
		public void ExecuteCollectValues()
		{// Method used by the "CollectValuesCommand"
			CollectValues();
		}
		public bool CollectValues()
		{
			if (string.IsNullOrWhiteSpace(NameOfFileToSearch)) {
				UpdateStatusBar("Need to specify the machine name/model");
				return false;
			}

			if (!File.Exists(BaseMachineParameters)) {
				UpdateStatusBar("There's a file missing");
				Document.WriteToLogFile(LogFilePath, $"In method: '{GetCurrentMethod()}()' -> '{BaseMachineParameters}' file is missing.");
				return false;
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
				Document.WriteToLogFile(LogFilePath, $"In method: '{nameof(GetPathsOfParametersFiles)}()' -> Didn't find any 'machineparameters.txt' file.");
				return false;
			}

			var validationProperties = new ParameterValidation();
			validationProperties.ValidateListedParameters(_listOfParametersCode, BaseMachineParameters);
			if (validationProperties.NumberOfParametersNotFound > 0/* || validationProperties.NumberOfDuplicates > 0*/) {
				DisplayParametersErrorMessages(validationProperties);
				Document.WriteToLogFile(LogFilePath, $"In method: '{nameof(validationProperties.ValidateListedParameters)}()' -> Failed to validate all the parameters from the list box.");
				return false;
			}

			_listOfValidParameters = _listOfParametersCode.Except(validationProperties.DuplicatedParameters);

			foreach (var parameterFromList in _listOfValidParameters) {
				var parameter = new Parameter();
				_isFirstCycle = true;
				foreach (var file in Document.YieldReturnLinesFromFile(ParametersFilesPaths)) {
					_currentMachineParametersFile = file;
					if (!CollectValidParameter(parameterFromList, parameter)) {
						UpdateStatusBar($"Error collecting values");
						Document.WriteToLogFile(LogFilePath, $"In method: '{nameof(CollectValidParameter)}()' -> Error collecting values, the parameter '{parameterFromList.Trim('=').Trim()}' doesn't have a value to collect.");
						return false;
					}
				}
				parameter.Average /= parameter.NumberOfOcurrences;
				parameter.Name = Text.RemoveDiacritics(parameter.Name);
				if (parameter.Name == string.Empty) {
					UpdateStatusBar("Error collecting values");
					Document.WriteToLogFile(LogFilePath, $"In method: '{GetCurrentMethod()}()' -> Error collecting values on parameter '{parameter.Name}', name returned empty.");
					return false;
				}
				SaveParameterToCSV(parameter.Name, parameter.Average.ToString());
			}
			Document.AppendToFile(CSVFilePath, "\n");
			UpdateStatusBar("Collected successfully");
			Document.WriteToLogFile(LogFilePath, $"Collected values successfully");
			return true;
		}
		private bool CollectValidParameter(string parameterFromList, Parameter parameter)
		{// Receives a "valid parameter" and gets its name and value from the "machineparameters.txt" file
			parameter.ParameterLine = GetParameterFromFile(parameterFromList);

			// If the "ParameterLine" is empty, is probably because it searched in an incompatible file
			// Anyway, it will proceed ('return true;') to the next file without throwing an error
			if (string.IsNullOrWhiteSpace(parameter.ParameterLine)/* || !parameter.ParameterLine.Contains('=')*/) return true;

			parameter.NumberOfOcurrences++;

			if (_isFirstCycle) {
				parameter.GetParameterName();
				_isFirstCycle = false;
			}

			parameter.GetParameterValue();
			var parameterValue = parameter.Value;

			if (string.IsNullOrWhiteSpace(parameterValue)) {
				if (parameter.Ignore)
					return true;
				return false;
			}

			var parameterValueAsDouble = 0.0d;
			if (!double.TryParse(parameterValue, out parameterValueAsDouble)) {
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
			var paths = Directory.GetFiles(InitialPathBoxContent, "machineparameters.txt", SearchOption.AllDirectories).ToList();

			if (!paths.Any()) return false;

			for (int i = paths.Count - 1; i >= 0; i--) {
				// Check if the file is in a folder that the user wants to see (ex: contains the name/model of the machine)
				if (!paths[i].ToLower().Contains(NameOfFileToSearch.ToLower())) {
					paths.RemoveAt(i);
					continue;
				}
				// Check if the file is of an incompatible version
				var listOfIncompatibleFileVersions = new Collection<string>() { "V2.2.10", /* Vx.x.xx */ };

				var versionLine = Document.YieldReturnLinesFromFile(paths[i]).Skip(2).First();

				var isIncompatible = listOfIncompatibleFileVersions.Any(version => versionLine.Contains(version));

				if (isIncompatible)
					paths.RemoveAt(i);
			}
			Document.WriteToFile(ParametersFilesPaths, paths.ToArray());
			return true;
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
			foreach (var item in Document.YieldReturnLinesFromFile(_currentMachineParametersFile)) {
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
			Document.AppendToFile(CSVFilePath, valueToSave);
		}
		[MethodImpl(MethodImplOptions.NoInlining)]
		public string GetCurrentMethod() => new StackTrace().GetFrame(1).GetMethod().Name;
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
