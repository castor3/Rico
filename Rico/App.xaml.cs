using System;
using System.Diagnostics;
using System.Windows;
using Rico.ViewModels;
using SupportFiles;
using System.Linq;

namespace Rico
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		static App()
		{
			AppDomain.CurrentDomain.UnhandledException += HandleGeneralException;
		}

		static void HandleGeneralException(object sender, UnhandledExceptionEventArgs e)
		{
			Document.WriteToLogFile(ParametersViewModel.LogFilePath, ((Exception)e.ExceptionObject).Message.TrimEnd('.') + $" in method '{GetCurrentMethod()}()'");
		}
		static string GetCurrentMethod() => new StackTrace().GetFrame(2).GetMethod().Name;
	}
}
