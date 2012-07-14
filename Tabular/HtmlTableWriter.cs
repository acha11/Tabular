using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;

namespace Tabular
{
	public class HtmlTableWriter : ITableWriter
	{
		StreamWriter _sw;
		TableStructure _structure;

		public HtmlTableWriter(StreamWriter sw)
		{
			_sw = sw;
		}

		public void StartTable(TableStructure tableStructure)
		{
			_structure = tableStructure;

			string id = "table" + Guid.NewGuid().ToString().Replace('-', '_');

			_sw.WriteLine(
				"<style>#" + id + ", #" + id + " th, #" + id + " td { border: solid rgb(200, 200, 200) 1px; border-collapse: collapse; border-spacing: 0px; padding: 3px; }" +
					" #" + id + " th { background-color: rgb(230, 230, 255); } #" + id + " { font-family: tahoma; }</style>");
			_sw.WriteLine("<table id=\"" + id + "\">");

			if (_structure.ColumnGroups.Any(cg => cg.Title.Length > 0))
			{
				_sw.Write("<tr>");
				foreach (var cg in _structure.ColumnGroups)
				{
					_sw.Write("<th colspan=\"" + cg.Columns.Count() + "\">" + HttpUtility.HtmlEncode(cg.Title) + "</th>");
				}
				_sw.Write("</tr>");
			}

			var allColumns = _structure.GetAllColumns();

			if (allColumns.Any(c => c.Title.Length > 0))
			{
				_sw.Write("<tr>");

				foreach (var c in allColumns)
				{
					_sw.Write("<th>" + HttpUtility.HtmlEncode(c.Title) + "</th>");
				}

				_sw.Write("</tr>");
			}
		}

		public void EndTable()
		{
			_sw.WriteLine("</table>");
		}

		public void StartRow()
		{
			_sw.WriteLine("<tr>");
		}

		public void EndRow()
		{
			_sw.WriteLine("</tr>");
		}

		private string BuildCellCss(TableColumn column)
		{
			if (column.HorizontalAlignment == HorizontalAlignment.Right)
			{
				return "text-align: right;";
			}

			return "";
		}

		public void WriteCell(TableColumn column, string value)
		{
			_sw.WriteLine("<td style=\"" + BuildCellCss(column) + "\">" + HttpUtility.HtmlEncode(value) + "</td>");
		}
	}
}
