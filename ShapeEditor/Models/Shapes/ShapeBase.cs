using Avalonia;
using Avalonia.Media;

namespace ShapeEditor.Models.Shapes;

public abstract class ShapeBase : IShape
{
    public double StrokeThickness { get; set; } = 2;
    public Color StrokeColor { get; set; } = Colors.Black;
    public Color FillColor { get; set; } = Colors.Transparent;
    
    public abstract void Draw(DrawingContext context);
    public abstract bool HitTest(Point point);
    public abstract Rect Bounds { get; }
    public abstract IShape Clone();
    
    public virtual void Translate(double dx, double dy) { }
    public virtual void Rotate(double angle, Point center) { }
    public virtual void Scale(double sx, double sy, Point center) { }
}
