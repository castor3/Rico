using Rico.Models;
using SupportFiles;
//using Rico.ViewModels.Commands;
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
				//new Parameter { ParameterName = "Correção Referência ferram." },
				//new Parameter { ParameterName = "Horas" },
				//new Parameter { ParameterName = "Cursos" },
			};
            AddParameterCommand = new RelayCommand(CanAddParameter, AddParameter);
            RemoveParameterCommand = new RelayCommand(CanRemoveParameter, RemoveParameter);
            CollectValuesCommand = new RelayCommand(CanCollectValues, CollectValues);
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

        private string _parameterBoxContent;
        public string ParameterBoxContent
        {
            get { return _parameterBoxContent; }
            set
            {
                if (_parameterBoxContent == value) return;
                _parameterBoxContent = value;
                RaisePropertyChanged(nameof(ParameterBoxContent));
            }
        }
        private ObservableCollection<Parameter> _parametersCollection;
        public ObservableCollection<Parameter> ParametersCollection
        {
            get
            {
                return _parametersCollection;
            }
            set
            {
                if (_parametersCollection == value) return;
                _parametersCollection = value;
                RaisePropertyChanged(nameof(ParametersCollection));
            }
        }
        private Parameter _parametersCollectionSelectedItem;
        public Parameter ParametersCollectionSelectedItem
        {
            get
            {
                return _parametersCollectionSelectedItem;
            }
            set
            {
                if (_parametersCollectionSelectedItem == value) return;
                _parametersCollectionSelectedItem = value;
                RaisePropertyChanged(nameof(ParametersCollectionSelectedItem));
            }
        }
        private string _statusBarContent;
        public string StatusBarContent
        {
            get
            {
                return _statusBarContent;
            }
            set
            {
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
                    MessageBox.Show("Parâmetro já foi adicionado à lista");
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
            var duplicatedParameters = "";
            var amountOfDuplicates = 0;
            foreach (var item in ParametersCollection) {
                if (FindDuplicateParametersInFile(item.ParameterName) == true) {
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
                foreach (var item in ParametersCollection)
                    CollectParametersValues(item.ParameterName);
                StatusBarContent = "Collected successfuly";
            }
        }
        /// <summary>
        /// Returns TRUE if finds duplicates of the parameter passed
        /// </summary>
        private bool FindDuplicateParametersInFile(string parameter)
        {
            var found = 0;
            var array = parameter.Split(',');
            foreach (var item in Document.YieldReturnLinesFromFile(_machineParametersFilePath)) {
                if (item.Contains(array[0]) && item.Contains(array[array.Length - 1])) {
                    if (++found > 1) return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Return true = error -&&- false = OK
        /// </summary>
        private void CollectParametersValues(string originalParameter)
        {
            var parameterLine = SearchParameterInFile(originalParameter);
            if (string.IsNullOrEmpty(parameterLine)) return;
            var parameterNameAndValue = HandleParameterValue(parameterLine);
            Document.AppendToFile(_CSVFilePath, parameterNameAndValue.Item1 + ";" + parameterNameAndValue.Item2 + "\n");
        }
        private string SearchParameterInFile(string originalParameter)
        {
            var array = originalParameter.Split(',');
            foreach (var item in Document.YieldReturnLinesFromFile(_machineParametersFilePath)) {
                if (item.Contains(array[0]) && item.Contains(array[array.Length - 1])) {
                    return item;
                }
            }
            return "";
        }
        private Tuple<string, string> HandleParameterValue(string parameterLine)
        {// Receives the entire line of the parameter and returns a tuple with the name and the value
            if (!parameterLine.Contains('=')) return new Tuple<string, string>("", "");

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
