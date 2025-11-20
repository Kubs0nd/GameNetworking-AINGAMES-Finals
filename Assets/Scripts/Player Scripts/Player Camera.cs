using UnityEngine;
using Photon.Pun;
using static UnityEditor.Searcher.SearcherWindow.Alignment;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    [Header("Player Camera Settings")]
    public Transform cameraRoot;
    public float sensitivity = 200f;
    public float minPitch = -80f;
    public float maxPitch = 80f;
    public float mouseSmoothTime = 0.02f;

    private float pitch;
    private Vector2 smoothMouseInput;
    private Vector2 smoothMouseVelocity;

    private PhotonView pv;

    void Awake()
    {
        pv = GetComponentInParent<PhotonView>();
        // playerCamera is a child object, so we get the PhotonView from the parent object which is the player
    }

    void Start()
    {
        if (!pv.IsMine)
        {
            // disable camera for remote players
            Camera cam = cameraRoot.GetComponentInChildren<Camera>();
            if (cam != null) cam.enabled = false;

            AudioListener listener = cameraRoot.GetComponentInChildren<AudioListener>();
            if (listener != null) listener.enabled = false;

            return;
        }

        // hide cursor and keeps it from leaving the play screen
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!pv.IsMine) return;  // ensures only local player controls the camera

        // read raw mouse input for X (horizontal) and Y (vertical) movement
        Vector2 mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        // smooth the mouse input over time to avoid sudden jumps (like built-in mouse smoothing)
        // smoothMouseInput = current smoothed value
        // mouseInput = target value
        // smoothMouseVelocity = ref variable required by SmoothDamp to track speed
        // mouseSmoothTime = time it takes to reach the target smoothly
        // idk if you'll understand this anyway
        smoothMouseInput = Vector2.SmoothDamp(
            smoothMouseInput,
            mouseInput,
            ref smoothMouseVelocity,
            mouseSmoothTime
        );

        // rotate the player (Y-axis rotation) based on smoothed horizontal mouse movement
        transform.Rotate(Vector3.up * smoothMouseInput.x * sensitivity * Time.deltaTime);

        // adjust vertical rotation (pitch) using smoothed vertical mouse movement
        pitch -= smoothMouseInput.y * sensitivity * Time.deltaTime;

        // clamp pitch so the camera can't rotate too far up or down
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // apply vertical rotation to the camera root only (not the entire player)
        cameraRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}
