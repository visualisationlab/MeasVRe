using System.Collections.Generic;
using UnityEngine;

namespace MeasVRe
{
    // Template for creating a new measurement type.
    public class MeasurementTemplate : Measurement<float>
    {
        /// <summary>
        /// Constructor for the measurement class.
        /// </summary>
        public MeasurementTemplate(List<GameObject> markers, VisualizationPresets visualizationPresets)
                                  : base("Name of the measurement", markers, visualizationPresets) { }

        public override float CalculateMeasurement()
        {
            /* Add here the code for computing the value of the measurement.
             * Should return the computed value.
             */

            return 0.0f;
        }

        public override void VisualizeMeasurement()
        {
            /* Add here the code for visualizing the taken measurement.
             * Add all instantiated visualization objects to visualizationObjects
             * to be able to destroy them when the measurement is deleted.
             * The label must be stored in visualizationObjects["label"] or GetLabel() should be
             * overriden to be able to find the measurement with a label.
             */
        }
    }
}