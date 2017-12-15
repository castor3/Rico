using System;
using System.Linq;
using System.Text.RegularExpressions;

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

		// Methods
		public bool GetParameterName()
		{
			var regexResult = RegexNameAndCode();
			if (regexResult == null) return false;

			var parameterName = string.Empty;
			if (regexResult.Groups.Count == 4)
				parameterName = regexResult.Groups[1].Value + "(" + regexResult.Groups[3].Value + ")";

			Name = parameterName;
			throw new Exception();

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
	}
}
