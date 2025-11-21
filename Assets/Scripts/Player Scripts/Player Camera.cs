using UnityEngine;
using Photon.Pun;

public class PlayerCamera : MonoBehaviour
{
    // comments cleaned up

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
    private Rigidbody rb;

    void Awake()
    {
        pv = GetComponentInParent<PhotonView>();
        rb = GetComponentInParent<Rigidbody>();

        if (rb != null)
        {
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.freezeRotation = true;
        }
    }

    void Start()
    {
        if (!pv.IsMine) // prevents logic from running on other players
        {
            // disable camera and audio for remote players
            Camera cam = cameraRoot.GetComponentInChildren<Camera>();
            if (cam != null) cam.enabled = false;

            AudioListener listener = cameraRoot.GetComponentInChildren<AudioListener>();
            if (listener != null) listener.enabled = false;

            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!pv.IsMine) return;

        // read raw mouse input
        Vector2 mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        // smooth the input to avoid sudden jumps
        smoothMouseInput = Vector2.SmoothDamp(
            smoothMouseInput,
            mouseInput,
            ref smoothMouseVelocity,
            mouseSmoothTime
        );

        // handle vertical rotation (pitch) in Update/LateUpdate
        pitch -= smoothMouseInput.y * sensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    void LateUpdate()
    {
        if (!pv.IsMine) return;

        // apply pitch to camera root
        cameraRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    void FixedUpdate()
    {
        if (!pv.IsMine || rb == null) return;

        // handle horizontal rotation (yaw) by rotating Rigidbody smoothly
        float turnAmount = smoothMouseInput.x * sensitivity * Time.fixedDeltaTime;
        Quaternion targetRotation = rb.rotation * Quaternion.Euler(0f, turnAmount, 0f);
        rb.MoveRotation(targetRotation);
    }
}
