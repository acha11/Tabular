using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tabular
{
	public class TableStructure
	{
		public int RowsToExamineWhenAutoSizingColumns { get; set; }
		public List<TableColumnGroup> ColumnGroups { get; set; }

		public TableStructure()
		{
			ColumnGroups = new List<TableColumnGroup>();
			RowsToExamineWhenAutoSizingColumns = 10;
		}

		public IEnumerable<TableColumn> GetAllColumns()
		{
			return ColumnGroups.SelectMany(cg => cg.Columns);
		}

		public TableColumn GetNextColumnAfter(TableColumn tc, out bool leftRow)
		{
			bool foundColumn = false;
			leftRow = false;

			foreach (var cg in ColumnGroups)
			{
				foreach (var c in cg.Columns)
				{
					if (foundColumn)
					{
						return c;
					}

					if (c.Title == tc.Title)
					{
						foundColumn = true;
					}
				}
			}

			if (foundColumn)
			{
				leftRow = true;
				return ColumnGroups[0].Columns[0];
			}

			throw new Exception("Supplied column is not in list of columns");
		}

		public static TableStructureBuilder Build()
		{
			return new TableStructureBuilder();
		}
	}
}
