using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rico.Models;
using Rico.ViewModels;
using SupportFiles;

namespace Rico.Test.Integration
{
	[TestClass]
	public class ParametersViewModelTest
	{
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

		[TestMethod]
		public void Recheck_Size_DateOfModification_OfFilesAfterCollectingValues()
		{// If file size is bigger, then it means something was written to it

			// Arrange
			const int numberOfParametersToTest = 50;

			var viewModel = new ParametersViewModel();

			var initialSizeCSV = new FileInfo(viewModel.CSVFilePath).Length;
			var initialDateCSV = new FileInfo(viewModel.CSVFilePath).LastWriteTime;
			var initialDatePaths = new FileInfo(viewModel.ParametersFilesPaths).LastWriteTime;

			//var path = Path.Combine(@"..\..\..\..\Rico\Rico\bin\Debug\", viewModel.BaseMachineParameters);
			var path = viewModel.BaseMachineParameters;


			// Act
			var linesFromFile = General.GetValidLinesFromFile(path);

			if (linesFromFile.Count <= 0) Assert.Fail();

			var collectionOfRandomValues = General.GenerateRandomValues(numberOfParametersToTest, linesFromFile.Count);

			var lines = new string[numberOfParametersToTest];

			for (var i = 0; i < numberOfParametersToTest; i++)
			{
				lines[i] = Text.RemoveDiacritics(linesFromFile[collectionOfRandomValues[i]]);
			}

			GetParameterCodeFromEachOfTheChosenLines(viewModel, lines);

			var result = viewModel.CollectValues();

			var finalSizeCSV = new FileInfo(viewModel.CSVFilePath).Length;
			var finalDateCSV = new FileInfo(viewModel.CSVFilePath).LastWriteTime;
			var finalDatePaths = new FileInfo(viewModel.ParametersFilesPaths).LastWriteTime;


			// Assert
			if (result)
			{
				Assert.AreNotEqual(initialSizeCSV, finalSizeCSV);
				Assert.AreNotEqual(initialDateCSV, finalDateCSV);
				Assert.AreNotEqual(initialDatePaths, finalDatePaths);
			}
			else
			{
				Assert.AreEqual(initialSizeCSV, finalSizeCSV);
				Assert.AreEqual(initialDateCSV, finalDateCSV);
			}
		}

		[TestMethod]
		public void TestMultipleParameters()
		{// Randomly picks 50 parameters to test

			// Arrange
			const int numberOfParametersToTest = 50;
			var viewModel = new ParametersViewModel();
			//var path = Path.Combine(@"..\..\..\..\Rico\Rico\bin\Debug\", viewModel.BaseMachineParameters);
			var path = viewModel.BaseMachineParameters;
			viewModel.NameOfFolderToSearch = "Untitled Folder";

			// Act
			var linesFromFile = General.GetValidLinesFromFile(path);

			if (linesFromFile.Count <= 0)
			{
				Assert.Fail();
			}

			var collectionOfRandomValues = General.GenerateRandomValues(numberOfParametersToTest, linesFromFile.Count);

			var lines = new string[numberOfParametersToTest];

			for (var i = 0; i < numberOfParametersToTest; i++)
			{
				lines[i] = Text.RemoveDiacritics(linesFromFile[collectionOfRandomValues[i]]);
			}

			GetParameterCodeFromEachOfTheChosenLines(viewModel, lines);

			var result = viewModel.CollectValues();

			// Assert
			Assert.IsTrue(result);
		}

		private static void GetParameterCodeFromEachOfTheChosenLines(ParametersViewModel viewModel, string[] lines)
		{
			if (lines == null)
			{
				return;
			}

			var collectionOfValidCodes = new Collection<string> { "MS", "MA", "MD", "ES", "PR", "SL" };
			viewModel.ParametersCollection.Clear();
			foreach (var line in lines)
			{
				var parameter = new ParameterModel { ParameterLine = line };
				if (!parameter.GetParameterNameAndCode())
				{
					continue;
				}

				var isSafeCode = false;
				if (parameter.Code != null)
				{
					isSafeCode = collectionOfValidCodes.Any(str => parameter.Code.StartsWith(str));
				}

				int uselessInt;
				if (!int.TryParse(parameter.Code, out uselessInt) && !isSafeCode)
				{
					continue;
				}
				viewModel.ParametersCollection.Add(parameter);
			}
		}
	}
}
