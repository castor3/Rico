﻿using System;
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
			var lines = new string[50];
			var path = Path.Combine(@"..\..\..\..\Rico\Rico\bin\Debug\", viewModel.BaseMachineParameters);

			var linesFromFile = new Collection<string>();
			foreach (var item in Document.YieldReturnLinesFromFile(path)) {
				if (string.IsNullOrWhiteSpace(item) || !item.Contains('='))
					continue;
				else
					linesFromFile.Add(item);
			}

			if (!linesFromFile.Any()) Assert.Fail();

			//var tempLinesFromFile = tempLinesFromFile.ToArray();
			var random = new Random();
			var randomValue = 0;
			var auxString = string.Empty;
			for (int i = 0; i < numberOfParametersToTest; i++) {
				randomValue = random.Next(linesFromFile.Count - 1);
				lines[i] = Text.RemoveDiacritics(linesFromFile[randomValue - 1]);
			}

			viewModel.ParametersCollection.Clear();
			foreach (var line in lines) {
				var parameter = new Parameter();
				parameter.ParameterLine = line;
				parameter.GetParameterCode();
				viewModel.ParametersCollection.Add(parameter);
			}
			viewModel.CollectValues();
		}
	}
}
