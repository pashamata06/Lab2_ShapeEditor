using Avalonia;
using Avalonia.Media;
using System;

namespace ShapeEditor.Models.Shapes;

public class EllipseShape : ShapeBase
{
    public Point TopLeft { get; set; }
    public Point BottomRight { get; set; }
    
    public EllipseShape(Point start, Point end)
    {
        TopLeft = new Point(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y));
        BottomRight = new Point(Math.Max(start.X, end.X), Math.Max(start.Y, end.Y));
    }
    
    public override void Draw(DrawingContext context)
    {
        var rect = new Rect(TopLeft, BottomRight);
        var brush = new SolidColorBrush(FillColor);
        var pen = new Pen(new SolidColorBrush(StrokeColor), StrokeThickness);
        
        // Создаём геометрию эллипса
        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            var center = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
            double rx = rect.Width / 2;
            double ry = rect.Height / 2;
            
            // Аппроксимация эллипса через кубические кривые Безье
            double c = 0.5522847498;
            
            var points = new[]
            {
                new Point(center.X + rx, center.Y),
                new Point(center.X + rx, center.Y + ry * c),
                new Point(center.X + rx * c, center.Y + ry),
                new Point(center.X, center.Y + ry),
                new Point(center.X - rx * c, center.Y + ry),
                new Point(center.X - rx, center.Y + ry * c),
                new Point(center.X - rx, center.Y),
                new Point(center.X - rx, center.Y - ry * c),
                new Point(center.X - rx * c, center.Y - ry),
                new Point(center.X, center.Y - ry),
                new Point(center.X + rx * c, center.Y - ry),
                new Point(center.X + rx, center.Y - ry * c)
            };
            
            ctx.BeginFigure(points[0], true);
            ctx.CubicBezierTo(points[1], points[2], points[3]);
            ctx.CubicBezierTo(points[4], points[5], points[6]);
            ctx.CubicBezierTo(points[7], points[8], points[9]);
            ctx.CubicBezierTo(points[10], points[11], points[0]);
            ctx.EndFigure(true);
        }
        
        if (FillColor != Colors.Transparent)
            context.DrawGeometry(brush, null, geometry);
        context.DrawGeometry(null, pen, geometry);
    }
    
    public override bool HitTest(Point point)
    {
        var rect = new Rect(TopLeft, BottomRight);
        var center = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
        double rx = rect.Width / 2;
        double ry = rect.Height / 2;
        
        if (rx == 0 || ry == 0) return false;
        
        double dx = (point.X - center.X) / rx;
        double dy = (point.Y - center.Y) / ry;
        
        return dx * dx + dy * dy <= 1;
    }
    
    public override Rect Bounds => new Rect(TopLeft, BottomRight);
    
    public override IShape Clone() => new EllipseShape(TopLeft, BottomRight)
    {
        StrokeThickness = StrokeThickness,
        StrokeColor = StrokeColor,
        FillColor = FillColor
    };
    
    public override void Translate(double dx, double dy)
    {
        TopLeft = new Point(TopLeft.X + dx, TopLeft.Y + dy);
        BottomRight = new Point(BottomRight.X + dx, BottomRight.Y + dy);
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
        
        var oldTopLeft = RotatePoint(TopLeft);
        var oldBottomRight = RotatePoint(BottomRight);
        
        TopLeft = new Point(Math.Min(oldTopLeft.X, oldBottomRight.X), Math.Min(oldTopLeft.Y, oldBottomRight.Y));
        BottomRight = new Point(Math.Max(oldTopLeft.X, oldBottomRight.X), Math.Max(oldTopLeft.Y, oldBottomRight.Y));
    }
}
