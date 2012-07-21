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

			_tw.WriteLine(@"
<!-- DataTables CSS --> <link rel=""stylesheet"" type=""text/css"" href=""http://ajax.aspnetcdn.com/ajax/jquery.dataTables/1.8.2/css/jquery.dataTables.css"">
<!-- jQuery --> <script type=""text/javascript"" charset=""utf8"" src=""http://ajax.aspnetcdn.com/ajax/jQuery/jquery-1.7.1.min.js""></script>
 <!-- DataTables --> <script type=""text/javascript"" charset=""utf8"" src=""http://ajax.aspnetcdn.com/ajax/jquery.dataTables/1.8.2/jquery.dataTables.min.js""></script>

	<script type=""text/javascript"">
		jQuery.fn.dataTableExt.oSort['numeric-comma-thousands-separators-asc'] = function (a, b) {
			var x = (a == ""-"") ? 0 : a.replace(/,/, """");
			var y = (b == ""-"") ? 0 : b.replace(/,/, """");
			x = parseFloat(x);
			y = parseFloat(y);
			return ((x < y) ? -1 : ((x > y) ? 1 : 0));
		};

		jQuery.fn.dataTableExt.oSort['numeric-comma-thousands-separators-desc'] = function (a, b) {
			var x = (a == ""-"") ? 0 : a.replace(/,/, """");
			var y = (b == ""-"") ? 0 : b.replace(/,/, """");
			x = parseFloat(x);
			y = parseFloat(y);
			return ((x < y) ? 1 : ((x > y) ? -1 : 0));
		};

		jQuery.fn.dataTableExt.oSort['datetime-dd/mm/yyyy hh:mm:ss AM-asc'] = function (a, b) {
			var x = (a == ""-"") ? 0 : pd(a);
			var y = (b == ""-"") ? 0 : pd(b);

			return ((x < y) ? -1 : ((x > y) ? 1 : 0));
		};

		jQuery.fn.dataTableExt.oSort['datetime-dd/mm/yyyy hh:mm:ss AM-desc'] = function (a, b) {
			var x = (a == ""-"") ? 0 : pd(a);
			var y = (b == ""-"") ? 0 : pd(b);

			return ((x < y) ? 1 : ((x > y) ? -1 : 0));
		};

		function pd(s) {
			var parts = s.match(/(\w+)/g);

			var months = parts[1] - 1;
			var hours = parseInt(parts[3]);

			if (parts[6] == ""AM"") {
				if (hours == 12) hours = 0;				
			}
			else {
				if (hours < 12) hours += 12;
			}

			return new Date(parts[2], months, parts[0], hours, parts[4], parts[5]);
		}

    $(function() {
      var dt = $(""#" + id + @""")
        .dataTable(
          { 
            bPaginate: false,
            bInfo: false,
			bAutoWidth: false,
			""aoColumns"": [");


			bool first = true;

			foreach (var tc in _structure.GetAllColumns())
			{
				if (!first) { _tw.Write(","); }

				first = false;

				switch (tc.SortMethod)
				{
					case ValueSortMethod.Default:
						_tw.WriteLine("null");
						break;
					case ValueSortMethod.Int:
						_tw.WriteLine("{ \"sType\": \"numeric-comma-thousands-separators\" }");
						break;
					case ValueSortMethod.DateTime:
						_tw.WriteLine("{ \"sType\": \"datetime-dd/mm/yyyy hh:mm:ss AM\" }");
						break;
				}
			}


			_tw.WriteLine(@"      
				]
		});

		new FixedHeader(dt);
    });
	</script>

	<style>
		#" + id + @" th
		  { background-color: rgb(230, 230, 255); cursor: pointer; border: solid rgb(200, 200, 200) 1px;  }
		  
		.FixedHeader_Cloned th
		  { background-color: rgb(230, 230, 255); cursor: pointer; border: solid rgb(200, 200, 200) 1px; padding-right: 2px; }

		.datatables_filter { float: none; text-align: left; }

		#" + id + @",
		.FixedHeader_Cloned
		.dataTable
		  { font-family: tahoma; border-collapse: collapse; }

		#" + id + @" td
		{ border: solid rgb(230, 230, 230) 1px; padding: 3px }

		  

	</style>
");

			_tw.WriteLine("<table id=\"" + id + "\">");

			_tw.Write("<thead>");
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
			_tw.WriteLine("</thead>");

			_tw.WriteLine("<tbody>");
		}

		public void EndTable()
		{
			_tw.WriteLine("</tbody>");
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
