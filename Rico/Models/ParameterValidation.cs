using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Rico
{
	public class ParameterValidation
	{
		public string ParametersNotFound { get; set; }
		public ICollection<string> DuplicatedParameters { get; set; } = new Collection<string>();
		public int NumberOfParametersNotFound { get; set; }
		public int NumberOfDuplicates { get; set; }



		//public bool CheckMinimumAmountOfCharacters(string parameter, int minLength = 2)
		//{// Return true for enough chars
		//	var array = parameter.Split(',');
		//	if (array[0].Length > minLength) return true;
		//	return false;
		//}
	}
}
