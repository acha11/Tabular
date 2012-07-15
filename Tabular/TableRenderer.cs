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

		public class ValueGroup<T>
		{
			private string _groupName;
			private T _value;

			public ValueGroup(string groupName, T value)
			{
				_groupName = groupName;
				_value = value;
			}

			public string GroupName { get { return _groupName; } }
			public T Value { get { return _value; } }
		}

		public static void RenderToConsole<T>(IEnumerable<T> data, int numberOfRowsToInspectWhenDeterminingColumnWidth = 10)
		{
			Render(data, new ConsoleTableWriter(), numberOfRowsToInspectWhenDeterminingColumnWidth);
		}

		public static void RenderToConsole<T>(IEnumerable<T> data, TableStructure tableStructure)
		{
			RenderCollectionOfComplexes(data, new ConsoleTableWriter(), tableStructure);
		}
	
		/// <summary>
		/// Renders the supplied collection as a table to the supplied table writer.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data">The collection of objects to be rendered; each item in the collection appears as a row in the rendered table.</param>
		/// <param name="tableWriter">The table writer which will render the table.</param>
		public static void Render<T>(IEnumerable<T> data, ITableWriter tableWriter, int numberOfRowsToInspectWhenDeterminingColumnWidth = 10)
		{
			var typeOfT = GetEnumeratedType<T>(data);

			if (typeOfT.IsPrimitive)
			{
				RenderCollectionOfPrimitives<T>(data, tableWriter);
			}
			else
			{
				RenderCollectionOfComplexes<T>(data, tableWriter, numberOfRowsToInspectWhenDeterminingColumnWidth, typeOfT);
			}
		}

		/// <summary>
		/// Renders the supplied collection as a table to the supplied table writer.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data">The collection of objects to be rendered; each item in the collection appears as a row in the rendered table.</param>
		/// <param name="tableWriter">The table writer which will render the table.</param>
		public static void Render<T>(IEnumerable<T> data, ITableWriter tableWriter, TableStructure tableStructure)
		{
			RenderCollectionOfComplexes(data, tableWriter, tableStructure);
		}

		private static Type GetEnumeratedType<T>(IEnumerable<T> data)
		{
			Type enumerableType = data.GetType();

			Type ienumerableInterfaceType = enumerableType.GetInterfaces().FirstOrDefault(i => i.Name == "IEnumerable`1");

			var typeOfT = ienumerableInterfaceType.GetGenericArguments()[0];
			return typeOfT;
		}

		private static void RenderCollectionOfComplexes<T>(IEnumerable<T> data, ITableWriter tableWriter, int numberOfRowsToInspectWhenDeterminingColumnWidth, Type typeOfT)
		{
			// First, create structure
			TableStructure ts = new TableStructure();

			ts.RowsToExamineWhenAutoSizingColumns = numberOfRowsToInspectWhenDeterminingColumnWidth;

			TableColumnGroup tcg = new TableColumnGroup("");
			ts.ColumnGroups.Add(tcg);

			var properties = typeOfT.GetProperties();

			int propertyNumber = 0;

			foreach (var property in properties)
			{
				var col = new TableColumn() { Name = property.Name, Title = property.Name, Width = -1 };

				if (property.PropertyType == typeof(CurrencyValue))
				{
					col.HorizontalAlignment = HorizontalAlignment.Right;
				}

				tcg.Columns.Add(col);

				propertyNumber++;
			}

			RenderCollectionOfComplexes(data, tableWriter, ts);
		}

		private static void AutosizeColumns<T>(IEnumerable<T> data, TableStructure ts)
		{
			var typeOfT = GetEnumeratedType<T>(data);

			var properties = typeOfT.GetProperties();

			var previewObjects = data.Take(ts.RowsToExamineWhenAutoSizingColumns).ToArray();

			foreach (var tc in ts.ColumnGroups.SelectMany(cg => cg.Columns).Where(x => x.Width < 1))
			{
				var property = properties.Single(x => x.Name == tc.Name);

				// Choose a column width based on the longest value found in the first n rows
				int maxWidth = previewObjects.Max(x => property.GetValue(x, null).ToString().Length);

				maxWidth = Math.Max(maxWidth, tc.Title.Length);

				tc.Width = Math.Max(1, maxWidth);				
			}
		}

		private static void RenderCollectionOfComplexes<T>(IEnumerable<T> data, ITableWriter tableWriter, TableStructure ts)
		{
			var typeOfT = GetEnumeratedType<T>(data);

			var properties = typeOfT.GetProperties();

			if (ts.ColumnGroups.SelectMany(x => x.Columns).Any(x => x.Width < 1))
			{
				// Autosize columns
				if (tableWriter.UsesColumnWidth)
				{
					AutosizeColumns(data, ts);
				}
			}

			tableWriter.StartTable(ts);

			var allColumns = ts.GetAllColumns().ToArray();

			foreach (var item in data)
			{
				tableWriter.StartRow();

				foreach (var tc in ts.ColumnGroups.SelectMany(cg => cg.Columns))
				{
					var property = properties.Single(x => x.Name == tc.Name);

					object value = property.GetValue(item, null);
					string toRender = value == null ? "null" : value.ToString();

					tableWriter.WriteCell(tc, toRender);
				}

				tableWriter.EndRow();
			}

			tableWriter.EndTable();
		}

		private static void RenderCollectionOfPrimitives<T>(IEnumerable<T> data, ITableWriter tableWriter)
		{
			// First, create structure
			TableStructure ts = new TableStructure();
			TableColumnGroup tcg = new TableColumnGroup("");
			ts.ColumnGroups.Add(tcg);

			// Go through the primitive render path
			int columnWidth = 20;

			tcg.Columns.Add(new TableColumn() { Name = "", Title = "", Width = columnWidth });

			tableWriter.StartTable(ts);

			foreach (var item in data)
			{
				tableWriter.StartRow();

				tableWriter.WriteCell(tcg.Columns[0], item.ToString());

				tableWriter.EndRow();
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
