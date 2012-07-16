using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tabular
{
	public interface ITableWriter
	{
		void StartTable(TableStructure tableStructure);
		void EndTable();
		void StartRow();
		void EndRow();
		void WriteCell(TableColumn column, object value);

		bool UsesColumnWidth { get; }
	}
}
