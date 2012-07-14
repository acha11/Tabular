using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tabular
{
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
}
