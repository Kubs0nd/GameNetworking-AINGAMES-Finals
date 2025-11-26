using UnityEngine;
using Photon.Pun;

public class PlayerSetup : MonoBehaviour
{
    private PhotonView pv;
    public RB_PlayerMove movement;
    public PlayerCamera playerCam;
    public Camera mainCam;
    public AudioListener listener;

    public GameObject playerCanvas; 

    private void Awake()
    {
        pv = GetComponent<PhotonView>();

        movement = GetComponent<RB_PlayerMove>();
        playerCam = GetComponentInChildren<PlayerCamera>();
        mainCam = GetComponentInChildren<Camera>();
        listener = GetComponentInChildren<AudioListener>();
    }

    private void Start()
    {
        if (pv.IsMine)
            EnableLocalPlayer();
        else
            DisableRemotePlayer();
    }

    void EnableLocalPlayer()
    {
        movement.enabled = true;
        playerCam.enabled = true;

        if (mainCam != null) mainCam.enabled = true;
        if (listener != null) listener.enabled = true;

        if (playerCanvas != null) playerCanvas.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void DisableRemotePlayer()
    {
        movement.enabled = false;
        playerCam.enabled = false;

        if (mainCam != null) mainCam.enabled = false;
        if (listener != null) listener.enabled = false;

        if (playerCanvas != null) playerCanvas.SetActive(false);
    }
}
