using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Tabular
{
	public class CsvTableWriter : ITableWriter, IDisposable
	{
		StreamWriter _sw;
		TableStructure _structure;
		TableColumn _firstColumn;

		bool _ownsStreamWriter;

		public CsvTableWriter(StreamWriter sw)
		{
			_sw = sw;
		}

		public CsvTableWriter(string outputFile)
		{
			_ownsStreamWriter = true;
			_sw = new StreamWriter(outputFile);
		}

		public void StartTable(TableStructure tableStructure)
		{
			_structure = tableStructure;
			_firstColumn = tableStructure.ColumnGroups.First().Columns.First();

			if (_structure.GetAllColumns().Any(c => c.Title.Length > 0))
			{
				_sw.WriteLine(string.Join(",", _structure.ColumnGroups.SelectMany(cg => cg.Columns).Select(c => c.Title).ToArray()));
			}
		}

		public void EndTable()
		{
			if (_ownsStreamWriter)
			{
				_sw.Close();

				_sw.Dispose();
			}
		}

		public void StartRow()
		{
		}

		public void EndRow()
		{
			_sw.WriteLine();
		}

		public void WriteCell(TableColumn column, string value)
		{
			if (column != _firstColumn)
			{
				_sw.Write(",");
			}

			_sw.Write(value);
		}

		private string GetCsvEscapedValue(string value)
		{
			// If the value contains a comma
			if (value.Contains(','))
			{
				// We wrap the whole thing in double quotes, being sure to duplicate any double quotes it already contained.
				return "\"" + value.Replace("\"", "\"\"") + "\"";
			}
			else
			{
				return value;
			}
		}

		public bool UsesColumnWidth
		{
			get { return false; }
		}

		public void Dispose()
		{
			if (_ownsStreamWriter)
			{
				if (_sw != null)
				{
					_sw.Dispose();
				}
			}
		}
	}
}
