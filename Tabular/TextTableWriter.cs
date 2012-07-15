using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Tabular
{
	public class TextTableWriter : ITableWriter
	{
		public struct BorderCharacterSet
		{
			public char TopLeftCorner;
			public char TopRightCorner;
			public char BottomLeftCorner;
			public char BottomRightCorner;

			public char Horizontal;
			public char Vertical;

			public char RightwardsT;
			public char LeftwardsT;
			public char DownwardsT;
			public char UpwardsT;

			public char Cross;
		}

		public static readonly BorderCharacterSet ExtendedAsciiBorderCharacterSet = new BorderCharacterSet
		{
			TopLeftCorner = '┌',
			TopRightCorner = '┐',
			BottomLeftCorner = '└',
			BottomRightCorner = '┘',

			Horizontal = '─',
			Vertical = '│',

			RightwardsT = '├',
			LeftwardsT = '┤',

			DownwardsT = '┬',
			UpwardsT = '┴',

			Cross = '┼'
		};

		public static readonly BorderCharacterSet BasicAsciiBorderCharacterSet = new BorderCharacterSet
		{
			TopLeftCorner = '+',
			TopRightCorner = '+',
			BottomLeftCorner = '+',
			BottomRightCorner = '+',

			Horizontal = '-',
			Vertical = '|',

			RightwardsT = '+',
			LeftwardsT = '+',

			DownwardsT = '+',
			UpwardsT = '+',

			Cross = '+'
		};

		TextWriter _textWriter;
		TableStructure _structure;

		BorderCharacterSet _borderCharacterSet;

		public TextTableWriter(TextWriter textWriter)
		{
			_textWriter = textWriter;

			_borderCharacterSet = ExtendedAsciiBorderCharacterSet;
		}

		public void SetBorderCharacterSet(BorderCharacterSet borderCharacterSet)
		{
			_borderCharacterSet = borderCharacterSet;
		}

		public void StartTable(TableStructure tableStructure)
		{
			_structure = tableStructure;

			WriteHeader();
		}

		int _cellHorizontalPadding = 1;

		public void WriteFooter()
		{
			bool isFirstGroup = true;

			foreach (var cg in _structure.ColumnGroups)
			{
				_textWriter.Write(isFirstGroup ? _borderCharacterSet.BottomLeftCorner : _borderCharacterSet.UpwardsT);

				isFirstGroup = false;

				foreach (var c in cg.Columns)
				{
					_textWriter.Write("".PadLeft(c.Width + _cellHorizontalPadding * 2, _borderCharacterSet.Horizontal));
				}
			}

			_textWriter.WriteLine(_borderCharacterSet.BottomRightCorner);
		}

		private void WriteHeader()
		{
			// First line
			bool isFirstGroup = true;

			foreach (var cg in _structure.ColumnGroups)
			{
				_textWriter.Write(isFirstGroup ? _borderCharacterSet.TopLeftCorner : _borderCharacterSet.DownwardsT);

				isFirstGroup = false;

				foreach (var c in cg.Columns)
				{
					_textWriter.Write("".PadLeft(c.Width + _cellHorizontalPadding * 2, _borderCharacterSet.Horizontal));
				}
			}

			_textWriter.WriteLine(_borderCharacterSet.TopRightCorner);

			// If we have any column groups with headings, write a line for column group headings
			if (_structure.ColumnGroups.Any(cg => cg.Title.Length > 0))
			{
				foreach (var cg in _structure.ColumnGroups)
				{
					_textWriter.Write(_borderCharacterSet.Vertical);

					int cgWidth = cg.Columns.Sum(c => c.Width + _cellHorizontalPadding * 2);

					_textWriter.Write(cg.Title.Centre(cgWidth));
				}

				_textWriter.WriteLine(_borderCharacterSet.Vertical);

				// Followed by a line to separate the column group headings from the column headings
				isFirstGroup = true;

				foreach (var cg in _structure.ColumnGroups)
				{
					_textWriter.Write(isFirstGroup ? _borderCharacterSet.RightwardsT : _borderCharacterSet.Cross);

					isFirstGroup = false;

					foreach (var c in cg.Columns)
					{
						_textWriter.Write("".PadLeft(c.Width + _cellHorizontalPadding * 2, _borderCharacterSet.Horizontal));
					}
				}

				_textWriter.WriteLine(_borderCharacterSet.LeftwardsT);
			}

			// If we have any columns with titles
			if (_structure.GetAllColumns().Any(c => c.Title.Length > 0))
			{
				foreach (var cg in _structure.ColumnGroups)
				{
					_textWriter.Write(_borderCharacterSet.Vertical);

					foreach (var c in cg.Columns)
					{
						_textWriter.Write(c.Title.Centre(c.Width + _cellHorizontalPadding * 2));
					}
				}

				_textWriter.WriteLine(_borderCharacterSet.Vertical);

				isFirstGroup = true;

				foreach (var cg in _structure.ColumnGroups)
				{
					_textWriter.Write(isFirstGroup ? _borderCharacterSet.RightwardsT : _borderCharacterSet.Cross);

					isFirstGroup = false;

					foreach (var c in cg.Columns)
					{
						_textWriter.Write("".PadLeft(c.Width + _cellHorizontalPadding * 2, _borderCharacterSet.Horizontal));
					}
				}

				_textWriter.WriteLine(_borderCharacterSet.LeftwardsT);
			}
		}

		public void StartRow()
		{
			_textWriter.Write(_borderCharacterSet.Vertical);
		}

		public void EndTable()
		{
			WriteFooter();
		}

		void ITableWriter.EndRow()
		{
			_textWriter.WriteLine(_borderCharacterSet.Vertical);
		}

		public void WriteCell(TableColumn column, string value)
		{
			// If this is the first column in a group other than the first, then write a separator
			if (_structure.ColumnGroups.Skip(1).Any(cg => cg.Columns[0] == column))
			{
				_textWriter.Write(_borderCharacterSet.Vertical);
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


		public bool UsesColumnWidth
		{
			get { return true; }
		}

	}
}
