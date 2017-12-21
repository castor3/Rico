using System;
using System.Collections.ObjectModel;

namespace RicoTestes
{
	class General
	{
		public static Collection<int> GenerateRandomValues(int amountOfValuesToGenerate)
		{
			var random = new Random();
			var collectionOfRandomValues = new Collection<int>();

			for (int i = 0; i < amountOfValuesToGenerate; i++) {
				var value = random.Next(1000);
				if (!collectionOfRandomValues.Contains(value))
					collectionOfRandomValues.Add(value);
				else
					i--;
			}

			return collectionOfRandomValues;
		}
	}
}
