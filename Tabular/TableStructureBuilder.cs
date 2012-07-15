using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tabular
{
	public class TableStructureBuilder
	{
		private TableStructure _structure;

		private TableColumnGroup _currentGroup;

		public TableStructureBuilder()
		{
			_structure = new TableStructure();
		}

		public TableStructureBuilder ColumnGroup(string title)
		{
			var g = new TableColumnGroup(title);

			_structure.ColumnGroups.Add(g);

			_currentGroup = g;

			return this;
		}

		public TableStructureBuilder Column(string name, string title, int width = -1, string formatSpecifier = "", HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left)
		{
			if (_currentGroup == null)
			{
				ColumnGroup("");
			}

			_currentGroup.Columns.Add(new TableColumn() { Name = name, Title = title, Width = width, FormatSpecifier = formatSpecifier, HorizontalAlignment = horizontalAlignment });

			return this;
		}

		public TableStructureBuilder Column(string name, int width = -1, string formatSpecifier = "", HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left)
		{
			return Column(name, name, width, formatSpecifier, horizontalAlignment);
		}

		public TableStructure Finish()
		{
			return _structure;
		}
	}
}
