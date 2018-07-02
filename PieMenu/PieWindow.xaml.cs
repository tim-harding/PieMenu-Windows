namespace PieMenu
{
	using System;
	using System.Windows;
	using System.Windows.Forms;

	public partial class PieWindow : Window, IPieViewProvider, IBindingsPresentationProvider
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

			bindings.provider = this;
			bindings.Bind();
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
			SettingsWindow settings = new SettingsWindow();
			settings.Show();
		}

		private Point GetMousePosition()
		{
			System.Drawing.Point pt = System.Windows.Forms.Cursor.Position;
			return new Point(pt.X, pt.Y);
		}



		#region IPieViewProvider implementation

		public string[] TitleText()
		{
			return bindings.CurrentTitles();
		}

		public int Selection()
		{
			return currentSelection;
		}

		#endregion

		#region IBindingsPresentationProvider implementation

		public void Present()
		{
			if (isActive) return;

			isActive = true;
			pieCenter = GetMousePosition();
			double offset = this.Width / 2.0;
			this.Left = pieCenter.X - offset;
			this.Top = pieCenter.Y - offset;
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

			float innerFraction = Properties.Settings.Default.InnerFraction;
			Point relative = new Point(mouse.X - pieCenter.X, mouse.Y - pieCenter.Y);
			double length = Math.Sqrt(relative.X * relative.X + relative.Y * relative.Y);
			if (length / this.Width * 2.0 > innerFraction)
			{
				double select = Math.Atan2(relative.Y, relative.X) / Math.PI * 4.0;
				select += select < 0 ? 8.5 : 0.5;
				currentSelection = (int)(select) % 8;
			}
			view.UpdateHighlighting();
		}

		#endregion
	}
}
