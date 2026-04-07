using Avalonia;
using ShapeEditor.Models.Shapes;

namespace ShapeEditor.Models.Commands;

public class RotateShapeCommand : ICommand
{
    private readonly IShape _shape;
    private readonly double _angle;
    private readonly Point _center;
    
    public RotateShapeCommand(IShape shape, double angle, Point center)
    {
        _shape = shape;
        _angle = angle;
        _center = center;
    }
    
    public void Execute()
    {
        _shape.Rotate(_angle, _center);
    }
    
    public void Undo()
    {
        _shape.Rotate(-_angle, _center);
    }
}
