using Avalonia.Media;
using ShapeEditor.Models.Shapes;

namespace ShapeEditor.Models.Commands;

public class UpdateShapeCommand : ICommand
{
    private readonly IShape _shape;
    private readonly Color _oldStrokeColor;
    private readonly Color _newStrokeColor;
    private readonly double _oldStrokeThickness;
    private readonly double _newStrokeThickness;
    private readonly Color _oldFillColor;
    private readonly Color _newFillColor;
    
    public UpdateShapeCommand(IShape shape, 
        Color oldStrokeColor, Color newStrokeColor,
        double oldStrokeThickness, double newStrokeThickness,
        Color oldFillColor, Color newFillColor)
    {
        _shape = shape;
        _oldStrokeColor = oldStrokeColor;
        _newStrokeColor = newStrokeColor;
        _oldStrokeThickness = oldStrokeThickness;
        _newStrokeThickness = newStrokeThickness;
        _oldFillColor = oldFillColor;
        _newFillColor = newFillColor;
    }
    
    public void Execute()
    {
        _shape.StrokeColor = _newStrokeColor;
        _shape.StrokeThickness = _newStrokeThickness;
        _shape.FillColor = _newFillColor;
    }
    
    public void Undo()
    {
        _shape.StrokeColor = _oldStrokeColor;
        _shape.StrokeThickness = _oldStrokeThickness;
        _shape.FillColor = _oldFillColor;
    }
}
