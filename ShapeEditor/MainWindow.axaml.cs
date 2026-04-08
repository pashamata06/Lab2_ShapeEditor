using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using ShapeEditor.Models;
using ShapeEditor.Models.Layers;
using System;
using System.Linq;

namespace ShapeEditor;

public partial class MainWindow : Window
{
    private StackPanel? _layersPanel;
    
    public MainWindow()
    {
        InitializeComponent();
        
        var selectBtn = this.Find<Button>("SelectBtn");
        var lineBtn = this.Find<Button>("LineBtn");
        var rectBtn = this.Find<Button>("RectBtn");
        var ellipseBtn = this.Find<Button>("EllipseBtn");
        var polylineBtn = this.Find<Button>("PolylineBtn");
        var polygonBtn = this.Find<Button>("PolygonBtn");
        var copyBtn = this.Find<Button>("CopyBtn");
        var deleteBtn = this.Find<Button>("DeleteBtn");
        var rotateBtn = this.Find<Button>("RotateBtn");
        var strokeColorBox = this.Find<ComboBox>("StrokeColorBox");
        var thicknessInput = this.Find<TextBox>("ThicknessInput");
        var thicknessUpBtn = this.Find<Button>("ThicknessUpBtn");
        var thicknessDownBtn = this.Find<Button>("ThicknessDownBtn");
        var fillColorBox = this.Find<ComboBox>("FillColorBox");
        var canvas = this.Find<DrawingCanvas>("Canvas");
        
        var addLayerBtn = this.Find<Button>("AddLayerBtn");
        var deleteLayerBtn = this.Find<Button>("DeleteLayerBtn");
        var moveLayerUpBtn = this.Find<Button>("MoveLayerUpBtn");
        var moveLayerDownBtn = this.Find<Button>("MoveLayerDownBtn");
        var moveShapeToLayerBtn = this.Find<Button>("MoveShapeToLayerBtn");
        _layersPanel = this.Find<StackPanel>("LayersPanel");
        
        if (selectBtn != null) selectBtn.Click += (s, e) => { if (canvas != null) canvas.CurrentShapeType = ShapeType.Select; };
        if (lineBtn != null) lineBtn.Click += (s, e) => { if (canvas != null) canvas.CurrentShapeType = ShapeType.Line; };
        if (rectBtn != null) rectBtn.Click += (s, e) => { if (canvas != null) canvas.CurrentShapeType = ShapeType.Rectangle; };
        if (ellipseBtn != null) ellipseBtn.Click += (s, e) => { if (canvas != null) canvas.CurrentShapeType = ShapeType.Ellipse; };
        if (polylineBtn != null) polylineBtn.Click += (s, e) => { if (canvas != null) canvas.CurrentShapeType = ShapeType.Polyline; };
        if (polygonBtn != null) polygonBtn.Click += (s, e) => { if (canvas != null) canvas.CurrentShapeType = ShapeType.Polygon; };
        if (copyBtn != null) copyBtn.Click += (s, e) => { if (canvas != null) canvas.CopySelectedShape(); };
        if (deleteBtn != null) deleteBtn.Click += (s, e) => { if (canvas != null) canvas.DeleteSelectedShape(); };
        if (rotateBtn != null) rotateBtn.Click += (s, e) => { if (canvas != null) canvas.RotateSelectedShape(15); };
        
        void UpdateLayersList()
        {
            if (canvas == null || _layersPanel == null) return;
            _layersPanel.Children.Clear();
            
            for (int i = 0; i < canvas.Layers.Count; i++)
            {
                var layer = canvas.Layers[i];
                var isActive = canvas.GetActiveLayer() == layer;
                
                var layerPanel = new Border
                {
                    Margin = new Thickness(0, 2, 0, 2),
                    Padding = new Thickness(5),
                    CornerRadius = new CornerRadius(3),
                    BorderBrush = isActive ? new SolidColorBrush(Colors.Blue) : null,
                    BorderThickness = isActive ? new Thickness(2) : new Thickness(0),
                    Tag = layer,
                    Child = new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        Spacing = 5,
                        Children =
                        {
                            new StackPanel
                            {
                                Orientation = Orientation.Horizontal,
                                Spacing = 10,
                                Children =
                                {
                                    new CheckBox
                                    {
                                        IsChecked = layer.IsVisible,
                                        VerticalAlignment = VerticalAlignment.Center,
                                        Tag = layer
                                    },
                                    new TextBox
                                    {
                                        Text = layer.Name,
                                        Width = 120,
                                        VerticalAlignment = VerticalAlignment.Center,
                                        Tag = layer
                                    }
                                }
                            },
                            new Button
                            {
                                Content = "Выбрать слой",
                                Margin = new Thickness(0, 2, 0, 0),
                                Background = isActive ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.LightGray),
                                Tag = layer
                            }
                        }
                    }
                };
                
                var checkBox = ((layerPanel.Child as StackPanel)?.Children[0] as StackPanel)?.Children[0] as CheckBox;
                if (checkBox != null)
                {
                    checkBox.Click += (s, e) =>
                    {
                        layer.IsVisible = checkBox.IsChecked == true;
                        canvas.InvalidateVisual();
                        UpdateLayersList();
                    };
                }
                
                var textBox = ((layerPanel.Child as StackPanel)?.Children[0] as StackPanel)?.Children[1] as TextBox;
                if (textBox != null)
                {
                    textBox.LostFocus += (s, e) =>
                    {
                        layer.Name = textBox.Text ?? "";
                        UpdateLayersList();
                    };
                }
                
                var selectLayerBtn = ((layerPanel.Child as StackPanel)?.Children[1] as Button);
                if (selectLayerBtn != null)
                {
                    selectLayerBtn.Click += (s, e) =>
                    {
                        canvas.SetActiveLayer(layer);
                        UpdateLayersList();
                    };
                }
                
                _layersPanel.Children.Add(layerPanel);
            }
        }
        
        if (addLayerBtn != null && canvas != null)
        {
            addLayerBtn.Click += (s, e) =>
            {
                int newId = canvas.Layers.Count + 1;
                canvas.AddLayer($"Слой {newId}");
                UpdateLayersList();
            };
        }
        
        if (deleteLayerBtn != null && canvas != null)
        {
            deleteLayerBtn.Click += (s, e) =>
            {
                var activeLayer = canvas.GetActiveLayer();
                if (activeLayer != null && canvas.Layers.Count > 1)
                {
                    var shapesToDelete = activeLayer.Shapes.ToList();
                    foreach (var shape in shapesToDelete)
                        canvas.RemoveShape(shape);
                    canvas.RemoveLayer(activeLayer);
                    UpdateLayersList();
                }
            };
        }
        
        if (moveLayerUpBtn != null && canvas != null)
        {
            moveLayerUpBtn.Click += (s, e) =>
            {
                var activeLayer = canvas.GetActiveLayer();
                if (activeLayer != null)
                {
                    canvas.MoveLayerUp(activeLayer);
                    UpdateLayersList();
                }
            };
        }
        
        if (moveLayerDownBtn != null && canvas != null)
        {
            moveLayerDownBtn.Click += (s, e) =>
            {
                var activeLayer = canvas.GetActiveLayer();
                if (activeLayer != null)
                {
                    canvas.MoveLayerDown(activeLayer);
                    UpdateLayersList();
                }
            };
        }
        
        if (moveShapeToLayerBtn != null && canvas != null)
        {
            moveShapeToLayerBtn.Click += (s, e) =>
            {
                var selectedShape = canvas.GetSelectedShape();
                var activeLayer = canvas.GetActiveLayer();
                if (selectedShape != null && activeLayer != null)
                {
                    canvas.MoveShapeToLayer(selectedShape, activeLayer);
                    UpdateLayersList();
                    canvas.InvalidateVisual();
                }
            };
        }
        
        void UpdateThickness(double value)
        {
            if (canvas == null) return;
            value = Math.Clamp(value, 1, 20);
            canvas.CurrentStrokeThickness = value;
            canvas.UpdateSelectedShapeStrokeThickness(value);
            if (thicknessInput != null) thicknessInput.Text = value.ToString();
        }
        
        if (thicknessUpBtn != null)
        {
            thicknessUpBtn.Click += (s, e) =>
            {
                if (canvas != null) UpdateThickness(canvas.CurrentStrokeThickness + 1);
            };
        }
        
        if (thicknessDownBtn != null)
        {
            thicknessDownBtn.Click += (s, e) =>
            {
                if (canvas != null) UpdateThickness(canvas.CurrentStrokeThickness - 1);
            };
        }
        
        if (thicknessInput != null)
        {
            thicknessInput.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter && canvas != null && double.TryParse(thicknessInput.Text, out double value))
                    UpdateThickness(value);
            };
            thicknessInput.LostFocus += (s, e) =>
            {
                if (canvas != null && double.TryParse(thicknessInput.Text, out double value))
                    UpdateThickness(value);
            };
        }
        
        if (strokeColorBox != null)
        {
            strokeColorBox.SelectionChanged += (s, e) =>
            {
                if (canvas == null) return;
                var selected = strokeColorBox.SelectedItem as ComboBoxItem;
                if (selected != null)
                {
                    var colorName = selected.Content as string;
                    var color = colorName switch
                    {
                        "Черный" => Colors.Black,
                        "Красный" => Colors.Red,
                        "Синий" => Colors.Blue,
                        "Зеленый" => Colors.Green,
                        "Желтый" => Colors.Yellow,
                        "Оранжевый" => Colors.Orange,
                        "Фиолетовый" => Colors.Purple,
                        "Коричневый" => Colors.Brown,
                        "Розовый" => Colors.Pink,
                        "Голубой" => Colors.LightBlue,
                        _ => Colors.Black
                    };
                    canvas.CurrentStrokeColor = color;
                    canvas.UpdateSelectedShapeStrokeColor(color);
                }
            };
        }
        
        if (fillColorBox != null)
        {
            fillColorBox.SelectionChanged += (s, e) =>
            {
                if (canvas == null) return;
                var selected = fillColorBox.SelectedItem as ComboBoxItem;
                if (selected != null)
                {
                    var colorName = selected.Content as string;
                    var color = colorName switch
                    {
                        "Прозрачный" => Colors.Transparent,
                        "Черный" => Colors.Black,
                        "Красный" => Colors.Red,
                        "Синий" => Colors.Blue,
                        "Зеленый" => Colors.Green,
                        "Желтый" => Colors.Yellow,
                        "Оранжевый" => Colors.Orange,
                        "Фиолетовый" => Colors.Purple,
                        "Коричневый" => Colors.Brown,
                        "Розовый" => Colors.Pink,
                        "Голубой" => Colors.LightBlue,
                        _ => Colors.Transparent
                    };
                    canvas.CurrentFillColor = color;
                    canvas.UpdateSelectedShapeFillColor(color);
                }
            };
        }
        
        this.KeyDown += (s, e) =>
        {
            if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.Z)
            {
                canvas?.Undo();
                e.Handled = true;
            }
            else if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.Y)
            {
                canvas?.Redo();
                e.Handled = true;
            }
        };
        
        UpdateLayersList();
    }
}
