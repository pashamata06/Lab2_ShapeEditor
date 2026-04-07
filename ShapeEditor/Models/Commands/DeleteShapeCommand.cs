using ShapeEditor.Models.Shapes;

namespace ShapeEditor.Models.Commands;

public class DeleteShapeCommand : ICommand
{
    private readonly DrawingCanvas _canvas;
    private readonly IShape _shape;
    
    public DeleteShapeCommand(DrawingCanvas canvas, IShape shape)
    {
        _canvas = canvas;
        _shape = shape;
    }
    
    public void Execute()
    {
        _canvas.RemoveShape(_shape);
    }
    
    public void Undo()
    {
        _canvas.AddShape(_shape);
    }
}
