using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tabular
{
	public static class StringHelpers
	{
		public static string Centre(this string s, int width)
		{
			int totalMarginWidth = width - s.Length;

			string l = "".PadLeft((totalMarginWidth) / 2, ' ');
			string r = "".PadLeft(width - s.Length - l.Length, ' ');

			return l + s + r;
		}
	}
}
