using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Tabular
{
	public class GridTableWriter : ITableWriter
	{
		GridForm.Grid _grid;

		static object _gridFormCreationLock = new object();
		static GridForm _gridForm;

		private static void BuildGridForm()
		{
			// Create an event used by the grid form that's being created to signal back to this thread that creation is complete
			ManualResetEvent gridFormIsReadyEvent = new ManualResetEvent(false);

			Thread t = new Thread(
				new ThreadStart(() =>
				{
					_gridForm = new GridForm(gridFormIsReadyEvent);				

					// TODO: When running from within a WinForms app (WPF?), this call's likely to cause issues. Detect and take evasive action.
					Application.Run(_gridForm);
				}
			));

			t.SetApartmentState(ApartmentState.STA);

			t.Start();

			// Wait for the grid form to be created.
			gridFormIsReadyEvent.WaitOne();
			gridFormIsReadyEvent.Close();
		}

		private GridForm GetGridForm()
		{
			if (_gridForm == null)
			{
				lock (_gridFormCreationLock)
				{
					if (_gridForm == null)
					{
						BuildGridForm();
					}
				}
			}

			return _gridForm;
		}

		public void StartTable(TableStructure tableStructure)
		{
			_grid = GetGridForm().CreateNewGrid(tableStructure.GetAllColumns().ToArray());
		}

		List<object> _cellValuesForCurrentRow;

		public void EndTable()
		{
		}

		public void StartRow()
		{
			_cellValuesForCurrentRow = new List<object>();
		}

		public void EndRow()
		{
			_grid.AddRow(_cellValuesForCurrentRow);
		}

		public void WriteCell(TableColumn column, object value)
		{
			_cellValuesForCurrentRow.Add(value);
		}

		public bool UsesColumnWidth
		{
			get { return false; }
		}
	}
}
