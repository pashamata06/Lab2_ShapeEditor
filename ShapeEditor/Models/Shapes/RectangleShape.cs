using Avalonia;
using Avalonia.Media;
using System;
using System.Collections.Generic;

namespace ShapeEditor.Models.Shapes;

public class RectangleShape : ShapeBase
{
    public Point TopLeft { get; set; }
    public Point BottomRight { get; set; }
    private double _rotationAngle = 0;
    private Point _center;
    
    public RectangleShape(Point start, Point end)
    {
        TopLeft = new Point(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y));
        BottomRight = new Point(Math.Max(start.X, end.X), Math.Max(start.Y, end.Y));
        UpdateCenter();
    }
    
    private void UpdateCenter()
    {
        _center = new Point(
            (TopLeft.X + BottomRight.X) / 2,
            (TopLeft.Y + BottomRight.Y) / 2
        );
    }
    
    private Point RotatePoint(Point p, Point center, double angle)
    {
        double rad = angle * Math.PI / 180;
        double cos = Math.Cos(rad);
        double sin = Math.Sin(rad);
        double dx = p.X - center.X;
        double dy = p.Y - center.Y;
        return new Point(
            center.X + dx * cos - dy * sin,
            center.Y + dx * sin + dy * cos
        );
    }
    
    private List<Point> GetCorners()
    {
        return new List<Point>
        {
            TopLeft,
            new Point(BottomRight.X, TopLeft.Y),
            BottomRight,
            new Point(TopLeft.X, BottomRight.Y)
        };
    }
    
    public override void Draw(DrawingContext context)
    {
        var corners = GetCorners();
        var rotatedCorners = new List<Point>();
        foreach (var corner in corners)
            rotatedCorners.Add(RotatePoint(corner, _center, _rotationAngle));
        
        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(rotatedCorners[0], true);
            ctx.LineTo(rotatedCorners[1]);
            ctx.LineTo(rotatedCorners[2]);
            ctx.LineTo(rotatedCorners[3]);
            ctx.EndFigure(true);
        }
        var brush = new SolidColorBrush(FillColor);
        var pen = new Pen(new SolidColorBrush(StrokeColor), StrokeThickness);
        if (FillColor != Colors.Transparent)
            context.DrawGeometry(brush, null, geometry);
        context.DrawGeometry(null, pen, geometry);
    }
    
    public override bool HitTest(Point point)
    {
        var corners = GetCorners();
        var rotatedCorners = new List<Point>();
        foreach (var corner in corners)
            rotatedCorners.Add(RotatePoint(corner, _center, _rotationAngle));
        double minX = double.MaxValue, minY = double.MaxValue;
        double maxX = double.MinValue, maxY = double.MinValue;
        foreach (var p in rotatedCorners)
        {
            minX = Math.Min(minX, p.X);
            minY = Math.Min(minY, p.Y);
            maxX = Math.Max(maxX, p.X);
            maxY = Math.Max(maxY, p.Y);
        }
        var bounds = new Rect(minX, minY, maxX - minX, maxY - minY);
        return bounds.Contains(point);
    }
    
    public override Rect Bounds
    {
        get
        {
            var corners = GetCorners();
            var rotatedCorners = new List<Point>();
            foreach (var corner in corners)
                rotatedCorners.Add(RotatePoint(corner, _center, _rotationAngle));
            double minX = double.MaxValue, minY = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue;
            foreach (var p in rotatedCorners)
            {
                minX = Math.Min(minX, p.X);
                minY = Math.Min(minY, p.Y);
                maxX = Math.Max(maxX, p.X);
                maxY = Math.Max(maxY, p.Y);
            }
            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }
    }
    
    public override IShape Clone() => new RectangleShape(TopLeft, BottomRight)
    {
        StrokeThickness = StrokeThickness,
        StrokeColor = StrokeColor,
        FillColor = FillColor,
        _rotationAngle = _rotationAngle
    };
    
    public override void Translate(double dx, double dy)
    {
        TopLeft = new Point(TopLeft.X + dx, TopLeft.Y + dy);
        BottomRight = new Point(BottomRight.X + dx, BottomRight.Y + dy);
        UpdateCenter();
    }
    
    public override void Rotate(double angle, Point center)
    {
        _rotationAngle += angle;
        UpdateCenter();
    }
    
    public override void Scale(double sx, double sy, Point center)
    {
        TopLeft = new Point(center.X + (TopLeft.X - center.X) * sx, center.Y + (TopLeft.Y - center.Y) * sy);
        BottomRight = new Point(center.X + (BottomRight.X - center.X) * sx, center.Y + (BottomRight.Y - center.Y) * sy);
        UpdateCenter();
    }
}
