using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tabular
{
	public static class TableWriterHelper
	{
		public static string RenderValue(TableColumn tableColumn, object value)
		{
			if (value == null) return "null";

			if (tableColumn.FormatSpecifier != null)
			{
				Type t = value.GetType();

				if (t == typeof(long))
				{
					return ((long)value).ToString(tableColumn.FormatSpecifier);
				}

				if (t == typeof(int))
				{
					return ((int)value).ToString(tableColumn.FormatSpecifier);
				}

				if (t == typeof(float))
				{
					return ((float)value).ToString(tableColumn.FormatSpecifier);
				}

				if (t == typeof(decimal))
				{
					return ((decimal)value).ToString(tableColumn.FormatSpecifier);
				}

				if (t == typeof(double))
				{
					return ((double)value).ToString(tableColumn.FormatSpecifier);
				}

				throw new Exception("Unable to apply format specifier ('" + tableColumn.FormatSpecifier + "') to value of type '" + t.FullName + "'");
			}

			return value.ToString();
		}
	}
}
