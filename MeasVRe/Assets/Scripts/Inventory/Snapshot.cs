using UnityEngine;

namespace MeasVRe
{
    /// <summary>
    /// Class for storing a snapshot.
    /// </summary>
    public class Snapshot
    {
        /// <summary> The measurement of this snapshot. </summary>
        public IMeasurable measurement;

        /// <summary> The file name id of the snapshot on the logging server. </summary>
        public string id = "";

        /// <summary> The snapshot encoded as PNG. </summary>
        public byte[] encoded;

        /// <summary> The snapshot as a texture. </summary>
        public Texture2D texture;

        public Snapshot(IMeasurable measurement, Texture2D texture)
        {
            this.measurement = measurement;
            this.texture = texture;
            encoded = this.texture.EncodeToPNG();
        }
    }
}
