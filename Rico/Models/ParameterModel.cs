using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows;
using System.Text.RegularExpressions;

namespace Rico.Models
{
	public class Parameter
	{
		private string _name;
		public string Name
		{
			get { return _name; }
			set {
				if (_name != value && value != null)
					_name = value;
			}
		}
		private string _value;
		public string Value
		{
			get { return _value; }
			set {
				if (_value != value && value != null)
					_value = value;
			}
		}
		private string _parameterLine;
		public string ParameterLine
		{
			get { return _parameterLine; }
			set {
				if (_parameterLine != value && value != null)
					_parameterLine = value;
			}
		}
		private double _average;
		public double Average
		{
			get { return _average; }
			set {
				if (_average != value)
					_average = value;
			}
		}
		public int NumberOfOcurrences { get; set; }
		private string _code;
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
		private bool _ignore;
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
