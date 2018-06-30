using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Gma.System.MouseKeyHook;
using System.Runtime.InteropServices;
using WindowsInput;
using WindowsInput.Native;

namespace PieMenu
{
	public partial class MainWindow : Window
	{
		private IKeyboardMouseEvents GlobalHook;
		private const double INNER_FRACTION = 0.2;
		private const double GAP = 0.02; // Radians on outer circumfrence
		private bool isActive = false;
		private bool pieHasDisplayed = false;
		private Point pieCenter;
		private int currentSelection = 0;
		private InputSimulator simulator = new InputSimulator();

		public MainWindow()
		{
			InitializeComponent();

			this.ShowActivated = false;
			this.Hide();

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

		private void GlobalHook_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if (isActive)
			{
				isActive = false;
				Recede();
			}
		}

		private void UpdatePie(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			Point mouse = GetMousePosition();
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
			if (length / this.Width * 2.0 < INNER_FRACTION)
			{
				currentSelection = -1;
			}
			else
			{
				double select = Math.Atan2(relative.Y, relative.X) / Math.PI * 4.0;
				select += select < 0 ? 8.5 : 0.5;
				currentSelection = (int)(select) % 8;
			}
			for (int i = 0; i < 8; i++)
			{
				Path path = canvas.Children[i] as Path;
				path.Opacity = i == currentSelection ? 1.0 : 0.5;
			}
		}

		private void PerformSliceAction()
		{
			Console.WriteLine(string.Format("Slice {0} was pressed.", currentSelection));
			simulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.TAB);
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
				const byte value = 200;
				slice.Fill = new SolidColorBrush(Color.FromRgb(value, value, value));
				slice.Opacity = 0.5;
				yield return slice;
			}
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
			segments.Add(new ArcSegment(endOuter, sizeOuter, 45.0, false, SweepDirection.Clockwise, true));
			segments.Add(new LineSegment(endInner, true));
			segments.Add(new ArcSegment(startInner, sizeInner, 45.0, false, SweepDirection.Counterclockwise, true));
			segments.Add(new LineSegment(startOuter, true));

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
