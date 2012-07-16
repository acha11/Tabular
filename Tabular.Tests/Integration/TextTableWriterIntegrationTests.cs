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
	public class TextTableWriterIntegrationTests
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

			TextTableWriter ttw = new TextTableWriter(sw);

			ttw.SetBorderCharacterSet(TextTableWriter.BasicAsciiBorderCharacterSet);

			TableRenderer.Render(toRender, ttw);

			string x = sw.ToString();

			Assert.AreEqual(
@"+-------+
| Value |
+-------+
|    35 |
| 1,542 |
+-------+
",
				x);
		}

		[TestMethod]
		public void LongsAreRightJustifiedByDefault()
		{
			var toRender = new[]
			{
				new { Value = 35L },
				new { Value = 42236455234L }
			};

			var sw = new StringWriter();

			TextTableWriter ttw = new TextTableWriter(sw);

			ttw.SetBorderCharacterSet(TextTableWriter.BasicAsciiBorderCharacterSet);

			TableRenderer.Render(toRender, ttw);

			string x = sw.ToString();

			Assert.AreEqual(
@"+----------------+
|     Value      |
+----------------+
|             35 |
| 42,236,455,234 |
+----------------+
",
				x);
		}

	}
}
