using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tabular
{
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
}
