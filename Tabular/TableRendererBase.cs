using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tabular
{
	public class TableRenderer
	{
		public static void Render<T>(IEnumerable<T> data, ITableWriter tableWriter, params int[] columnWidths)
		{
			// First, create structure
			TableStructure ts = new TableStructure();
			TableColumnGroup tcg = new TableColumnGroup("");
			ts.ColumnGroups.Add(tcg);

			Type enumerableType = data.GetType();

			Type ienumerableInterfaceType = enumerableType.GetInterfaces().FirstOrDefault(i => i.Name == "IEnumerable`1");

			var typeOfT = ienumerableInterfaceType.GetGenericArguments()[0];

			if (typeOfT.IsPrimitive)
			{
				// Go through the primitive render path
				int columnWidth = 20;

				if (columnWidths.Length > 0)
				{
					columnWidth = columnWidths[0];
				}

				tcg.Columns.Add(new TableColumn() { Name = "", Title = "", Width = columnWidth });

				tableWriter.StartTable(ts);

				foreach (var item in data)
				{
					tableWriter.StartRow();

					tableWriter.WriteCell(tcg.Columns[0], item.ToString());

					tableWriter.EndRow();
				}
			}
			else
			{
				// Enumerate properties and render those
				var properties = typeOfT.GetProperties();

				int propertyNumber = 0;

				foreach (var property in properties)
				{
					int columnWidth = 30;

					if (columnWidths.Length > propertyNumber)
					{
						columnWidth = columnWidths[propertyNumber];
					}

					var col = new TableColumn() { Name = property.Name, Title = property.Name, Width = columnWidth };

					if (property.PropertyType == typeof(CurrencyValue))
					{
						col.HorizontalAlignment = HorizontalAlignment.Right;
					}

					tcg.Columns.Add(col);

					propertyNumber++;
				}

				var allColumns = ts.GetAllColumns().ToArray();

				tableWriter.StartTable(ts);

				foreach (var item in data)
				{
					tableWriter.StartRow();

					int colIdx = 0;

					foreach (var property in properties)
					{
						object value =  property.GetValue(item, null);
						string toRender = value == null ? "null" : value.ToString();

						tableWriter.WriteCell(allColumns[colIdx++], toRender);
					}

					tableWriter.EndRow();
				}
			}

			tableWriter.EndTable();
		}

		ITableWriter _target;
		TableStructure _structure;
		TableColumn _currentColumn;
		bool _haveSentStartRow = false;

		public TableRenderer(ITableWriter target)
		{
			_target = target;
		}

		public void StartTable(TableStructure tableStructure)
		{
			_structure = tableStructure;

			_currentColumn = tableStructure.ColumnGroups[0].Columns[0];

			_target.StartTable(tableStructure);
		}

		public void EndTable()
		{
			// If we're writing a row, then finish it
			if (_haveSentStartRow)
			{
				while (_currentColumn != _structure.ColumnGroups[0].Columns[0])
				{
					SkipCell();
				}
			}

			_target.EndTable();
		}

		private void SkipCellsUntil(string columnName)
		{
			if (!_structure.GetAllColumns().Any(c => c.Name == columnName))
			{
				throw new ArgumentException("Table structure doesn't contain a column named '" + columnName + "'");
			}

			while (_currentColumn.Name != columnName)
			{
				SkipCell();
			}
		}

		private void WriteCellInternal(TableColumn column, string value)
		{
			if (!_haveSentStartRow)
			{
				_target.StartRow();

				_haveSentStartRow = true;
			}

			_target.WriteCell(column, value);

			bool leftRow = false;
			_currentColumn = _structure.GetNextColumnAfter(_currentColumn, out leftRow);

			if (leftRow)
			{
				_target.EndRow();
				_haveSentStartRow = false;
			}
		}

		private void SkipCell()
		{
			WriteCellInternal(_currentColumn, "");
		}

		public void WriteCell(string columnName, string value)
		{
			SkipCellsUntil(columnName);

			WriteCellInternal(_currentColumn, value);
		}

		public void WriteCell(string columnName, decimal value)
		{
			SkipCellsUntil(columnName);

			WriteCellInternal(_currentColumn, value.ToString(_currentColumn.FormatSpecifier));
		}
	}
}
