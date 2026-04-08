using ShapeEditor.Models.Shapes;

namespace ShapeEditor.Models.Commands;

public class AddShapeCommand : ICommand
{
    private readonly global::ShapeEditor.DrawingCanvas _canvas;
    private readonly IShape _shape;
    
    public AddShapeCommand(global::ShapeEditor.DrawingCanvas canvas, IShape shape)
    {
        _canvas = canvas;
        _shape = shape;
    }
    
    public void Execute() => _canvas.AddShape(_shape);
    public void Undo() => _canvas.RemoveShape(_shape);
}
