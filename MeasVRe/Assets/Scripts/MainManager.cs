using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using TMPro;
using UnityEngine.UI;

namespace MeasVRe
{
    /// <summary>
    /// Contains responses to user/menu input and handles calibration at the start of the application.
    /// </summary>
    public class MainManager : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Manages taking/deleting snapshots and displaying the snapshots in the snapshot panel.")]
        SnapshotManager snapshotManager;

        [Header("Markers")]
        [SerializeField]
        [Tooltip("The input action for placing a marker in the scene")]
        InputAction placeAction;

        [SerializeField]
        [Tooltip("Handles placing markers in the scene")]
        MarkerPlacer markerPlacer;

        [SerializeField]
        [Tooltip("The MarkersInventory asset that will keep track of all the markers in the scene.")]
        Inventory.MarkersInventory markersInv;

        [Header("Measurements")]
        [SerializeField]
        [Tooltip("The MeasurementsInventory asset that keeps track of all taken measurements.")]
        Inventory.MeasurementsInventory measurementsInv;

        [SerializeField]
        [Tooltip("The VisualizationPresets asset that holds prefabs and other data for measurements visualisation.")]
        VisualizationPresets visualizationPresets;

        [Header("Calibration")]
        [SerializeField]
        [Tooltip("Input field of the calibration panel.")]
        TMP_InputField calibrationInput;

        [SerializeField]
        [Tooltip("The menu for calibration.")]
        GameObject calibrationPanel;

        [SerializeField]
        [Tooltip("Panel with measurement buttons to turn on after calibration.")]
        GameObject measurementsPanel;

        [Header("Status messages")]
        [SerializeField]
        [Tooltip("Text field for displaying (error) messages.")]
        TextMeshProUGUI messageField;

        [SerializeField]
        [Tooltip("Amount of time to show the message.")]
        float displayTime;

        [SerializeField]
        [Tooltip("Amount of time for fading the message.")]
        float fadeTime;

        bool selectNewMarkers = true;

        bool unselectAfterMeasurement = true;

        // Variables for calibration.
        bool calibrating = true;
        GameObject calibLine;

        List<GameObject> calibMarkers = new List<GameObject>();

        // Variables for displaying status messages in the menu panel.
        static volatile Queue<string> messageQueue = new Queue<string>();
        readonly static object queueLock = new object();
        bool displayingMessage = false;

        // Update is called once per frame
        void Update()
        {
            if (!displayingMessage)
            {
                lock (queueLock)
                {
                    if (messageQueue.Count > 0)
                    {
                        StartCoroutine(DisplayMessageCoroutine(messageQueue.Dequeue()));
                    }
                }
            }
        }

        private void OnEnable()
        {
            placeAction.Enable();
        }

        private void OnDisable()
        {
            placeAction.Disable();
        }

        private void Awake()
        {
            placeAction.performed += ctx => AddMarker();
        }

        // Display a message for a given amount of time and fade out the text
        // when the timer runs out.
        private IEnumerator DisplayMessageCoroutine(string message)
        {
            displayingMessage = true;
            messageField.text = message;
            messageField.alpha = 1;
            yield return new WaitForSeconds(displayTime);

            float waitTime = 0;
            while (waitTime < 1)
            {
                messageField.alpha = Mathf.Lerp(1, 0, waitTime);
                yield return null;
                waitTime += Time.deltaTime / fadeTime;
            }

            displayingMessage = false;
        }

        /// <summary> Enqueue a status message to display in the menu panel.  </summary>
        /// <param name="message"> The message to show. </param>
        public static void EnqueueMessage(string message)
        {
            if (message != "")
            {
                lock (MainManager.queueLock)
                {
                    messageQueue.Enqueue(message);
                }
            }
        }

        /// <summary>
        /// Take a measurement that requires a certain (minimum) amount of markers.
        /// The arguments are passed via a string type because the OnClick
        /// event in the Unity editor does not accept functions with more than one parameter.
        /// </summary>
        /// <param name="args">
        /// A comma-separated string with the class of the measurement, the required amount of
        /// markers, and whether this required amount is a minimum.
        /// <example> "MeasVRe.Distance, 2, false" </example>
        /// </param>
        public void TakeMeasurement(string args)
        {
            string[] subs = args.Split(',');
            Debug.Assert(subs.Length == 3, "TakeMeasurement() argument must be in the format: \"<measurement_class>, " +
                        "<required_markers (int)>, <required_is_minimum (bool)>\"");

            int requiredMarkers = int.Parse(subs[1]);
            bool requiredIsMinimum = bool.Parse(subs[2]);

            if (markersInv.selected.Count == requiredMarkers ||
                (requiredIsMinimum && markersInv.selected.Count > requiredMarkers))
            {
                List<GameObject> selectedMarkers = new List<GameObject>(markersInv.selected);

                try
                {
                    Type type = Type.GetType(subs[0]);
                    IMeasurable measurement = (IMeasurable)Activator.CreateInstance(type, selectedMarkers,
                                                                                    visualizationPresets);

                    measurement.Measure();
                    measurementsInv.Add(measurement);

                    if (unselectAfterMeasurement)
                        markersInv.UnselectAll();
                }
                catch (System.Exception e)
                {
                    EnqueueMessage("Something went wrong while taking the measurement");
                    Debug.LogError(e.ToString());
                }
            }
            else
            {
                EnqueueMessage("Select " + (requiredIsMinimum ? "at least " : "") +
                               requiredMarkers + " markers");
            }
        }

        /// <summary>
        /// Stop calibrating the scale and unit of measurements if the user has drawn a line and
        /// inserted a value as the known distance of this line or if the user pressed the
        /// Default button to set 1 Unity unit = 1 metre.
        /// </summary>
        /// <param name="defaultVal">
        /// True if the the world space should be in metres, False if the scale and unit is based
        /// on user input.
        /// </param>
        public void StopCalibration(bool defaultVal)
        {
            if (defaultVal)
            {
                visualizationPresets.scaleFactor = 1;
                visualizationPresets.SetCurrentUnit(0); // Metres.
            }
            else if (calibLine != null && float.TryParse(calibrationInput.text, out float input))
            {
                float factor = input / Vector3.Distance(calibMarkers[0].transform.position,
                                                        calibMarkers[1].transform.position);
                visualizationPresets.scaleFactor = factor;
                // The current unit is set via OnValueChanged() in the dropdown.
            }
            else
            {
                return;
            }

            if (calibLine)
                Destroy(calibLine);

            for (int i = calibMarkers.Count - 1; i >= 0; i--)
            {
                Destroy(calibMarkers[i]);
                calibMarkers.Remove(calibMarkers[i]);
            }

            Button[] buttons = measurementsPanel.GetComponentsInChildren<Button>();
            foreach (Button b in buttons)
                b.interactable = true;

            calibrating = false;
            calibrationPanel.SetActive(false);
        }

        /// <summary>
        /// Turn on/off whether the markers should be unselected after a measurement has been
        /// taken with these markers.
        /// </summary>
        /// <param name="val"> True for on, false for off. </param>
        public void SetUnselectAfterMeasurement(bool val)
        {
            unselectAfterMeasurement = val;
        }

        /// <summary> Turn on/off selecting the marker when it is created. </summary>
        /// <param name="val"> True for on, false for off. </param>
        public void SetSelectNewMarkers(bool val)
        {
            selectNewMarkers = val;
        }

        /// <summary>
        /// Select the marker or unselect the marker if it was already selected.
        /// </summary>
        /// <param name="marker"> The marker to select/unselect. </param>
        public void SelectUnselectMarker(GameObject marker)
        {
            if (!calibrating)
            {
                if (markersInv.selected.Contains(marker))
                    markersInv.Unselect(marker);
                else
                    markersInv.Select(marker);
            }
        }

        /// <summary> Select or unselect all markers in the scene. </summary>
        /// <param name="select"> True for select, false for unselect. </param>
        public void SelectUnselectAllMarkers(bool select)
        {
            if (select)
                markersInv.SelectAll();
            else
                markersInv.UnselectAll();
        }

        /// <summary>
        /// Select a measurement or unselect the measurement if it was already selected.
        /// </summary>
        /// <param name="label"> The label of the measurement to select. </param>
        public void SelectUnselectMeasurement(GameObject label)
        {
            if (measurementsInv.selectedLabel == label)
            {
                measurementsInv.Unselect();
                snapshotManager.ClosePanel();
            }
            else
            {
                measurementsInv.Select(label);
                snapshotManager.OpenPanel();
            }
        }

        /// <summary>
        /// Add a marker to the scene. If calibration is on, the marker is used to draw the
        /// calibration line. Otherwise the marker can be used for taking measurements.
        ///  </summary>
        public void AddMarker()
        {
            GameObject newMarker = markerPlacer.PlaceMarker();

            if (calibrating)
            {
                calibMarkers.Add(newMarker);

                if (calibMarkers.Count < 2)
                {
                    return;
                }
                else if (calibMarkers.Count > 2)
                {
                    GameObject overflow = calibMarkers[0];
                    calibMarkers.Remove(overflow);
                    Destroy(overflow);
                }

                if (calibLine != null)
                    Destroy(calibLine);

                Vector3 p1 = calibMarkers[0].transform.position;
                Vector3 p2 = calibMarkers[1].transform.position;
                calibLine = VisualizationUtils.DrawLine(visualizationPresets.linePrefab, p1, p2);
            }
            else
            {
                markersInv.Add(newMarker);

                if (selectNewMarkers)
                    markersInv.Select(newMarker);
            }
        }

        /// <summary>
        /// Remove a given marker and all the measurements attached to it from the scene.
        /// </summary>
        /// <param name="marker"> The marker to remove. </param>
        public void DeleteMarker(GameObject marker)
        {
            if (calibrating)
            {
                Destroy(marker);
            }
            else
            {
                measurementsInv.RemoveMeasurementsOfMarker(marker);
                markersInv.Remove(marker);
            }
        }

        /// <summary> Delete the measurement attached to the given label. </summary>
        /// <param name="label"> The label of the measurement that needs to be deleted. </param>
        public void DeleteMeasurement(GameObject label)
        {
            if (label == measurementsInv.selectedLabel)
                snapshotManager.ClosePanel();

            measurementsInv.RemoveMeasurementOfLabel(label);
        }

        /// <summary>
        /// Save the measurement data locally.
        /// </summary>
        public void Download()
        {
            if (measurementsInv.measurements.Count > 0)
            {
                string path = measurementsInv.Download();
                EnqueueMessage("Data saved to " + path);
            }
            else
            {
                EnqueueMessage("Nothing to save");
            }
        }
    }
}
