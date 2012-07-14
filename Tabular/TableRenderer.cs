using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

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

	public class MultipleTargetTableWriter : ITableWriter
	{
		ITableWriter[] _targets;

		public MultipleTargetTableWriter(IEnumerable<ITableWriter> targets)
		{
			_targets = targets.ToArray();
		}

		public MultipleTargetTableWriter(params ITableWriter[] targets)
		{
			_targets = targets.ToArray();
		}
		
		public void StartTable(TableStructure tableStructure)
		{
			foreach (var target in _targets) target.StartTable(tableStructure);
		}

		public void EndTable()
		{
			foreach (var target in _targets) target.EndTable();
		}

		public void StartRow()
		{
			foreach (var target in _targets) target.StartRow();
		}

		public void EndRow()
		{
			foreach (var target in _targets) target.EndRow();
		}

		public void WriteCell(TableColumn column, string value)
		{
			foreach (var target in _targets) target.WriteCell(column, value);
		}
	}

	public class HtmlTableWriter : ITableWriter
	{
		StreamWriter _sw;
		TableStructure _structure;

		public HtmlTableWriter(StreamWriter sw)
		{
			_sw = sw;
		}

		public void StartTable(TableStructure tableStructure)
		{
			_structure = tableStructure;

			string id = "table" + Guid.NewGuid().ToString().Replace('-', '_');

			//_sw.WriteLine("<style>td, th { border: solid black 1px; border-collapse: collapse; border-spacing: 0px; }</style>");
			_sw.WriteLine("<style>#" + id + ", #" + id + " th, #" + id + " td { border: solid rgb(200, 200, 200) 1px; border-collapse: collapse; border-spacing: 0px; padding: 3px; }" +
					" #" + id + " th { background-color: rgb(230, 230, 255); }</style>");
			_sw.WriteLine("<table id=\"" + id + "\">");

			if (_structure.ColumnGroups.Any(cg => cg.Title.Length > 0))
			{
				_sw.Write("<tr>");
				foreach (var cg in _structure.ColumnGroups)
				{
					_sw.Write("<th colspan=\"" + cg.Columns.Count() + "\">" + cg.Title + "</th>");
				}
				_sw.Write("</tr>");
			}

			var allColumns = _structure.GetAllColumns();

			if (allColumns.Any(c => c.Title.Length > 0))
			{
				_sw.Write("<tr>");

				foreach (var c in allColumns)
				{
					_sw.Write("<th>" + c.Title + "</th>");
				}

				_sw.Write("</tr>");
			}
		}

		public void EndTable()
		{
			_sw.WriteLine("</table>");
		}

		public void StartRow()
		{
			_sw.WriteLine("<tr>");
		}

		public void EndRow()
		{
			_sw.WriteLine("<tr>");
		}

		private string BuildCellCss(TableColumn column)
		{
			if (column.HorizontalAlignment == HorizontalAlignment.Right)
			{
				return "text-align: right;";
			}

			return "";
		}

		public void WriteCell(TableColumn column, string value)
		{
			_sw.WriteLine("<td style=\"" + BuildCellCss(column) + "\">" + value + "</td>");
		}
	}

	public class TextTableRenderer : ITableWriter
	{
		TableStructure _structure;

		public void StartTable(TableStructure tableStructure)
		{
			_structure = tableStructure;

			WriteHeader();
		}

		int _cellHorizontalPadding = 1;

		public void WriteFooter()
		{
			foreach (var cg in _structure.ColumnGroups)
			{
				Console.Write("+");

				foreach (var c in cg.Columns)
				{
					Console.Write("".PadLeft(c.Width + _cellHorizontalPadding * 2, '-'));
				}
			}

			Console.WriteLine("+");
		}

		private void WriteHeader()
		{
			foreach (var cg in _structure.ColumnGroups)
			{
				Console.Write("+");

				foreach (var c in cg.Columns)
				{
					Console.Write("".PadLeft(c.Width + _cellHorizontalPadding * 2, '-'));
				}
			}

			Console.WriteLine("+");

			if (_structure.ColumnGroups.Any(cg => cg.Title.Length > 0))
			{

				foreach (var cg in _structure.ColumnGroups)
				{
					Console.Write("|");

					int cgWidth = cg.Columns.Sum(c => c.Width + _cellHorizontalPadding * 2);

					Console.Write(cg.Title.Centre(cgWidth));
				}

				Console.WriteLine("|");

				foreach (var cg in _structure.ColumnGroups)
				{
					Console.Write("+");

					foreach (var c in cg.Columns)
					{
						Console.Write("".PadLeft(c.Width + _cellHorizontalPadding * 2, '-'));
					}
				}

				Console.WriteLine("+");
			}

			if (_structure.GetAllColumns().Any(c => c.Title.Length > 0))
			{
				foreach (var cg in _structure.ColumnGroups)
				{
					Console.Write("|");

					foreach (var c in cg.Columns)
					{
						Console.Write(c.Title.Centre(c.Width + _cellHorizontalPadding * 2));
					}
				}

				Console.WriteLine("|");

				foreach (var cg in _structure.ColumnGroups)
				{
					Console.Write("+");

					foreach (var c in cg.Columns)
					{
						Console.Write("".PadLeft(c.Width + _cellHorizontalPadding * 2, '-'));
					}
				}

				Console.WriteLine("+");
			}
		}

		public void StartRow()
		{
			Console.Write("|");
		}

		public void EndTable()
		{
			WriteFooter();
		}

		void ITableWriter.EndRow()
		{
			Console.WriteLine("|");
		}

		public void WriteCell(TableColumn column, string value)
		{
			// If this is the first column in a group other than the first, then write a separator
			if (_structure.ColumnGroups.Skip(1).Any(cg => cg.Columns[0] == column))
			{
				Console.Write("|");
			}

            // Pad left
            Console.Write("".PadLeft(_cellHorizontalPadding));

            // Write no more than columnWidth characters

            string toWrite = value;

            if (toWrite.Length > column.Width)
            {
                if (column.Width < 3)
                {
                    toWrite = toWrite.Substring(0, column.Width);
                }
                else
                {
                    toWrite = toWrite.Substring(0, column.Width - 3) + "...";
                }
            }

            switch (column.HorizontalAlignment)
            {
                case HorizontalAlignment.Right:
			        Console.Write(toWrite.PadLeft(column.Width));
                    break;
                case HorizontalAlignment.Left:
			        Console.Write(toWrite.PadRight(column.Width));
                    break;
                case HorizontalAlignment.Centre:
                    Console.Write(toWrite.Centre(column.Width));
                    break;
            }

            // Pad right
            Console.Write("".PadLeft(_cellHorizontalPadding));
        }
	}

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
