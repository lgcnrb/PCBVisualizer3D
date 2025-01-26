// ViewModel for binding data between the UI and the MainWindow logic.
// Implements INotifyPropertyChanged to enable two-way data binding in WPF.
using System.ComponentModel;

public class MainWindowViewModel : INotifyPropertyChanged
{
    // Backing field for the CameraPosition property.
    // Represents the current position of the camera in the 3D scene.
    private string _cameraPosition = "X: 0, Y: 0, Z: 500";

    /// <summary>
    /// Gets or sets the camera position in the 3D view.
    /// Updates the UI whenever the position changes.
    /// </summary>
    public string CameraPosition
    {
        get => _cameraPosition;
        set
        {
            _cameraPosition = value;
            OnPropertyChanged(nameof(CameraPosition));
        }
    }

    // Backing field for the ZoomLevel property.
    // Represents the zoom level of the camera in the 3D scene as a percentage.
    private string _zoomLevel = "100%";

    /// <summary>
    /// Gets or sets the zoom level of the camera.
    /// Updates the UI whenever the zoom level changes.
    /// </summary>
    public string ZoomLevel
    {
        get => _zoomLevel;
        set
        {
            _zoomLevel = value;
            OnPropertyChanged(nameof(ZoomLevel));
        }
    }

    // Backing field for the MousePosition property.
    // Represents the current mouse position in the PCB's coordinate system.
    private string _mousePosition = "X: 0, Y: 0";

    /// <summary>
    /// Gets or sets the mouse position on the PCB in 2D space.
    /// Updates the UI whenever the mouse position changes.
    /// </summary>
    public string MousePosition
    {
        get => _mousePosition;
        set
        {
            _mousePosition = value;
            OnPropertyChanged(nameof(MousePosition));
        }
    }

    /// <summary>
    /// Event that triggers when a property value changes.
    /// Required for WPF data binding to notify the UI about updates.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Helper method to raise the PropertyChanged event for a given property name.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
