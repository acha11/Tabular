using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;

namespace Tabular
{
	public class HtmlTableWriter : ITableWriter, IDisposable
	{
		TextWriter _tw;
		TableStructure _structure;
		bool _ownsStreamWriter;

		public HtmlTableWriter(TextWriter tw)
		{
			_tw = tw;
			_ownsStreamWriter = false;
			GenerateRandomHtmlTableElementId();
		}

		public HtmlTableWriter(string outputFile)
		{
			_ownsStreamWriter = true;
			_tw = new StreamWriter(outputFile);
			GenerateRandomHtmlTableElementId();
		}

		private void GenerateRandomHtmlTableElementId()
		{
			HtmlTableElementId = "table" + Guid.NewGuid().ToString().Replace('-', '_');
		}

		/// <summary>
		/// This is randomly generated when the HtmlTableWriter is constructed. However, it can be over-ridden before calling StartTable.
		/// </summary>
		public string HtmlTableElementId { get; set; }

		public void StartTable(TableStructure tableStructure)
		{
			_structure = tableStructure;

			string id = HtmlTableElementId;

			_tw.WriteLine(
				"<style>#" + id + ", #" + id + " th, #" + id + " td { border: solid rgb(200, 200, 200) 1px; border-collapse: collapse; border-spacing: 0px; padding: 3px; }" +
					" #" + id + " th { background-color: rgb(230, 230, 255); } #" + id + " { font-family: tahoma; }</style>");
			_tw.WriteLine("<table id=\"" + id + "\">");

			if (_structure.ColumnGroups.Any(cg => cg.Title.Length > 0))
			{
				_tw.Write("<tr>");
				foreach (var cg in _structure.ColumnGroups)
				{
					_tw.Write("<th colspan=\"" + cg.Columns.Count() + "\">" + HttpUtility.HtmlEncode(cg.Title) + "</th>");
				}
				_tw.Write("</tr>");
			}

			var allColumns = _structure.GetAllColumns();

			if (allColumns.Any(c => c.Title.Length > 0))
			{
				_tw.Write("<tr>");

				foreach (var c in allColumns)
				{
					_tw.Write("<th>" + HttpUtility.HtmlEncode(c.Title) + "</th>");
				}

				_tw.Write("</tr>");
			}
		}

		public void EndTable()
		{
			_tw.WriteLine("</table>");

			if (_ownsStreamWriter)
			{
				_tw.Close();

				_tw.Dispose();
			}
		}

		public void StartRow()
		{
			_tw.WriteLine("<tr>");
		}

		public void EndRow()
		{
			_tw.WriteLine("</tr>");
		}

		private string BuildCellCss(TableColumn column)
		{
			if (column.HorizontalAlignment == HorizontalAlignment.Right)
			{
				return "text-align: right;";
			}

			return "";
		}

		public void WriteCell(TableColumn column, object value)
		{
			string renderedString = TableWriterHelper.RenderValue(column, value);

			_tw.WriteLine("<td style=\"" + BuildCellCss(column) + "\">" + HttpUtility.HtmlEncode(renderedString) + "</td>");
		}
		
		public bool UsesColumnWidth
		{
			get { return false; }
		}

		public void Dispose()
		{
			if (_ownsStreamWriter)
			{
				if (_tw != null)
				{
					_tw.Dispose();
				}
			}
		}
	}
}
