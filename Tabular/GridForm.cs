using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Tabular
{
	public partial class GridForm : Form
	{
		public class Grid
		{
			object _incomingRowLock = new object();
			public DataGridView DataGridView { get; set; }
			public Queue<List<object>> IncomingRows { get; set; }

			public Grid()
			{
				IncomingRows = new Queue<List<object>>();
			}

			public void ProcessIncomingRows()
			{
				lock (_incomingRowLock)
				{
					while (IncomingRows.Any())
					{
						DataGridView.Rows.Add(IncomingRows.Dequeue().ToArray());
					}
				}
			}

			public void AddRow(List<object> rowValues)
			{
				lock (_incomingRowLock)
				{
					IncomingRows.Enqueue(rowValues);
				}
			}
		}

		System.Windows.Forms.Timer _timer;
		ManualResetEvent _gridFormIsReadyEvent;
		List<Grid> _grids;

		public GridForm(ManualResetEvent gridFormIsReadyEvent)
		{
			InitializeComponent();

			_gridFormIsReadyEvent = gridFormIsReadyEvent;

			_grids = new List<Grid>();

			_timer = new System.Windows.Forms.Timer();
			_timer.Interval = 250;
			
			_timer.Tick += new EventHandler(_timer_Tick);
			_timer.Start();
		}

		void _timer_Tick(object sender, EventArgs e)
		{
			foreach (var grid in _grids)
			{
				grid.ProcessIncomingRows();
			}
		}

		public Grid CreateNewGrid(TableColumn[] tableColumns)
		{
			return (Grid)Invoke(new Func<Grid>(() =>
				{
					TabPage newPage = new TabPage("Grid " + (tabControl.TabPages.Count + 1));

					tabControl.TabPages.Add(newPage);

					DataGridView newDataGridView = new DataGridView();

					newDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
					newDataGridView.AllowUserToAddRows = false;
					newDataGridView.AllowUserToDeleteRows = false;
					newDataGridView.ReadOnly = true;
					newDataGridView.RowHeadersVisible = false;

					newPage.Controls.Add(newDataGridView);

					newDataGridView.Dock = DockStyle.Fill;

					foreach (var tc in tableColumns)
					{
						newDataGridView.Columns.Add(tc.Name, tc.Title);
					}

					Grid grid = new Grid()
					{
						DataGridView = newDataGridView
					};

					_grids.Add(grid);

					return grid;
				})
			);
		}

		private void GridForm_Load(object sender, EventArgs e)
		{
			_gridFormIsReadyEvent.Set();

			_gridFormIsReadyEvent = null;
		}
	}
}
