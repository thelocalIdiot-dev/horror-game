using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(Rigidbody))]
public class playerMovement : MonoBehaviour
{
    [Header("----------MOVEMENT----------")]
    public float runSpeed;
    public float groundDrag;
    public float playerheight;
    public LayerMask WhatIsGround;
    public Vector3 moveDir;
    float moveSpeed;
    float desiredMoveSpeed;
    public float airMultiplier = 0.4f;
    [Header("---Slope movement---")]
    public float maxSlopeAngle;
    RaycastHit Slopehit;
    public bool exitingSlope;
    [Header("input")]
    float horizontalInput;
    float verticalInput;
    bool jumping;
    [Header("----------REFRENCES----------")]
    public Transform orientation;
    public Rigidbody rb;
    [Header("----------STATES----------")]
    public STATES States;
    public enum STATES { idle, walking, air};
    
    public void restartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public float GetFallSpeed()
    {
        return this.rb.velocity.y;
    }

    public Vector2 FindVelRelativeToLook()
    {
        float lookAngle = orientation.transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = rb.velocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMag, yMag);
    }


    public static playerMovement instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

    }

    private void Update()
    {
        GetInput();
        stateHandler();

        if (States == STATES.idle || States == STATES.walking)
        {
            rb.drag = groundDrag;
        }
        else if (States == STATES.air)
        {
            rb.drag = 0f;
        }
    }


    void stateHandler()
    {
        if (grounded() && (horizontalInput != 0 || verticalInput != 0))
        {
            States = STATES.walking;
            desiredMoveSpeed = runSpeed;
        }
        else if (grounded())
        {
            States = STATES.idle;
            desiredMoveSpeed = runSpeed;
        }
        else if (!grounded())
        {
            States = STATES.air;
        }

        moveSpeed = desiredMoveSpeed;
    }


    private void FixedUpdate()
    {
        MovePlayer();
        speedControl();

    }
    void GetInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    void MovePlayer()
    {
        moveDir = orientation.right * horizontalInput + orientation.forward * verticalInput;

        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDir) * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y < 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        if (grounded())
            rb.AddForce(moveDir.normalized * moveSpeed * 10, ForceMode.Force);
        else
            rb.AddForce(moveDir.normalized * moveSpeed * 10 * airMultiplier, ForceMode.Force);

        rb.useGravity = !OnSlope();
    }

    public bool grounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, playerheight * 0.5f + 0.2f, WhatIsGround);
    }
    void speedControl()
    {
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    public bool OnSlope()
    {

        if (Physics.Raycast(transform.position, Vector3.down, out Slopehit, playerheight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, Slopehit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }


    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, Slopehit.normal).normalized;
    }

    [ContextMenu("DZA WORDOOOOOOOOOOOOOOOOOOOOOOO")]
    void deleteAll()
    {
        PlayerPrefs.DeleteAll();
    }
}

