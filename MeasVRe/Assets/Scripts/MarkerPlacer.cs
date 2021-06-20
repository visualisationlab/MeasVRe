using System;
using System.Collections.Generic;
using UnityEngine;

namespace MeasVRe
{
    /// <summary>
    /// Manages placing markers into the scene (using snapping).
    /// </summary>
    public class MarkerPlacer : MonoBehaviour
    {
        /// <summary> Possible snapping modes. </summary>
        public enum SnapOptions { surface, vertex, edge, none };

        [SerializeField]
        [Tooltip("The VisualizationPresets asset that holds prefabs and other data for measurements visualisation.")]
        VisualizationPresets visualizationPresets;

        [SerializeField]
        [Tooltip("The controller that is used to place markers.")]
        GameObject controller;

        [SerializeField]
        [Tooltip("The current snapping mode.")]
        SnapOptions snapMode;

        [SerializeField]
        [Tooltip("The attach point of the new marker if snap is turned off")]
        Transform markerAnchor;

        // The object that shows a preview of where a marker will be snapped to if snapping is on.
        GameObject snapPreview;

        // Cached variables of the collider that was last hit when snapping is on.
        MeshCollider hitCollider;
        List<Vector3> vertices = new List<Vector3>();
        int[] triangles;

        // Start is called before the first frame update
        void Start()
        {
            snapPreview = Instantiate(visualizationPresets.snapPreviewPrefab, Vector3.zero, Quaternion.identity);
            snapPreview.SetActive(false);
        }

        // Update the position of the snap preview every frame if snapping is on.
        void Update()
        {
            if (snapMode != SnapOptions.none)
            {
                if (TryGetSnapPos(out Vector3 snapPos))
                {
                    snapPreview.transform.position = snapPos;
                    snapPreview.SetActive(true);
                }
                else
                {
                    snapPreview.SetActive(false);
                }
            }
        }

        // Get the vertices list and triangles array of the mesh of the collider that was hit.
        // When these enumerables are retrieved, a new list/array is allocated.
        // To avoid allocating every frame, these enumerables are cached.
        // Imported meshes must have read/write enabled to get access to the data.
        bool GetMeshData(MeshCollider collider)
        {
            if (collider == null || collider.sharedMesh == null)
                return false;

            if (hitCollider == null || collider != hitCollider)
            {
                hitCollider = collider;
                Mesh mesh = collider.sharedMesh;
                mesh.GetVertices(vertices);
                triangles = mesh.triangles;
            }

            return true;
        }

        // Get the vertex closest to the hit point.
        Vector3 GetVertexSnapPos(RaycastHit hit)
        {
            Vector3 snapPos;

            Vector3 baryCenter = hit.barycentricCoordinate;
            int i = hit.triangleIndex * 3;
            if (baryCenter.x > baryCenter.y)
                snapPos = baryCenter.x > baryCenter.z ? vertices[triangles[i]] : vertices[triangles[i + 2]];
            else
                snapPos = baryCenter.y > baryCenter.z ? vertices[triangles[i + 1]] : vertices[triangles[i + 2]];

            return hit.collider.transform.TransformPoint(snapPos);
        }

        // Get the closest point on the closest edge to the hit point.
        Vector3 GetEdgeSnapPos(RaycastHit hit)
        {
            Vector3 snapPos;

            int i = hit.triangleIndex * 3;
            Vector3 p0 = hit.collider.transform.TransformPoint(vertices[triangles[i]]);
            Vector3 p1 = hit.collider.transform.TransformPoint(vertices[triangles[i + 1]]);
            Vector3 p2 = hit.collider.transform.TransformPoint(vertices[triangles[i + 2]]);

            Vector3 baryCenter = hit.barycentricCoordinate;
            if (baryCenter.x < baryCenter.y)
            {
                if (baryCenter.x < baryCenter.z)
                    snapPos = Vector3.Project(hit.point - p1, p2 - p1) + p1;
                else
                    snapPos = Vector3.Project(hit.point - p0, p1 - p0) + p0;
            }
            else
            {
                if (baryCenter.y < baryCenter.z)
                    snapPos = Vector3.Project(hit.point - p0, p2 - p0) + p0;
                else
                    snapPos = Vector3.Project(hit.point - p0, p1 - p0) + p0;
            }

            return snapPos;
        }

        // Try to get the position on a mesh to snap a marker to. Returns true if the marker
        // can be snapped, false otherwise. The result is written to the passed variable.
        bool TryGetSnapPos(out Vector3 snapPos)
        {
            snapPos = markerAnchor.position;

            // All layers except for the UI and Ignore Raycast layers.
            int layerMask = ~(1 << 2 | 1 << 5);

            if (Physics.Raycast(markerAnchor.position, controller.transform.forward, out RaycastHit hit, Mathf.Infinity, layerMask,
                                QueryTriggerInteraction.Ignore))
            {
                snapPos = hit.point;

                // The triangleindex will be negative if read/write is disabled on the hit mesh.
                if ((snapMode == SnapOptions.vertex || snapMode == SnapOptions.edge) && hit.triangleIndex >= 0 &&
                    GetMeshData(hit.collider as MeshCollider))
                    snapPos = snapMode == SnapOptions.vertex ? GetVertexSnapPos(hit) : GetEdgeSnapPos(hit);

                return true;
            }

            return false;
        }

        /// <summary> Set the current snapping mode. </summary>
        /// <param name="option"> The new mode </param>
        public void SetSnapOption(string option)
        {
            snapMode = (SnapOptions)Enum.Parse(typeof(SnapOptions), option);

            if (snapMode == SnapOptions.none && snapPreview != null)
                snapPreview.SetActive(false);
        }

        /// <summary>
        /// Place a new marker in the scene at a snapped position or at the marker anchor.
        /// </summary>
        public GameObject PlaceMarker()
        {
            Vector3 markerPos;

            if (snapMode != SnapOptions.none)
                TryGetSnapPos(out markerPos);
            else
                markerPos = markerAnchor.position;

            return Instantiate(visualizationPresets.markerPrefab, markerPos, controller.transform.rotation);
        }

        /// <summary>
        /// Snap a marker which was moved by grabbing it.
        /// This is called once the marker is released.
        /// </summary>
        /// <param name="marker"> The marker that has been moved. </param>
        public void SnapMovedMarker(GameObject marker)
        {
            if (snapMode != SnapOptions.none && TryGetSnapPos(out Vector3 markerPos))
                marker.transform.position = markerPos;
        }
    }
}
