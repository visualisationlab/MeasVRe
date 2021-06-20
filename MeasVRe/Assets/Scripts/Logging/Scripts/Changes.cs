using System.Collections.Generic;
using UnityEngine;

namespace MeasVRe.Log
{
    public class Changes
    {
        // List of measurements to upload to the logging server.
        public List<IMeasurable> newMeasurements = new List<IMeasurable>();

        // List of uploaded measurements of which the markers and value have changed.
        public List<IMeasurable> updatedMeasurements = new List<IMeasurable>();

        // List of snapshots to upload the logging server.
        public List<Snapshot> newSnapshots = new List<Snapshot>();

        // List of uploaded measurements to delete from the logging server.
        public List<IMeasurable> deletedMeasurements = new List<IMeasurable>();

        // List of snapshots to delete from the logging server.
        public List<Snapshot> deletedSnapshots = new List<Snapshot>();

        public enum ChangeType { Added, Deleted, Modified };

        public void RegisterMeasurement(ChangeType change, IMeasurable measurement)
        {
            if (change == ChangeType.Added)
            {
                if (!newMeasurements.Contains(measurement))
                    newMeasurements.Add(measurement);
            }
            else if (change == ChangeType.Deleted)
            {
                if (!newMeasurements.Remove(measurement))
                {
                    updatedMeasurements.Remove(measurement);
                    if (!deletedMeasurements.Contains(measurement))
                        deletedMeasurements.Add(measurement);
                }

                for (int i = newSnapshots.Count - 1; i >= 0; i--)
                {
                    if (newSnapshots[i].measurement == measurement)
                        newSnapshots.Remove(newSnapshots[i]);
                }

                for (int i = deletedSnapshots.Count - 1; i >= 0; i--)
                {
                    if (deletedSnapshots[i].measurement == measurement)
                        deletedSnapshots.Remove(deletedSnapshots[i]);
                }
            }
            else
            {
                if (!newMeasurements.Contains(measurement) && !updatedMeasurements.Contains(measurement))
                    updatedMeasurements.Add(measurement);
            }
        }

        public void RegisterSnapshot(ChangeType change, Snapshot snapshot)
        {
            if (change == ChangeType.Added)
            {
                if (!newSnapshots.Contains(snapshot))
                    newSnapshots.Add(snapshot);
            }
            else if (change == ChangeType.Deleted)
            {
                if (!newSnapshots.Remove(snapshot) && !deletedSnapshots.Contains(snapshot))
                    deletedSnapshots.Add(snapshot);
            }
        }

        public void AppendChanges(Changes toAppend)
        {
            newMeasurements.AddRange(toAppend.newMeasurements);
            deletedMeasurements.AddRange(toAppend.deletedMeasurements);
            updatedMeasurements.AddRange(toAppend.updatedMeasurements);
            newSnapshots.AddRange(toAppend.newSnapshots);
            deletedSnapshots.AddRange(toAppend.deletedSnapshots);
        }

        public void Print()
        {
            Debug.Log(newMeasurements.Count + " " + deletedMeasurements.Count + " " + updatedMeasurements.Count + " " + newSnapshots.Count + " " + deletedSnapshots.Count);
        }
    }
}
