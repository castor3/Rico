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
		private string _name;
		public string Name
		{
			get { return _name; }
			set {
				if (_name != value)
					_name = value;
			}
		}
		public double Average { get; set; }
		public int NumberOfOcurrencesFound { get; set; }
		public bool DidFindParameter { get; set; }
	}
}
