using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MeasVRe
{
    /// <summary>
    /// Manages displaying, adding, and deleting snapshots for the selected measurement.
    /// </summary>
    public class SnapshotManager : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The MeasurementsInventory asset that keeps track of all taken measurements.")]
        Inventory.MeasurementsInventory measurementsInv;

        [SerializeField]
        [Tooltip("The menu panel for adding snapshots.")]
        GameObject snapshotPanel;

        [SerializeField]
        [Tooltip("The previous and next buttons in the snapshot panel.")]
        Button prevButton, nextButton;

        [SerializeField]
        [Tooltip("The object with a Raw Image component that displays the taken snapshots.")]
        GameObject imageObject;

        [SerializeField]
        [Tooltip("The text field on the snapshot panel for showing a timer.")]
        TextMeshProUGUI textField;

        [SerializeField]
        [Tooltip("Amount of time to wait before a snapshot is taken")]
        int snapshotTimer = 3;

        // Current time on the timer.
        int timer = 0;

        // Index of the currently displayed image.
        int currentIndex = -1;

        // Cached Raw Image component on the imageObject.
        RawImage imagePreview;

        // Start is called before the first frame update
        void Start()
        {
            imagePreview = imageObject.GetComponentInChildren<RawImage>();
        }

        /// <summary>
        /// Enable or disable the next and previous image buttons in the snapshot panel.
        /// </summary>
        void UpdateButtons()
        {
            prevButton.interactable = currentIndex > 0;
            nextButton.interactable = measurementsInv.selected.snapshots.Count > currentIndex + 1;
        }

        /// <summary> Add a snaphot to the currently selected measurement. </summary>
        public void AddSnapshot()
        {
            if (timer == 0)
            {
                Debug.Log("Add snapshot");
                StartCoroutine(TakeSnapshot(measurementsInv.selected));
            }
        }

        /// <summary> Delete the snapshot that is currently shown in the snapshot panel. </summary>
        public void DeleteSnapshot()
        {
            if (currentIndex >= 0)
            {
                measurementsInv.RemoveSnapshot(measurementsInv.selected.snapshots[currentIndex]);
                currentIndex--;

                if (measurementsInv.selected.snapshots.Count == 0)
                {
                    textField.text = "None";
                }
                else
                {
                    if (currentIndex < 0)
                        currentIndex = 0;

                    imagePreview.texture = measurementsInv.selected.snapshots[currentIndex].texture;
                }

                UpdateButtons();
            }
        }

        // Take a snapshot after waiting for a set amount of time.
        IEnumerator TakeSnapshot(IMeasurable measurement)
        {
            imageObject.SetActive(false);
            timer = snapshotTimer;

            while (timer != 0)
            {
                textField.text = timer.ToString();
                yield return new WaitForSeconds(1);
                timer -= 1;
            }

            textField.text = "";

            // To make sure rendering is complete.
            yield return new WaitForEndOfFrame();
            Texture2D newImg = ScreenCapture.CaptureScreenshotAsTexture(ScreenCapture.StereoScreenCaptureMode.LeftEye);
            measurementsInv.AddSnapshot(measurement, newImg);

            // Update the snapshot panel if the given measurement is still selected.
            if (measurementsInv.selected == measurement)
            {
                imageObject.SetActive(true);
                imagePreview.texture = newImg;
                currentIndex = measurement.snapshots.Count - 1;
                UpdateButtons();
            }
        }

        /// <summary> Change the currently displayed image in the snapshots panel. </summary>
        /// <param name="next">
        /// True if the next image should be displayed, false for the previous image.
        /// </param>
        public void ChangeImage(bool next)
        {
            currentIndex = next ? currentIndex + 1 : currentIndex - 1;
            imagePreview.texture = measurementsInv.selected.snapshots[currentIndex].texture;
            UpdateButtons();
        }

        /// <summary> Close the snapshot panel. </summary>
        public void ClosePanel()
        {
            snapshotPanel.SetActive(false);
        }

        /// <summary>
        /// Open the snapshot panel where the user can add snapshots to the selected measurement.
        /// The panel shows a preview of the snapshots. When the panel is opened, the first image is
        /// shown if the currently selected measurement has snapshots.
        /// </summary>
        public void OpenPanel()
        {
            if (measurementsInv.selected.snapshots.Count > 0)
            {
                currentIndex = 0;
                imagePreview.texture = measurementsInv.selected.snapshots[currentIndex].texture;
                imageObject.SetActive(true);
                textField.text = "";
            }
            else
            {
                currentIndex = -1;
                textField.text = "None";
                imageObject.SetActive(false);
            }

            UpdateButtons();
            snapshotPanel.SetActive(true);
        }
    }
}
