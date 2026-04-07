using Avalonia;
using Avalonia.Media;
using System.Collections.Generic;
using System.Linq;

namespace ShapeEditor.Models.Shapes;

public class PolylineShape : ShapeBase
{
    public List<Point> Points { get; set; } = new();
    
    public PolylineShape()
    {
    }
    
    public PolylineShape(List<Point> points)
    {
        Points = points.ToList();
    }
    
    public void AddPoint(Point point)
    {
        Points.Add(point);
    }
    
    public override void Draw(DrawingContext context)
    {
        if (Points.Count < 2) return;
        
        var pen = new Pen(new SolidColorBrush(StrokeColor), StrokeThickness);
        
        for (int i = 0; i < Points.Count - 1; i++)
        {
            context.DrawLine(pen, Points[i], Points[i + 1]);
        }
    }
    
    public override bool HitTest(Point point)
    {
        // Сначала проверяем попадание в bounding box
        var bounds = Bounds;
        if (!bounds.Contains(point)) return false;
        
        // Затем проверяем расстояние до каждого отрезка
        for (int i = 0; i < Points.Count - 1; i++)
        {
            double distance = DistanceToSegment(point, Points[i], Points[i + 1]);
            if (distance < 10) return true;
        }
        return false;
    }
    
    private double DistanceToSegment(Point p, Point a, Point b)
    {
        double ax = b.X - a.X;
        double ay = b.Y - a.Y;
        
        if (ax == 0 && ay == 0) return System.Math.Sqrt((p.X - a.X) * (p.X - a.X) + (p.Y - a.Y) * (p.Y - a.Y));
        
        double t = ((p.X - a.X) * ax + (p.Y - a.Y) * ay) / (ax * ax + ay * ay);
        t = System.Math.Clamp(t, 0, 1);
        
        double dx = a.X + t * ax - p.X;
        double dy = a.Y + t * ay - p.Y;
        return System.Math.Sqrt(dx * dx + dy * dy);
    }
    
    public override Rect Bounds
    {
        get
        {
            if (Points.Count == 0) return new Rect();
            double minX = Points.Min(p => p.X);
            double minY = Points.Min(p => p.Y);
            double maxX = Points.Max(p => p.X);
            double maxY = Points.Max(p => p.Y);
            return new Rect(minX - 5, minY - 5, (maxX - minX) + 10, (maxY - minY) + 10);
        }
    }
    
    public override IShape Clone()
    {
        var copy = new PolylineShape(Points);
        copy.StrokeThickness = StrokeThickness;
        copy.StrokeColor = StrokeColor;
        copy.FillColor = FillColor;
        return copy;
    }
    
    public override void Translate(double dx, double dy)
    {
        for (int i = 0; i < Points.Count; i++)
        {
            Points[i] = new Point(Points[i].X + dx, Points[i].Y + dy);
        }
    }
    
    public override void Rotate(double angle, Point center)
    {
        double rad = angle * System.Math.PI / 180;
        double cos = System.Math.Cos(rad);
        double sin = System.Math.Sin(rad);
        
        for (int i = 0; i < Points.Count; i++)
        {
            double dx = Points[i].X - center.X;
            double dy = Points[i].Y - center.Y;
            Points[i] = new Point(
                center.X + dx * cos - dy * sin,
                center.Y + dx * sin + dy * cos
            );
        }
    }
}
