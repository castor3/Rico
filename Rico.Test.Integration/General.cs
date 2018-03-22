using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SupportFiles;

namespace Rico.Test.Integration
{
	internal class General
	{
		public static Collection<int> GenerateRandomValues(int amountOfValuesToGenerate, int maxValue)
		{
			var random = new Random();
			var collectionOfRandomValues = new Collection<int>();

			for (var i = 0; i < amountOfValuesToGenerate; i++)
			{
				var value = random.Next(maxValue);
				if (!collectionOfRandomValues.Contains(value))
					collectionOfRandomValues.Add(value);
				else
					i--;
			}

			return collectionOfRandomValues;
		}

		public static List<string> GetValidLinesFromFile(string path)
		{
			var linesFromFile = Document.ReadFromFile(path).ToList();
			for (var i = linesFromFile.Count - 1; i >= 0; i--)
			{
				if (string.IsNullOrWhiteSpace(linesFromFile[i]) || !linesFromFile[i].Contains("="))
				{
					linesFromFile.RemoveAt(i);
				}
			}
			return linesFromFile;
		}
	}
}
