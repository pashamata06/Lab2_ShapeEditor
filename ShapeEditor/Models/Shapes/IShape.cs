using Avalonia;
using Avalonia.Media;

namespace ShapeEditor.Models.Shapes;

public interface IShape
{
    void Draw(DrawingContext context);
    bool HitTest(Point point);
    Rect Bounds { get; }
    IShape Clone();
    
    double StrokeThickness { get; set; }
    Color StrokeColor { get; set; }
    Color FillColor { get; set; }
    
    void Translate(double dx, double dy);
    void Rotate(double angle, Point center);
    void Scale(double sx, double sy, Point center);
}
