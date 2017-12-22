using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
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
			ParametersCollection = new ObservableCollection<ParameterModel>() {
				new ParameterModel { Code = "3895" },	// P-ganho
				new ParameterModel { Code = "3915" },	// Ganho de paralelismo
				new ParameterModel { Code = "1496" },	// Maximum Y1Y2 difference
				new ParameterModel { Code = "4960" },	// Pressão (subindo)
			};
			NameOfFileToSearch = "Untitled folder";
			AddParameterCommand = new RelayCommand(CanAddParameter, AddParameter);
			RemoveParameterCommand = new RelayCommand(CanRemoveParameter, RemoveParameter);
			CollectValuesCommand = new RelayCommand(CanCollectValues, ExecuteCollectValues);
		}

		#region Fields
		private IList<string> _listOfParametersCode = new List<string>();
		private IEnumerable<string> _listOfValidParameters;
		#endregion

		#region Properties
		public ICommand AddParameterCommand { get; }
		public ICommand RemoveParameterCommand { get; }
		public ICommand CollectValuesCommand { get; }

		private string _initialPathBoxContent = Directory.GetCurrentDirectory();
		private string _parameterBoxContent;
		private ObservableCollection<ParameterModel> _parametersCollection;
		private ParameterModel _parametersCollectionSelectedItem;
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
		public ObservableCollection<ParameterModel> ParametersCollection
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
		public ParameterModel ParametersCollectionSelectedItem
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
			ParametersCollection.Add(new ParameterModel { Name = ParameterBoxContent });
			ParameterBoxContent = string.Empty;
			UpdateStatusBar("Added successfully");
			return;
		}
		private bool CanRemoveParameter()
		{
			if (_parametersCollectionSelectedItem == null) return false;

			return !string.IsNullOrEmpty(_parametersCollectionSelectedItem.Name);


			//return !string.IsNullOrEmpty(_parametersCollectionSelectedItem?.Name);
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
				Document.WriteToLogFile(LogFilePath,
										$"In method: '{nameof(CollectValues)}()' " +
										$"-> '{BaseMachineParameters}' file is missing.");
				return false;
			}


			_listOfParametersCode = new List<string>();


			foreach (var item in ParametersCollection) {
				_listOfParametersCode.Add(item.Code + " =");
			}


			if (string.IsNullOrWhiteSpace(InitialPathBoxContent))
				InitialPathBoxContent = Directory.GetCurrentDirectory();


			if (!GetPathsOfParametersFiles(InitialPathBoxContent)) {
				UpdateStatusBar("Failed to find parameters files");
				Document.WriteToLogFile(LogFilePath,
										$"In method: '{nameof(GetPathsOfParametersFiles)}()' " +
										"-> Didn't find any 'machineparameters.txt' file.");
				return false;
			}


			var validationProperties = new ParameterValidationModel();
			validationProperties.ValidateListedParameters(_listOfParametersCode, BaseMachineParameters);

			if (validationProperties.NumberOfParametersNotFound > 0) {
				validationProperties.DisplayParametersErrorMessages();
				Document.WriteToLogFile(LogFilePath,
										$"In method: '{nameof(validationProperties.ValidateListedParameters)}()' " +
										"-> Failed to validate all the parameters from the list box.");
				return false;
			}


			_listOfValidParameters = _listOfParametersCode.Except(validationProperties.DuplicatedParameters);


			var parameterDataToSaveToCSV = new StringBuilder($"--> Values for: '{NameOfFileToSearch}'\n");


			foreach (var parameterFromList in _listOfValidParameters) {
				var parameter = new ParameterModel();

				foreach (var file in Document.ReadFromFile(ParametersFilesPaths)) {
				
					// Might be an auxiliary axis and thus the parameter might not exist in this file
					if (!parameter.GetParameterFromFile(parameterFromList, file)) continue;


					if (!parameter.CollectValidParameter()) {
						UpdateStatusBar($"Error collecting values");
						Document.WriteToLogFile(LogFilePath,
												$"In method: '{nameof(parameter.CollectValidParameter)}()' " +
												$"-> Error collecting values, the parameter '{parameterFromList.Trim('=').Trim()}' " +
												"doesn't have a value to collect.");
						return false;
					}
				}

				parameter.Average /= parameter.NumberOfOccurrences;
				parameter.Name = Text.RemoveDiacritics(parameter.Name);

				
				var indexOfComma = parameter.Name.IndexOf(",");
				if (indexOfComma > -1)
					parameter.Name = parameter.Name.Remove(indexOfComma, 1);


				if (string.IsNullOrWhiteSpace(parameter.Name)) {
					UpdateStatusBar("Error collecting values");
					Document.WriteToLogFile(LogFilePath,
											$"In method: '{nameof(CollectValues)}()' " +
											$"-> Error collecting values on parameter '{parameter.Name}', name returned empty.");
					return false;
				}

				parameterDataToSaveToCSV.Append(parameter.Name + "," + parameter.Average + "\n");
			}

			if (!Document.AppendToFile(CSVFilePath, parameterDataToSaveToCSV + "\n")) return false;

			UpdateStatusBar("Collected successfully");

			return Document.WriteToLogFile(LogFilePath, "Collected values successfully");
		}
		private bool GetPathsOfParametersFiles(string initialPath)
		{// What: Gets all paths for the parameters files, recursively, starting on the "InitialPathBox" path
		 // Why: To have a list with the paths of all the "machineparameters.txt" from which we will retrieve the values
			var paths = Directory.GetFiles(initialPath, "machineparameters.txt", SearchOption.AllDirectories).ToList();

			if (paths.Count <= 0) return false;

			for (int i = paths.Count - 1; i >= 0; i--) {
				// Check if the file is in a folder that the user wants to see (ex: contains the name/model of the machine)
				if (!paths[i].ToLower().Contains(NameOfFileToSearch.ToLower())) {
					paths.RemoveAt(i);
					continue;
				}
				// Check if the file is of an incompatible version
				var listOfIncompatibleFileVersions = new Collection<string>() { "V2.2.10", /* Vx.x.xx */ };

				var versionLine = Document.ReadSpecificLineFromFile(paths[i], 2);

				var isIncompatible = listOfIncompatibleFileVersions.Any(version => versionLine.Contains(version));

				if (isIncompatible)
					paths.RemoveAt(i);
			}

			return Document.WriteToFile(ParametersFilesPaths, paths.ToArray());
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
