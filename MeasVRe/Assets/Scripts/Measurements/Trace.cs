using System.Collections.Generic;
using UnityEngine;

namespace MeasVRe
{
    public class Trace : Measurement<float>
    {
        public Trace(List<GameObject> markers, VisualizationPresets visualizationPresets)
                : base("Trace", markers, visualizationPresets) { }

        /// <summary> Calculate the length of a line through the list of markers. </summary>
        /// <returns> The calculated value. </returns>
        public override float CalculateMeasurement()
        {
            float length = 0.0f;

            for (int i = 1; i < markers.Count; i++)
            {
                length += Vector3.Distance(markers[i].transform.position,
                                           markers[i - 1].transform.position);
            }

            return length * presets.scaleFactor;
        }

        /// <summary>
        /// Draws a line through the list of markers and places a label facing the camera
        /// near the first marker.
        /// </summary>
        public override void VisualizeMeasurement()
        {
            visualizationObjects.Add("line", VisualizationUtils.DrawLine(presets.linePrefab, markers));

            Vector3 forward = VisualizationUtils.GetCameraDirection();
            Vector3 labelPos = markers[0].transform.position - forward * presets.labelOffset;
            Quaternion labelRot = Quaternion.LookRotation(-forward, Vector3.up);
            visualizationObjects.Add(
                "label",
                VisualizationUtils.AddLabel(presets.labelPrefab,
                                            "<b>Trace</b>\n" + value.ToString() + " " + presets.currentUnit.ToString(),
                                            labelPos, labelRot)
            );
        }
    }
}
