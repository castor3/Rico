using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rico.Models;
using Rico.ViewModels;
using SupportFiles;

namespace RicoTestes
{
	[TestClass]
	public class ParametersTest
	{
		// ParametersViewModel.cs
		[TestMethod]
		public void AddParameterTest()
		{
			var viewModel = new ParametersViewModel();
			var initialCount = viewModel.ParametersCollection.Count;
			viewModel.ParameterBoxContent = "test";
			viewModel.AddParameter();
			var finalCount = viewModel.ParametersCollection.Count;
			Assert.AreNotEqual(initialCount, finalCount);
		}

		[TestMethod]
		public void RemoveParameterTest()
		{
			var viewModel = new ParametersViewModel();
			var initialCount = viewModel.ParametersCollection.Count;
			viewModel.ParameterBoxContent = "test";
			viewModel.AddParameter();
			var intermediateCount = viewModel.ParametersCollection.Count;
			if (intermediateCount == initialCount) Assert.Fail();
			viewModel.ParametersCollectionSelectedItem = new Parameter { Name = "test" };
			viewModel.RemoveParameter();
			var finalCount = viewModel.ParametersCollection.Count;
			Assert.AreEqual(initialCount, finalCount);
		}

		[TestMethod]
		public void PreventDuplicateParameterTest()
		{
			var viewModel = new ParametersViewModel();

			viewModel.ParameterBoxContent = "test";
			viewModel.AddParameter();
			var initialCount = viewModel.ParametersCollection.Count;
			viewModel.ParameterBoxContent = "test";
			viewModel.AddParameter();
			var finalCount = viewModel.ParametersCollection.Count;

			Assert.AreEqual(initialCount, finalCount);
		}

		[TestMethod]
		public void CheckInputBoxIsClearAfterParameterAdded()
		{
			var viewModel = new ParametersViewModel();
			viewModel.ParameterBoxContent = "test";
			viewModel.AddParameter();
			Assert.IsTrue(string.IsNullOrEmpty(viewModel.ParameterBoxContent));
		}

		[TestMethod]
		public void TestMultipleParameters()
		{// Randomly picks 50 parameters to test
			const int numberOfParametersToTest = 50;
			var viewModel = new ParametersViewModel();
			//var path = Path.Combine(@"..\..\..\..\Rico\Rico\bin\Debug\", viewModel.BaseMachineParameters);
			var path = viewModel.BaseMachineParameters;

			//var linesFromFile = new Collection<string>();

			//// Get all valid lines from the parameters file
			//foreach (var item in Document.ReadFromFile(path)) {
			//	if (string.IsNullOrWhiteSpace(item) || !item.Contains("="))
			//		continue;
			//	else
			//		linesFromFile.Add(item);
			//}

			var linesFromFile = Document.ReadFromFile(path);

			if (linesFromFile.Length <= 0) Assert.Fail();

			var collectionOfRandomValues = GenerateRandomValues(numberOfParametersToTest);

			var lines = new string[50];
			for (int i = 0; i < numberOfParametersToTest; i++)
				lines[i] = Text.RemoveDiacritics(linesFromFile[collectionOfRandomValues[i]]);

			GetParameterCodeFromEachOfTheChosenLines(viewModel, lines);

			var result = viewModel.CollectValues();

			Assert.IsTrue(result);
		}
		private static void GetParameterCodeFromEachOfTheChosenLines(ParametersViewModel viewModel, string[] lines)
		{
			var collectionOfValidCodes = new Collection<string>() { "MS", "MA", "MD", "ES", "PR", "SL" };
			int uselessInt;
			viewModel.ParametersCollection.Clear();
			foreach (var line in lines) {
				var parameter = new Parameter();
				parameter.ParameterLine = line;
				if(!parameter.GetParameterCode()) continue;
				bool isSafeCode = false;
				if (parameter.Code != null)
					isSafeCode = collectionOfValidCodes.Any(str => parameter.Code.StartsWith(str));
				if (!int.TryParse(parameter.Code, out uselessInt) && !isSafeCode)
					continue;
				viewModel.ParametersCollection.Add(parameter);
			}
		}

		[TestMethod]
		public void RecheckSizeOfCSVFileAfterCollectingValues()
		{// If file size is bigger, then it means something was written to it
			var viewModel = new ParametersViewModel();
			var initialSize = new FileInfo(viewModel.CSVFilePath).Length;
			TestMultipleParameters();
			var finalSize = new FileInfo(viewModel.CSVFilePath).Length;
			Assert.AreNotEqual(initialSize, finalSize);
		}

		[TestMethod]
		public void CollectValues_NameOfFileToSearchIsEmpty_ReturnFalse()
		{
			var viewModel = new ParametersViewModel { NameOfFileToSearch = string.Empty };
			var result = viewModel.CollectValues();

			Assert.IsFalse(result);
		}

		[TestMethod]
		public void CollectValues_BaseMachinesParametersNotExists_ReturnFalse()
		{
			// Arrange
			var viewModel = new ParametersViewModel();

			// Act
			var collectResult = viewModel.CollectValues();

			// Assert
			if (!File.Exists(viewModel.BaseMachineParameters))
				Assert.IsFalse(collectResult);
		}


		// ParameterModel.cs
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
			var randomValues = GenerateRandomValues(numberOfLines);
			var parameterLines = new Collection<string>();
			var path = new ParametersViewModel().BaseMachineParameters;

			foreach (var item in randomValues) {
				parameterLines.Add(Document.ReadSpecificLineFromFile(path, item));
			}

			return parameterLines;
		}
		private static Collection<int> GenerateRandomValues(int amountOfValuesToGenerate)
		{
			var random = new Random();
			var collectionOfRandomValues = new Collection<int>();

			for (int i = 0; i < amountOfValuesToGenerate; i++) {
				var value = random.Next(1000);
				if (!collectionOfRandomValues.Contains(value))
					collectionOfRandomValues.Add(value);
				else
					i--;
			}

			return collectionOfRandomValues;
		}
	}
}
