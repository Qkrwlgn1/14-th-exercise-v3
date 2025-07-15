using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float walkSpeed;
    [SerializeField]
    private float runSpeed;
    [SerializeField]
    private float crouchSpeed;
    [SerializeField]
    private LayerMask layerMask;

    private float applySpeed;

    [SerializeField]
    private float jumpForce;

    private bool isWalk = false;
    private bool isRun = false;
    private bool isCrouch = false;
    private bool isGround = true;

    private Vector3 lastPos;

    [SerializeField]
    private float crouchPosY;
    private float originPosY;
    private float applyCrouchPosY; 

    private CapsuleCollider capusleCollider;

    [SerializeField]
    private float lookSensitivity;

    [SerializeField]
    private float cameraRotationLimit;
    private float currentCameraRotationX = 0f;  

    [SerializeField]
    private Camera theCamera;
    private Rigidbody myRigid;
    private GunController gunController;
    private CrossHair crosshair;
    private StatusController statusController;


    void Start()
    {
        capusleCollider = GetComponent<CapsuleCollider>();
        myRigid = GetComponent<Rigidbody>();
        gunController = FindObjectOfType<GunController>();
        crosshair = FindObjectOfType<CrossHair>();
        statusController = FindObjectOfType<StatusController>();


        // ÃÊ±âÈ­
        applySpeed = walkSpeed;
        originPosY = theCamera.transform.localPosition.y;
        applyCrouchPosY = originPosY;   
    }


    void Update()
    {
        IsGround();
        TryJump();
        TryRun();
        TryCrouch();
        Move();
        MoveCheck();
        if (!Inventory.inventoryActivated)
        {
            CameraRotation();
            CharacterRotation();
        }
    }


    private void TryCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Crouch();
        }
    }


    private void Crouch()
    {
        isCrouch = !isCrouch;
        crosshair.CrouchingAnimation(isCrouch);

        if (isCrouch)
        {
            applySpeed = crouchSpeed;
            applyCrouchPosY = crouchPosY;
        }
        else
        {
            applySpeed = walkSpeed;
            applyCrouchPosY = originPosY;
        }

        StartCoroutine(CrouchCoroutine());
    }


    IEnumerator CrouchCoroutine()
    {
        float _posY = theCamera.transform.localPosition.y;
        int count = 0;

        while (_posY != applyCrouchPosY)
        {
            count ++;
            _posY = Mathf.Lerp(_posY, applyCrouchPosY, 0.3f);
            theCamera.transform.localPosition = new Vector3(0, _posY, 0);
            if (count > 15)
            {
                break;
            }
            yield return null;
        }
        theCamera.transform.localPosition = new Vector3(0, applyCrouchPosY, 0);
    }


    private void IsGround()
    {
        isGround = Physics.Raycast(transform.position, Vector3.down, capusleCollider.bounds.extents.y + 0.3f, layerMask);
        crosshair.JumpingAnimation(!isGround);
    }


    private void TryJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && statusController.GetCurrentSP() > 0 && isGround)
        {
            Jump();
        }
    }


    private void Jump()
    {
        if (isCrouch)
            Crouch();
        statusController.DecreaseStamina(100);
        myRigid.velocity = transform.up * jumpForce;
    }


    private void TryRun()
    {
        if (Input.GetKey(KeyCode.LeftShift) && statusController.GetCurrentSP() > 0)
        {
            Runnig();
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift) || statusController.GetCurrentSP() <= 0)
        {
            RunnigCancel();
        }
    }


    private void Runnig()
    {
        if(isCrouch)
            Crouch();

        gunController.CancelFineSight();
        statusController.DecreaseStamina(5);
        applySpeed = runSpeed;
        isRun = true;
        crosshair.RunningAnimation(isRun);
    }


    private void RunnigCancel()
    {
        isRun = false;
        crosshair.RunningAnimation(isRun);
        applySpeed = walkSpeed;
    }


    private void Move()
    {
        float _moveDirX = Input.GetAxis("Horizontal");
        float _moveDirZ = Input.GetAxis("Vertical");

        Vector3 _moveHorizontal = transform.right * _moveDirX;
        Vector3 _moveVertical = transform.forward * _moveDirZ;

        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * applySpeed;

        myRigid.MovePosition(transform.position + _velocity * Time.deltaTime);
    }


    private void MoveCheck()
    {
        if (!isRun && !isCrouch && isGround)
        {
            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            {
                isWalk = true;
            }
            else
            {
                isWalk = false;
            }

            crosshair.WalkingAnimation(isWalk);
            lastPos = transform.position;
        }
    }


    private void CameraRotation()
    {
        float _xRotation = Input.GetAxis("Mouse Y");
        float _cameraRotationX = _xRotation * lookSensitivity;
        currentCameraRotationX -= _cameraRotationX;
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, - cameraRotationLimit, cameraRotationLimit);

        theCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
    }


    private void CharacterRotation()
    {
        float _yRotation = Input.GetAxis("Mouse X");
        Vector3 _characterRotationY = new Vector3(0f, _yRotation * lookSensitivity, 0f);
        myRigid.MoveRotation(myRigid.rotation * Quaternion.Euler(_characterRotationY));
    }
}
