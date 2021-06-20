using UnityEngine;
using System.Collections.Generic;
using System;

namespace MeasVRe
{
    /// <summary>
    /// Class for implementing a measurement type.
    /// </summary>
    /// <typeparam name="T"> The type of the measured value. </typeparam>
    public abstract class Measurement<T> : IMeasurable
    {
        /// <summary> Value of the measurement. </summary>
        protected T value;

        /// <summary> String representation of the measurement type. </summary>
        protected string type;

        /// <summary>
        /// Objects used for visualizing the measurement. The values are objects to allow for
        /// GameObjects or a list of GameObjects.
        /// </summary>
        protected Dictionary<string, object> visualizationObjects = new Dictionary<string, object>();

        /// <summary>
        /// The VisualizationPresets asset that holds prefabs and other data
        /// for measurements visualisation.
        /// </summary>
        protected VisualizationPresets presets;

        /// <inheritdoc/>
        public int id { get; set; } = -1;

        /// <inheritdoc/>
        public List<GameObject> markers { get; protected set; }

        /// <inheritdoc/>
        public List<Snapshot> snapshots { get; protected set; } = new List<Snapshot>();

        protected Measurement(string type, List<GameObject> markers, VisualizationPresets visualizationPresets)
        {
            this.type = type;
            this.markers = markers;
            presets = visualizationPresets;
        }

        /// <summary> Calculate the measurement. </summary>
        /// <returns> The calculated value. </returns>
        public abstract T CalculateMeasurement();

        /// <summary>
        /// Spawn objects in the scene to visualize the measured value. All instantiated
        /// visualization objects shoudl be added to visualizationObjects to be able to destroy them
        /// when the measurement is deleted. The label must be stored in
        /// visualizationObjects["label"] or GetLabel() should be overriden to be able to find
        /// the measurement with a label.
        /// </summary>
        public abstract void VisualizeMeasurement();

        /// <inheritdoc/>
        public void Measure()
        {
            this.value = CalculateMeasurement();
            VisualizeMeasurement();
        }

        /// <inheritdoc />
        public virtual GameObject GetLabel()
        {
            return visualizationObjects["label"] as GameObject;
        }

        /// <inheritdoc />
        public virtual void RemoveVisualization()
        {
            foreach (var item in visualizationObjects.Values)
            {
                Debug.Log(item);

                if (item is GameObject)
                {
                    UnityEngine.Object.Destroy((GameObject)item);
                }
                else if (item is List<GameObject>)
                {
                    List<GameObject> objectList = (List<GameObject>)item;
                    foreach (GameObject obj in objectList)
                    {
                        UnityEngine.Object.Destroy(obj);
                    }
                }
            }

            visualizationObjects.Clear();
        }

        /// <inheritdoc />
        public virtual string ToJSON()
        {
            string json = String.Format("{{\"id\":{0},\"type\":\"{1}\",\"value\":{2},\"markers\":[",
                                     id, type, value.ToString());

            foreach (GameObject marker in markers)
            {
                Vector3 pos = marker.transform.position;
                json += String.Format("[{0},{1},{2}],", pos.x, pos.y, pos.z);
            }

            return json.Trim(',') + "]}";
        }

        /// <inheritdoc />
        public virtual string ToCSV()
        {
            return type + "," + value.ToString();
        }
    }
}
