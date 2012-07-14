using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Tabular
{
	public class TableRenderer
	{
		ITableWriter _target;
		TableStructure _structure;
		TableColumn _currentColumn;
		bool _haveSentStartRow = false;

		public static void RenderToConsole<T>(IEnumerable<T> data, int numberOfRowsToInspectWhenDeterminingColumnWidth = 10)
		{
			var tableWriter = new ConsoleTableWriter();

			RenderInternal(data, tableWriter, numberOfRowsToInspectWhenDeterminingColumnWidth, new int[0]);
		}
		
		/// <summary>
		/// Renders the supplied collection as a table to the supplied table writer.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data">The collection of objects to be rendered; each item in the collection appears as a row in the rendered table.</param>
		/// <param name="tableWriter">The table writer which will render the table.</param>
		/// <param name="columnWidths">An array of integers specifying the width, in characters, of each column.</param>
		public static void Render<T>(IEnumerable<T> data, ITableWriter tableWriter, int numberOfRowsToInspectWhenDeterminingColumnWidth = 10)
		{
			RenderInternal(data, tableWriter, numberOfRowsToInspectWhenDeterminingColumnWidth, new int[0]);
		}

		public static void RenderWithSpecifiedColumnWidths<T>(IEnumerable<T> data, ITableWriter tableWriter, params int[] columnWidths)
		{
			RenderInternal(data, tableWriter, 0, columnWidths);
		}

		private static void RenderInternal<T>(IEnumerable<T> data, ITableWriter tableWriter, int numberOfRowsToInspectWhenDeterminingColumnWidth, params int[] columnWidths)
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

				var previewObjects = data.Take(numberOfRowsToInspectWhenDeterminingColumnWidth).ToArray();

				foreach (var property in properties)
				{
					int columnWidth = -1;

					// If the caller has nominated a column width, then use that
					if (columnWidths.Length > propertyNumber)
					{
						columnWidth = columnWidths[propertyNumber];
					}
					else
					{
						// Otherwise, choose a column width based on the longest value found in the first 10 rows
						int maxWidth = previewObjects.Max(x => property.GetValue(x, null).ToString().Length);

						columnWidth = maxWidth;
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
						object value = property.GetValue(item, null);
						string toRender = value == null ? "null" : value.ToString();

						tableWriter.WriteCell(allColumns[colIdx++], toRender);
					}

					tableWriter.EndRow();
				}
			}

			tableWriter.EndTable();
		}

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
