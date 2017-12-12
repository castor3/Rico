using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rico.Models;
using Rico.ViewModels;
using SupportFiles;
using System.Linq;
using System.IO;

namespace RicoTestes
{
	[TestClass]
	public class ParametersTest
	{
		[TestMethod]
		public void AddParameterTest()
		{
			ParametersViewModel viewModel = new ParametersViewModel();
			var initialCount = viewModel.ParametersCollection.Count;
			viewModel.ParameterBoxContent = "test";
			viewModel.AddParameter();
			var finalCount = viewModel.ParametersCollection.Count;
			Assert.AreNotEqual(initialCount, finalCount);
		}
		[TestMethod]
		public void RemoveParameterTest()
		{
			ParametersViewModel viewModel = new ParametersViewModel();
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
			ParametersViewModel viewModel = new ParametersViewModel();
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
			ParametersViewModel viewModel = new ParametersViewModel();
			viewModel.ParameterBoxContent = "test";
			viewModel.AddParameter();
			Assert.IsTrue(string.IsNullOrEmpty(viewModel.ParameterBoxContent));
		}
		//[TestMethod]
		//public void TestAverageCalculation()
		//{// Randomly picks multiple parameters to test
		//	ParametersViewModel viewModel = new ParametersViewModel();

		//}
		[TestMethod]
		public void TestMultipleParameters()
		{// Randomly picks 50 parameters to test
			const int numberOfParametersToTest = 50;
			ParametersViewModel viewModel = new ParametersViewModel();
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

			var random = new Random();
			var collectionOfRandomValues = new Collection<int>();
			for (int i = 0; i < numberOfParametersToTest; i++) {
				var value = random.Next(linesFromFile.Count - 1);
				if (!collectionOfRandomValues.Contains(value))
					collectionOfRandomValues.Add(value);
				else
					i--;
			}

			var lines = new string[50];
			for (int i = 0; i < numberOfParametersToTest; i++) {
				lines[i] = Text.RemoveDiacritics(linesFromFile[collectionOfRandomValues[i]]);
			}

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
			foreach (var item in viewModel.ParametersCollection) {
				item.Name = item.Code;
			}
			viewModel.CollectValues();
		}
	}
}
