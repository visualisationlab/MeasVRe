using System.Collections.Generic;
using UnityEngine;

namespace MeasVRe
{
    public class BoundingBoxVolume : Measurement<float>
    {
        private Vector3 center, extent, axis1, axis2, axis3;
        public BoundingBoxVolume(List<GameObject> markers, VisualizationPresets presets)
                : base("Volume", markers, presets) { }

        /// <summary>
        /// Calculate the volume using a set of markers by creating a minimum bounding box
        /// around the markers. This bounding box is not axis-aligned.
        /// </summary>
        /// <returns> The volume of the bounding box. </returns>
        public override float CalculateMeasurement()
        {
            float[] points = new float[3 * markers.Count];
            for (int i = 0; i < markers.Count; i++)
            {
                Vector3 markerPos = markers[i].transform.position;
                points[3 * i] = markerPos.x;
                points[3 * i + 1] = markerPos.y;
                points[3 * i + 2] = markerPos.z;
            }

            float[] center = new float[3];
            float[] extent = new float[3];
            float[] axis = new float[9];
            uint numThreads = 4;
            uint lgMaxSample = 3;


            GTE.ComputeMinimumVolumeBoxFromPoints(numThreads, markers.Count, points, lgMaxSample,
                                                  center, axis, extent, out float volume);

            this.center = new Vector3(center[0], center[1], center[2]);
            this.extent = new Vector3(extent[0], extent[1], extent[2]);
            axis1 = new Vector3(axis[0], axis[1], axis[2]);
            axis2 = new Vector3(axis[3], axis[4], axis[5]);
            axis3 = new Vector3(axis[6], axis[7], axis[8]);

            return volume * Mathf.Pow(presets.scaleFactor, 3);
        }

        /// <summary>
        /// Places a box in the scene that represents the bounding box and places a label
        /// outside the box aligned with one of the faces.
        /// </summary>
        public override void VisualizeMeasurement()
        {
            GameObject volumeBox = Object.Instantiate(presets.boxPrefab, center,
                                                      Quaternion.LookRotation(axis1, axis2));
            volumeBox.transform.localScale = new Vector3(2.0f * extent[2], 2.0f * extent[1],
                                                         2.0f * extent[0]);
            visualizationObjects.Add("box", volumeBox);

            Vector3 labelPos = center + (extent.x + presets.labelOffset) * axis1.normalized;
            Quaternion labelRot = Quaternion.LookRotation(axis2, Vector3.up);
            string labelText = "<b>Volume</b>\n" + value + " " + presets.currentUnit.ToString() + "<sup>3</sup>";
            visualizationObjects.Add("label", VisualizationUtils.AddLabel(presets.labelPrefab, labelText, labelPos, labelRot));
        }
    }
}
