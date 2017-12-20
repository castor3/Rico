using System;
using System.Linq;
using System.Text.RegularExpressions;
using SupportFiles;

namespace Rico.Models
{
	public class Parameter
	{
		private string _name;
		private string _value;
		private string _parameterLine;
		private double _average;
		private string _code;
		private bool _ignore;
		private bool _isFirstCycle = true;
		private int _numberOfOccurrences;

		public string Name
		{
			get { return _name; }
			set {
				if (_name != value && value != null)
					_name = value;
			}
		}
		public string Value
		{
			get { return _value; }
			set {
				if (_value != value && value != null)
					_value = value;
			}
		}
		public string ParameterLine
		{
			get { return _parameterLine; }
			set {
				if (_parameterLine != value && value != null)
					_parameterLine = value;
			}
		}
		public double Average
		{
			get { return _average; }
			set {
				if (_average != value)
					_average = value;
			}
		}
		public int NumberOfOccurrences { get { return _numberOfOccurrences; } }
		public string Code
		{
			get {
				return _code;
			}
			set {
				if (_code != value && value != null)
					_code = value;
			}
		}
		public bool Ignore
		{
			get { return _ignore; }
			set { _ignore = value; }
		}
		public bool IsFirstCycle
		{
			get { return _isFirstCycle; }
			set { _isFirstCycle = value; }
		}


		// Methods
		public bool CollectValidParameter()
		{// Receives a "valid parameter" and gets its name and value from the "machineparameters.txt" file
			
			_numberOfOccurrences++;

			if (IsFirstCycle) {
				GetParameterName();
				IsFirstCycle = false;
			}

			GetParameterValue();

			if (string.IsNullOrWhiteSpace(Value)) {
				return Ignore;
			}

			var parameterValueAsDouble = 0.0d;
			if (!double.TryParse(Value, out parameterValueAsDouble)) {
				return false;
			}
			else {
				Average += parameterValueAsDouble;
				return true;
			}
		}
		public bool GetParameterFromFile(string parameterFromList, string machineParametersFile)
		{// Retrieves, from the parameters file, the full line of the parameter passed
			var array = parameterFromList.Split(',');
			var hasSplited = array.Length > 1;
			foreach (var item in Document.ReadFromFile(machineParametersFile)) {
				if (hasSplited) {
					if ((item.Contains(array[0]) && item.Contains(array[array.Length - 1]))) {
						ParameterLine = item;
						return true;
					}
				}
				else {
					if (item.Contains(parameterFromList)) {
						ParameterLine = item;
						return true;
					}
				}
			}
			ParameterLine = string.Empty;
			return false;
		}
		public bool GetParameterCode()
		{
			var regexResult = RegexNameAndCode();
			if (regexResult == null) return false;

			var parameterCode = string.Empty;
			if (regexResult.Groups.Count == 4)
				parameterCode = regexResult.Groups[3].Value;

			Code = parameterCode;
			return true;
		}
		public bool GetParameterName()
		{
			var regexResult = RegexNameAndCode();

			if (regexResult == null) return false;

			var parameterName = string.Empty;
			if (regexResult.Groups.Count == 4)
				parameterName = regexResult.Groups[1].Value + "(" + regexResult.Groups[3].Value + ")";
			else
				return false;


			Name = parameterName;
			return true;
		}
		public bool GetParameterValue()
		{// Uses the entire parameter line to extract the value (as string)
			if (string.IsNullOrWhiteSpace(ParameterLine)) return false;

			var index = (byte)ParameterLine.IndexOf('=');
			ParameterLine = ParameterLine.Substring(index + 1).Trim();

			// This means that the ParameterLine does not contain any value after the '='
			if (string.IsNullOrWhiteSpace(ParameterLine)) {
				Ignore = true;
				return false;
			}

			var regexResult = RegexValue();

			if (!regexResult.Success) return false;

			Value = regexResult.Groups[1].Value;

			return true;
		}
		public Match RegexNameAndCode()
		{
			Match regexResult;
			try {
				// escapes -> -  .  ,  /  \  "  )  ( 
				regexResult = Regex.Match(ParameterLine, @"\s{2}(([\w\-\.\,\/\\""\)\(\d]+\s{0,2})+)\s+(\w+)");
			}
			catch (Exception exc) when (exc is ArgumentException || exc is ArgumentNullException || exc is RegexMatchTimeoutException) {
				return null;
			}
			return regexResult;
		}
		public Match RegexValue()
		{
			Match regexResult;
			try {
				regexResult = Regex.Match(ParameterLine, @"=?\s*([0-9]*\.?[0-9]*)");
				//Value = Regex.Split(ParameterLine, @"[^0-9\.]+")
				//							.Where(c => c != "." && c.Trim() != "")
				//							.First();
			}
			catch (Exception exc) when (exc is ArgumentException || exc is ArgumentNullException || exc is RegexMatchTimeoutException) {
				return null;
			}

			return regexResult;
		}
	}
}
