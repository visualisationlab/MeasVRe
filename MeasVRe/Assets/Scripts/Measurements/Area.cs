using System.Collections.Generic;
using UnityEngine;

namespace MeasVRe
{
    public class Area : Measurement<float>
    {
        List<Vector3> vertices;
        List<int> triangles;

        public Area(List<GameObject> markers, VisualizationPresets visualizationPresets)
                   : base("Area", markers, visualizationPresets) { }

        /// <summary>
        /// Calculate the sufrace area by creating a convex hull using the list of markers and
        /// adding the areas of the triangles of this hull.
        /// </summary>
        /// <returns></returns>
        public override float CalculateMeasurement()
        {
            vertices = new List<Vector3>();
            triangles = new List<int>();
            float area = 0.0f;

            float[] points = new float[3 * markers.Count];
            for (int i = 0; i < markers.Count; i++)
            {
                vertices.Add(markers[i].transform.position);
                points[3 * i] = markers[i].transform.position.x;
                points[3 * i + 1] = markers[i].transform.position.y;
                points[3 * i + 2] = markers[i].transform.position.z;
            }

            uint hullSizeIn = (uint)markers.Count * 3;
            uint[] hull = new uint[hullSizeIn];

            // The function will return false if the hull buffer was too small to fit the result.
            if (!GTE.ComputeConvexHull3D(0, (uint)markers.Count, points, out uint dimensions, hullSizeIn, hull,
                                         out uint hullSize))
            {
                hullSizeIn = hullSize;
                hull = new uint[hullSizeIn];
                GTE.ComputeConvexHull3D(0, (uint)markers.Count, points, out dimensions, hullSizeIn, hull,
                                        out hullSize);
            }

            if (dimensions == 2)
            {
                // If the dimenionality of the hull was two, we get the extreme points of the
                // hull so we create a triangle fan to visualise it.
                triangles.Clear();
                int first = (int)hull[0];

                for (int i = 2; i < hullSize; i++)
                {
                    if (i % 2 == 0)
                    {
                        triangles.Add(first);
                        triangles.Add((int)hull[i - 1]);
                        triangles.Add((int)hull[i]);
                    }
                    else
                    {
                        triangles.Add((int)hull[i]);
                        triangles.Add((int)hull[i - 1]);
                        triangles.Add(first);
                    }

                    area += Vector3.Dot(vertices[first] - vertices[(int)hull[i]],
                                        vertices[first] - vertices[(int)hull[i - 1]]) / 2.0f;
                }
            }
            else if (dimensions == 3)
            {
                // If the dimenionality of the hull was three, we get a list of triangle indices.
                int numTriangles = (int)hullSize / 3;
                triangles.Clear();

                for (int i = 0; i < numTriangles; i++)
                {
                    // GTE returns the triangles in a counter-clockwise order, but Unity uses
                    // a clockwise order.
                    triangles.Add((int)hull[3 * i + 2]);
                    triangles.Add((int)hull[3 * i + 1]);
                    triangles.Add((int)hull[3 * i]);

                    area += Vector3.Dot(vertices[triangles[3 * i]] - vertices[triangles[3 * i + 1]],
                                        vertices[triangles[3 * i]] - vertices[triangles[3 * i + 2]]) / 2.0f;
                }
            }

            return area * presets.scaleFactor * presets.scaleFactor;
        }

        /// <summary>
        /// Visualize the calculated area by creating a mesh that represents the convex hull.
        /// The spawned label faces the camera and is placed at the first marker.
        /// </summary>
        public override void VisualizeMeasurement()
        {
            GameObject meshObject = new GameObject("Area Visualisation", typeof(MeshRenderer), typeof(MeshFilter));
            meshObject.GetComponent<MeshRenderer>().material = presets.areaMaterial;
            visualizationObjects.Add("meshObject", meshObject);

            Mesh mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            meshObject.GetComponent<MeshFilter>().mesh = mesh;

            Quaternion labelRot = Quaternion.FromToRotation(Vector3.forward, VisualizationUtils.GetCameraDirection());
            string text = "<b>Area</b>\n" + value.ToString() + " " + presets.currentUnit.ToString() + "<sup>2</sup>";
            visualizationObjects.Add("label", VisualizationUtils.AddLabel(presets.labelPrefab, text,
                                     markers[0].transform.position, labelRot));
        }
    }
}