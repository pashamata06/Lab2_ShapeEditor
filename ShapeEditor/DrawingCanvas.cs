using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using ShapeEditor.Models;
using ShapeEditor.Models.Commands;
using ShapeEditor.Models.Layers;
using ShapeEditor.Models.Shapes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ShapeEditor;

public class DrawingCanvas : Control
{
    public ObservableCollection<Layer> Layers { get; set; } = new();
    private Layer? _activeLayer;
    
    private readonly Stack<ICommand> _undoStack = new();
    private readonly Stack<ICommand> _redoStack = new();
    
    private IShape? _previewShape;
    private Point? _drawStartPoint;
    private List<IShape> _selectedShapes = new();
    private bool _isDragging;
    private Point _dragStartPoint;
    private Point _dragStartForSelected;
    
    private enum ResizeHandle { None, TopLeft, Top, TopRight, Right, BottomRight, Bottom, BottomLeft, Left }
    private ResizeHandle _activeHandle = ResizeHandle.None;
    private Rect _selectedShapeBounds;   // исходные границы при начале изменения размера
    private Point _scaleCenter;           // центр для масштабирования
    private Point _lastResizePoint;       // последняя точка мыши при изменении размера
    
    private PolylineShape? _currentPolyline;
    private bool _isDrawingPolyline;
    
    private PolygonShape? _currentPolygon;
    private bool _isDrawingPolygon;
    
    public ShapeType CurrentShapeType { get; set; } = ShapeType.Select;
    public Color CurrentStrokeColor { get; set; } = Colors.Black;
    public double CurrentStrokeThickness { get; set; } = 2;
    public Color CurrentFillColor { get; set; } = Colors.Transparent;
    
    public DrawingCanvas()
    {
        Layers.Add(new Layer("Слой 1"));
        _activeLayer = Layers[0];
        
        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;
        
        Focusable = true;
        KeyDown += OnKeyDown;
    }
    
    public IShape? GetSelectedShape() => _selectedShapes.FirstOrDefault();
    public List<IShape> GetSelectedShapes() => _selectedShapes.ToList();
    public Layer? GetActiveLayer() => _activeLayer;
    
    public void AddLayer(string name)
    {
        var layer = new Layer(name);
        Layers.Add(layer);
        InvalidateVisual();
    }
    
    public void RemoveLayer(Layer layer)
    {
        if (Layers.Count <= 1) return;
        if (_activeLayer == layer && Layers.Count > 1)
            _activeLayer = Layers.First(l => l != layer);
        Layers.Remove(layer);
        InvalidateVisual();
    }
    
    public void SetActiveLayer(Layer layer)
    {
        _activeLayer = layer;
        _selectedShapes.Clear();
        InvalidateVisual();
    }
    
    public void MoveShapeToLayer(IShape shape, Layer targetLayer)
    {
        var currentLayer = Layers.FirstOrDefault(l => l.Shapes.Contains(shape));
        if (currentLayer != null)
        {
            currentLayer.Shapes.Remove(shape);
            targetLayer.Shapes.Add(shape);
            InvalidateVisual();
        }
    }
    
    public void MoveLayerUp(Layer layer)
    {
        int index = Layers.IndexOf(layer);
        if (index < Layers.Count - 1)
        {
            Layers.Move(index, index + 1);
            InvalidateVisual();
        }
    }
    
    public void MoveLayerDown(Layer layer)
    {
        int index = Layers.IndexOf(layer);
        if (index > 0)
        {
            Layers.Move(index, index - 1);
            InvalidateVisual();
        }
    }
    
    public void AddShape(IShape shape)
    {
        if (_activeLayer != null)
        {
            _activeLayer.Shapes.Add(shape);
        }
        InvalidateVisual();
    }
    
    public void RemoveShape(IShape shape)
    {
        var layer = Layers.FirstOrDefault(l => l.Shapes.Contains(shape));
        layer?.Shapes.Remove(shape);
        _selectedShapes.Remove(shape);
        InvalidateVisual();
    }
    
    private void ExecuteCommand(ICommand command)
    {
        command.Execute();
        _undoStack.Push(command);
        _redoStack.Clear();
        InvalidateVisual();
    }
    
    public void Undo()
    {
        if (_undoStack.Count == 0) return;
        var command = _undoStack.Pop();
        command.Undo();
        _redoStack.Push(command);
        InvalidateVisual();
    }
    
    public void Redo()
    {
        if (_redoStack.Count == 0) return;
        var command = _redoStack.Pop();
        command.Execute();
        _undoStack.Push(command);
        InvalidateVisual();
    }
    
    private ResizeHandle HitTestResizeHandle(Point point, Rect bounds)
    {
        double handleSize = 8;
        if (new Rect(bounds.X - handleSize/2, bounds.Y - handleSize/2, handleSize, handleSize).Contains(point)) return ResizeHandle.TopLeft;
        if (new Rect(bounds.X + bounds.Width/2 - handleSize/2, bounds.Y - handleSize/2, handleSize, handleSize).Contains(point)) return ResizeHandle.Top;
        if (new Rect(bounds.X + bounds.Width - handleSize/2, bounds.Y - handleSize/2, handleSize, handleSize).Contains(point)) return ResizeHandle.TopRight;
        if (new Rect(bounds.X + bounds.Width - handleSize/2, bounds.Y + bounds.Height/2 - handleSize/2, handleSize, handleSize).Contains(point)) return ResizeHandle.Right;
        if (new Rect(bounds.X + bounds.Width - handleSize/2, bounds.Y + bounds.Height - handleSize/2, handleSize, handleSize).Contains(point)) return ResizeHandle.BottomRight;
        if (new Rect(bounds.X + bounds.Width/2 - handleSize/2, bounds.Y + bounds.Height - handleSize/2, handleSize, handleSize).Contains(point)) return ResizeHandle.Bottom;
        if (new Rect(bounds.X - handleSize/2, bounds.Y + bounds.Height - handleSize/2, handleSize, handleSize).Contains(point)) return ResizeHandle.BottomLeft;
        if (new Rect(bounds.X - handleSize/2, bounds.Y + bounds.Height/2 - handleSize/2, handleSize, handleSize).Contains(point)) return ResizeHandle.Left;
        return ResizeHandle.None;
    }
    
    private void DrawResizeHandles(DrawingContext context, Rect bounds)
    {
        double handleSize = 8;
        var handleBrush = new SolidColorBrush(Colors.Blue);
        var handlePen = new Pen(Brushes.White, 2);
        Point[] handles = new Point[]
        {
            new Point(bounds.X - handleSize/2, bounds.Y - handleSize/2),
            new Point(bounds.X + bounds.Width/2 - handleSize/2, bounds.Y - handleSize/2),
            new Point(bounds.X + bounds.Width - handleSize/2, bounds.Y - handleSize/2),
            new Point(bounds.X + bounds.Width - handleSize/2, bounds.Y + bounds.Height/2 - handleSize/2),
            new Point(bounds.X + bounds.Width - handleSize/2, bounds.Y + bounds.Height - handleSize/2),
            new Point(bounds.X + bounds.Width/2 - handleSize/2, bounds.Y + bounds.Height - handleSize/2),
            new Point(bounds.X - handleSize/2, bounds.Y + bounds.Height - handleSize/2),
            new Point(bounds.X - handleSize/2, bounds.Y + bounds.Height/2 - handleSize/2)
        };
        foreach (var handle in handles)
            context.DrawRectangle(handleBrush, handlePen, new Rect(handle, new Size(handleSize, handleSize)));
    }
    
    private void DrawSelectionFrame(DrawingContext context, Rect bounds)
    {
        var pen = new Pen(Brushes.Blue, 2, DashStyle.Dash);
        context.DrawRectangle(pen, bounds);
    }
    
    public override void Render(DrawingContext context)
    {
        base.Render(context);
        context.FillRectangle(Brushes.White, new Rect(0, 0, Bounds.Width, Bounds.Height));
        
        foreach (var layer in Layers)
        {
            if (layer.IsVisible)
            {
                foreach (var shape in layer.Shapes)
                    shape.Draw(context);
            }
        }
        
        if (_currentPolyline != null && _currentPolyline.Points.Count > 0)
            _currentPolyline.Draw(context);
        if (_currentPolygon != null && _currentPolygon.Points.Count > 0)
            _currentPolygon.Draw(context);
        
        foreach (var shape in _selectedShapes)
            DrawSelectionFrame(context, shape.Bounds);
        
        if (_selectedShapes.Count == 1)
            DrawResizeHandles(context, _selectedShapes[0].Bounds);
        
        if (_previewShape != null && CurrentShapeType != ShapeType.Polyline && CurrentShapeType != ShapeType.Polygon && CurrentShapeType != ShapeType.Select)
            _previewShape.Draw(context);
    }
    
    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        Focus();
        var clickPoint = e.GetPosition(this);
        var isShiftPressed = (e.KeyModifiers & KeyModifiers.Shift) != 0;
        
        if (_selectedShapes.Count == 1 && CurrentShapeType == ShapeType.Select)
        {
            _activeHandle = HitTestResizeHandle(clickPoint, _selectedShapes[0].Bounds);
            if (_activeHandle != ResizeHandle.None)
            {
                _selectedShapeBounds = _selectedShapes[0].Bounds;
                _scaleCenter = new Point(_selectedShapeBounds.X + _selectedShapeBounds.Width / 2,
                                         _selectedShapeBounds.Y + _selectedShapeBounds.Height / 2);
                _isDragging = true;
                _dragStartPoint = clickPoint;
                _lastResizePoint = clickPoint;
                return;
            }
        }
        
        if (CurrentShapeType == ShapeType.Select)
        {
            IShape? clickedShape = null;
            for (int i = Layers.Count - 1; i >= 0; i--)
            {
                var layer = Layers[i];
                if (!layer.IsVisible) continue;
                for (int j = layer.Shapes.Count - 1; j >= 0; j--)
                {
                    if (layer.Shapes[j].HitTest(clickPoint))
                    {
                        clickedShape = layer.Shapes[j];
                        break;
                    }
                }
                if (clickedShape != null) break;
            }
            
            if (clickedShape != null)
            {
                if (isShiftPressed)
                {
                    if (_selectedShapes.Contains(clickedShape))
                        _selectedShapes.Remove(clickedShape);
                    else
                        _selectedShapes.Add(clickedShape);
                }
                else
                {
                    _selectedShapes.Clear();
                    _selectedShapes.Add(clickedShape);
                }
                _isDragging = true;
                _dragStartPoint = clickPoint;
                _dragStartForSelected = clickPoint;
            }
            else if (!isShiftPressed)
                _selectedShapes.Clear();
            InvalidateVisual();
            return;
        }
        
        if (CurrentShapeType == ShapeType.Polyline)
        {
            if (_isDrawingPolyline && _currentPolyline != null)
            {
                _currentPolyline.AddPoint(clickPoint);
                InvalidateVisual();
                return;
            }
            else
            {
                _currentPolyline = new PolylineShape();
                _currentPolyline.StrokeColor = CurrentStrokeColor;
                _currentPolyline.StrokeThickness = CurrentStrokeThickness;
                _currentPolyline.AddPoint(clickPoint);
                _isDrawingPolyline = true;
                InvalidateVisual();
                return;
            }
        }
        
        if (CurrentShapeType == ShapeType.Polygon)
        {
            if (_isDrawingPolygon && _currentPolygon != null)
            {
                _currentPolygon.AddPoint(clickPoint);
                InvalidateVisual();
                return;
            }
            else
            {
                _currentPolygon = new PolygonShape();
                _currentPolygon.StrokeColor = CurrentStrokeColor;
                _currentPolygon.StrokeThickness = CurrentStrokeThickness;
                _currentPolygon.FillColor = CurrentFillColor;
                _currentPolygon.AddPoint(clickPoint);
                _isDrawingPolygon = true;
                InvalidateVisual();
                return;
            }
        }
        
        _selectedShapes.Clear();
        _drawStartPoint = clickPoint;
        InvalidateVisual();
    }
    
    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        var currentPoint = e.GetPosition(this);
        
        if (_isDragging && _activeHandle != ResizeHandle.None && _selectedShapes.Count == 1)
        {
            var shape = _selectedShapes[0];
            // Вычисляем дельту относительно предыдущей точки
            double deltaX = currentPoint.X - _lastResizePoint.X;
            double deltaY = currentPoint.Y - _lastResizePoint.Y;
            
            // Коэффициенты масштабирования (относительные)
            double sx = 1, sy = 1;
            double width = _selectedShapeBounds.Width;
            double height = _selectedShapeBounds.Height;
            
            switch (_activeHandle)
            {
                case ResizeHandle.TopLeft:
                    sx = (width - deltaX) / width;
                    sy = (height - deltaY) / height;
                    break;
                case ResizeHandle.Top:
                    sx = 1;
                    sy = (height - deltaY) / height;
                    break;
                case ResizeHandle.TopRight:
                    sx = (width + deltaX) / width;
                    sy = (height - deltaY) / height;
                    break;
                case ResizeHandle.Right:
                    sx = (width + deltaX) / width;
                    sy = 1;
                    break;
                case ResizeHandle.BottomRight:
                    sx = (width + deltaX) / width;
                    sy = (height + deltaY) / height;
                    break;
                case ResizeHandle.Bottom:
                    sx = 1;
                    sy = (height + deltaY) / height;
                    break;
                case ResizeHandle.BottomLeft:
                    sx = (width - deltaX) / width;
                    sy = (height + deltaY) / height;
                    break;
                case ResizeHandle.Left:
                    sx = (width - deltaX) / width;
                    sy = 1;
                    break;
            }
            
            // Ограничиваем минимальный размер (не даём уменьшиться меньше 10)
            if (width * sx < 10) sx = 10 / width;
            if (height * sy < 10) sy = 10 / height;
            
            shape.Scale(sx, sy, _scaleCenter);
            
            // Обновляем начальные границы и точку для следующего шага
            _selectedShapeBounds = shape.Bounds;
            _lastResizePoint = currentPoint;
            InvalidateVisual();
            return;
        }
        
        if (_isDragging && _selectedShapes.Count > 0 && CurrentShapeType == ShapeType.Select && _activeHandle == ResizeHandle.None)
        {
            double deltaX = currentPoint.X - _dragStartForSelected.X;
            double deltaY = currentPoint.Y - _dragStartForSelected.Y;
            foreach (var shape in _selectedShapes)
                shape.Translate(deltaX, deltaY);
            _dragStartForSelected = currentPoint;
            InvalidateVisual();
            return;
        }
        
        if (CurrentShapeType == ShapeType.Polyline || CurrentShapeType == ShapeType.Polygon || CurrentShapeType == ShapeType.Select)
            return;
        if (_drawStartPoint == null) return;
        
        var currentPos = e.GetPosition(this);
        _previewShape = CurrentShapeType switch
        {
            ShapeType.Line => new LineShape(_drawStartPoint.Value, currentPos),
            ShapeType.Rectangle => new RectangleShape(_drawStartPoint.Value, currentPos),
            ShapeType.Ellipse => new EllipseShape(_drawStartPoint.Value, currentPos),
            _ => new LineShape(_drawStartPoint.Value, currentPos)
        };
        if (_previewShape != null)
        {
            _previewShape.StrokeColor = CurrentStrokeColor;
            _previewShape.StrokeThickness = CurrentStrokeThickness;
            _previewShape.FillColor = CurrentFillColor;
        }
        InvalidateVisual();
    }
    
    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (CurrentShapeType == ShapeType.Select)
        {
            _isDragging = false;
            _activeHandle = ResizeHandle.None;
            return;
        }
        
        if (CurrentShapeType == ShapeType.Polyline || CurrentShapeType == ShapeType.Polygon)
            return;
        
        if (_drawStartPoint == null) return;
        
        var endPoint = e.GetPosition(this);
        IShape newShape = CurrentShapeType switch
        {
            ShapeType.Line => new LineShape(_drawStartPoint.Value, endPoint),
            ShapeType.Rectangle => new RectangleShape(_drawStartPoint.Value, endPoint),
            ShapeType.Ellipse => new EllipseShape(_drawStartPoint.Value, endPoint),
            _ => new LineShape(_drawStartPoint.Value, endPoint)
        };
        newShape.StrokeColor = CurrentStrokeColor;
        newShape.StrokeThickness = CurrentStrokeThickness;
        newShape.FillColor = CurrentFillColor;
        AddShape(newShape);
        _previewShape = null;
        _drawStartPoint = null;
        InvalidateVisual();
    }
    
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.Z)
        {
            Undo();
            e.Handled = true;
            return;
        }
        if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.Y)
        {
            Redo();
            e.Handled = true;
            return;
        }
        
        if (CurrentShapeType == ShapeType.Polyline && _isDrawingPolyline && _currentPolyline != null)
        {
            if (e.Key == Key.Enter)
            {
                if (_currentPolyline.Points.Count >= 2)
                {
                    AddShape(_currentPolyline);
                    _selectedShapes.Clear();
                    _selectedShapes.Add(_currentPolyline);
                }
                _currentPolyline = null;
                _isDrawingPolyline = false;
                InvalidateVisual();
                e.Handled = true;
                return;
            }
            if (e.Key == Key.Escape)
            {
                _currentPolyline = null;
                _isDrawingPolyline = false;
                InvalidateVisual();
                e.Handled = true;
                return;
            }
        }
        
        if (CurrentShapeType == ShapeType.Polygon && _isDrawingPolygon && _currentPolygon != null)
        {
            if (e.Key == Key.Enter)
            {
                if (_currentPolygon.Points.Count >= 3)
                {
                    AddShape(_currentPolygon);
                    _selectedShapes.Clear();
                    _selectedShapes.Add(_currentPolygon);
                }
                _currentPolygon = null;
                _isDrawingPolygon = false;
                InvalidateVisual();
                e.Handled = true;
                return;
            }
            if (e.Key == Key.Escape)
            {
                _currentPolygon = null;
                _isDrawingPolygon = false;
                InvalidateVisual();
                e.Handled = true;
                return;
            }
        }
        
        if (_selectedShapes.Count == 0) return;
        double step = 1;
        double deltaX = 0, deltaY = 0;
        switch (e.Key)
        {
            case Key.Left: deltaX = -step; break;
            case Key.Right: deltaX = step; break;
            case Key.Up: deltaY = -step; break;
            case Key.Down: deltaY = step; break;
            default: return;
        }
        foreach (var shape in _selectedShapes)
            shape.Translate(deltaX, deltaY);
        InvalidateVisual();
        e.Handled = true;
    }
    
    public void CopySelectedShape()
    {
        if (_selectedShapes.Count == 0) return;
        var copy = _selectedShapes[0].Clone();
        copy.Translate(20, 20);
        AddShape(copy);
        _selectedShapes.Clear();
        _selectedShapes.Add(copy);
    }
    
    public void DeleteSelectedShape()
    {
        if (_selectedShapes.Count == 0) return;
        foreach (var shape in _selectedShapes.ToList())
            RemoveShape(shape);
        _selectedShapes.Clear();
    }
    
    public void UpdateSelectedShapeStrokeColor(Color color)
    {
        foreach (var shape in _selectedShapes)
        {
            shape.StrokeColor = color;
            InvalidateVisual();
        }
    }
    
    public void UpdateSelectedShapeStrokeThickness(double thickness)
    {
        foreach (var shape in _selectedShapes)
        {
            shape.StrokeThickness = thickness;
            InvalidateVisual();
        }
    }
    
    public void UpdateSelectedShapeFillColor(Color color)
    {
        foreach (var shape in _selectedShapes)
        {
            shape.FillColor = color;
            InvalidateVisual();
        }
    }
    
    public void RotateSelectedShape(double angle)
    {
        if (_selectedShapes.Count == 0) return;
        foreach (var shape in _selectedShapes)
        {
            var center = new Point(shape.Bounds.X + shape.Bounds.Width / 2, shape.Bounds.Y + shape.Bounds.Height / 2);
            shape.Rotate(angle, center);
            InvalidateVisual();
        }
    }
}
