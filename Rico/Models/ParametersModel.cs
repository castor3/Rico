using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows;

namespace Rico.Models
{
	public class Parameter
	{
		private string _parameterName;
		public string ParameterName
		{
			get { return _parameterName; }
			set {
				if (_parameterName != value)
					_parameterName = value;
			}
		}
	}
}
