using System.Collections.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rico.Models;

namespace Rico.Test.Unit
{
	[TestClass]
	public class ParameterModelTest
	{
		// CollectValidParameter
		[TestMethod]
		public void CollectValidParameter_ValueIsNotDigits_ReturnFalse()
		{
			var result = true;

			var parameterLines = new Collection<string> {
				"  Compr.máx.dobra                        1493 =     4100     mm",
				"  Distância entre as réguas ópticas      4590 =     3400     mm",
				"  Dist. centros dos cilindros            1492 =     24n00     mm",
				"  Machine length between side frames     5341 =     3200     mm",
				"  Inércia do avental                     1494 = 400",
				"  Inercia do prensador                   1495 = 40t0     ",
			};

			foreach (var item in parameterLines) {

				var paramModel = new ParameterModel { ParameterLine = item };
				var parameterValueAsDouble = 0.0d;

				var collectResult = paramModel.CollectValidParameter();
				var conversionSuccessful = double.TryParse(paramModel.Value, out parameterValueAsDouble);

				if (conversionSuccessful != collectResult)
					result = false;
			}

			Assert.IsTrue(result);
		}

		[TestMethod]
		public void CollectValidParameter_IfAverageChanged_ReturnHasToBeTrue()
		{
			var paramModel = new ParameterModel { ParameterLine = "  Referênc.ferram.      1485 =    523.44  mm" };
			var initialAverage = paramModel.Average;

			var result = paramModel.CollectValidParameter();
			var finalAverage = paramModel.Average;

			if (initialAverage != finalAverage)
				Assert.IsTrue(result);
		}

		// GetParameterNameAndCode
		[TestMethod]
		public void GetParameterNameAndCode_ParameterLineIsValid_ReturnTrue()
		{
			// Arrange
			var paramModel = new ParameterModel { ParameterLine = "  Referênc.ferram.      1485 =    523.44  mm" };

			// Act
			var result = paramModel.GetParameterNameAndCode();

			// Assert
			Assert.IsTrue(result);
		}

		[TestMethod]
		public void GetParameterNameAndCode_ParameterLineIsValid_NameAndCodeHaveToChange()
		{
			// Arrange
			var paramModel = new ParameterModel { ParameterLine = "  Referênc.ferram.      1485 =    523.44  mm" };


			// Act
			var initialName = paramModel.Name;
			var initialCode = paramModel.Code;
			
			paramModel.GetParameterNameAndCode();
			
			var finalName = paramModel.Name;
			var finalCode = paramModel.Code;

			var NameIsDifferent = initialName != finalName;
			var CodeIsDifferent = initialCode != finalCode;
			
			
			// Assert
			Assert.IsTrue(NameIsDifferent && CodeIsDifferent);
		}

		[TestMethod]
		public void GetParameterNameAndCode_ParameterLineIsEmpty_ReturnFalse()
		{
			// Arrange
			var paramModel = new ParameterModel { ParameterLine = string.Empty };

			var result = paramModel.GetParameterNameAndCode();

			// Assert
			Assert.IsFalse(result);
		}

		[TestMethod]
		public void GetParameterNameAndCode_ParameterLineIsNull_ReturnFalse()
		{
			// Arrange
			var paramModel = new ParameterModel { ParameterLine = null };

			var result = paramModel.GetParameterNameAndCode();

			// Assert
			Assert.IsFalse(result);
		}

		// GetParameterValue
		[TestMethod]
		public void GetParameterValue_ParameterLineIsValid_ReturnTrue()
		{
			var paramModel = new ParameterModel { ParameterLine = "  Referênc.ferram.      1485 =    523.44  mm" };
			
			var result = paramModel.CollectValidParameter();

			Assert.IsTrue(result);
		}

		[TestMethod]
		public void GetParameterValue_ParameterLineIsValid_ValueHasToChange()
		{
			var paramModel = new ParameterModel { ParameterLine = "  Referênc.ferram.      1485 =    523.44  mm" };

			var initialValue = paramModel.Value;
			paramModel.CollectValidParameter();
			var finalValue = paramModel.Value;

			Assert.AreNotEqual(initialValue, finalValue);
		}

		[TestMethod]
		public void GetParameterValue_ParameterLineIsNullOrWhiteSpace_ReturnFalse()
		{
			// Arrange
			var paramModel = new ParameterModel { ParameterLine = string.Empty };

			// Act
			var result = paramModel.GetParameterValue();

			// Assert
			Assert.IsFalse(result);
		}

		// RegexNameAndCode
		[TestMethod]
		public void RegexNameAndCode_ParameterLineIsValid_ReturnSuccessRegex()
		{
			// Arrange
			var paramModel = new ParameterModel { ParameterLine = "  Referênc.ferram.      1485 =    523.44  mm" };

			// Act
			var result = paramModel.RegexNameAndCode().Success;

			// Assert
			Assert.IsTrue(result);
		}

		[TestMethod]
		public void RegexNameAndCode_ParameterLineIsNull_ReturnFalse()
		{
			// Arrange
			var paramModel = new ParameterModel { ParameterLine = null };

			// Act
			var result = paramModel.RegexNameAndCode().Success;

			// Assert
			Assert.IsFalse(result);
		}

		[TestMethod]
		public void RegexNameAndCode_ParameterLineIsEmpty_ReturnFalse()
		{
			// Arrange
			var paramModel = new ParameterModel { ParameterLine = string.Empty };

			// Act
			var result = paramModel.RegexNameAndCode().Success;

			// Assert
			Assert.IsFalse(result);
		}

		// RegexValue
		[TestMethod]
		public void RegexValue_ParameterLineIsValid_ReturnSuccessRegex()
		{
			// Arrange
			var paramModel = new ParameterModel { ParameterLine = "  Referênc.ferram.      1485 =    523.44  mm" };

			// Act
			var result = paramModel.RegexValue().Success;

			// Assert
			Assert.IsTrue(result);
		}

		[TestMethod]
		public void RegexValue_ParameterLineIsNull_ReturnFalse()
		{
			// Arrange
			var paramModel = new ParameterModel { ParameterLine = null };

			// Act
			var result = paramModel.RegexValue().Success;

			// Assert
			Assert.IsFalse(result);
		}

		[TestMethod]
		public void RegexValue_ParameterLineIsEmpty_ReturnFalse()
		{
			// Arrange
			var paramModel = new ParameterModel { ParameterLine = string.Empty };

			// Act
			var result = paramModel.RegexValue().Success;

			// Assert
			Assert.IsFalse(result);
		}

		//private Collection<string> GetRandomParameterLines(int numberOfLines)
		//{
		//	var randomValues = General.GenerateRandomValues(numberOfLines, 1000);
		//	var parameterLines = new Collection<string>();
		//	var path = new ParametersViewModel().BaseMachineParameters;

		//	foreach (var item in randomValues) {
		//		parameterLines.Add(Document.ReadSpecificLineFromFile(path, item));
		//	}

		//	return parameterLines;
		//}
	}
}
