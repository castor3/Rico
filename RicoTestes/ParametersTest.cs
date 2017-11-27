using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rico.Models;
using Rico.ViewModels;
using SupportFiles;
using System.Linq;

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
		[TestMethod]
		public void TestAverageCalculation()
		{// Randomly picks multiple parameters to test
			ParametersViewModel viewModel = new ParametersViewModel();

		}
		[TestMethod]
		public void TestMultipleParameters()
		{// Randomly picks multiple parameters to test
			ParametersViewModel viewModel = new ParametersViewModel();
			var lines = new string[50];
			viewModel.ParametersCollection.Clear();
			IEnumerable<string> tempLinesFromFile = new Collection<string>();
			Document.ReadFromFile(viewModel.BaseMachineParameters, out tempLinesFromFile);
			var linesFromFile = tempLinesFromFile.ToArray(); 
			for (int i = 0; i < 49; i++) {
				var randomLine = new Random().Next(1050);
				lines[i] = linesFromFile[randomLine - 1];
			}
			var parameterModel = new Parameter();
			foreach (var line in lines) {
				parameterModel.GetParameterName
			}
			viewModel.ParametersCollection=
			viewModel.CollectValues();
		}
	}
}
