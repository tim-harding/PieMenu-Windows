namespace PieMenu
{
	using System;
	using System.Windows;
	using System.Windows.Forms;

	public partial class PieWindow : Window, IPieViewProvider
	{
		private NotifyIcon tray = null;
		private PieView view = new PieView();
		private BindingsManager bindings = new BindingsManager();
		private Point pieCenter = new Point();
		private int currentSelection = -1;
		private bool isActive = false;
		private bool pieHasDisplayed = false;

		public PieWindow()
		{
			this.InitializeComponent();
			this.ShowActivated = false;
			this.ShowInTaskbar = false;
			this.Hide();
			double size = Properties.Settings.Default.Size;
			this.Width = size;
			this.Height = size;

			bindings.beginSelection += Present;
			bindings.endSelection += Recede;
			bindings.updateSelection += Update;
			view.provider = this;
			view.Draw();
			this.AddChild(view.content);
		}

		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);
			tray = new NotifyIcon();
			tray.Click += TrayClicked;
			tray.Icon = System.Drawing.SystemIcons.Exclamation;
			tray.Visible = true;
		}

		private void TrayClicked(object sender, EventArgs e)
		{
			var settings = new SettingsWindow();
			settings.Show();
		}

		public void Present()
		{
			if (isActive) return;

			isActive = true;
			pieCenter = GetMousePosition();
			double offset = this.Width / 2.0;
			this.Left = pieCenter.X - offset;
			this.Top = pieCenter.Y - offset;
			view.Draw();
		}

		public void Recede()
		{
			if (!isActive) return;

			isActive = false;
			this.Hide();
			bindings.PerformSliceAction(currentSelection);
			pieHasDisplayed = false;
		}

		public void Update()
		{
			if (!isActive) return;

			Point mouse = GetMousePosition();
			currentSelection = -1;
			if (!pieHasDisplayed)
			{
				// If the mouse doesn't get moved, don't show the pie
				if (mouse == pieCenter) return;
				this.Show();
				this.Topmost = true;
				pieHasDisplayed = true;
			}

			double innerFraction = Properties.Settings.Default.InnerFraction;
			var relative = new Point(mouse.X - pieCenter.X, mouse.Y - pieCenter.Y);
			double length = Math.Sqrt(relative.X * relative.X + relative.Y * relative.Y);
			if (length / this.Width * 2.0 > innerFraction)
			{
				int sliceCount = bindings.SliceCount();
				double select = Math.Atan2(relative.Y, relative.X) / Math.PI * sliceCount / 2.0;
				select += 2.25 * sliceCount + 0.5;
				currentSelection = (int)(select) % sliceCount;
			}
			view.UpdateHighlighting();
		}

		private Point GetMousePosition()
		{
			System.Drawing.Point pt = System.Windows.Forms.Cursor.Position;
			return new Point(pt.X, pt.Y);
		}

		#region IPieViewProvider implementation

		public int SliceCount()
		{
			return bindings.SliceCount();
		}

		public string TitleText(int slice)
		{
			return bindings.TitleForSlice(slice);
		}

		public int Selection()
		{
			return currentSelection;
		}

		#endregion
	}
}
