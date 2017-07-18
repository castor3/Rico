using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rico.ViewModels;
using DeepEqual.Syntax;


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
			//Assert.AreEqual(initialCount-1, initialCount);
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
	}
}
