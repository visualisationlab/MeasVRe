using UnityEngine;
using System.Collections.Generic;

namespace MeasVRe
{
    public class Distance : Measurement<float>
    {
        public Distance(List<GameObject> markers, VisualizationPresets presets)
                       : base("Distance", markers, presets) { }

        /// <summary>
        /// Calculate the distance between the first two markers in the markers list.
        /// </summary>
        /// <returns> The calculated distance. </returns>
        public override float CalculateMeasurement()
        {
            return Vector3.Distance(markers[0].transform.position, markers[1].transform.position) * presets.scaleFactor;
        }

        /// <summary>
        /// Draws the a line between the two points the distance was taken and places a label
        /// that aligns with this line.
        /// </summary>
        public override void VisualizeMeasurement()
        {
            visualizationObjects.Add("line", VisualizationUtils.DrawLine(presets.linePrefab,
                                                                         markers[0].transform.position,
                                                                         markers[1].transform.position));

            Vector3 labelPos = (markers[0].transform.position + markers[1].transform.position) / 2;
            labelPos -= VisualizationUtils.GetCameraDirection() * presets.labelOffset;
            Quaternion labelRot = Quaternion.FromToRotation(presets.labelPrefab.transform.right,
                                                            markers[0].transform.position -
                                                            markers[1].transform.position);
            string labelText = "<b>Distance</b>\n" + value.ToString() + " " + presets.currentUnit.ToString();
            visualizationObjects.Add("label", VisualizationUtils.AddLabel(presets.labelPrefab, labelText, labelPos,
                                                                          labelRot));
        }
    }
}
