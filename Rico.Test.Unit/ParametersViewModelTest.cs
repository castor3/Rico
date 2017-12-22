using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Rico.Models;
using Rico.ViewModels;

namespace Rico.Test.Unit
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
		public void CheckInputBoxIsClearAfterParameterAdded()
		{
			var viewModel = new ParametersViewModel();

			viewModel.ParameterBoxContent = "test";
			viewModel.AddParameter();

			Assert.IsTrue(string.IsNullOrEmpty(viewModel.ParameterBoxContent));
		}

		[TestMethod]
		public void CollectValues_NameOfFileToSearchIsEmpty_ReturnFalse()
		{
			var viewModel = new ParametersViewModel { NameOfFileToSearch = string.Empty };

			var result = viewModel.CollectValues();

			Assert.IsFalse(result);
		}

		//[TestMethod]
		//public void CollectValues_IfGetPathsOfParametersFilesFails_ReturnFalse()
		//{
		//	var viewModel = new ParametersViewModel();
		//	var mockParametersViewModel = new Mock<ParametersViewModel>();

		//	var result = viewModel.CollectValues();

		//	Assert.IsFalse(result);
		//}

		//[TestMethod]
		//public void CollectValues_NumberOfParametersNotFoundBiggerThan0_ReturnFalse()
		//{
		//	var viewModel = new ParametersViewModel();
		//	var parameterValidationModel = new ParameterValidationModel();

		//	parameterValidationModel.NumberOfParametersNotFound = 1;
		//	var result = viewModel.CollectValues();

		//	Assert.IsFalse(result);
		//}

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
		public void RemoveParameterTest()
		{
			// Act
			var viewModel = new ParametersViewModel();
			var mockParameterModel = new Mock<ParameterModel>().Object;
			mockParameterModel.Name = "test";
			

			// Act
			var initialCount = viewModel.ParametersCollection.Count;
			
			viewModel.ParametersCollection.Add(mockParameterModel);
			
			var intermediateCount = viewModel.ParametersCollection.Count;

			if (intermediateCount == initialCount) Assert.Fail();

			viewModel.ParametersCollectionSelectedItem = mockParameterModel;
			viewModel.RemoveParameter();
			
			var finalCount = viewModel.ParametersCollection.Count;


			// Assert
			Assert.AreEqual(initialCount, finalCount);
		}
	}
}
