using System;
using System.IO;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace MeasVRe.Inventory
{
    /// <summary>
    /// Manages all taken measurements and takes care of saving data locally and submitting changes
    /// to the log manager.
    /// </summary>
    [CreateAssetMenu(fileName = "MeasurementsInventory", menuName = "MeasVRe/MeasurementsInventory", order = 0)]
    public class MeasurementsInventory : ScriptableObject
    {
        [SerializeField]
        [Tooltip("Manages uploading changes to a logging server.")]

        Log.LogManager logManager;

        [SerializeField]
        [Tooltip("The VisualizationPresets asset that holds prefabs and other data for measurements visualisation.")]
        VisualizationPresets presets;

        // Available formats for downloading the data.
        enum DataFormat { CSV, JSON };

        DataFormat dataFormat = DataFormat.CSV;

        List<IMeasurable> m_measurements = new List<IMeasurable>();

        /// <summary>  List of all taken measurements. </summary>
        public ReadOnlyCollection<IMeasurable> measurements { get; private set; }

        /// <summary>  Currently selected measurement. </summary>
        public IMeasurable selected { get; private set; }

        /// <summary>  Label of the selected measurement. </summary>
        public GameObject selectedLabel { get; private set; }

        /// <summary>  Path to the folder of the downloads. </summary>
        public string dataPath { get; set; } = "Data/";

        private void OnEnable()
        {
            measurements = m_measurements.AsReadOnly();
        }

        // This is called to find the measurement we need to select/delete
        // when a label is selected/deleted.
        IMeasurable GetMeasurementOfLabel(GameObject label)
        {
            // Iterate backwards because it is likely that user
            // will select/remove recent measurements.
            for (int i = measurements.Count - 1; i >= 0; i--)
            {
                if (measurements[i].GetLabel() == label)
                    return measurements[i];
            }

            return null;
        }

        /// <summary>  Add a measurement to the inventory </summary>
        /// <param name="measurement">  The new measurement. </param>
        public void Add(IMeasurable measurement)
        {
            if (!m_measurements.Contains(measurement))
            {
                m_measurements.Add(measurement);
                logManager.RegisterChange(Log.Changes.ChangeType.Added, measurement);
            }
        }

        /// <summary>  Remove a measurement from the inventory </summary>
        /// <param name="measurement">  The measurement to remove. </param>
        public void Remove(IMeasurable measurement)
        {
            if (m_measurements.Remove(measurement))
            {
                measurement.RemoveVisualization();
                logManager.RegisterChange(Log.Changes.ChangeType.Deleted, measurement);

                if (selected == measurement)
                    selected = null;
            }
        }

        /// <summary>
        /// Select the measurement given by a label.
        /// The currently selected measurement will be unselected.
        /// </summary>
        /// <param name="label">  Label of the measurement to select. </param>
        public void Select(GameObject label)
        {
            if (selected != null)
                Unselect();

            label.GetComponentInChildren<Image>().color = presets.selectedLabelColor;
            selectedLabel = label;
            selected = GetMeasurementOfLabel(label);
        }

        /// <summary>  Unselect the currently selected measurement. </summary>
        public void Unselect()
        {
            selectedLabel.GetComponentInChildren<Image>().color = presets.baseLabelColor;
            selected = null;
            selectedLabel = null;
        }

        /// <summary>  Add a snapshot to a measurement. </summary>
        /// <param name="measurement">  The measurement to add the snapshot to. </param>
        /// <param name="snapshot">  The new snapshot </param>
        public void AddSnapshot(IMeasurable measurement, Texture2D snapshot)
        {
            Snapshot newSnapshot = new Snapshot(measurement, snapshot);
            measurement.snapshots.Add(newSnapshot);
            logManager.RegisterChange(Log.Changes.ChangeType.Added, newSnapshot);
        }

        /// <summary>  Remove a snapshot from a measurement. </summary>
        /// <param name="measurement"> The measurement to remove the snapshot from. </param>
        /// <param name="snapshot"> The snapshot to remove. </param>
        public void RemoveSnapshot(Snapshot snapshot)
        {
            if (snapshot.measurement.snapshots.Remove(snapshot))
            {
                logManager.RegisterChange(Log.Changes.ChangeType.Deleted, snapshot);
                Destroy(snapshot.texture);
            }
        }

        /// <summary> Remove all measurements attached to a given marker. </summary>
        /// <param name="marker"> The marker to remove the measurements of. </param>
        public void RemoveMeasurementsOfMarker(GameObject marker)
        {
            // Iterate backwards because to remove elements while iterating.
            for (int i = measurements.Count - 1; i >= 0; i--)
            {
                if (measurements[i].markers.Contains(marker))
                    Remove(measurements[i]);
            }
        }

        /// <summary> Delete the measurement given by a label. </summary>
        /// <param name="label"> The label of the measurement. </param>
        public void RemoveMeasurementOfLabel(GameObject label)
        {
            Remove(GetMeasurementOfLabel(label));
        }

        /// <summary>
        /// Recalculate and revisualize all measurements attached to the given marker.
        /// This is called when a marker is moved.
        /// </summary>
        /// <param name="marker"> The moved marker. </param>
        public void UpdateMeasurementsOfMarker(GameObject marker)
        {
            for (int i = measurements.Count - 1; i >= 0; i--)
            {
                if (measurements[i].markers.Contains(marker))
                {
                    // This works, but destroying and reinstantiating a large amount of
                    // visualization objects can become costly. Should consider updating the
                    // visualization objects instead.
                    measurements[i].RemoveVisualization();
                    measurements[i].Measure();
                    if (selected == measurements[i])
                    {
                        GameObject label = measurements[i].GetLabel();
                        label.GetComponentInChildren<Image>().color = presets.selectedLabelColor;
                        selectedLabel = label;
                    }

                    logManager.RegisterChange(Log.Changes.ChangeType.Modified, measurements[i]);
                }
            }
        }

        /// <summary>
        /// Create a new project on the logging server and set all current measurements
        /// and snapshots as new changes.
        /// </summary>
        public void CreateProject()
        {
            List<Snapshot> allSnapshots = new List<Snapshot>();
            foreach (IMeasurable measurement in measurements)
                allSnapshots.AddRange(measurement.snapshots);

            logManager.CreateProject(m_measurements, allSnapshots);
        }

        /// <summary> Set the data format for saving data locally. </summary>
        /// <param name="format"></param>
        public void SetDataFormat(string format)
        {
            dataFormat = (DataFormat)Enum.Parse(typeof(DataFormat), format);
        }

        /// <summary>
        /// Save the snapshots as PNG files and the measurements as a JSON
        /// or CSV file on the device.
        /// </summary>
        /// <returns> Path to the file. </returns>
        public string Download()
        {
            string folder = Path.Combine(Application.persistentDataPath, dataPath);
            Directory.CreateDirectory(folder);
            string filePath;

            if (dataFormat == DataFormat.CSV)
            {
                filePath = folder + DateTime.Now.ToString("yyyy-dd-M-HHmmss") + ".csv";
                WriteToCSV(folder, filePath);
            }
            else
            {
                filePath = folder + DateTime.Now.ToString("yyyy-dd-M-HHmmss") + ".json";
                WriteToJSON(folder, filePath);
            }

            return filePath;
        }

        void WriteToJSON(string folder, string filePath)
        {
            int numSnapshots = 0;
            string snapshotPath = folder;

            using (StreamWriter sw = File.CreateText(filePath))
            {
                sw.WriteLine("\"measurements\":[");

                for (int i = 0; i < measurements.Count; i++)
                {
                    string json = measurements[i].ToJSON();
                    sw.Write(measurements[i].snapshots.Count > 0 ? json.Trim('}') + ",\"snapshots\":[" : json);
                    for (int j = 0; j < measurements[i].snapshots.Count; j++)
                    {
                        byte[] img = measurements[i].snapshots[j].encoded;
                        string imgName = "snapshot-" + numSnapshots.ToString() + ".png";
                        sw.Write(j == 0 ? imgName : "," + imgName);
                        string imgPath = snapshotPath + imgName;

                        new System.Threading.Thread(() =>
                        {
                            File.WriteAllBytes(imgPath, img);
                        }).Start();

                        numSnapshots++;

                        if (j == measurements[i].snapshots.Count - 1)
                            sw.WriteLine("]}");
                    }

                    if (i != measurements.Count - 1)
                        sw.WriteLine(",");
                }

                sw.WriteLine("]");
            }
        }

        void WriteToCSV(string folder, string filePath)
        {
            int numSnapshots = 0;
            string snapshotPath = folder;

            using (StreamWriter sw = File.CreateText(filePath))
            {
                sw.WriteLine("type,value,snapshots");

                foreach (IMeasurable m in measurements)
                {
                    sw.Write(m.ToCSV());

                    for (int i = 0; i < m.snapshots.Count; i++)
                    {
                        byte[] img = m.snapshots[i].encoded;
                        string imgName = "snapshot-" + numSnapshots.ToString() + ".png";
                        sw.Write("," + imgName);
                        string imgPath = snapshotPath + imgName;

                        new System.Threading.Thread(() =>
                        {
                            File.WriteAllBytes(imgPath, img);
                        }).Start();

                        numSnapshots++;
                    }

                    sw.Write("\n");
                }
            }
        }

        void SaveSnapshots(IMeasurable measurement)
        {

        }
    }
}

