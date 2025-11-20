using UnityEngine;
using Photon.Pun;

// too lazy to comment this part
// this script is just for double checking to make sure which is the local and which is the remote player
public class PlayerSetup : MonoBehaviour
{
    private PhotonView pv;
    public RB_PlayerMove movement;
    public PlayerCamera playerCam;
    public Camera mainCam;
    public AudioListener listener;

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

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void DisableRemotePlayer()
    {
        movement.enabled = false;
        playerCam.enabled = false;

        if (mainCam != null) mainCam.enabled = false;
        if (listener != null) listener.enabled = false;
    }
}
