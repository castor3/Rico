using System;
using System.Collections.ObjectModel;
using Rico.Models;
using System.ComponentModel;
using Rico.ViewModels.Commands;
using System.Windows;
using Aux;
using System.Text.RegularExpressions;
using System.Linq;

namespace Rico.ViewModels
{
	public class ParametersViewModel : INotifyPropertyChanged
	{
		#region Class constructor
		public ParametersViewModel()
		{
			ParametersCollection = new ObservableCollection<Parameter>();
			AddParameterCommand = new AddParameterCommand(this);
		}
		#endregion

		#region Fields
		readonly Parameter _model = new Parameter();
		#endregion

		#region Properties
		public AddParameterCommand AddParameterCommand { get; }

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
		#endregion

		#region Property changed Event
		public event PropertyChangedEventHandler PropertyChanged;

		private void RaisePropertyChanged(string property)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
		}
		#endregion

		// Methods
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
		private void SearchParameterInFile(string parameter)
		{
			string match = "";
			string filePath = @"machineparameters_original.txt";
			foreach (var item in Document.YieldReturnLinesFromFile(filePath)) {
				if (item.Contains(parameter)) {
					MessageBox.Show("Match: " + item);
					match = item;
					break;
				}
			}
			int index = match.IndexOf('=');
			match = match.Substring(index + 1).Trim();
			MessageBox.Show("Match: " + match);
			var stringValor = Regex.Split(match, @"[^0-9\.]+").Where(c => c != "." && c.Trim() != "");
			MessageBox.Show("stringValor: " + stringValor.First());
			double valor = double.Parse(stringValor.First());
		}
	}
}
