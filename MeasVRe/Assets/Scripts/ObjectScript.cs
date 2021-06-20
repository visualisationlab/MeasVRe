using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace MeasVRe
{
    /// <summary>
    /// Script for objects that are selectable and deletable. Invokes a select or delete event
    /// when the user hovers over the object with a controller and presses a given
    /// button (combination).
    /// </summary>
    public class ObjectScript : MonoBehaviour
    {
        [SerializeField]

        [Tooltip("Event that will be raised when the select action is performed.")]
        Events.InputEvent selectEvent;

        [SerializeField]
        [Tooltip("Controls used to select the object when a contoller tagged with \"GameController\" hovers over it.")]
        InputAction selectAction;

        [Space]
        [SerializeField]
        [Tooltip("Event that will be raised when the delete action is performed.")]
        Events.InputEvent deleteEvent;

        [SerializeField]
        [Tooltip("Controls used to delete the object when a contoller tagged with \"GameController\" hovers over it.")]
        InputAction deleteAction;

        // Grab interactable attached to this object.
        private XRBaseInteractable grabInteractable;

        private void OnDisable()
        {
            // Unregister collider from the XR interaction manager to prevent MissingReferenceException.
            grabInteractable.colliders.Clear();
            selectAction.Disable();
            deleteAction.Disable();
        }

        private void Awake()
        {
            selectAction.performed += ctx => selectEvent.Raise(gameObject);
            deleteAction.performed += ctx => deleteEvent.Raise(gameObject);
            grabInteractable = GetComponent<XRGrabInteractable>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("GameController"))
            {
                selectAction.Enable();
                deleteAction.Enable();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("GameController"))
            {
                selectAction.Disable();
                deleteAction.Disable();
            }
        }
    }
}