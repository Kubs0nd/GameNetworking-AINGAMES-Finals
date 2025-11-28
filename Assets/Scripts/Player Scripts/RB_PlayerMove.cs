using Photon.Pun;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class RB_PlayerMove : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 8f;
    public float maxVelocityChange = 10f;
    public float jumpForce = 30f;

    [Header("Animation")]
    public Animator animator;
    public Transform modelRoot;

    private bool isSprinting;
    private bool isJumping;
    private bool isGrounded;
    private bool jumpTriggered;

    private Vector2 input;
    private Rigidbody rb;
    private PhotonView pv;

    public float groundCheckRadius = 0.35f;
    public float groundCheckDistance = 0.6f;
    public LayerMask groundMask;

    void Start()
    {
        // gets required components from the game object this script is attached to
        rb = GetComponent<Rigidbody>();
        pv = GetComponent<PhotonView>();

        // local-only setup
        if (pv.IsMine)
        {
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        rb.linearDamping = 2.5f;
    }

    void Update()
    {
        // if the player doesn't own this component, prevents logic from operating on other existing players
        if (!pv.IsMine) return;

        // detects input, uses W A S D as parameters for Vector2
        // A and D for (X), W and S for (Y)
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // keeps input values consistent
        input.Normalize();

        // boolean values based on input
        isSprinting = Input.GetButton("Sprint");
        isJumping = Input.GetButtonDown("Jump");

        // --- Jump ---
        if (isJumping && isGrounded && !jumpTriggered)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            if (animator)
                animator.SetTrigger("Jump");

            jumpTriggered = true; // prevent double jump
        }

        // update animator
        if (animator)
        {
            float speedPercent = rb.linearVelocity.magnitude / sprintSpeed;
            animator.SetFloat("Speed", speedPercent, 0.1f, Time.deltaTime);

            animator.SetBool("IsGrounded", isGrounded);
        }

        // rotate model based on motion
        if (modelRoot && rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            Vector3 look = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            Quaternion targetRot = Quaternion.LookRotation(look);
            modelRoot.rotation = Quaternion.Slerp(modelRoot.rotation, targetRot, Time.deltaTime * 10f);
        }
    }

    private void FixedUpdate()
    {
        if (!pv.IsMine) return;

        // --- Ground Check ---
        Vector3 groundCheckPos = transform.position + Vector3.down * 0.1f; // small offset below the player
        isGrounded = Physics.CheckSphere(groundCheckPos, groundCheckRadius, groundMask, QueryTriggerInteraction.Ignore);

        // --- Gravity ---
        rb.AddForce(Physics.gravity * 3f, ForceMode.Acceleration);

        // --- Movement ---
        rb.AddForce(CalculateMovement(isSprinting ? sprintSpeed : walkSpeed), ForceMode.VelocityChange);

        

        // reset jump when grounded
        if (isGrounded && rb.linearVelocity.y <= 0.01f)
            jumpTriggered = false;
    }



    // literally in the name, for calculating movement
    Vector3 CalculateMovement(float _speed)
    {
        // uses Vector2 input and transform it into a Vector3
        // input.y is placed in z because its a 3D axis instead of 2D
        Vector3 targetVelocity = new Vector3(input.x, 0, input.y);

        // allows rotation
        targetVelocity = transform.TransformDirection(targetVelocity);
        targetVelocity *= _speed;

        Vector3 velocity = rb.linearVelocity;

        // if condition to determine if the strength of input is detected
        if (input.magnitude > 0.5f)
        {
            // determine how much the velocity needs to change to reach the target velocity
            Vector3 velocityChange = targetVelocity - velocity;

            // limit how much the velocity can change on the X axis,
            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);

            // limit how much the velocity can change on the Z axis,
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);

            // prevents any velocity changes to the Y axis
            velocityChange.y = 0;

            // returns the clamped velocity change
            return velocityChange;
        }
        else
        {
            // if no movement is detected, return an empty vector
            return Vector3.zero;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + transform.forward * 0, groundCheckRadius);
    }
}
