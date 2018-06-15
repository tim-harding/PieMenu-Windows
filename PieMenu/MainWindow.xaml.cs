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

namespace PieMenu
{
	public partial class MainWindow : Window
	{
		IKeyboardMouseEvents GlobalHook;
		private const double INNER_RADIUS = 0.2;
		private bool isShowing = false;
		Point pieCenter;
		int currentSelection = 0;

		public MainWindow()
		{
			InitializeComponent();

			this.Hide();
			this.ShowActivated = false; // Just pop over, don't steal focus

			GlobalHook = Hook.GlobalEvents();
			GlobalHook.KeyDown += GlobalHook_KeyDown;
			GlobalHook.KeyUp += GlobalHook_KeyUp;

			foreach (Path slice in Slices())
			{
				canvas.Children.Add(slice);
			}
		}


		private void GlobalHook_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if (e.KeyCode == Keys.D)
			{
				if (!isShowing)	
				{
					this.Show();
					this.Topmost = true;

					pieCenter = GetMousePosition();
					Left = pieCenter.X - ActualWidth / 2.0;
					Top = pieCenter.Y - ActualHeight / 2.0;
					GlobalHook.MouseMove += UpdatePie;
					isShowing = true;
				}

			}
		}

		private void GlobalHook_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if (e.KeyCode == Keys.D)
			{
				this.Hide();
				isShowing = false;
				GlobalHook.MouseMove -= UpdatePie;
				PerformSliceAction();
			}
		}
		
		private void UpdatePie(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			Point mouse = GetMousePosition();
			Point relative = new Point(mouse.X - pieCenter.X, mouse.Y - pieCenter.Y);
			double rad = Math.Atan2(mouse.Y - pieCenter.Y, mouse.X - pieCenter.X);
			currentSelection = ((int)((rad / Math.PI * 0.5 + 0.5) * 8.0 - 0.5) + 4) % 8;
			for (int i = 0; i < 8; i++)
			{
				Path path = canvas.Children[i] as Path;
				path.Opacity = i == currentSelection ? 1.0 : 0.5;
			}
		}

		private void PerformSliceAction()
		{
			Console.WriteLine(string.Format("Slice {0} was pressed.", currentSelection));
		}

		private Point GetMousePosition()
		{
			Matrix tx = PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice;
			System.Drawing.Point pos = System.Windows.Forms.Control.MousePosition;
			Point mouse = tx.Transform(new Point(pos.X, pos.Y));
			return mouse;
		}

		private IEnumerable<Path> Slices()
		{
			for (int i = 0; i < 8; i++)
			{
				Path slice = DrawSlice(i);
				byte v = i % 2 == 0 ? (byte)200 : (byte)180;
				slice.Fill = new SolidColorBrush(Color.FromRgb(v, v, v));
				slice.Opacity = 0.5;
				yield return slice;
			}
		}

		private Path DrawSlice(int slice)
		{
			Point startOuter = PointOnCircumfrence(slice, 1.0);
			Point startInner = PointOnCircumfrence(slice, INNER_RADIUS);
			Point endOuter = PointOnCircumfrence(slice + 1, 1.0);
			Point endInner = PointOnCircumfrence(slice + 1, INNER_RADIUS);
			Size sizeOuter = new Size(Width / 2.0, Height / 2.0);
			Size sizeInner = new Size(Width / 2.0 * INNER_RADIUS, Height / 2.0 * INNER_RADIUS);

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

		private Point PointOnCircumfrence(int slice, double fraction)
		{
			double rad = (slice / 4.0 + 1.0 / 8.0) * Math.PI;
			double x = (fraction * Math.Cos(rad) + 1.0) * Width / 2.0;
			double y = (fraction * Math.Sin(rad) + 1.0) * Height / 2.0;
			return new Point(x, y);
		}
	}
}
