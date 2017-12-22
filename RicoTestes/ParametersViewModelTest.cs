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
	public class ParametersViewModelTest
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

			var collectionOfRandomValues = General.GenerateRandomValues(numberOfParametersToTest);

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
	}
}
