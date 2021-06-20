using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Net.Http;

namespace MeasVRe.Log
{
    /// <summary>
    /// Contains method for generating the content of a HTTP request.
    /// </summary>
    public static class RequestContent
    {
        /// <summary> Get the JSON content needed for a request to add measurements. </summary>
        /// <param name="added"> List of new measurements </param>
        /// <returns> The JSON string content. </returns>
        public static StringContent GetAddMeasurementsContent(List<IMeasurable> added)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("{\"measurements\":[");

            foreach (IMeasurable item in added)
                builder.Append(item.ToJSON()).Append(",");

            string content = builder.ToString().Trim(',') + "]}";
            Debug.Log(content);
            return new StringContent(content, Encoding.UTF8, "application/json");
        }

        /// <summary>
        /// Get the JSON content needed for a request to update and delete measurements.
        /// </summary>
        /// <param name="deleted"> List of deleted measurements. </param>
        /// <param name="modified"> List of modified measurements. </param>
        /// <returns> The JSON string content. </returns>
        public static StringContent GetUpdateMeasurementsContent(List<IMeasurable> deleted, List<IMeasurable> modified)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("{\"remove\":[");

            for (int i = 0; i < deleted.Count; i++)
            {
                builder.Append(deleted[i].id);
                if (i != deleted.Count - 1)
                    builder.Append(",");
            }

            builder.Append("],\"replace\":[");
            foreach (IMeasurable item in modified)
                builder.Append(item.ToJSON()).Append(",");

            string content = builder.ToString().Trim(',') + "]}";
            Debug.Log(content);
            return new StringContent(content, Encoding.UTF8, "application/json");
        }

        /// <summary>
        /// Get the content needed for a request to add snapshots to a measurement.
        /// </summary>
        /// <param name="snapshots"> List of snapshots of the same measurement. </param>
        /// <returns> Multipart/form-data content with the snapshots encoded as PNG. </returns>
        public static MultipartFormDataContent GetAddSnapshotsContent(List<Snapshot> snapshots)
        {
            MultipartFormDataContent content = new MultipartFormDataContent();
            StringContent id = new StringContent(snapshots[0].measurement.id.ToString());
            content.Add(id, "id");

            for (int i = 0; i < snapshots.Count; i++)
            {
                ByteArrayContent imageArray = new ByteArrayContent(snapshots[i].encoded);
                imageArray.Headers.Add("Content-type", "image/png");
                content.Add(imageArray, "file", "snapshot" + i.ToString() + ".png");
            }

            return content;
        }

        /// <summary>
        /// Get the JSON content needed for a request to delete snapshots.
        /// </summary>
        /// <param name="snapshots"> List of snapshots that were deleted. </param>
        /// <returns> The JSON string content. </returns>
        public static StringContent GetDeleteSnapshotsContent(List<Snapshot> snapshots)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("{\"remove\":[");

            foreach (Snapshot item in snapshots)
                builder.Append("\"").Append(item.id).Append("\",");

            string content = builder.ToString().Trim(',') + "]}";
            Debug.Log(content);

            return new StringContent(content, Encoding.UTF8, "application/json");
        }
    }
}
