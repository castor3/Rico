using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rico
{
	public class ParameterValidation
	{
		public string parametersNotFound { get; set; }
		public string duplicatedParameters { get; set; }
		public int amountOfParametersNotFound { get; set; }
		public int amountOfDuplicates { get; set; }
		//public bool parameterWithNotEnoughChars { get; set; }

		//public bool CheckMinimumAmountOfCharacters(string parameter, int minLength = 2)
		//{// Return true for enough chars
		//	var array = parameter.Split(',');
		//	if (array[0].Length > minLength) return true;
		//	return false;
		//}
	}
}
