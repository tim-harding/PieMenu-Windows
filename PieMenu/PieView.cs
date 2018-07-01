namespace PieMenu
{
	using System;
	using System.Collections.Generic;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Media;
	using System.Windows.Shapes;

	interface IPieViewProvider
	{
		string[] TitleText();
		int Selection();
	}

	class PieView
	{
		public readonly Canvas content = new Canvas();
		public IPieViewProvider provider;

		public PieView()
		{
			double size = Properties.Settings.Default.Size;
			content.Width = size;
			content.Height = size;
		}

		public void Draw()
		{
			content.Children.Clear();
			foreach (Path slice in Slices())
			{
				content.Children.Add(slice);
			}
			foreach (TextBlock title in Titles())
			{
				content.Children.Add(title);
			}
			UpdateHighlighting();
		}

		public void UpdateHighlighting()
		{
			int selection = provider?.Selection() ?? -1;
			for (int i = 0; i < 8; i++)
			{
				Path path = content.Children[i] as Path;
				path.Opacity = i == selection ? 0.8 : 0.6;
			}
		}

		private IEnumerable<Path> Slices()
		{
			for (int i = 0; i < 8; i++)
			{
				Path slice = DrawSlice(i);
				const byte brightness = 0;
				slice.Fill = new SolidColorBrush(Color.FromRgb(brightness, brightness, brightness));
				yield return slice;
			}
		}

		private IEnumerable<TextBlock> Titles()
		{
			string[] titles = provider?.TitleText() ?? new string[0];
			for (int i = 0; i < 8; i++)
			{
				TextBlock text = new TextBlock();
				text.Text = i < titles.Length ? titles[i] : "";

				// Styled as BaseTextBlockStyle
				text.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
				text.TextAlignment = TextAlignment.Center;
				text.Width = 100;
				text.Height = 22;
				text.FontFamily = new FontFamily("Segoe UI");
				text.FontWeight = FontWeights.SemiBold;
				text.FontSize = 15.0;

				float textFraction = Properties.Settings.Default.TextFraction;
				double bisector = i * Math.PI / 4.0;
				Point point = PointOnCircle(bisector, content.Width / 2.0 * textFraction);
				Canvas.SetLeft(text, point.X - text.Width / 2.0);
				Canvas.SetTop(text, point.Y - text.Height / 2.0);
				yield return text;
			}
		}

		private Path DrawSlice(int slice)
		{
			float gap = Properties.Settings.Default.Gap;
			float innerFraction = Properties.Settings.Default.InnerFraction;
			const double halfSliceSweep = Math.PI / 8.0;
			double bisector = slice * Math.PI / 4.0;
			double angleOffsetOuter = halfSliceSweep - gap;
			double angleOffsetInner = halfSliceSweep - gap / innerFraction;
			double outerRadius = content.Width / 2.0;
			double innerRadius = outerRadius * innerFraction;

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
			double offset = content.Width / 2.0;
			return new Point(Math.Cos(radians) * radius + offset, Math.Sin(radians) * radius + offset);
		}
	}
}
