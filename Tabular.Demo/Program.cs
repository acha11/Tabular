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
			RunDemo("Demo 1: Simple render to console",                     () => DemonstrateSimpleRenderToConsole());
			RunDemo("Demo 2: Simultaneous render to console and html file", () => DemonstrateSimultaneousRenderToConsoleAndHtmlFile());
		}

		private static void DemonstrateSimpleRenderToConsole()
		{
			// First, let's build a collection of objects to render.
			var filesInCurrentDirectory =
				new DirectoryInfo(Directory.GetCurrentDirectory()).GetFiles();

			// The anonymous type we're building has properties Name, Extension and CreationTime.
			// The resulting table will have a column for each of those properties.
			var objectsToRender =
				filesInCurrentDirectory
				.Select(x => new { x.Name, x.Extension, x.CreationTime });

			// Now, we render the table to the console.
			TableRenderer.RenderToConsole(objectsToRender);
		}
		
		private static void DemonstrateSimultaneousRenderToConsoleAndHtmlFile()
		{
			using (StreamWriter demoOut = new StreamWriter("demo2Out.html"))
			{
				// First, let's build a collection of objects to render.
				var filesInCurrentDirectory =
					new DirectoryInfo(Directory.GetCurrentDirectory()).GetFiles();

				// The anonymous type we're building has properties Name, Extension and CreationTime.
				// The resulting table will have a column for each of those properties.
				var objectsToRender =
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
				TableRenderer.Render(objectsToRender, multipleTableRenderer);
			}

			// And launch a browser to display the generated html
			Process.Start("demo2Out.html");
		}

		private static void RunDemo(string title, Action demo)
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine(title);
			Console.ForegroundColor = ConsoleColor.Gray;

			demo();

			Console.WriteLine();
		}
	}
}
