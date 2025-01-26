using System.Data;
using System;
using System.IO;
using System.Windows;
using HelixToolkit.Wpf;
using Newtonsoft.Json;
using System.Windows.Media.Media3D;
using Microsoft.Win32;
using PCBVisualizer3D.Models;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Input;
using ClosedXML.Excel;

namespace PCBVisualizer3D.Views
{
    public partial class MainWindow : Window
    {
        // ---------------------------
        // Fields and Properties
        // ---------------------------
        private readonly Dictionary<GeometryModel3D, string> _modelInfo = new Dictionary<GeometryModel3D, string>();
        private PCBModel _currentPCBModel;
        private GeometryModel3D _selectedComponent = null;
        private bool _isAnimating = false; 
        private readonly MainWindowViewModel _viewModel;
        private Dictionary<GeometryModel3D, Material> _originalMaterials = new Dictionary<GeometryModel3D, Material>();
        private GeometryModel3D _pcbGeometry;

        // Default colors for PCB and components
        private Color _pcbColor = Colors.ForestGreen; // Daha zengin bir yeşil
        private readonly Dictionary<string, Color> _componentColorMap = new Dictionary<string, Color>
        {
            { "Capacitor", Colors.Orange },
            { "Resistor", Colors.Blue },
            { "Inductor", Colors.Purple },
            { "Diode", Colors.Red },
            { "Transistor", Colors.Black },
            { "IC", Colors.Gray },
            { "Connector", Colors.WhiteSmoke },
            { "LED", Colors.Yellow },
            { "CrystalOscillator", Colors.LightBlue },
            { "Switch", Colors.Brown },
            { "Socket", Colors.LightGreen },
            { "Varistor", Colors.Cyan },
            { "Harness", Colors.Pink }
        };

        public enum ComponentType
        {
            Capacitor,           // Kondansatör
            Resistor,            // Direnç
            Inductor,            // Endüktör
            Diode,               // Diyot
            Transistor,          // Transistör
            IC,                  // Entegre devre
            Connector,           // Konnektör
            LED,                 // LED
            CrystalOscillator,   // Kristal Osilatör
            Switch,              // Anahtar
            Socket,              // Soket
            Varistor,            // Varistör
            Harness              // Kablo demeti
        }

        public MainWindow()
        {
            InitializeComponent();
            // Initialize ViewModel and DataContext
            _viewModel = new MainWindowViewModel();
            DataContext = _viewModel;

            // Subscribe to viewport events
            helixViewport.CameraChanged += HelixViewport_CameraChanged;
            // Fare hareketlerini izlemek için
            helixViewport.MouseMove += HelixViewport_MouseMove;
        }

        // ---------------------------
        // Event Handlers
        // ---------------------------

        /// <summary>
        /// Handles mouse movement in the viewport to update mouse position.
        /// </summary>
        private void HelixViewport_MouseMove(object sender, MouseEventArgs e)
        {
            // Fare pozisyonunu al
            var position = e.GetPosition(helixViewport);

            // RayHitTest işlemi yap
            var hitTestParams = new PointHitTestParameters(position);
            VisualTreeHelper.HitTest(helixViewport, null, ResultCallback, hitTestParams);
        }

        private HitTestResultBehavior ResultCallback(HitTestResult result)
        {
            if (result is RayMeshGeometry3DHitTestResult meshResult)
            {
                // Mesh üzerinde koordinatları al
                var point = meshResult.PointHit;

                // ViewModel'e gönder
                _viewModel.MousePosition = $"X: {point.X:F2}, Y: {point.Y:F2}";
            }

            return HitTestResultBehavior.Stop;
        }

        /// <summary>
        /// Handles the camera position and zoom level updates when the camera changes.
        /// </summary>
        private void HelixViewport_CameraChanged(object sender, RoutedEventArgs e)
        {
            if (helixViewport.Camera is PerspectiveCamera camera)
            {
                // Kamera pozisyonunu ViewModel'e güncelle
                _viewModel.CameraPosition = $"X: {camera.Position.X:F2}, Y: {camera.Position.Y:F2}, Z: {camera.Position.Z:F2}";

                // Mesafeyi manuel olarak hesapla (vektörün büyüklüğü)
                double distance = Math.Sqrt(
                    camera.Position.X * camera.Position.X +
                    camera.Position.Y * camera.Position.Y +
                    camera.Position.Z * camera.Position.Z
                );

                // Zoom seviyesini güncelle
                _viewModel.ZoomLevel = $"{(500 / distance * 100):F0}%";
            }
        }

        private void MenuLoadPCB_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Select a PCB JSON File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                LoadPCB(filePath);
            }
        }

        private void ButtonZoomIn_Click(object sender, RoutedEventArgs e)
        {
            if (helixViewport.Camera is PerspectiveCamera camera)
            {
                var lookDirection = camera.LookDirection;
                camera.Position += lookDirection * 0.1;
            }
        }

        private void ButtonZoomOut_Click(object sender, RoutedEventArgs e)
        {
            if (helixViewport.Camera is PerspectiveCamera camera)
            {
                var lookDirection = camera.LookDirection;
                camera.Position -= lookDirection * 0.1;
            }
        }

        private void ButtonResetView_Click(object sender, RoutedEventArgs e)
        {
            CenterCameraOnPCB();
        }

        private void ButtonLoadPCB_Click(object sender, RoutedEventArgs e)
        {
            MenuLoadPCB_Click(sender, e);
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("PCB Visualizer 3D\nVersion 1.0", "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ---------------------------
        // Helper Methods
        // ---------------------------

        /// <summary>
        /// Centers the camera on the PCB model.
        /// </summary>
        private void CenterCameraOnPCB()
        {
            if (_currentPCBModel == null || !(helixViewport.Camera is PerspectiveCamera camera))
            {
                return;
            }

            double centerX = _currentPCBModel.Dimensions.Width / 2.0;
            double centerY = _currentPCBModel.Dimensions.Height / 2.0;
            double centerZ = _currentPCBModel.Dimensions.Thickness / 2.0;

            double offsetMultiplier = 2.0;
            double distanceZ = Math.Max(_currentPCBModel.Dimensions.Width, _currentPCBModel.Dimensions.Height) * offsetMultiplier;

            camera.Position = new Point3D(centerX, centerY, distanceZ);
            camera.LookDirection = new Vector3D(0, 0, -distanceZ);
            camera.UpDirection = new Vector3D(0, 1, 0);

            camera.FieldOfView = 45;

            helixViewport.ZoomExtents();
        }

        // ---------------------------
        // PCB and Components Rendering
        // ---------------------------

        /// <summary>
        /// Loads and visualizes a PCB from a JSON file.
        /// </summary>
        private void LoadPCB(string filePath)
        {
            try
            {
                string jsonContent = File.ReadAllText(filePath);
                _currentPCBModel = JsonConvert.DeserializeObject<PCBModel>(jsonContent);

                if (_currentPCBModel != null)
                {
                    DrawPCBWithComponents(_currentPCBModel);
                    CenterCameraOnPCB();
                }
                else
                {
                    MessageBox.Show("Invalid JSON format.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing JSON file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddLights()
        {
            var ambientLight = new AmbientLight { Color = Colors.Gray };
            helixViewport.Children.Add(new ModelVisual3D { Content = ambientLight });

            var directionalLightTop = new DirectionalLight
            {
                Color = Colors.White,
                Direction = new Vector3D(-1, -1, -1)
            };
            helixViewport.Children.Add(new ModelVisual3D { Content = directionalLightTop });

            var directionalLightBottom = new DirectionalLight
            {
                Color = Colors.White,
                Direction = new Vector3D(1, 1, 1)
            };
            helixViewport.Children.Add(new ModelVisual3D { Content = directionalLightBottom });
        }

        private void DrawCoordinateSystem()
        {
            var meshBuilder = new MeshBuilder();

            // X Axis
            meshBuilder.AddCylinder(new Point3D(0, 0, 0), new Point3D(100, 0, 0), 1, 10);
            var xAxis = new GeometryModel3D
            {
                Geometry = meshBuilder.ToMesh(),
                Material = MaterialHelper.CreateMaterial(new SolidColorBrush(Colors.Red))
            };
            helixViewport.Children.Add(new ModelVisual3D { Content = xAxis });

            // Y Axis
            meshBuilder = new MeshBuilder();
            meshBuilder.AddCylinder(new Point3D(0, 0, 0), new Point3D(0, 100, 0), 1, 10);
            var yAxis = new GeometryModel3D
            {
                Geometry = meshBuilder.ToMesh(),
                Material = MaterialHelper.CreateMaterial(new SolidColorBrush(Colors.Green))
            };
            helixViewport.Children.Add(new ModelVisual3D { Content = yAxis });

            // Z Axis
            meshBuilder = new MeshBuilder();
            meshBuilder.AddCylinder(new Point3D(0, 0, 0), new Point3D(0, 0, 100), 1, 10);
            var zAxis = new GeometryModel3D
            {
                Geometry = meshBuilder.ToMesh(),
                Material = MaterialHelper.CreateMaterial(new SolidColorBrush(Colors.Blue))
            };
            helixViewport.Children.Add(new ModelVisual3D { Content = zAxis });
        }
        /// <summary>
        /// Draws the PCB and its components.
        /// </summary>
        private void DrawPCBWithComponents(PCBModel pcbModel)
        {
            helixViewport.Children.Clear();
            AddLights();
            DrawCoordinateSystem();

            var meshBuilder = new MeshBuilder();
            meshBuilder.AddBox(
                new Point3D(pcbModel.Dimensions.Width / 2,
                            pcbModel.Dimensions.Height / 2,
                            pcbModel.Dimensions.Thickness / 2),
                pcbModel.Dimensions.Width,
                pcbModel.Dimensions.Height,
                pcbModel.Dimensions.Thickness
            );

            _pcbGeometry = new GeometryModel3D
            {
                Geometry = meshBuilder.ToMesh(),
                Material = MaterialHelper.CreateMaterial(
                    new SolidColorBrush(_pcbColor),
                    specularPower: 300
                )
            };

            helixViewport.Children.Add(new ModelVisual3D { Content = _pcbGeometry });

            foreach (var component in pcbModel.Components)
            {
                if (component.Face == "Top")
                {
                    DrawComponent(component, true);
                }
                else if (component.Face == "Bottom")
                {
                    DrawComponent(component, false);
                }
            }
        }
        /// <summary>
        /// Draws a specific component on the PCB.
        /// </summary>
        private void DrawComponent(Component component, bool isTopFace)
        {
            var meshBuilder = new MeshBuilder();

            double zPosition = isTopFace ? component.Z : -component.Z;
            meshBuilder.AddBox(
                new Point3D(component.X, component.Y, zPosition),
                component.Dimensions.Width,
                component.Dimensions.Height,
                component.Dimensions.Thickness
            );

            Color componentColor = _componentColorMap.ContainsKey(component.Type)
                ? _componentColorMap[component.Type]
                : Colors.LightGray;

            var material = MaterialHelper.CreateMaterial(new SolidColorBrush(componentColor), specularPower: 200);
            var componentGeometry = new GeometryModel3D
            {
                Geometry = meshBuilder.ToMesh(),
                Material = material
            };

            _originalMaterials[componentGeometry] = material;

            helixViewport.Children.Add(new ModelVisual3D { Content = componentGeometry });

            _modelInfo[componentGeometry] =
                $"Location: {component.Location}\n" +
                $"Type: {component.Type}\n" +
                $"X={component.X}, Y={component.Y} Rotation={component.Rotation}\n" +
                $"Face: {component.Face}\n" +
                $"KDTEC P/N: {component.KDTECPN}\n" +
                $"Customer P/N: {component.CustomerPN}\n" +
                $"Maker P/N: {component.MakerPN}\n" +
                $"Description: {component.Description}\n" +
                $"MakerName: {component.MakerName}\n" +
                $"Process: {component.Process}\n" +
                $"Dimensions: {component.Dimensions.Width}x{component.Dimensions.Height}x{component.Dimensions.Thickness}";
        }

        // ---------------------------
        // Interaction Methods
        // ---------------------------

        /// <summary>
        /// Handles mouse clicks to select a component on the PCB.
        /// </summary>
        private void HelixViewport_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var hitTestResult = VisualTreeHelper.HitTest(helixViewport, e.GetPosition(helixViewport))
                                 as RayMeshGeometry3DHitTestResult;

            if (hitTestResult != null)
            {
                var model = hitTestResult.ModelHit as GeometryModel3D;
                if (model != null && _modelInfo.ContainsKey(model))
                {
                    if (_selectedComponent != null && _selectedComponent != model)
                    {
                        StopHighlightAnimation(_selectedComponent);
                    }

                    _selectedComponent = model;
                    StartHighlightAnimation(model);

                    UpdateTextBlocks(_modelInfo[model]);
                }
            }
            else
            {
                if (_selectedComponent != null)
                {
                    StopHighlightAnimation(_selectedComponent);
                    ClearTextBlocks();
                    _selectedComponent = null;
                }
            }
        }

        /// <summary>
        /// Starts a highlight animation on the selected 3D model. Adds an emissive effect for visual feedback.
        /// </summary>
        /// <param name="model">The 3D geometry model to highlight.</param>
        private void StartHighlightAnimation(GeometryModel3D model)
        {
            if (!_originalMaterials.TryGetValue(model, out var originalMaterial))
                return;

            var group = new MaterialGroup();
            group.Children.Add(originalMaterial);

            var emissiveBrush = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
            var emissiveMat = new EmissiveMaterial(emissiveBrush);
            group.Children.Add(emissiveMat);

            model.Material = group;

            var colorAnimation = new ColorAnimation
            {
                From = Color.FromArgb(0, 255, 255, 255),
                To = Color.FromArgb(128, 255, 255, 255),
                Duration = TimeSpan.FromMilliseconds(800),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            emissiveBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
        }

        /// <summary>
        /// Stops the highlight animation on the selected 3D model and restores its original material.
        /// </summary>
        /// <param name="model">The 3D geometry model to restore.</param>
        private void StopHighlightAnimation(GeometryModel3D model)
        {
            if (model == null) return;

            if (model.Material is MaterialGroup group)
            {
                var emissiveMat = group.Children.OfType<EmissiveMaterial>().FirstOrDefault();
                if (emissiveMat?.Brush is SolidColorBrush scb)
                {
                    scb.BeginAnimation(SolidColorBrush.ColorProperty, null);
                }
            }

            if (_originalMaterials.ContainsKey(model))
            {
                model.Material = _originalMaterials[model];
            }
            else
            {
                model.Material = MaterialHelper.CreateMaterial(Colors.LightGray);
            }

            _isAnimating = false;
        }

        /// <summary>
        /// Updates the text blocks in the UI with the provided component information.
        /// </summary>
        /// <param name="componentInfo">Formatted string containing component details.</param>
        private void UpdateTextBlocks(string componentInfo)
        {
            var lines = componentInfo.Split('\n');
            TxtComponentLocation.Text = lines.FirstOrDefault(l => l.StartsWith("Location:"))?.Split(':')[1]?.Trim() ?? "--";
            TxtComponentType.Text = lines.FirstOrDefault(l => l.StartsWith("Type:"))?.Split(':')[1]?.Trim() ?? "--";
            TxtComponentInfo.Text = lines.FirstOrDefault(l => l.StartsWith("X="))?.Trim() ?? "--";
            TxtComponentFace.Text = lines.FirstOrDefault(l => l.StartsWith("Face:"))?.Split(':')[1]?.Trim() ?? "--";
            TxtComponentKDTEC.Text = lines.FirstOrDefault(l => l.StartsWith("KDTEC P/N:"))?.Split(':')[1]?.Trim() ?? "--";
            TxtComponentCustomer.Text = lines.FirstOrDefault(l => l.StartsWith("Customer P/N:"))?.Split(':')[1]?.Trim() ?? "--";
            TxtComponentMaker.Text = lines.FirstOrDefault(l => l.StartsWith("Maker P/N:"))?.Split(':')[1]?.Trim() ?? "--";
            TxtDescription.Text = lines.FirstOrDefault(l => l.StartsWith("Description:"))?.Split(':')[1]?.Trim() ?? "--";
            TxtMakerName.Text = lines.FirstOrDefault(l => l.StartsWith("MakerName:"))?.Split(':')[1]?.Trim() ?? "--";
            TxtProcess.Text = lines.FirstOrDefault(l => l.StartsWith("Process:"))?.Split(':')[1]?.Trim() ?? "--";
            TxtDimensions.Text = lines.FirstOrDefault(l => l.StartsWith("Dimensions:"))?.Split(':')[1]?.Trim() ?? "--";
        }

        /// <summary>
        /// Clears all text blocks, resetting their content to default ("--").
        /// </summary>
        private void ClearTextBlocks()
        {
            TxtComponentLocation.Text = "--";
            TxtComponentType.Text = "--";
            TxtComponentInfo.Text = "--";
            TxtComponentFace.Text = "--";
            TxtComponentKDTEC.Text = "--";
            TxtComponentCustomer.Text = "--";
            TxtComponentMaker.Text = "--";
            TxtDescription.Text = "--";
            TxtMakerName.Text = "--";
            TxtProcess.Text = "--";
            TxtDimensions.Text = "--";
        }

        /// <summary>
        /// Applies X, Y, and Z offsets to all components and redraws them.
        /// </summary>
        private void ApplyOffset_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPCBModel == null) return;

            double.TryParse(XOffsetBox.Text, out double xOffset);
            double.TryParse(YOffsetBox.Text, out double yOffset);
            double.TryParse(ZOffsetBox.Text, out double zOffset);

            DrawComponentsWithOffset(_currentPCBModel.Components, xOffset, yOffset, zOffset);
        }

        /// <summary>
        /// Redraws components with the given offsets applied to their positions.
        /// </summary>
        private void DrawComponentsWithOffset(List<Component> components,
                                              double xOffset, double yOffset, double zOffset)
        {
            var componentVisuals = helixViewport.Children
                .OfType<ModelVisual3D>()
                .Where(m => m.Content is GeometryModel3D geometry && _modelInfo.ContainsKey(geometry))
                .ToList();

            foreach (var visual in componentVisuals)
            {
                helixViewport.Children.Remove(visual);
            }

            foreach (var component in components)
            {
                var meshBuilder = new MeshBuilder();
                meshBuilder.AddBox(
                    new Point3D(component.X + xOffset,
                                component.Y + yOffset,
                                component.Z + zOffset),
                    component.Dimensions.Width,
                    component.Dimensions.Height,
                    component.Dimensions.Thickness
                );

                Color componentColor = _componentColorMap.ContainsKey(component.Type)
                    ? _componentColorMap[component.Type]
                    : Colors.LightGray;

                var componentGeometry = new GeometryModel3D
                {
                    Geometry = meshBuilder.ToMesh(),
                    Material = MaterialHelper.CreateMaterial(new SolidColorBrush(componentColor), specularPower: 200)
                };

                if (componentGeometry.Geometry != null)
                {
                    _modelInfo[componentGeometry] =
                        $"Location: {component.Location}\n" +
                        $"Type: {component.Type}\n" +
                        $"X={component.X + xOffset}, Y={component.Y + yOffset}, Z={component.Z + zOffset}, Rotation={component.Rotation}\n" +
                        $"Face: {component.Face}\n" +
                        $"KDTEC P/N: {component.KDTECPN}\n" +
                        $"Customer P/N: {component.CustomerPN}\n" +
                        $"Maker P/N: {component.MakerPN}\n" +
                        $"Description: {component.Description}\n" +
                        $"MakerName: {component.MakerName}\n" +
                        $"Process: {component.Process}\n" +
                        $"Dimensions: {component.Dimensions.Width}x{component.Dimensions.Height}x{component.Dimensions.Thickness}";

                    helixViewport.Children.Add(new ModelVisual3D { Content = componentGeometry });
                }
            }
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://selimbirincioglu.bio.link/";
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open link: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonResetSelection_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedComponent != null)
            {
                StopHighlightAnimation(_selectedComponent);
                _selectedComponent = null;
            }
            ClearTextBlocks();
        }

        private void ToggleInfoPanel_Click(object sender, RoutedEventArgs e)
        {
            if (InfoPanelBorder.Visibility == Visibility.Visible)
            {
                InfoPanelBorder.Visibility = Visibility.Collapsed;
            }
            else
            {
                InfoPanelBorder.Visibility = Visibility.Visible;
            }
        }
        private void FileButton_Click(object sender, RoutedEventArgs e)
        {
            FilePopup.PlacementTarget = (UIElement)sender;
            FilePopup.IsOpen = !FilePopup.IsOpen;

        }

        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            ViewPopup.PlacementTarget = (UIElement)sender;
            ViewPopup.IsOpen = !ViewPopup.IsOpen;
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            HelpPopup.PlacementTarget = (UIElement)sender;
            HelpPopup.IsOpen = !HelpPopup.IsOpen;
        }
        private void ToolsButton_Click(object sender, RoutedEventArgs e)
        {
            ToolsPopup.PlacementTarget = (UIElement)sender;
            ToolsPopup.IsOpen = !HelpPopup.IsOpen;
        }

        /// <summary>
        /// Handles the Convert Excel to JSON button click event.
        /// </summary>
        private void ConvertExcelToJsonButton_Click(object sender, RoutedEventArgs e)
        {
            // Open File Dialog to select an Excel file
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel files (*.xlsx;*.xls)|*.xlsx;*.xls|All files (*.*)|*.*",
                Title = "Select Excel File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string excelFilePath = openFileDialog.FileName;

                // Convert Excel file to JSON
                string jsonContent;
                try
                {
                    jsonContent = ConvertExcelToJson(excelFilePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error converting Excel to JSON: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Save JSON using SaveFileDialog
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    Title = "Save JSON File",
                    FileName = "output.json"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string jsonFilePath = saveFileDialog.FileName;

                    try
                    {
                        File.WriteAllText(jsonFilePath, jsonContent);
                        MessageBox.Show("Excel successfully converted to JSON and saved!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving JSON file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Converts an Excel file to a JSON string matching the PCBModel format.
        /// </summary>
        /// <param name="filePath">Path to the Excel file</param>
        /// <returns>A JSON string in the desired format</returns>
        private string ConvertExcelToJson(string filePath)
        {
            using (var workbook = new XLWorkbook(filePath))
            {
                // Read PCB metadata from "Pcb_Data" sheet
                var pcbSheet = workbook.Worksheet("Pcb_Data");
                if (pcbSheet == null) throw new Exception("Sheet 'Pcb_Data' not found.");

                var pcbModel = new PCBModel
                {
                    Name = pcbSheet.Cell("A2").GetString(),
                    Dimensions = new Dimensions
                    {
                        Width = (float)pcbSheet.Cell("B2").GetDouble(),
                        Height = (float)pcbSheet.Cell("C2").GetDouble(),
                        Thickness = (float)pcbSheet.Cell("D2").GetDouble()
                    },
                    Components = new List<Component>()
                };

                // Read components from "Components" sheet
                var componentSheet = workbook.Worksheet("Components");
                if (componentSheet == null) throw new Exception("Sheet 'Components' not found.");

                var rows = componentSheet.RowsUsed().Skip(1); // Skip the header row
                foreach (var row in rows)
                {
                    var component = new Component
                    {
                        Location = row.Cell(1).GetString(),
                        Type = row.Cell(2).GetString(),
                        X = (float)row.Cell(3).GetDouble(),
                        Y = (float)row.Cell(4).GetDouble(),
                        Z = (float)row.Cell(5).GetDouble(),
                        Rotation = row.Cell(6).GetValue<int>(),
                        Face = row.Cell(7).GetString(),
                        KDTECPN = row.Cell(8).GetString(),
                        CustomerPN = row.Cell(9).GetString(),
                        MakerPN = row.Cell(10).GetString(),
                        Description = row.Cell(11).GetString(),
                        MakerName = row.Cell(12).GetString(),
                        Process = row.Cell(13).GetString(),
                        Dimensions = new Dimensions
                        {
                            Width = (float)row.Cell(14).GetDouble(),
                            Height = (float)row.Cell(15).GetDouble(),
                            Thickness = (float)row.Cell(16).GetDouble()
                        }
                    };

                    pcbModel.Components.Add(component);
                }

                // Convert PCBModel to JSON
                return JsonConvert.SerializeObject(pcbModel, Formatting.Indented);
            }
        }

    }
}
