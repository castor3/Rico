using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rico.Models;
using Rico.ViewModels;
using SupportFiles;

namespace RicoTestes
{
	[TestClass]
	public class ParametersTest
	{
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

			var linesFromFile = new Collection<string>();

			foreach (var item in Document.YieldReturnLinesFromFile(path)) {
				if (string.IsNullOrWhiteSpace(item) || !item.Contains('='))
					continue;
				else
					linesFromFile.Add(item);
			}

			if (!linesFromFile.Any()) Assert.Fail();

			var collectionOfRandomValues = GenerateRandomValues(numberOfParametersToTest, linesFromFile);

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
				parameter.GetParameterCode();
				bool isSafeCode = false;
				if (parameter.Code != null)
					isSafeCode = collectionOfValidCodes.Any(str => parameter.Code.StartsWith(str));
				if (!int.TryParse(parameter.Code, out uselessInt) && !isSafeCode)
					continue;
				viewModel.ParametersCollection.Add(parameter);
			}
		}
		private static Collection<int> GenerateRandomValues(int numberOfParametersToTest, Collection<string> linesFromFile)
		{
			var random = new Random();
			var collectionOfRandomValues = new Collection<int>();
			for (int i = 0; i < numberOfParametersToTest; i++) {
				var value = random.Next(linesFromFile.Count - 1);
				if (!collectionOfRandomValues.Contains(value))
					collectionOfRandomValues.Add(value);
				else
					i--;
			}

			return collectionOfRandomValues;
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
	}
}
