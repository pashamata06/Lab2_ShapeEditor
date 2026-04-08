using System;
using Avalonia;
using Avalonia.Media;

namespace ShapeEditor.Models.Shapes;

public class LineShape : ShapeBase
{
    public Point Start { get; set; }
    public Point End { get; set; }
    
    public LineShape(Point start, Point end)
    {
        Start = start;
        End = end;
    }
    
    public override void Draw(DrawingContext context)
    {
        var pen = new Pen(new SolidColorBrush(StrokeColor), StrokeThickness);
        context.DrawLine(pen, Start, End);
    }
    
    public override bool HitTest(Point point)
    {
        double distance = DistanceToSegment(point, Start, End);
        return distance < 5;
    }
    
    private double DistanceToSegment(Point p, Point a, Point b)
    {
        double ax = b.X - a.X;
        double ay = b.Y - a.Y;
        double t = ((p.X - a.X) * ax + (p.Y - a.Y) * ay) / (ax * ax + ay * ay);
        t = Math.Clamp(t, 0, 1);
        double dx = a.X + t * ax - p.X;
        double dy = a.Y + t * ay - p.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
    
    public override Rect Bounds => new Rect(Start, End);
    
    public override IShape Clone() => new LineShape(Start, End)
    {
        StrokeThickness = StrokeThickness,
        StrokeColor = StrokeColor,
        FillColor = FillColor
    };
    
    public override void Translate(double dx, double dy)
    {
        Start = new Point(Start.X + dx, Start.Y + dy);
        End = new Point(End.X + dx, End.Y + dy);
    }
    
    public override void Rotate(double angle, Point center)
    {
        double rad = angle * Math.PI / 180;
        double cos = Math.Cos(rad);
        double sin = Math.Sin(rad);
        
        Point RotatePoint(Point p)
        {
            double dx = p.X - center.X;
            double dy = p.Y - center.Y;
            return new Point(
                center.X + dx * cos - dy * sin,
                center.Y + dx * sin + dy * cos
            );
        }
        Start = RotatePoint(Start);
        End = RotatePoint(End);
    }
    
    public override void Scale(double sx, double sy, Point center)
    {
        Start = new Point(center.X + (Start.X - center.X) * sx, center.Y + (Start.Y - center.Y) * sy);
        End = new Point(center.X + (End.X - center.X) * sx, center.Y + (End.Y - center.Y) * sy);
    }
}
