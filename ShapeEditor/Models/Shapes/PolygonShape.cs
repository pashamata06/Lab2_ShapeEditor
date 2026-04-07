using Avalonia;
using Avalonia.Media;
using System.Collections.Generic;
using System.Linq;

namespace ShapeEditor.Models.Shapes;

public class PolygonShape : ShapeBase
{
    public List<Point> Points { get; set; } = new();
    
    public PolygonShape()
    {
    }
    
    public PolygonShape(List<Point> points)
    {
        Points = points.ToList();
    }
    
    public void AddPoint(Point point)
    {
        Points.Add(point);
    }
    
    public override void Draw(DrawingContext context)
    {
        if (Points.Count < 3) return;
        
        var brush = new SolidColorBrush(FillColor);
        var pen = new Pen(new SolidColorBrush(StrokeColor), StrokeThickness);
        
        // Создаём геометрию многоугольника
        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(Points[0], true);
            for (int i = 1; i < Points.Count; i++)
            {
                ctx.LineTo(Points[i]);
            }
            ctx.EndFigure(true); // true = замкнуть фигуру
        }
        
        if (FillColor != Colors.Transparent)
            context.DrawGeometry(brush, null, geometry);
        
        context.DrawGeometry(null, pen, geometry);
    }
    
    public override bool HitTest(Point point)
    {
        var bounds = Bounds;
        if (!bounds.Contains(point)) return false;
        
        // Проверка попадания в многоугольник (ray casting)
        bool inside = false;
        for (int i = 0, j = Points.Count - 1; i < Points.Count; j = i++)
        {
            if (((Points[i].Y > point.Y) != (Points[j].Y > point.Y)) &&
                (point.X < (Points[j].X - Points[i].X) * (point.Y - Points[i].Y) / (Points[j].Y - Points[i].Y) + Points[i].X))
            {
                inside = !inside;
            }
        }
        return inside;
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
        var copy = new PolygonShape(Points);
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
