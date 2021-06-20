using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

namespace MeasVRe
{
    /// <summary>
    /// Contains utility methods for measurement visualization.
    /// </summary>
    public static class VisualizationUtils
    {
        // Cache Camera.main because this calls FindObject internally
        // which is an expensive operation.
        static Camera mainCamera;

        /// <summary> Get the look direction of the main camera. </summary>
        /// <returns> The look vector of the main camera </returns>
        public static Vector3 GetCameraDirection()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            return mainCamera.transform.forward;
        }

        /// <summary> Draw a line between the two 3D points. </summary>
        /// <param name="linePrefab"> Prefab with a LineRenderer component </param>
        /// <param name="p1"> Starting point of the line. </param>
        /// <param name="p2"> End point of the line. </param>
        /// <returns> The line object. </returns>
        public static GameObject DrawLine(GameObject linePrefab, Vector3 p1, Vector3 p2)
        {
            GameObject newLine = Object.Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
            LineRenderer line = newLine.GetComponent<LineRenderer>();
            line.positionCount = 2;
            line.SetPositions(new Vector3[] { p1, p2 });

            return newLine;
        }

        /// <summary> Draw a line through a list of 3D points. </summary>
        /// <param name="linePrefab"> Prefab with a LineRenderer component </param>
        /// <param name="positions"> Line points. </param>
        /// <returns> The line object. </returns>
        public static GameObject DrawLine(GameObject linePrefab, List<Vector3> positions)
        {
            if (positions.Count <= 1)
            {
                return null;
            }
            else if (positions.Count == 2)
            {
                return DrawLine(linePrefab, positions[0], positions[1]);
            }
            else
            {
                GameObject newLine = Object.Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
                LineRenderer line = newLine.GetComponent<LineRenderer>();
                line.positionCount = positions.Count;
                line.SetPositions(positions.ToArray());

                return newLine;
            }
        }

        /// <summary> Draw a line through a list of markers. </summary>
        /// <param name="linePrefab"> Prefab with a LineRenderer component </param>
        /// <param name="markers"> The list of markers. </param>
        /// <returns></returns>
        public static GameObject DrawLine(GameObject linePrefab, List<GameObject> markers)
        {
            return DrawLine(linePrefab, markers.Select(item => item.transform.position).ToList());
        }

        /// <summary> Set the text of a clone of the label prefab. </summary>
        /// <param name="label"> The label object to set the text of. </param>
        /// <param name="text"> The new label text. </param>
        public static void SetLabelText(GameObject label, string text)
        {
            TextMeshProUGUI[] textMeshes = label.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var t in textMeshes)
            {
                t.text = text;
            }
        }

        /// <summary> Place a label in the scene. </summary>
        /// <param name="labelPrefab"> Prefab with TextMeshProUGUI components </param>
        /// <param name="text"> The text to display on the label. </param>
        /// <param name="labelPos"> Position of the label. </param>
        /// <param name="rotation"> Rotation of the label. </param>
        /// <returns></returns>
        public static GameObject AddLabel(GameObject labelPrefab, string text, Vector3 labelPos, Quaternion rotation)
        {
            GameObject newLabel = Object.Instantiate(labelPrefab, labelPos, rotation);
            SetLabelText(newLabel, text);

            // Flip the label if the user has to turn their head more than 90 degrees to read it.
            if (Vector3.Angle(Vector3.up, newLabel.transform.up) > 90)
                newLabel.transform.RotateAround(newLabel.transform.position, Vector3.forward,
                                                180 - newLabel.transform.rotation.z);

            return newLabel;
        }
    }
}
