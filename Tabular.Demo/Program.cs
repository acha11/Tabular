using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Tabular.Demo
{
	class Program
	{
		static void Main(string[] args)
		{
			using (StreamWriter demoOut = new StreamWriter("demoOut.html"))
			{
				// First, build an IEnumerable of objects we want rendered into a table,
				// one row per item in the sequence, one column per property of the
				// objects.
				var filesInCurrentDirectory =
					new DirectoryInfo(Directory.GetCurrentDirectory()).GetFiles();

				var objectsToRenderIntoTable = 
					filesInCurrentDirectory
					.Select(x => new { x.Name, x.Extension, x.CreationTime });

				// Next, set up some table renderers. First, a renderer that will write the
				// table to the console.
				var textTableRenderer = new TextTableRenderer();

				// Now, a renderer that will write HTML to an output file.
				var htmlTableRenderer = new HtmlTableWriter(demoOut);

				// Now, a single renderer which takes care of writing our single table to
				// both of the above destinations.
				var multipleTableRenderer = new MultipleTargetTableWriter(textTableRenderer, htmlTableRenderer);
				
				// Finally, we actually render the table
				TableRenderer.Render(objectsToRenderIntoTable, multipleTableRenderer);
			}

			// And launch a browser to display the generated html
			Process.Start("demoOut.html");
		}
	}
}
