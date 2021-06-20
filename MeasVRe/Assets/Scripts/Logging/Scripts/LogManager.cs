using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;
using UnityEngine;

namespace MeasVRe.Log
{
    /// <summary> Manages uploading measurement data to the logging server. </summary>
    [CreateAssetMenu(fileName = "LogManager", menuName = "MeasVRe/LogManager", order = 0)]
    public class LogManager : ScriptableObject
    {
        [SerializeField]
        [Tooltip("Event to raise when a project has been created.")]
        MeasVRe.Events.InputEvent projectCreatedEvent;

        [SerializeField]
        [Tooltip("Host of the logging server.")]
        private string m_host = "127.0.0.1";

        [SerializeField]
        [Tooltip("Port of the logging server.")]
        private int m_port = 5000;

        [SerializeField]
        [Tooltip("The name of the project.")]
        private string m_projectName = "my_project";

        [SerializeField]
        [Tooltip("The key of the current project.")]
        private string m_key = "";

        /// <summary> Host of the logging server without http:// </summary>
        public string host
        {
            get => m_host;
            set => m_host = value;
        }

        /// <summary> Port of the logging server. </summary>
        public string port
        {
            get => m_port.ToString();
            set => m_port = int.Parse(value);
        }

        /// <summary> The key of the project. </summary>
        public string key
        {
            get => m_key;
            set => m_key = value;
        }

        /// <summary> The name of the project. </summary>
        public string projectName
        {
            get => m_projectName;
            set => m_projectName = value;
        }

        // Threading objects for asynchronous sending of data
        Thread sendThread;
        volatile bool sendThreadRunning = true;
        readonly object poolLock = new object();
        static volatile Queue<Task<bool>> taskPool = new Queue<Task<bool>>();

        // Changes that have not been uploaded yet.
        volatile Changes changes = new Changes();
        readonly object changesLock = new object();

        private void OnEnable()
        {
            // Create and start send thread
            sendThread = new Thread(new ThreadStart(ThreadJob));
            sendThread.Start();
        }

        void OnApplicationQuit()
        {
            // Stop the send thread on application quit
            lock (poolLock)
            {
                sendThreadRunning = false;
            }
        }

        /// <summary> Register the addition/deletion/modification of a measurement. </summary>
        /// <param name="change"> Type of change. </param>
        /// <param name="measurement"> The measurement that was involved in the change. </param>
        public void RegisterChange(Changes.ChangeType change, IMeasurable measurement)
        {
            lock (changesLock)
            {
                changes.RegisterMeasurement(change, measurement);
            }
        }

        /// <summary> Register the addition/deletion of a snapshot. </summary>
        /// <param name="change"> Type of change. </param>
        /// <param name="snapshot"> The snapshot that was involved in the change. </param>
        public void RegisterChange(Changes.ChangeType change, Snapshot snapshot)
        {
            lock (changesLock)
            {
                changes.RegisterSnapshot(change, snapshot);
            }
        }

        /// <summary> Upload all changes to the logging server. </summary>
        public void UploadChanges()
        {
            lock (changesLock)
            {
                EnqueueTask(UploadChanges(changes));
                changes = new Changes();
            }
        }

        /// <summary>
        /// Create a new project and reset all changes. The passed measurements and
        /// snapshots are set as "new" changes to upload to the new project.
        /// </summary>
        /// <param name="measurements"> List of measurements to set as new. </param>
        /// <param name="snapshots"> List of snapshots to set as new. </param>
        public void CreateProject(List<IMeasurable> measurements, List<Snapshot> snapshots)
        {
            EnqueueTask(CreateProjectAsync(measurements, snapshots));
        }

        /// <summary>
        /// Add an asynchronous task that sends HTTP requests to the requests queue.
        /// </summary>
        /// <param name="request"> The task that sends a HTTP request. </param>
        protected void EnqueueTask(Task<bool> task)
        {
            lock (poolLock)
            {
                taskPool.Enqueue(task);
            }
        }

        void ThreadJob()
        {
            // Ends thread execution when application quits
            // (assuming the application quits gracefully and flag is set)
            while (sendThreadRunning)
            {
                // Putting this here ensures that the queue is emptied before the
                // thread terminates.
                while (taskPool.Count > 0)
                {
                    lock (poolLock)
                    {
                        taskPool.Dequeue().ConfigureAwait(false);
                    }
                }
            }
        }

        /// <summary>
        /// Upload all added/modified/deleted measurements and added/deleted snapshots, if any.
        /// </summary>
        /// <param name="toUpload"> Changes to upload. </param>
        /// <returns>
        /// True if changes have been succesfully uploaded or if there were no changes,
        /// false otherwise.
        /// </returns>
        protected async Task<bool> UploadChanges(Changes toUpload)
        {
            bool? result = null;

            // Adding measurements should finish first because we need the measurement ids
            // to upload snapshots.
            if (toUpload.newMeasurements.Count > 0)
                result = await AddMeasurements(toUpload.newMeasurements);

            List<Task<bool>> tasks = new List<Task<bool>>();
            if (toUpload.deletedMeasurements.Count > 0 || toUpload.updatedMeasurements.Count > 0)
                tasks.Add(UpdateMeasurements(toUpload.deletedMeasurements, toUpload.updatedMeasurements));
            if (toUpload.deletedSnapshots.Count > 0)
                tasks.Add(DeleteSnapshots(toUpload.deletedSnapshots));
            if (toUpload.newSnapshots.Count > 0)
                tasks.Add(AddSnapshots(toUpload.newSnapshots));

            bool[] results = await Task.WhenAll(tasks);
            foreach (bool r in results)
                result = result.HasValue ? result.Value && r : r;

            if (result.HasValue)
            {
                if (result.Value)
                    MainManager.EnqueueMessage("Succesfully uploaded changes");
                else
                    MainManager.EnqueueMessage("Failed to (some) upload changes");
            }

            // Read any failed changes. Changes that were uploaded succesfully were removed
            // in their respective task.
            lock (changesLock)
            {
                changes.AppendChanges(toUpload);
            }

            return !result.HasValue || result.Value;
        }

        /// <summary> Send an HTTP request to an endpoint with the given content. </summary>
        /// <param name="endpoint"> The endpoint to the request to. </param>
        /// <param name="method"> The HTTP method. </param>
        /// <param name="content"> The content of the HTTP request. </param>
        /// <returns>
        /// A Task that represents the asynchronous send operation.
        /// The result of the Task contains a HttpResponseMessage.
        /// </returns>
        protected async Task<HttpResponseMessage> SendRequest(string endpoint, HttpMethod method, HttpContent content)
        {
            string uri = string.Format("http://{0}:{1}/measvre-api", host, port) + endpoint;
            HttpRequestMessage request = new HttpRequestMessage(method, uri);
            request.Content = content;

            HttpResponseMessage response;
            using (var client = new HttpClient())
            {
                response = await client.SendAsync(request);
            }

            return response;
        }

        /// <summary> Parses the JSON response of an HTTP request. </summary>
        /// <param name="response"> The HTTP response received from a request. </param>
        /// <returns>
        /// A Task that represents the asynchronous delete operation.
        /// The result of the Task contains a ResponseMessage with the parsed JSON fields.
        /// </returns>
        protected async Task<ResponseMessage> ParseResponse(HttpResponseMessage response)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            ResponseMessage responseMsg = JsonUtility.FromJson<ResponseMessage>(responseContent);

            if (!response.IsSuccessStatusCode)
            {
                if (responseMsg.message != null)
                    Debug.LogError(string.Format("Log: Response Error ({0})", responseMsg.message));

                return null;
            }

            return responseMsg;
        }

        /// <summary>
        /// Send an HTTP POST request to the logging server to create a project and
        /// assign the current key to the key of this new project.
        /// </summary>
        /// <returns> A Task that represents the asynchronous create operation. </returns>
        public async Task<bool> CreateProjectAsync(List<IMeasurable> measurements, List<Snapshot> snapshots)
        {
            StringContent content = new StringContent(string.Format("{{\"projectName\": \"{0}\"}}", projectName),
                                                      Encoding.UTF8, "application/json");
            HttpResponseMessage response = await SendRequest("/projects", HttpMethod.Post, content);
            ResponseMessage responseMsg = await ParseResponse(response);

            if (responseMsg != null && responseMsg.key != null)
            {
                key = responseMsg.key;
                lock (changesLock)
                {
                    changes = new Changes();
                    changes.newMeasurements = new List<IMeasurable>(measurements);
                    changes.newSnapshots = new List<Snapshot>(snapshots);
                    projectCreatedEvent.Raise(null);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Send an HTTP POST request to the logging server to upload a list of new measurements.
        /// </summary>
        /// <param name="added"> The list of new measurements. </param>
        /// <returns> A Task that represents the asynchronous upload operation. </returns>
        protected async Task<bool> AddMeasurements(List<IMeasurable> added)
        {
            HttpResponseMessage response = await SendRequest(string.Format("/projects/{0}/measurements", key),
                                                             HttpMethod.Post,
                                                             RequestContent.GetAddMeasurementsContent(added));
            ResponseMessage responseMsg = await ParseResponse(response);

            if (responseMsg != null && responseMsg.ids != null)
            {
                Debug.Log("Adding ids");

                for (int i = 0; i < responseMsg.ids.Count; i++)
                {
                    added[i].id = responseMsg.ids[i];
                    Debug.Log(responseMsg.ids[i]);
                }

                added.Clear();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Send an HTTP PATCH request to the logging server to delete a list of measurements.
        /// </summary>
        /// <param name="deleted"> The list of measurements to delete. </param>
        /// <returns> A Task that represents the asynchronous delete operation. </returns>
        protected async Task<bool> UpdateMeasurements(List<IMeasurable> deleted, List<IMeasurable> modified)
        {
            HttpResponseMessage response = await SendRequest(string.Format("/projects/{0}/measurements", key),
                                                             new HttpMethod("PATCH"),
                                                             RequestContent.GetUpdateMeasurementsContent(deleted,
                                                                                                         modified));

            if (response.IsSuccessStatusCode)
            {
                deleted.Clear();
                modified.Clear();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Sendn a HTTP POST request to the logging server to upload a list of snapshots.
        /// </summary>
        /// <param name="snapshots"> The list of snapshots to upload. </param>
        /// <returns> A Task that represents the asynchronous upload operation. </returns>
        protected async Task<bool> AddSnapshots(List<Snapshot> snapshots)
        {
            Dictionary<int, List<Snapshot>> snapshotsPerMeasurement = new Dictionary<int, List<Snapshot>>();
            foreach (Snapshot s in snapshots)
            {
                if (snapshotsPerMeasurement.ContainsKey(s.measurement.id))
                    snapshotsPerMeasurement[s.measurement.id].Add(s);
                else
                    snapshotsPerMeasurement[s.measurement.id] = new List<Snapshot>() { s };
            }

            string uri = string.Format("/projects/{0}/measurements", key);
            bool success = true;

            foreach (KeyValuePair<int, List<Snapshot>> entry in snapshotsPerMeasurement)
            {
                string requestUri = string.Format("{0}/{1}/snapshots", uri, entry.Key);
                HttpResponseMessage response = await SendRequest(requestUri, HttpMethod.Post,
                                                                 RequestContent.GetAddSnapshotsContent(entry.Value));
                ResponseMessage responseMsg = await ParseResponse(response);

                if (responseMsg != null && responseMsg.file_names != null)
                {
                    for (int i = 0; i < responseMsg.file_names.Count; i++)
                    {
                        entry.Value[i].id = responseMsg.file_names[i];
                        snapshots.Remove(entry.Value[i]);
                    }
                }
                else
                {
                    success = false;
                }
            }

            return success;
        }

        /// <summary>
        /// Send an HTTP PATCH request to the logging server to delete a list of snapshots.
        /// </summary>
        /// <param name="snapshots"> The list of snapshots to delete. </param>
        /// <returns> A Task that represents the asynchronous delete operation. </returns>
        protected async Task<bool> DeleteSnapshots(List<Snapshot> snapshots)
        {
            Dictionary<int, List<Snapshot>> snapshotsPerMeasurement = new Dictionary<int, List<Snapshot>>();
            foreach (Snapshot s in snapshots)
            {
                if (snapshotsPerMeasurement.ContainsKey(s.measurement.id))
                    snapshotsPerMeasurement[s.measurement.id].Add(s);
                else
                    snapshotsPerMeasurement[s.measurement.id] = new List<Snapshot>() { s };
            }

            bool success = true;
            string uri = string.Format("/projects/{0}/measurements", key);

            foreach (KeyValuePair<int, List<Snapshot>> entry in snapshotsPerMeasurement)
            {
                string requestUri = string.Format("{0}/{1}/snapshots", uri, entry.Key);
                HttpResponseMessage response = await SendRequest(requestUri, new HttpMethod("PATCH"),
                                                                 RequestContent.GetDeleteSnapshotsContent(entry.Value));

                if (response.IsSuccessStatusCode)
                {
                    for (int i = 0; i < entry.Value.Count; i++)
                        snapshots.Remove(entry.Value[i]);
                }
                else
                {
                    success = false;
                }
            }

            return success;
        }
    }
}
