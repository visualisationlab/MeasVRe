using System.Collections.Generic;
using UnityEngine;

namespace MeasVRe
{
    /// <summary>
    /// Interface for measurements. This interface can be used to perform operations with
    /// generic measurments.
    /// </summary>
    public interface IMeasurable
    {
        /// <summary> Id of the measurement on the logging server. </summary>
        int id { get; set; }

        /// <summary> Markers used for taking the measurement. </summary>
        List<GameObject> markers { get; }

        /// <summary> Snapshots of this measurement. </summary>
        List<Snapshot> snapshots { get; }

        /// <summary> Calculate the value of the measurement and create the visualization. </summary>
        void Measure();

        /// <summary> Get the label of the measurement. </summary>
        /// <returns>The label GameObject of the measurement.</returns>
        GameObject GetLabel();

        /// <summary> Remove all visualization objects of the measurement. </summary>
        void RemoveVisualization();

        /// <summary> Get a JSON representation of the measurement. </summary>
        /// <returns>A string with the JSON data.</returns>
        string ToJSON();

        /// <summary> Get a CSV representation of the measurement. </summary>
        /// <returns>A string with the measurement data as comma-separated values.</returns>
        string ToCSV();
    }
}