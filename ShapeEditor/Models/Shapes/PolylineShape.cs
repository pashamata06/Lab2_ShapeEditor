using Avalonia;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShapeEditor.Models.Shapes;

public class PolylineShape : ShapeBase
{
    public List<Point> Points { get; set; } = new();
    
    public PolylineShape() { }
    public PolylineShape(List<Point> points) { Points = points.ToList(); }
    public void AddPoint(Point point) { Points.Add(point); }
    
    public override void Draw(DrawingContext context)
    {
        if (Points.Count < 2) return;
        var pen = new Pen(new SolidColorBrush(StrokeColor), StrokeThickness);
        for (int i = 0; i < Points.Count - 1; i++)
            context.DrawLine(pen, Points[i], Points[i + 1]);
    }
    
    public override bool HitTest(Point point)
    {
        if (Points.Count < 2) return false;
        for (int i = 0; i < Points.Count - 1; i++)
        {
            double dist = DistanceToSegment(point, Points[i], Points[i + 1]);
            if (dist <= 10) return true;
        }
        return false;
    }
    
    private double DistanceToSegment(Point p, Point a, Point b)
    {
        double ax = b.X - a.X, ay = b.Y - a.Y;
        if (ax == 0 && ay == 0) return Math.Sqrt((p.X - a.X) * (p.X - a.X) + (p.Y - a.Y) * (p.Y - a.Y));
        double t = ((p.X - a.X) * ax + (p.Y - a.Y) * ay) / (ax * ax + ay * ay);
        t = Math.Clamp(t, 0, 1);
        double dx = a.X + t * ax - p.X, dy = a.Y + t * ay - p.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
    
    public override Rect Bounds
    {
        get
        {
            if (Points.Count == 0) return new Rect();
            double minX = Points.Min(p => p.X), minY = Points.Min(p => p.Y);
            double maxX = Points.Max(p => p.X), maxY = Points.Max(p => p.Y);
            return new Rect(minX - 5, minY - 5, maxX - minX + 10, maxY - minY + 10);
        }
    }
    
    public override IShape Clone() => new PolylineShape(Points)
    {
        StrokeThickness = StrokeThickness,
        StrokeColor = StrokeColor,
        FillColor = FillColor
    };
    
    public override void Translate(double dx, double dy)
    {
        for (int i = 0; i < Points.Count; i++)
            Points[i] = new Point(Points[i].X + dx, Points[i].Y + dy);
    }
    
    public override void Rotate(double angle, Point center)
    {
        double rad = angle * Math.PI / 180;
        double cos = Math.Cos(rad), sin = Math.Sin(rad);
        for (int i = 0; i < Points.Count; i++)
        {
            double dx = Points[i].X - center.X, dy = Points[i].Y - center.Y;
            Points[i] = new Point(center.X + dx * cos - dy * sin, center.Y + dx * sin + dy * cos);
        }
    }
    
    public override void Scale(double sx, double sy, Point center)
    {
        for (int i = 0; i < Points.Count; i++)
        {
            Points[i] = new Point(center.X + (Points[i].X - center.X) * sx, center.Y + (Points[i].Y - center.Y) * sy);
        }
    }
}
