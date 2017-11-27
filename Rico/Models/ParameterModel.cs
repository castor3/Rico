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
				if (_name != value)
					_name = value;
			}
		}
		private string _value;
		public string Value
		{
			get { return _value; }
			set {
				if (_value != value)
					_value = value;
			}
		}
		private string _parameterLine;
		public string ParameterLine
		{
			get { return _parameterLine; }
			set {
				if (_parameterLine != value)
					_parameterLine = value;
			}
		}
		public double Average { get; set; }
		public int NumberOfOcurrences { get; set; }
		//public bool DidFindParameter { get; set; }

		// Methods
		public bool GetParameterName()
		{
			Match regexResult;
			try {
				regexResult = Regex.Match(ParameterLine, @"\s{2}(([\w\-]+\s?)+)\s+(\w+)");
			}
			catch (Exception exc) when (exc is ArgumentException || exc is ArgumentNullException || exc is RegexMatchTimeoutException) {
				return false;
			}
			if (!regexResult.Success) return false;

			var parameterName = string.Empty;
			if (regexResult.Groups.Count == 4)
				parameterName = regexResult.Groups[1].Value + "(" + regexResult.Groups[3].Value + ")";

			Name = parameterName;
			return true;
		}
		public bool GetParameterValue()
		{// Uses the entire parameter line to extract the value (as string)
			if (string.IsNullOrWhiteSpace(ParameterLine)) return false;
			byte index = (byte)ParameterLine.IndexOf('=');
			ParameterLine = ParameterLine.Substring(index + 1).Trim();
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

		}
	}
}
