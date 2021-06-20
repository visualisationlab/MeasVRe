using UnityEngine;
using TMPro;

namespace MeasVRe.Log
{
    /// <summary>
    /// Sets the input fields in the menu to the values stored in the log manager and
    /// measurements inventory.
    /// </summary>
    public class LoggingInputFieldsInitializer : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("LogManager asset with the intial values for the logging server fields.")]
        LogManager logManager;

        [SerializeField]
        [Tooltip("MeasurementsInventory asset with the initial values for saving locally fields.")]
        MeasVRe.Inventory.MeasurementsInventory measurementsInv;

        [SerializeField]
        [Tooltip("Host input field")]
        TMP_InputField hostField;

        [SerializeField]
        [Tooltip("Port input field")]
        TMP_InputField portField;

        [SerializeField]
        [Tooltip("Key input field")]
        TMP_InputField keyField;

        [SerializeField]
        [Tooltip("Project name input field")]
        TMP_InputField projectNameField;

        [SerializeField]
        [Tooltip("Data path input field")]
        TMP_InputField dataPathField;

        // Start is called before the first frame update
        void Start()
        {
            hostField.text = logManager.host;
            hostField.MoveTextEnd(false);

            portField.text = logManager.port;
            portField.MoveTextEnd(false);

            projectNameField.text = logManager.projectName;
            projectNameField.MoveTextEnd(false);

            ResetKey();

            dataPathField.text = measurementsInv.dataPath;
            dataPathField.MoveTextEnd(false);
        }

        /// <summary>
        /// Set the value in the key input field to the current key in the log manager.
        /// This is called when a new project is created.
        /// </summary>
        public void ResetKey()
        {
            keyField.text = logManager.key;
            keyField.MoveTextEnd(false);
        }
    }
}
