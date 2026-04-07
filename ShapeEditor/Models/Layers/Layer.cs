using ShapeEditor.Models.Shapes;
using System.Collections.ObjectModel;

namespace ShapeEditor.Models.Layers;

public class Layer
{
    public string Name { get; set; }
    public bool IsVisible { get; set; } = true;
    public ObservableCollection<IShape> Shapes { get; set; } = new();
    
    public Layer(string name)
    {
        Name = name;
    }
}
