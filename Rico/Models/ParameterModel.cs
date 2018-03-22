using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using SupportFiles;

namespace Rico.Models
{
	public class ParameterModel
	{
		private string _name;
		private string _value;
		private string _parameterLine;
		private double _average;
		private string _code;

		public string Name
		{
			get { return _name; }
			set
			{
				if (_name != value && value != null)
					_name = value;
			}
		}
		public string Value
		{
			get { return _value; }
			set
			{
				if (_value != value && value != null)
					_value = value;
			}
		}
		public string ParameterLine
		{
			get { return _parameterLine; }
			set
			{
				if (_parameterLine != value && value != null)
					_parameterLine = value;
			}
		}
		public double Average
		{
			get { return _average; }
			set
			{
				if (_average != value)
					_average = value;
			}
		}
		public int NumberOfOccurrences { get; private set; }

		public string Code
		{
			get
			{
				return _code;
			}
			set
			{
				if (_code != value && value != null)
					_code = value;
			}
		}
		public bool Ignore { get; set; }


		// Methods
		public bool CollectValidParameter()
		{// Receives a "valid parameter" and gets its name and value from the "machineparameters.txt" file

			GetParameterValue();

			if (string.IsNullOrWhiteSpace(Value))
			{
				return Ignore;
			}

			double parameterValueAsDouble;
			if (!double.TryParse(Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out parameterValueAsDouble))
			{
				return false;
			}

			Average += parameterValueAsDouble;
			NumberOfOccurrences++;
			return true;
		}
		public bool GetParameterFromFile(string parameterFromList, string machineParametersFile)
		{// Retrieves, from the parameters file, the full line of the parameter passed
			var array = parameterFromList.Split(',');
			var hasSplited = array.Length > 1;

			foreach (var item in Document.ReadFromFile(machineParametersFile))
			{
				if (!item.Contains('='))
				{
					continue;
				}

				if (hasSplited)
				{
					if (!item.Contains(array[0]) || !item.Contains(array[array.Length - 1]))
					{
						continue;
					}
					ParameterLine = item;
					return true;
				}

				if (!item.Contains(parameterFromList))
				{
					continue;
				}
				ParameterLine = item;
				return true;
			}
			ParameterLine = string.Empty;
			return false;
		}
		public bool GetParameterNameAndCode()
		{
			var regexResult = RegexNameAndCode();

			if (regexResult == null || !regexResult.Success || regexResult.Groups.Count != 4)
			{
				return false;
			}

			Name = regexResult.Groups[1].Value + "(" + regexResult.Groups[3].Value + ")";
			Code = regexResult.Groups[3].Value;

			return true;
		}
		public bool GetParameterValue()
		{// Uses the entire parameter line to extract the value (as string)

			if (string.IsNullOrWhiteSpace(ParameterLine) || !ParameterLine.Contains("="))
			{
				return false;
			}

			var index = (byte)ParameterLine.IndexOf('=');
			ParameterLine = ParameterLine.Substring(index + 1).Trim();

			// This means that the ParameterLine does not contain any value after the '='
			if (string.IsNullOrWhiteSpace(ParameterLine))
			{
				Ignore = true;
				return false;
			}

			var regexResult = RegexValue();

			if (regexResult == null || !regexResult.Success || regexResult.Groups.Count < 2)
			{
				return false;
			}

			Value = regexResult.Groups[1].Value;

			return true;
		}
		public Match RegexNameAndCode()
		{
			try
			{
				// escapes -> -  .  ,  /  \  "  )  ( 
				return Regex.Match(ParameterLine, @"\s{2}(([\w\-\.\,\/\\""\)\(\d]+\s{0,2})+)\s+(\w+)\s=");
			}
			catch (Exception)
			{
				return Regex.Match(string.Empty, @"\S");
			}
		}
		public Match RegexValue()
		{
			try
			{
				if (!string.IsNullOrWhiteSpace(ParameterLine))
				{
					return Regex.Match(ParameterLine, @"\s?\=?\s?(\-?[0-9]*\.?[0-9]*)");
				}
			}
			catch (Exception)
			{
				return Regex.Match(string.Empty, @"\S");
			}

			return Regex.Match(string.Empty, @"\S");
		}
	}
}
