using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tabular
{
	public class CurrencyValue
	{
		public decimal Value;

		public CurrencyValue(decimal value)
		{
			Value = value;
		}

		public override string ToString()
		{
			return Value.ToString("C");
		}
	}
}
