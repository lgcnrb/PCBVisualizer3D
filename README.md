# PCBVisualizer3D

**PCBVisualizer3D** is a 3D visualization and analysis tool for Printed Circuit Boards (PCBs). It enables users to load PCB data, visualize the board and its components in 3D, and perform various interactions like inspecting individual components, applying offsets, and exporting the design data.

---

## ✨ Features

- **3D Visualization**: Render PCB boards and components in an interactive 3D environment.
- **Data Input**:
  - Load PCB data directly from JSON files.
  - Convert Excel files to JSON for visualization.
- **Component Analysis**: Highlight and inspect individual components with detailed information (type, position, dimensions, and more).
- **Interactive Controls**:
  - Rotate, zoom, and pan the 3D view.
  - Toggle component visibility and highlight features.
- **Real-Time Editing**: Apply X-Y-Z offsets to component positions and update the visualization dynamically.
- **Data Export**: Save PCB data in JSON format for further processing or sharing.

---

## 🔧 Getting Started

### Prerequisites

- **Operating System**: Windows 10 or later
- **Development Environment**: Visual Studio 2022 or later
- **Framework**: .NET Framework 4.8 or higher
- **Dependencies**:
  - [HelixToolkit.Wpf](https://github.com/helix-toolkit/helix-toolkit) (3D visualization)
  - [ClosedXML](https://github.com/ClosedXML/ClosedXML) (Excel file handling)
  - [Newtonsoft.Json](https://www.newtonsoft.com/json) (JSON processing)

---

## 🚀 Installation

### Cloning the Repository

```bash
git clone https://github.com/lgcnrb/PCBVisualizer3D.git
cd PCBVisualizer3D
```

### Setting Up the Project

1. Open the solution file `PCBVisualizer3D.sln` in Visual Studio.
2. Restore required NuGet packages:
   - In Visual Studio: Use **Tools > NuGet Package Manager > Manage NuGet Packages**.
   - Alternatively, use:
     ```bash
     dotnet restore
     ```
3. Build and run the project.

---

## 🐝 How to Use

### Main Features

1. **Loading Data**:
   - Use the "Load PCB" feature to open a JSON file with PCB data.
   - Alternatively, use "Convert Excel to JSON" to load PCB data from an Excel file and convert it to JSON.

2. **Exploring the Board**:
   - Rotate the view by dragging the mouse.
   - Zoom in and out using the mouse scroll wheel.
   - Pan the view by holding the middle mouse button and dragging.

3. **Inspecting Components**:
   - Click on a component to view detailed information such as its type, location, and dimensions.
   - Highlighted components can be dynamically modified (e.g., repositioned).

4. **Saving Data**:
   - Export modified PCB data to a JSON file using the "Save" feature.

---

## 📂 File Structure

- **`PCBVisualizer3D/`**: Main application source code.
  - **`Models/`**: Classes defining PCB structure (`PCBModel.cs`, `Component.cs`, etc.).
  - **`Views/`**: XAML files for the application's UI.
  - **`ViewModels/`**: Logic for binding data to the UI.
- **`pcb_example.xlsx`**: Example Excel file demonstrating the required data structure.
- **`PCBVisualizer3D.sln`**: Visual Studio solution file.
- **`packages/`**: Dependencies restored via NuGet.

---

## 🖍 Example File Formats

### Excel Format (`pcb_example.xlsx`)

The Excel file should have two sheets:
1. **Pcb_Data**: Contains general PCB information.
   | Name         | Width  | Height | Thickness |
   |--------------|--------|--------|-----------|
   | Sample PCB   | 280    | 190    | 1.6       |

2. **Components**: Contains details of each component.
   | Location | Type       | X      | Y      | Z   | Rotation | Face | KDTECPN | CustomerPN | MakerPN | Description            | MakerName                | Process | Width | Height | Thickness |
   |----------|------------|--------|--------|-----|----------|------|---------|------------|--------|------------------------|--------------------------|---------|-------|--------|-----------|
   | C1       | Capacitor  | 40.125 | 135.375 | 1.6 | 90       | Top  | EDA...  | 4DE...     | LE105  | C C-FILM 1.0uF 310V K | OKAYA ELECTRIC INDUSTRIES | MI      | 1     | 1      | 2         |

### JSON Format

The resulting JSON format will look like this:
```json
{
  "Name": "Sample PCB",
  "Dimensions": {
    "Width": 280,
    "Height": 190,
    "Thickness": 1.6
  },
  "Components": [
    {
      "Location": "C1",
      "Type": "Capacitor",
      "X": 40.125,
      "Y": 135.375,
      "Z": 1.6,
      "Rotation": 90,
      "Face": "Top",
      "KDTECPN": "EDA4DE63095L9105P",
      "CustomerPN": "4DE63095L9105P",
      "MakerPN": "LE105-MX-C",
      "Description": "C C-FILM 1.0uF 310V K",
      "MakerName": "OKAYA ELECTRIC INDUSTRIES",
      "Process": "MI",
      "Dimensions": {
        "Width": 1,
        "Height": 1,
        "Thickness": 2
      }
    }
  ]
}
```

---

## 🗋 License

This project is licensed under the **MIT License**. See the [LICENSE](LICENSE) file for details.

---

## 🙋 Support

If you encounter any issues, feel free to create a GitHub issue or contact the maintainer:

- **Author:** Selim Birincioğlu  
- **Email:** selimbirincioglu@hotmail.com
