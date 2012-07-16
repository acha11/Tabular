using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Diagnostics;

namespace Tabular.Tests.Integration
{
	[TestClass]
	public class HtmlTableWriterIntegrationTests
	{
		[TestMethod]
		public void IntegersAreRightJustifiedByDefault()
		{
			var toRender = new[]
			{
				new { Value = 35 },
				new { Value = 1542 }
			};

			var sw = new StringWriter();

			HtmlTableWriter htw = new HtmlTableWriter(sw);

			htw.HtmlTableElementId = "x";

			TableRenderer.Render(toRender, htw);

			string x = sw.ToString();

			Assert.IsTrue(x.Contains("<td style=\"text-align: right;\">1,542</td>"));
		}
	}
}
