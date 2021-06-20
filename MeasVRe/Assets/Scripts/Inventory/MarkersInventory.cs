using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEditor;

namespace MeasVRe.Inventory
{
    /// <summary>
    /// Manages of all (selected) markers.
    /// </summary>
    [CreateAssetMenu(fileName = "MarkersInventory", menuName = "MeasVRe/MarkersInventory", order = 0)]
    public class MarkersInventory : ScriptableObject
    {
        [SerializeField]
        [Tooltip("The VisualizationPresets asset that holds prefabs and other data for measurements visualisation.")]
        VisualizationPresets visualizationPresets;

        List<GameObject> m_markers = new List<GameObject>();

        List<GameObject> m_selected = new List<GameObject>();

        /// <summary> List of selected markers. </summary>
        public ReadOnlyCollection<GameObject> selected { get; private set; }

        /// <summary> List of all markers in the scene. </summary>
        public ReadOnlyCollection<GameObject> markers { get; private set; }

        private void OnEnable()
        {
            markers = m_markers.AsReadOnly();
            selected = m_selected.AsReadOnly();
        }

        /// <summary> Add a new marker to the inventory. </summary>
        /// <param name="marker"> The new marker </param>
        public void Add(GameObject marker)
        {
            if (!markers.Contains(marker))
                m_markers.Add(marker);
        }

        /// <summary> Remove the marker from the inventory. </summary>
        /// <param name="marker"> The marker to remove. </param>
        public void Remove(GameObject marker)
        {
            m_markers.Remove(marker);
            m_selected.Remove(marker);
            Destroy(marker);
        }

        /// <summary> Select a marker. </summary>
        /// <param name="marker"> The marker to select. </param>
        public void Select(GameObject marker)
        {
            if (!selected.Contains(marker))
            {
                Add(marker);
                m_selected.Add(marker);
                marker.GetComponent<Renderer>().material.color = visualizationPresets.selectedMarkerColor;
            }
        }

        /// <summary> Unselect a marker. </summary>
        /// <param name="marker"> The marker to unselect. </param>
        public void Unselect(GameObject marker)
        {
            if (m_selected.Remove(marker))
                marker.GetComponent<Renderer>().material.color = visualizationPresets.baseMarkerColor;
        }

        /// <summary> Select all markers in the inventory. </summary>
        public void SelectAll()
        {
            m_selected = new List<GameObject>(m_markers);
            selected = m_selected.AsReadOnly();

            foreach (GameObject marker in m_selected)
            {
                marker.GetComponent<Renderer>().material.color = visualizationPresets.selectedMarkerColor;
            }
        }

        /// <summary> Unselect all markers in the inventory. </summary>
        public void UnselectAll()
        {
            foreach (GameObject marker in selected)
            {
                marker.GetComponent<Renderer>().material.color = visualizationPresets.baseMarkerColor;
            }

            m_selected.Clear();
        }
    }
}
