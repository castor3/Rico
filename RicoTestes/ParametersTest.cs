using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rico.ViewModels;
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
			viewModel.AddParameter("test");
			var finalCount = viewModel.ParametersCollection.Count;
			Assert.AreNotEqual(initialCount, finalCount);
		}
		[TestMethod]
		public void RemoveParameterTest()
		{
			ParametersViewModel viewModel = new ParametersViewModel();
			viewModel.AddParameter("test");
			var initialCount = viewModel.ParametersCollection.Count;
			viewModel.RemoveParameter("test");
			Assert.AreEqual(initialCount, 1);
		}

		[TestMethod]
		public void PreventDuplicateParameterTest()
		{
			ParametersViewModel viewModel = new ParametersViewModel();
			viewModel.AddParameter("test");
			var initialCount = viewModel.ParametersCollection.Count;
			viewModel.AddParameter("test");
			var finalCount = viewModel.ParametersCollection.Count;
			Assert.AreEqual(initialCount, finalCount);
		}

		[TestMethod]
		public void CheckInputBoxIsClearAfterParameterAdded()
		{
			ParametersViewModel viewModel = new ParametersViewModel();
			viewModel.ParameterBoxContent = "valor";
			viewModel.AddParameter("test");
			bool nullOrEmpty = string.IsNullOrEmpty(viewModel.ParameterBoxContent);
			bool nullOrWhiteSpace = string.IsNullOrWhiteSpace(viewModel.ParameterBoxContent);
			Assert.IsTrue(nullOrEmpty || nullOrWhiteSpace);
		}
	}
}
