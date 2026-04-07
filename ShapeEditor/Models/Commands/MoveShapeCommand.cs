using ShapeEditor.Models.Shapes;

namespace ShapeEditor.Models.Commands;

public class MoveShapeCommand : ICommand
{
    private readonly IShape _shape;
    private readonly double _deltaX;
    private readonly double _deltaY;
    
    public MoveShapeCommand(IShape shape, double deltaX, double deltaY)
    {
        _shape = shape;
        _deltaX = deltaX;
        _deltaY = deltaY;
    }
    
    public void Execute()
    {
        _shape.Translate(_deltaX, _deltaY);
    }
    
    public void Undo()
    {
        _shape.Translate(-_deltaX, -_deltaY);
    }
}
