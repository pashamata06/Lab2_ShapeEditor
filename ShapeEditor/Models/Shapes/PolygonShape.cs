using Avalonia;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShapeEditor.Models.Shapes;

public class PolygonShape : ShapeBase
{
    public List<Point> Points { get; set; } = new();
    
    public PolygonShape() { }
    public PolygonShape(List<Point> points) { Points = points.ToList(); }
    public void AddPoint(Point point) { Points.Add(point); }
    
    public override void Draw(DrawingContext context)
    {
        if (Points.Count < 3) return;
        var brush = new SolidColorBrush(FillColor);
        var pen = new Pen(new SolidColorBrush(StrokeColor), StrokeThickness);
        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(Points[0], true);
            for (int i = 1; i < Points.Count; i++) ctx.LineTo(Points[i]);
            ctx.EndFigure(true);
        }
        if (FillColor != Colors.Transparent) context.DrawGeometry(brush, null, geometry);
        context.DrawGeometry(null, pen, geometry);
    }
    
    public override bool HitTest(Point point)
    {
        if (Points.Count < 3) return false;
        var bounds = Bounds;
        if (!bounds.Contains(point)) return false;
        bool inside = false;
        for (int i = 0, j = Points.Count - 1; i < Points.Count; j = i++)
        {
            if (((Points[i].Y > point.Y) != (Points[j].Y > point.Y)) &&
                (point.X < (Points[j].X - Points[i].X) * (point.Y - Points[i].Y) / (Points[j].Y - Points[i].Y) + Points[i].X))
                inside = !inside;
        }
        return inside;
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
    
    public override IShape Clone() => new PolygonShape(Points)
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
