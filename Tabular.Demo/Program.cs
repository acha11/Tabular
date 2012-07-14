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
			RunDemo("Demo 1: Simple render to console",                     () => Demo1_DemonstrateSimpleRenderToConsole());
			RunDemo("Demo 2: Simultaneous render to console and html file", () => Demo2_DemonstrateSimultaneousRenderToConsoleAndHtmlFile());
			RunDemo("Demo 3: Render to text file",                          () => Demo3_DemonstrateRenderToTextFile());
			RunDemo("Demo 4: Render to csv file",                           () => Demo4_DemonstrateRenderToCsvFile());
		}

		private static void Demo1_DemonstrateSimpleRenderToConsole()
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
		
		private static void Demo2_DemonstrateSimultaneousRenderToConsoleAndHtmlFile()
		{
			using (StreamWriter demo2Out = new StreamWriter("demo2Out.html"))
			{
				// First, let's build a collection of objects to render.
				var filesInCurrentDirectory =
					new DirectoryInfo(Directory.GetCurrentDirectory()).GetFiles();

				// The anonymous type we're building has properties Name, Extension and CreationTime.
				// The resulting table will have a column for each of those properties.
				var objectsToRender =
					filesInCurrentDirectory
					.Select(x => new { x.Name, x.Extension, x.CreationTime });

				// Next, set up some table renderers. First, a writer that will write the
				// table to the console.
				var consoleTableWriter = new ConsoleTableWriter();

				// Now, a writer that will write HTML to an output file.
				var htmlTableWriter = new HtmlTableWriter(demo2Out);

				// Now, a single renderer which takes care of writing our single table to
				// both of the above destinations.
				var multipleTableRenderer = new MultipleTargetTableWriter(consoleTableWriter, htmlTableWriter);

				// Finally, we actually render the table
				TableRenderer.Render(objectsToRender, multipleTableRenderer);
			}

			// And launch a browser to display the generated html
			Process.Start("demo2Out.html");
		}

		private static void Demo3_DemonstrateRenderToTextFile()
		{
			// First, let's build a collection of objects to render.
			var filesInCurrentDirectory =
				new DirectoryInfo(Directory.GetCurrentDirectory()).GetFiles();

			// The anonymous type we're building has properties Name, Extension and CreationTime.
			// The resulting table will have a column for each of those properties.
			var objectsToRender =
				filesInCurrentDirectory
				.Select(x => new { x.Name, x.Extension, x.CreationTime });

			using (StreamWriter demo3TextOut = new StreamWriter("demo3Out.txt"))
			{
				// Finally, we actually render the table
				TableRenderer.Render(objectsToRender, new TextTableWriter(demo3TextOut));
			}

			// And display the generated file
			Process.Start("demo3Out.txt");
		}

		private static void Demo4_DemonstrateRenderToCsvFile()
		{
			// First, let's build a collection of objects to render.
			var filesInCurrentDirectory =
				new DirectoryInfo(Directory.GetCurrentDirectory()).GetFiles();

			// The anonymous type we're building has properties Name, Extension and CreationTime.
			// The resulting table will have a column for each of those properties.
			var objectsToRender =
				filesInCurrentDirectory
				.Select(x => new { x.Name, x.Extension, x.CreationTime });

			using (StreamWriter demo4Out = new StreamWriter("demo4Out.csv"))
			{
				// Finally, we actually render the table
				TableRenderer.Render(objectsToRender, new CsvTableWriter(demo4Out));
			}

			// And display the generated file
			Process.Start("demo4Out.csv");
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
