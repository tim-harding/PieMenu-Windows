namespace PieMenu
{
	using System;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Media;
	using System.Windows.Shapes;

	interface IPieViewProvider
	{
		int SliceCount();
		string TitleText(int slice);
		int Selection();
	}

	class PieView
	{
		public readonly Canvas content = new Canvas();
		public IPieViewProvider provider;

		private Path[] paths;

		public PieView()
		{
			double size = Properties.Settings.Default.Size;
			content.Width = size;
			content.Height = size;
		}

		public void Draw()
		{
			int sliceCount = provider.SliceCount();
			paths = new Path[sliceCount];
			content.Children.Clear();
			for (var i = 0; i < sliceCount; i++)
			{
				Path path = DrawSlice(i, sliceCount);
				paths[i] = path;
				content.Children.Add(path);
				content.Children.Add(DrawTitle(i, sliceCount, provider.TitleText(i)));
			}
			UpdateHighlighting();
		}

		public void UpdateHighlighting()
		{
			int selection = provider?.Selection() ?? -1;
			for (var i = 0; i < paths.Length; i++)
			{
				paths[i].Opacity = i == selection ? 0.8 : 0.6;
			}
		}

		private TextBlock DrawTitle(int slice, int count, string text)
		{
			var title = new TextBlock();
			title.Text = text;

			// Styled as BaseTextBlockStyle
			title.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
			title.TextAlignment = TextAlignment.Center;
			title.Width = 100;
			title.Height = 22;
			title.FontFamily = new FontFamily("Segoe UI");
			title.FontWeight = FontWeights.SemiBold;
			title.FontSize = 15.0;

			double textFraction = Properties.Settings.Default.TextFraction;
			double bisector = Math.PI * ((double)slice / count * 2.0 - 0.5);
			Point point = PointOnCircle(bisector, content.Width / 2.0 * textFraction);
			Canvas.SetLeft(title, point.X - title.Width / 2.0);
			Canvas.SetTop(title, point.Y - title.Height / 2.0);
			return title;
		}

		private Path DrawSlice(int slice, int count)
		{
			double gap = Properties.Settings.Default.Gap;
			double innerFraction = Properties.Settings.Default.InnerFraction;
			double halfSliceSweep = Math.PI / count;
			double bisector = Math.PI * ((double)slice / count * 2.0 - 0.5);
			double angleOffsetOuter = halfSliceSweep - gap;
			double angleOffsetInner = halfSliceSweep - gap / innerFraction;
			double outerRadius = content.Width / 2.0;
			double innerRadius = outerRadius * innerFraction;

			Point startOuter = PointOnCircle(bisector - angleOffsetOuter, outerRadius);
			Point startInner = PointOnCircle(bisector - angleOffsetInner, innerRadius);
			Point endOuter = PointOnCircle(bisector + angleOffsetOuter, outerRadius);
			Point endInner = PointOnCircle(bisector + angleOffsetInner, innerRadius);

			var sizeOuter = new Size(outerRadius, outerRadius);
			var sizeInner = new Size(innerRadius, innerRadius);

			var segments = new PathSegmentCollection();
			segments.Add(new ArcSegment(endOuter, sizeOuter, 45.0, isLargeArc: false, SweepDirection.Clockwise, isStroked: false));
			segments.Add(new LineSegment(endInner, isStroked: false));
			segments.Add(new ArcSegment(startInner, sizeInner, 45.0, isLargeArc: false, SweepDirection.Counterclockwise, isStroked: false));
			segments.Add(new LineSegment(startOuter, isStroked: false));

			var figure = new PathFigure(startOuter, segments, true);
			var figures = new PathFigureCollection();
			figures.Add(figure);
			var path = new Path();
			var geo = new PathGeometry(figures);
			path.Data = geo;
			path.Fill = new SolidColorBrush(Color.FromRgb(0, 0, 0));
			return path;
		}

		private Point PointOnCircle(double radians, double radius)
		{
			double offset = content.Width / 2.0;
			return new Point(Math.Cos(radians) * radius + offset, Math.Sin(radians) * radius + offset);
		}
	}
}
