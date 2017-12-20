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
		public int NumberOfOcurrences { get; set; }
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
		private Match RegexNameAndCode()
		{
			Match regexResult;
			try {
				// escapes -> -  .  ,  /  \  "  )  ( 
				regexResult = Regex.Match(ParameterLine, @"\s{2}(([\w\-\.\,\/\\""\)\(\d]+\s{0,2})+)\s+(\w+)");
			}
			catch (Exception exc) when (exc is ArgumentException || exc is ArgumentNullException || exc is RegexMatchTimeoutException) {
				return null;
			}
			if (!regexResult.Success) return null;
			return regexResult;
		}
		public bool CollectValidParameter(string parameterFromList, string machineParametersFile)
		{// Receives a "valid parameter" and gets its name and value from the "machineparameters.txt" file
			if(!GetParameterFromFile(parameterFromList, machineParametersFile)) return false;

			// If the "ParameterLine" is empty, is probably because it searched in an incompatible file
			// Anyway, it will proceed ('return true;') to the next file without throwing an error
			if (string.IsNullOrWhiteSpace(ParameterLine)/* || !parameter.ParameterLine.Contains('=')*/) return true;

			NumberOfOcurrences++;

			if (IsFirstCycle) {
				GetParameterName();
				IsFirstCycle = false;
			}

			GetParameterValue();
			var parameterValue = Value;

			if (string.IsNullOrWhiteSpace(parameterValue)) {
				if (Ignore)
					return true;
				return false;
			}

			var parameterValueAsDouble = 0.0d;
			if (!double.TryParse(parameterValue, out parameterValueAsDouble)) {
				return false;
			}
			else {
				Average += parameterValueAsDouble;
				return true;
			}
		}
		private bool GetParameterFromFile(string parameterFromList, string machineParametersFile)
		{// Retrieves, from the parameters file, the full line of the parameter passed
			var array = parameterFromList.Split(',');
			var hasSplited = array.Count() > 1;
			foreach (var item in Document.YieldReturnLinesFromFile(machineParametersFile)) {
				if (hasSplited) {
					if ((item.Contains(array[0]) && item.Contains(array[array.Length - 1]))) {
						ParameterLine =  item;
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
			return false;
		}
		public bool GetParameterName()
		{
			var regexResult = RegexNameAndCode();
			if (regexResult == null) return false;

			var parameterName = string.Empty;
			if (regexResult.Groups.Count == 4)
				parameterName = regexResult.Groups[1].Value + "(" + regexResult.Groups[3].Value + ")";

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

			try {
				Value = Regex.Split(ParameterLine, @"[^0-9\.]+")
											.Where(c => c != "." && c.Trim() != "")
											.First();
			}
			catch (Exception exc) when (exc is ArgumentException || exc is ArgumentNullException || exc is RegexMatchTimeoutException) {
				return false;
			}
			return true;
		}
	}
}
