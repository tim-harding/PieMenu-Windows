using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Forms;
using System.Windows.Shapes;
using Gma.System.MouseKeyHook;
using WindowsInput;
using WindowsInput.Native;

namespace PieMenu
{
	public partial class MainWindow : Window
	{
		private IKeyboardMouseEvents GlobalHook;
		private const double INNER_FRACTION = 0.2;
		private const double TEXT_FRACTION = 0.7;
		private const double GAP = 0.02; // Radians on outer circumfrence
		private bool isActive = false;
		private bool pieHasDisplayed = false;
		private Point pieCenter;
		private int currentSelection = 0;
		private InputSimulator simulator = new InputSimulator();

		public MainWindow()
		{
			this.InitializeComponent();
			this.ShowActivated = false;
			this.Hide();

			SolidColorBrush windowBackgroundBrush = new SolidColorBrush();
			windowBackgroundBrush.Color = Color.FromArgb(0, 0, 0, 0);
			windowBackgroundBrush.Opacity = 0;
			this.Background = windowBackgroundBrush;

			GlobalHook = Hook.GlobalEvents();
			GlobalHook.KeyUp += GlobalHook_KeyUp;

			Combination combo = Combination.TriggeredBy(Keys.B).With(Keys.Control);
			Dictionary<Combination, Action> assignment = new Dictionary<Combination, Action>
			{
				{combo, Present}
			};
			GlobalHook.OnCombination(assignment);

			foreach (Path slice in Slices())
			{
				canvas.Children.Add(slice);
			}

			for (int i = 0; i < 8; i++)
			{
				double bisector = i * Math.PI / 4.0;
				Point point = PointOnCircle(bisector, this.Width / 2.0 * TEXT_FRACTION);
				TextBlock text = new TextBlock();
				text.Text = TitleProvider(i);
				// Styled as BaseTextBlockStyle
				text.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
				text.TextAlignment = TextAlignment.Center;
				text.Width = 100;
				text.Height = 22;
				text.FontFamily = new FontFamily("Segoe UI");
				text.FontWeight = FontWeights.SemiBold;
				text.FontSize = 15.0;
				canvas.Children.Add(text);
				Canvas.SetLeft(text, point.X - text.Width / 2.0);
				Canvas.SetTop(text, point.Y - text.Height / 2.0);
			}
		}

		private void Present()
		{
			if (!isActive)
			{
				pieCenter = GetMousePosition();
				double offset = this.Width / 2.0;
				this.Left = pieCenter.X - offset;
				this.Top = pieCenter.Y - offset;
				GlobalHook.MouseMove += UpdatePie;
				isActive = true;
			}
		}

		private void Recede()
		{
			this.Hide();
			GlobalHook.MouseMove -= UpdatePie;
			PerformSliceAction();
			pieHasDisplayed = false;
		}

		private void GlobalHook_KeyUp(object sender, KeyEventArgs e)
		{
			if (isActive)
			{
				isActive = false;
				Recede();
			}
		}

		private void UpdatePie(object sender, MouseEventArgs e)
		{
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

			Point relative = new Point(mouse.X - pieCenter.X, mouse.Y - pieCenter.Y);
			double length = Math.Sqrt(relative.X * relative.X + relative.Y * relative.Y);
			if (length / this.Width * 2.0 > INNER_FRACTION)
			{
				double select = Math.Atan2(relative.Y, relative.X) / Math.PI * 4.0;
				select += select < 0 ? 8.5 : 0.5;
				currentSelection = (int)(select) % 8;
			}
			for (int i = 0; i < 8; i++)
			{
				Path path = canvas.Children[i] as Path;
				path.Opacity = i == currentSelection ? 0.8 : 0.6;
			}
		}

		private void PerformSliceAction()
		{
			Console.WriteLine(string.Format("Slice {0} was pressed.", currentSelection));
			switch (currentSelection)
			{
				case 0:
					simulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.TAB);
					break;
				case 2:
					simulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_W);
					break;
				case 4:
					// This shouldn't work
					simulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.SHIFT, VirtualKeyCode.TAB);
					break;
				case 6:
					simulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_T);
					break;
				default:
					break;
			}
		}

		private Point GetMousePosition()
		{
			System.Drawing.Point pt = System.Windows.Forms.Cursor.Position;
			return new Point(pt.X, pt.Y);
		}

		private IEnumerable<Path> Slices()
		{
			for (int i = 0; i < 8; i++)
			{
				Path slice = DrawSlice(i);
				const byte value = 0;
				slice.Fill = new SolidColorBrush(Color.FromRgb(value, value, value));
				yield return slice;
			}
		}

		private string TitleProvider(int slice)
		{
			string[] titles = new string[]
			{
				"first",
				"second",
				"third",
				"fourth",
				"fifth",
				"sixth",
				"seventh",
				"eighth"
			};
			return titles[slice];
		}

		private Path DrawSlice(int slice)
		{
			const double halfSliceSweep = Math.PI / 8.0;
			double bisector = slice * Math.PI / 4.0;
			double angleOffsetOuter = halfSliceSweep - GAP;
			double angleOffsetInner = halfSliceSweep - GAP / INNER_FRACTION;
			double outerRadius = this.Width / 2.0;
			double innerRadius = outerRadius * INNER_FRACTION;

			Point startOuter = PointOnCircle(bisector - angleOffsetOuter, outerRadius);
			Point startInner = PointOnCircle(bisector - angleOffsetInner, innerRadius);
			Point endOuter = PointOnCircle(bisector + angleOffsetOuter, outerRadius);
			Point endInner = PointOnCircle(bisector + angleOffsetInner, innerRadius);

			Size sizeOuter = new Size(outerRadius, outerRadius);
			Size sizeInner = new Size(innerRadius, innerRadius);

			PathSegmentCollection segments = new PathSegmentCollection();
			segments.Add(new ArcSegment(endOuter, sizeOuter, 45.0, isLargeArc: false, SweepDirection.Clockwise, isStroked: false));
			segments.Add(new LineSegment(endInner, isStroked: false));
			segments.Add(new ArcSegment(startInner, sizeInner, 45.0, isLargeArc: false, SweepDirection.Counterclockwise, isStroked: false));
			segments.Add(new LineSegment(startOuter, isStroked: false));

			PathFigure figure = new PathFigure(startOuter, segments, true);
			PathFigureCollection figures = new PathFigureCollection();
			figures.Add(figure);
			Path path = new Path();
			PathGeometry geo = new PathGeometry(figures);
			path.Data = geo;
			return path;
		}

		private Point PointOnCircle(double radians, double radius)
		{
			double offset = this.Width / 2.0;
			return new Point(Math.Cos(radians) * radius + offset, Math.Sin(radians) * radius + offset);
		}
	}
}
