using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rico.Models;

namespace Rico.Test.Unit
{
	[TestClass]
	public class ParameterValidationModelTest
	{
		[TestMethod]
		public void ValidateListedParameters_IfListIsEmpty_ReturnFalse()
		{
			var validationModel = new ParameterValidationModel();

			var result = validationModel.ValidateListedParameters(new List<string>(), "text");

			Assert.IsFalse(result);
		}

		[TestMethod]
		public void ValidateListedParameters_IfPathIsEmpty_ReturnFalse()
		{
			var validationModel = new ParameterValidationModel();

			var result = validationModel.ValidateListedParameters(new List<string> { "text" }, string.Empty);

			Assert.IsFalse(result);
		}

		[TestMethod]
		public void ValidateListedParameters_IfListAndPathNotEmpty_ReturnTrue()
		{
			var validationModel = new ParameterValidationModel();

			var result = validationModel.ValidateListedParameters(new List<string> { "text" }, "text");

			Assert.IsTrue(result);
		}
	}
}
