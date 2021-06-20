using UnityEngine;

namespace MeasVRe
{
    /// <summary>
    /// Contains prefabs, values and materials to use for measurment visualization.
    /// </summary>
    [CreateAssetMenu(fileName = "VisualizationPresets", menuName = "MeasVRe/VisualizationPresets", order = 0)]
    public class VisualizationPresets : ScriptableObject
    {
        [Tooltip("Prefab with a LineRenderer component for drawing lines.")]
        public GameObject linePrefab;

        [Tooltip("Prefab with TextMeshProUGUI components for displaying the measured value")]
        public GameObject labelPrefab;

        [Tooltip("Distance between the label and the visualization of the measurement")]
        public float labelOffset;

        [Tooltip("Color of unselected labels.")]
        public Color baseLabelColor;

        [Tooltip("Color of a selected label.")]
        public Color selectedLabelColor;

        [Header("Angle measurement")]
        [Tooltip("Number of line segments to use for drawing an arc.")]
        public int segments;

        [Tooltip("Radius of the arc that is drawn to visualize an angle.")]
        public float radius;

        [Header("Area measurement")]
        [Tooltip("Material used for the mesh that visualizes the measured area.")]
        public Material areaMaterial;

        [Header("Volume measurement")]
        [Tooltip("Cube prefab used for showing the calculated bounding box.")]
        public GameObject boxPrefab;

        [Header("Markers")]
        [Tooltip("The prefab used for visualizing the markers")]
        public GameObject markerPrefab;

        [Tooltip("Color of unselected markers.")]
        public Color baseMarkerColor;

        [Tooltip("Color of selected markers.")]
        public Color selectedMarkerColor;

        [Tooltip("The prefab used for visualizing the point where the marker will be snapped to")]
        public GameObject snapPreviewPrefab;

        /// <summary> Unit of the measured values. </summary>
        public enum Units { m = 0, cm = 1, mm = 2, nm = 3, km = 4 }

        /// <summary> The unit that will be displayed in the labels. </summary>
        public Units currentUnit { get; protected set; } = Units.m;

        /// <summary> The factor used for scaling the measured values. </summary>
        public float scaleFactor { get; set; } = 1;

        /// <summary> Set the current unit to a Units enum type. </summary>
        /// <param name="val"> The Unit enum type. </param>
        public void SetCurrentUnit(int val)
        {
            currentUnit = (Units)val;
        }
    }
}
