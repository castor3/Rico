using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
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
			ParametersCollection = new ObservableCollection<ParameterModel>();
			AddParameterCommand = new RelayCommand(CanAddParameter, AddParameter);
			RemoveParameterCommand = new RelayCommand(CanRemoveParameter, RemoveParameter);
			CollectValuesCommand = new RelayCommand(CanCollectValues, ExecuteCollectValues);
		}

		#region Fields
		private string _initialPathBoxContent /*= Directory.GetCurrentDirectory()*/;
		private string _parameterBoxContent;
		private ObservableCollection<ParameterModel> _parametersCollection;
		private ParameterModel _parametersCollectionSelectedItem;
		private string _statusBarContent = "Pronto";
		private string _nameOfFileToSearch;
		#endregion

		#region Properties
		public ICommand AddParameterCommand { get; }
		public ICommand RemoveParameterCommand { get; }
		public ICommand CollectValuesCommand { get; }

		public string InitialPathBoxContent
		{
			get { return _initialPathBoxContent; }
			set
			{
				if (_initialPathBoxContent == value || value == null) return;
				_initialPathBoxContent = value;
				RaisePropertyChanged(nameof(InitialPathBoxContent));
			}
		}
		public string ParameterBoxContent
		{
			get { return _parameterBoxContent; }
			set
			{
				if (_parameterBoxContent == value || value == null) return;
				_parameterBoxContent = value;
				RaisePropertyChanged(nameof(ParameterBoxContent));
			}
		}
		public ObservableCollection<ParameterModel> ParametersCollection
		{
			get { return _parametersCollection; }
			set
			{
				if (_parametersCollection == value || value == null) return;
				_parametersCollection = value;
				RaisePropertyChanged(nameof(ParametersCollection));
			}
		}
		public ParameterModel ParametersCollectionSelectedItem
		{
			get { return _parametersCollectionSelectedItem; }
			set
			{
				if (_parametersCollectionSelectedItem == value || value == null) return;
				_parametersCollectionSelectedItem = value;
				RaisePropertyChanged(nameof(ParametersCollectionSelectedItem));
			}
		}
		public string StatusBarContent
		{
			get { return _statusBarContent; }
			set
			{
				if (_statusBarContent == value || value == null) return;
				_statusBarContent = value;
				RaisePropertyChanged(nameof(StatusBarContent));
			}
		}
		public string NameOfFolderToSearch
		{
			get { return _nameOfFileToSearch; }
			set
			{
				if (value != _nameOfFileToSearch && value != null)
					_nameOfFileToSearch = value;
			}
		}
		public string CSVFilePath => "parameters_values.csv";
		public string BaseMachineParameters => "Parameters.txt";
		public string ParametersFilesPaths => "machinepaths.txt";
		public static string LogFilePath => "output_log.txt";
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
			if (ParametersCollection.Any(item => item.Code == ParameterBoxContent))
			{
				UpdateStatusBar("Parâmetro já foi adicionado à lista");
				ParameterBoxContent = string.Empty;
				return;
			}
			ParametersCollection.Add(new ParameterModel { Code = ParameterBoxContent });
			ParameterBoxContent = string.Empty;
			UpdateStatusBar("Added successfully");
		}
		private bool CanRemoveParameter()
			=> !string.IsNullOrEmpty(_parametersCollectionSelectedItem?.Code);

		public void RemoveParameter()
		{
			foreach (var item in ParametersCollection)
			{
				if (item.Code != ParametersCollectionSelectedItem.Code)
				{
					continue;
				}
				ParametersCollection.Remove(item);
				UpdateStatusBar("Removed successfully");
				return;
			}
		}
		private bool CanCollectValues()
		{
			return ParametersCollection.Count > 0;
		}
		public void ExecuteCollectValues()
		{// Method used by the "CollectValuesCommand"
			CollectValues();
		}
		public bool CollectValues()
		{
			if (string.IsNullOrWhiteSpace(NameOfFolderToSearch))
			{
				UpdateStatusBar("Need to specify the machine name/model");
				return false;
			}


			if (!File.Exists(BaseMachineParameters))
			{
				UpdateStatusBar("There's a file missing");
				Document.WriteToLogFile(LogFilePath,
										$"In method: '{nameof(CollectValues)}()' " +
										$"-> '{BaseMachineParameters}' file is missing.");
				return false;
			}


			var listOfParametersCode = ParametersCollection.Select(item => item.Code + " =").ToArray();


			if (string.IsNullOrWhiteSpace(InitialPathBoxContent))
			{
				InitialPathBoxContent = Directory.GetCurrentDirectory();
			}

			if (!GetPathsOfParametersFiles(InitialPathBoxContent))
			{
				UpdateStatusBar("Failed to find parameters files");
				Document.WriteToLogFile(LogFilePath,
										$"In method: '{nameof(GetPathsOfParametersFiles)}()' " +
										"-> Didn't find any 'machineparameters.txt' file.");
				return false;
			}


			var validationProperties = new ParameterValidationModel();
			validationProperties.ValidateListedParameters(listOfParametersCode, BaseMachineParameters);

			if (validationProperties.NumberOfParametersNotFound > 0)
			{
				validationProperties.DisplayParametersErrorMessages();
				Document.WriteToLogFile(LogFilePath,
										$"In method: '{nameof(validationProperties.ValidateListedParameters)}()' " +
										"-> Failed to validate all the parameters from the list box.");
				return false;
			}


			var listOfValidParameters = listOfParametersCode.Except(validationProperties.DuplicatedParameters);


			var parameterDataToSaveToCSV = new StringBuilder($"--> Medias p/ modelo: '{NameOfFolderToSearch}'" +
																Environment.NewLine);


			foreach (var parameterFromList in listOfValidParameters)
			{
				// If parameter code is 3065, skip it (the parameter 3065 is the "Machine name")
				if (parameterFromList.Contains("3065"))
				{
					continue;
				}

				var parameter = new ParameterModel();

				foreach (var line in Document.ReadFromFile(BaseMachineParameters))
				{
					if (line.Contains(parameterFromList))
					{
						parameter.ParameterLine = line;
					}
				}

				if (string.IsNullOrWhiteSpace(parameter.ParameterLine))
				{
					return false;
				}

				parameter.GetParameterNameAndCode();

				if (Document.ReadFromFile(ParametersFilesPaths).Where(line => parameter.GetParameterFromFile(parameterFromList, line)).Any(file => !parameter.CollectValidParameter()))
				{
					UpdateStatusBar("Error collecting values");
					Document.WriteToLogFile(LogFilePath,
						$"In method: '{nameof(parameter.CollectValidParameter)}()' " +
						$"-> Error collecting values, the parameter '{parameterFromList.Trim('=').Trim()}' " +
						"doesn't have a value to collect.");

					return false;
				}

				parameter.Average /= parameter.NumberOfOccurrences;
				parameter.Name = Text.RemoveDiacritics(parameter.Name);


				var indexOfComma = parameter.Name.IndexOf(",", StringComparison.Ordinal);
				if (indexOfComma > -1)
				{
					parameter.Name = parameter.Name.Remove(indexOfComma, 1);
				}

				if (string.IsNullOrWhiteSpace(parameter.Name))
				{
					UpdateStatusBar("Error collecting values");
					Document.WriteToLogFile(LogFilePath,
											$"In method: '{nameof(CollectValues)}()' " +
											$"-> Error collecting values on parameter '{parameter.Code}', name returned empty.");
					return false;
				}

				parameterDataToSaveToCSV.Append(parameter.Name + "," + parameter.Average + Environment.NewLine);
			}

			if (!Document.AppendToFile(CSVFilePath, parameterDataToSaveToCSV + Environment.NewLine))
			{
				return false;
			}

			UpdateStatusBar("Collected successfully");

			return Document.WriteToLogFile(LogFilePath, "Collected values successfully");
		}
		private bool GetPathsOfParametersFiles(string initialPath)
		{// What: Gets all paths for the parameters files, recursively, starting on the "InitialPathBox" path
		 // Why: To have a list with the paths of all the "machineparameters.txt" from which we will retrieve the values
			var paths = new List<string>();
			try
			{
				paths = Directory.GetFiles(initialPath, "machineparameters.txt", SearchOption.AllDirectories).ToList();
			}
			catch (DirectoryNotFoundException exc)
			{
				UpdateStatusBar(exc.Message);
				Document.WriteToLogFile(LogFilePath, $"Directory not found -> {exc.Message}");
				return false;
			}

			if (paths.Count <= 0)
			{
				return false;
			}

			for (var i = paths.Count - 1; i >= 0; i--)
			{
				// Check if the file is in a folder that the user wants to see (ex: contains the name/model of the machine)
				if (!paths[i].ToLower().Contains(NameOfFolderToSearch.ToLower()))
				{
					paths.RemoveAt(i);
					continue;
				}
				// Check if the file is of an incompatible version
				var listOfIncompatibleFileVersions = new Collection<string> { "V2.2.10", /* Vx.x.xx */ };

				var versionLine = Document.ReadSpecificLineFromFile(paths[i], 2);

				var isIncompatible = listOfIncompatibleFileVersions.Any(version => versionLine.Contains(version));
				if (isIncompatible)
				{
					paths.RemoveAt(i);
				}
			}
			return paths.Count > 0&& Document.WriteToFile(ParametersFilesPaths, paths.ToArray());
		}
		#region StatusBar
		// Status bar update
		private void SetStatusBarTimer()
		{//  DispatcherTimer setup
			var timer = new DispatcherTimer();
			timer.Tick += StatusBarTimer_Tick;
			timer.Interval = new TimeSpan(0, 0, 0, 2, 500);
			timer.Stop();
			timer.Start();
		}
		private void StatusBarTimer_Tick(object sender, EventArgs e)
		{
			var timer = (DispatcherTimer)sender;
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
