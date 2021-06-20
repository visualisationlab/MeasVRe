using System.Collections.Generic;
using UnityEngine;

namespace MeasVRe
{
    public class Angle : Measurement<float>
    {
        public Angle(List<GameObject> markers, VisualizationPresets presets)
                : base("Angle", markers, presets) { }

        /// <summary>
        /// Measure the angle between the markers vectors m1-m2 and m3-m2 where m1, m2, and m3
        /// are the first three marker in the markers list.
        /// </summary>
        /// <returns> The angle in degrees. </returns>
        public override float CalculateMeasurement()
        {
            return Vector3.Angle(markers[0].transform.position - markers[1].transform.position,
                                 markers[2].transform.position - markers[1].transform.position);
        }

        /// <summary>
        /// Draw the two vectors between which the angle was measured and
        /// draw an arc to indicate the angle. Places a label at the shared point of the
        /// two vectors. The label faces the camera when it is spawned.
        /// </summary>
        public override void VisualizeMeasurement()
        {
            Vector3 p1 = markers[0].transform.position;
            Vector3 p2 = markers[1].transform.position;
            Vector3 p3 = markers[2].transform.position;
            Vector3 v1 = p1 - p2;
            Vector3 v2 = p3 - p2;

            List<GameObject> lines = new List<GameObject>();
            lines.Add(VisualizationUtils.DrawLine(presets.linePrefab, p1, p2));
            lines.Add(VisualizationUtils.DrawLine(presets.linePrefab, p2, p3));

            Vector3 labelPos = markers[1].transform.position - (v1 + v2).normalized * presets.labelOffset;
            Quaternion labelRot = Quaternion.LookRotation(VisualizationUtils.GetCameraDirection(), Vector3.up);
            string labelText = "<b>Angle</b>\n" + value.ToString() + "<sup>o</sup>";
            visualizationObjects.Add("label", VisualizationUtils.AddLabel(presets.labelPrefab, labelText, labelPos,
                                                                          labelRot));

            int numPoints = presets.segments + 1;
            List<Vector3> arcPoints = new List<Vector3>();
            Vector3 arcStart = v1.normalized * presets.radius;
            Vector3 arcEnd = v2.normalized * presets.radius;

            for (int i = 0; i < numPoints; i++)
            {
                float theta = i / (float)presets.segments;
                arcPoints.Add(Vector3.Slerp(arcStart, arcEnd, theta) + p2);
            }

            lines.Add(VisualizationUtils.DrawLine(presets.linePrefab, arcPoints));
            visualizationObjects.Add("lines", lines);
        }
    }
}
