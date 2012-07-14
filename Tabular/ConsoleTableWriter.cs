using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tabular
{
	public class ConsoleTableWriter : TextTableWriter
	{
		public ConsoleTableWriter()
			: base(Console.Out)
		{
		}
	}
}
