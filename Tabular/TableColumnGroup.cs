using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tabular
{
	public class TableColumnGroup
	{
		public string Title;
		public List<TableColumn> Columns;

		public TableColumnGroup(string title)
		{
			Title = title;
			Columns = new List<TableColumn>();
		}
	}
}
