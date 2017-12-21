using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rico.Models;
using Rico.ViewModels;

namespace RicoTestes
{
	[TestClass]
	public class ParameterModelTest
	{
		[TestMethod]
		public void CollectValidParameter_IfIsFirstCycle_NameHasToChange()
		{
			var paramModel = new Parameter { ParameterLine = "  Referênc.ferram.      1485 =    523.44  mm" };

			var name1 = paramModel.Name;
			paramModel.CollectValidParameter();
			var name2 = paramModel.Name;

			Assert.AreNotEqual(name1, name2);
		}

		[TestMethod]
		public void CollectValidParameter_IfIsFirstCycleIsTrue_HasToBeFalseAfterRunning()
		{
			var paramModel = new Parameter { ParameterLine = "  Referênc.ferram.      1485 =    523.44  mm" };

			paramModel.IsFirstCycle = true;
			paramModel.CollectValidParameter();

			Assert.IsFalse(paramModel.IsFirstCycle);
		}

		[TestMethod]
		public void CollectValidParameter_ValueIsNotDigits_ReturnFalse()
		{
			var result = true;

			foreach (var item in GetRandomParameterLines(50)) {
				// Arrange
				var paramModel = new Parameter { ParameterLine = item };
				var parameterValueAsDouble = 0.0d;

				// Act
				var collectResult = paramModel.CollectValidParameter();
				var conversionSuccessful = double.TryParse(paramModel.Value, out parameterValueAsDouble);

				// Assert
				if (!conversionSuccessful && collectResult)
					result = false;
			}

			Assert.IsTrue(result);
		}

		[TestMethod]
		public void GetParameterValue_ParameterLineIsNullOrWhiteSpace_ReturnFalse()
		{
			// Arrange
			var paramModel = new Parameter { ParameterLine = string.Empty };

			// Act
			var result = paramModel.GetParameterValue();

			// Assert
			Assert.IsFalse(result);
		}

		[TestMethod]
		public void RegexNameAndCode_ParameterLineIsNullOrEmpty_ReturnFalse()
		{
			// Arrange
			var paramModel = new Parameter { ParameterLine = string.Empty };
			var privateObject = new PrivateObject(paramModel);

			// Act
			var result = (Match)privateObject.Invoke("RegexNameAndCode");

			// Assert
			Assert.IsNull(result);
		}

		private Collection<string> GetRandomParameterLines(int numberOfLines)
		{
			var randomValues = General.GenerateRandomValues(numberOfLines);
			var parameterLines = new Collection<string>();
			var path = new ParametersViewModel().BaseMachineParameters;

			foreach (var item in randomValues) {
				parameterLines.Add(Document.ReadSpecificLineFromFile(path, item));
			}

			return parameterLines;
		}
	}
}
