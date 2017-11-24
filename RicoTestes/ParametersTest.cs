using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rico.ViewModels;
using Rico.Models;
using System;
using SupportFiles;

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
			var randomLine = new Random().Next(1050);
			Document.ReadFromFile()
		}
	}
}
