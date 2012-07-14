using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Tabular
{
	public class TextTableWriter : ITableWriter
	{
		TextWriter _textWriter;

		TableStructure _structure;

		public TextTableWriter(TextWriter textWriter)
		{
			_textWriter = textWriter;
		}

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
				_textWriter.Write("+");

				foreach (var c in cg.Columns)
				{
					_textWriter.Write("".PadLeft(c.Width + _cellHorizontalPadding * 2, '-'));
				}
			}

			_textWriter.WriteLine("+");
		}

		private void WriteHeader()
		{
			foreach (var cg in _structure.ColumnGroups)
			{
				_textWriter.Write("+");

				foreach (var c in cg.Columns)
				{
					_textWriter.Write("".PadLeft(c.Width + _cellHorizontalPadding * 2, '-'));
				}
			}

			_textWriter.WriteLine("+");

			if (_structure.ColumnGroups.Any(cg => cg.Title.Length > 0))
			{

				foreach (var cg in _structure.ColumnGroups)
				{
					_textWriter.Write("|");

					int cgWidth = cg.Columns.Sum(c => c.Width + _cellHorizontalPadding * 2);

					_textWriter.Write(cg.Title.Centre(cgWidth));
				}

				_textWriter.WriteLine("|");

				foreach (var cg in _structure.ColumnGroups)
				{
					_textWriter.Write("+");

					foreach (var c in cg.Columns)
					{
						_textWriter.Write("".PadLeft(c.Width + _cellHorizontalPadding * 2, '-'));
					}
				}

				_textWriter.WriteLine("+");
			}

			if (_structure.GetAllColumns().Any(c => c.Title.Length > 0))
			{
				foreach (var cg in _structure.ColumnGroups)
				{
					_textWriter.Write("|");

					foreach (var c in cg.Columns)
					{
						_textWriter.Write(c.Title.Centre(c.Width + _cellHorizontalPadding * 2));
					}
				}

				_textWriter.WriteLine("|");

				foreach (var cg in _structure.ColumnGroups)
				{
					_textWriter.Write("+");

					foreach (var c in cg.Columns)
					{
						_textWriter.Write("".PadLeft(c.Width + _cellHorizontalPadding * 2, '-'));
					}
				}

				_textWriter.WriteLine("+");
			}
		}

		public void StartRow()
		{
			_textWriter.Write("|");
		}

		public void EndTable()
		{
			WriteFooter();
		}

		void ITableWriter.EndRow()
		{
			_textWriter.WriteLine("|");
		}

		public void WriteCell(TableColumn column, string value)
		{
			// If this is the first column in a group other than the first, then write a separator
			if (_structure.ColumnGroups.Skip(1).Any(cg => cg.Columns[0] == column))
			{
				_textWriter.Write("|");
			}

			// Pad left
			_textWriter.Write("".PadLeft(_cellHorizontalPadding));

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
					_textWriter.Write(toWrite.PadLeft(column.Width));
					break;
				case HorizontalAlignment.Left:
					_textWriter.Write(toWrite.PadRight(column.Width));
					break;
				case HorizontalAlignment.Centre:
					_textWriter.Write(toWrite.Centre(column.Width));
					break;
			}

			// Pad right
			_textWriter.Write("".PadLeft(_cellHorizontalPadding));
		}
	}
}
