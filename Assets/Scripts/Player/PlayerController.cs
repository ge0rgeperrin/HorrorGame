using System;

namespace Subterranea
{
    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    public PlayerAudio playerAudio;
    public CharacterController characterController;
    
    [Space(20)]
    [Header("Player Movement")]
    
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float crouchSpeed = 3f;
    public float jumpSpeed = 8.0f;
    public Camera playerCamera;
    public GameObject Mesh;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;

    [Space(20)] [Header("Item Interaction")]
    public KeyCode itemInteractionKey = KeyCode.E;
    public LayerMask itemMask;
    public Transform pickupTarget;
    public float pickupRange = 5f;

    private float lerpSpeed = 15f;
    private InteractableItem currentItem;
    private bool holdingItem => currentItem != null;

    private Vector3 originalScale;
    private const float gravity = 20.0f;
    private bool cursorLocked = true;
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;
    
    

    [HideInInspector] public bool canMove = true;

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cursorLocked = false;
    }
    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        cursorLocked = true;
    }
    void Start()
    {
        Instance = this;
        if (!characterController)
            characterController = GetComponent<CharacterController>();
        originalScale = transform.localScale;
        LockCursor();
    }

    private void Update()
    {
        playerAudio.RunAudioLogic();
        
        if (!cursorLocked)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UnlockCursor();
            }
        }
        
        CheckInput();
        CheckItems();
    }

    private void FixedUpdate()
    {
        ItemPhysics();
    }

    private void CheckInput()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        bool isCrouching = Input.GetKey(KeyCode.LeftControl);
        
        if (isCrouching && !isRunning)
        {
            transform.localScale = new Vector3(originalScale.x, originalScale.y / 2);
        }
        else
        {
            transform.localScale = originalScale;
        }

        float curSpeedX = canMove ? ((isRunning || isCrouching) ? (isRunning ? runningSpeed : crouchSpeed) : walkingSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? ((isRunning || isCrouching) ? (isRunning ? runningSpeed : crouchSpeed) : walkingSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpSpeed;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }
        
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }
        
        characterController.Move(moveDirection * Time.deltaTime);
        
        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }

    private void CheckItems()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (holdingItem)
            {
                currentItem.rigidbody.useGravity = true;
                currentItem = null;
                return;
            }

            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, itemMask))
            {
                currentItem = hit.rigidbody.GetComponent<InteractableItem>();
                currentItem.rigidbody.useGravity = false;
            }
        }
    }

    private void ItemPhysics()
    {
        if (holdingItem)
        {
            Vector3 currentPos = currentItem.rigidbody.position;
            Vector3 dir = pickupTarget.position - currentPos;

            Vector3 newVelocity = dir.normalized * dir.magnitude * lerpSpeed;
            currentItem.rigidbody.velocity = Vector3.Lerp(currentItem.rigidbody.velocity, newVelocity, Time.deltaTime * 10f);
        }
    }
}
}