using Avalonia;
using ShapeEditor.Models.Shapes;

namespace ShapeEditor.Models.Commands;

public class ResizeShapeCommand : ICommand
{
    private readonly RectangleShape _shape;
    private readonly Point _oldTopLeft;
    private readonly Point _oldBottomRight;
    private readonly Point _newTopLeft;
    private readonly Point _newBottomRight;
    
    public ResizeShapeCommand(RectangleShape shape, 
        Point oldTopLeft, Point oldBottomRight,
        Point newTopLeft, Point newBottomRight)
    {
        _shape = shape;
        _oldTopLeft = oldTopLeft;
        _oldBottomRight = oldBottomRight;
        _newTopLeft = newTopLeft;
        _newBottomRight = newBottomRight;
    }
    
    public void Execute()
    {
        _shape.TopLeft = _newTopLeft;
        _shape.BottomRight = _newBottomRight;
    }
    
    public void Undo()
    {
        _shape.TopLeft = _oldTopLeft;
        _shape.BottomRight = _oldBottomRight;
    }
}
