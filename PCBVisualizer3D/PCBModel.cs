using System.Collections.Generic;

namespace PCBVisualizer3D.Models
{
    /// <summary>
    /// Represents the PCB (Printed Circuit Board) model, including its name, dimensions, and components.
    /// </summary>
    public class PCBModel
    {
        /// <summary>
        /// Name of the PCB model.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Dimensions of the PCB.
        /// </summary>
        public Dimensions Dimensions { get; set; }

        /// <summary>
        /// List of components included in the PCB.
        /// </summary>
        public List<Component> Components { get; set; }
    }

    /// <summary>
    /// Represents the dimensions of a PCB or a component (e.g., width, height, thickness).
    /// </summary>
    public class Dimensions
    {
        /// <summary>
        /// The width of the PCB or component in millimeters.
        /// </summary>
        public float Width { get; set; }

        /// <summary>
        /// The height of the PCB or component in millimeters.
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        /// The thickness of the PCB or component in millimeters (used for 3D visualization).
        /// </summary>
        public float Thickness { get; set; }
    }

    /// <summary>
    /// Represents an individual electronic component on the PCB.
    /// </summary>
    public class Component
    {
        /// <summary>
        /// Location identifier for the component (e.g., label or position index).
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Type/category of the component (e.g., Resistor, Capacitor, Diode, etc.).
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// X-axis coordinate of the component on the PCB in millimeters.
        /// </summary>
        public float X { get; set; }

        /// <summary>
        /// Y-axis coordinate of the component on the PCB in millimeters.
        /// </summary>
        public float Y { get; set; }

        /// <summary>
        /// Z-axis coordinate (depth) of the component in millimeters (used for 3D visualization).
        /// </summary>
        public float Z { get; set; }

        /// <summary>
        /// Rotation angle of the component in degrees.
        /// </summary>
        public int Rotation { get; set; }

        /// <summary>
        /// The face of the PCB where the component is mounted (e.g., "Top" or "Bottom").
        /// </summary>
        public string Face { get; set; }

        /// <summary>
        /// KDTEC part number for the component.
        /// </summary>
        public string KDTECPN { get; set; }

        /// <summary>
        /// Customer-specific part number for the component.
        /// </summary>
        public string CustomerPN { get; set; }

        /// <summary>
        /// Manufacturer's part number for the component.
        /// </summary>
        public string MakerPN { get; set; }

        /// <summary>
        /// Description of the component.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Name of the component's manufacturer.
        /// </summary>
        public string MakerName { get; set; }

        /// <summary>
        /// Specific manufacturing process for the component (e.g., soldering type).
        /// </summary>
        public string Process { get; set; }

        /// <summary>
        /// Dimensions of the component (e.g., length, width, height).
        /// </summary>
        public Dimensions Dimensions { get; set; }
    }
}
