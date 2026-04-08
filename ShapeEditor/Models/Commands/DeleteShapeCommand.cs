using ShapeEditor.Models.Shapes;

namespace ShapeEditor.Models.Commands;

public class DeleteShapeCommand : ICommand
{
    private readonly global::ShapeEditor.DrawingCanvas _canvas;
    private readonly IShape _shape;
    
    public DeleteShapeCommand(global::ShapeEditor.DrawingCanvas canvas, IShape shape)
    {
        _canvas = canvas;
        _shape = shape;
    }
    
    public void Execute() => _canvas.RemoveShape(_shape);
    public void Undo() => _canvas.AddShape(_shape);
}
