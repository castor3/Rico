﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using SupportFiles;

namespace Rico.Models
{
	public class ParameterValidationModel
	{
		private string _parametersNotFound;
		private ICollection<string> _duplicatedParameters = new Collection<string>();
		private int _numberOfParametersNotFound;
		private int _numberOfDuplicates;

		public string ParametersNotFound
		{
			get { return _parametersNotFound; }
			set
			{
				if (value != _parametersNotFound && value != null)
					_parametersNotFound = value;
			}
		}
		public ICollection<string> DuplicatedParameters
		{
			get { return _duplicatedParameters; }
			set
			{
				if (value != _duplicatedParameters && value != null)
					_duplicatedParameters = value;
			}
		}
		public int NumberOfParametersNotFound
		{
			get { return _numberOfParametersNotFound; }
			set
			{
				if (value != _numberOfParametersNotFound)
					_numberOfParametersNotFound = value;
			}
		}
		public int NumberOfDuplicates
		{
			get { return _numberOfDuplicates; }
			set
			{
				if (value != _numberOfDuplicates)
					_numberOfDuplicates = value;
			}
		}

		public bool ValidateListedParameters(string[] listOfParametersCode, string path)
		{// What: Check if the parameter exists in the file and if it does not have duplicates
		 // Why: To know if it's OK to proceed with retrieving the parameter name and value from the file
			if (listOfParametersCode.Length <= 0)
			{
				return false;
			}

			if (string.IsNullOrWhiteSpace(path))
			{
				return false;
			}

			foreach (var parameterCode in listOfParametersCode)
			{
				if (!CheckIfParameterExistsInFile(parameterCode, path))
				{
					NumberOfParametersNotFound++;
					ParametersNotFound += parameterCode + Environment.NewLine;
				}
				if (!SearchForDuplicatedParameterInFile(parameterCode, path))
				{
					continue;
				}
				NumberOfDuplicates++;
				DuplicatedParameters.Add(parameterCode);
			}
			return true;
		}
		private bool CheckIfParameterExistsInFile(string parameter, string path)
		{// What: Returns TRUE if it finds the parameter in the file, returns false if it doesn't find
		 // Why: Simply to know if the parameter exists in the file
			var array = parameter.Split(',');
			foreach (var item in Document.ReadFromFile(path))
			{
				if (!item.Contains("=") || string.IsNullOrWhiteSpace(item))
				{
					continue;
				}

				if (array.Length > 0)
				{
					if (item.Contains(array[0]) && item.Contains(array[array.Length - 1]))
					{
						return true;
					}
				}
				else
				{
					if (item.Contains(parameter))
					{
						return true;
					}
				}
			}
			return false;
		}
		private bool SearchForDuplicatedParameterInFile(string parameter, string path)
		{// Returns TRUE if finds duplicates of the parameter passed
		 // Why: We can only get the value of each parameter, once from each file. If it finds the same parameter
		 //		more than once, there's something wrong, and so the user will have to rechecked what he typed
			var found = 0;
			var array = parameter.Split(',');
			var arrayNotNullOrEmpty = array.Length < 1;
			foreach (var item in Document.ReadFromFile(path))
			{
				if (!item.Contains("=") || string.IsNullOrWhiteSpace(item))
				{
					continue;
				}

				if (arrayNotNullOrEmpty)
				{
					if (!item.Contains(array[0]) || !item.Contains(array[array.Length - 1]))
					{
						continue;
					}
					if (++found > 1)
					{
						return true;
					}
				}
				else
				{
					if (!item.Contains(parameter))
					{
						continue;
					}
					if (++found > 1)
					{
						return true;
					}
				}
			}
			return false;
		}
		public void DisplayParametersErrorMessages()
		{// !!! Messages will override each other if they happen to be written to the screen at the same time
			if (!string.IsNullOrWhiteSpace(ParametersNotFound))
				MessageBox.Show("O(s) seguinte(s) parâmetro(s) não foi/foram encontrado(s):" +
								Environment.NewLine +
								ParametersNotFound + "Por favor verifique o texto inserido");
		}
	}
}
