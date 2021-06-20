using System.Collections.Generic;

namespace MeasVRe.Log
{
    /// <summary>
    /// The object to parse JSON responses from HTTP requests to.
    /// </summary>
    [System.Serializable]
    public class ResponseMessage
    {
        public string message;
        public string key;
        public List<string> file_names;
        public List<int> ids;
    }
}
