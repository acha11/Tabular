﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tabular
{
	public enum ValueSortMethod
	{
		Default,
		Int,
		DateTime
	}

	public class TableColumn
	{
		public string Name;
		public string Title;
		public int Width;
		public string FormatSpecifier;
		public HorizontalAlignment HorizontalAlignment;
		public ValueSortMethod SortMethod;
	}
}
