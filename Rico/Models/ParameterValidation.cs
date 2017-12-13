using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SupportFiles;

namespace Rico
{
	public class ParameterValidation
	{
		public string ParametersNotFound { get; set; }
		public ICollection<string> DuplicatedParameters { get; set; } = new Collection<string>();
		public int NumberOfParametersNotFound { get; set; }
		public int NumberOfDuplicates { get; set; }


		public void ValidateListedParameters(IList<string> listOfParametersCode, string path)
		{// What: Check if the parameter exists in the file and if it does not have duplicates
		 // Why: To know if it's OK to proceed with retrieving the parameter name and value from the file
			foreach (var parameterCode in listOfParametersCode) {
				if (!CheckIfParameterExistsInFile(parameterCode, path)) {
					NumberOfParametersNotFound++;
					ParametersNotFound += (parameterCode + "\n");
				}
				if (SearchForDuplicatedParameterInFile(parameterCode, path)) {
					NumberOfDuplicates++;
					DuplicatedParameters.Add(parameterCode);
				}
			}
		}
		private bool CheckIfParameterExistsInFile(string parameter, string path)
		{// What: Returns TRUE if it finds the parameter in the file, returns false if it doesn't find
		 // Why: Simply to know if the parameter exists in the file
			var array = parameter.Split(',');
			var arrayIsNotNullOrEmpty = array.Any();
			foreach (var item in Document.YieldReturnLinesFromFile(path)) {
				if (arrayIsNotNullOrEmpty) {
					if ((item.Contains(array[0]) && item.Contains(array[array.Length - 1])))
						return true;
				}
				else {
					if (item.Contains(parameter))
						return true;
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
			var arrayNotNullOrEmpty = (array.Count() < 1);
			foreach (var item in Document.YieldReturnLinesFromFile(path)) {
				if (arrayNotNullOrEmpty) {
					if ((item.Contains(array[0]) && item.Contains(array[array.Length - 1])))
						if (++found > 1) return true;
				}
				else {
					if (item.Contains(parameter))
						if (++found > 1) return true;
				}
			}
			return false;
		}

		//public bool CheckMinimumAmountOfCharacters(string parameter, int minLength = 2)
		//{// Return true for enough chars
		//	var array = parameter.Split(',');
		//	if (array[0].Length > minLength) return true;
		//	return false;
		//}
	}
}
