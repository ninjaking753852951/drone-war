using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityUtils;

public class CameraController : Singleton<CameraController>
{
    public Transform yawTarget;  // Target for horizontal rotation (yaw)
    public Transform pitchTarget; // Target for vertical rotation (pitch)

    public Transform moveTarget;
    
    public float panSpeed = 10f; // Sensitivity of the mouse movement

    public float moveSpeed = 10;

    public float sprintMultiplier = 2;
    
    public float smoothFallOff = 5;

    public CinemachineVirtualCamera vCam;
    CinemachineTransposer bodyTransposer;
    public float zoomSpeed = 10;
    public float zoomExponential = 0.001f;
    float zoom = 10;
    
    
    private float yaw;
    private float pitch = 45;

    float yawDelta;
    float pitchDelta;
    float curMoveSpeed;

    // Start is called before the first frame update
    void Start()
    {
        bodyTransposer = vCam.GetCinemachineComponent<CinemachineTransposer>();

        if (GameManager.Instance.currentGameMode == GameMode.Build)
        {
            moveTarget.position = BuildingManager.Instance.spawnPoint;
        }
        
        /*if (GameManager.Instance.currentGameMode == GameMode.Battle)
        {
            Vector3 spawnPos = FindObjectOfType<PlayerDroneSpawner>().spawnPoint.position;
            spawnPos.y = 0;
            moveTarget.position = spawnPos;
        }*/
        
        // Initialize yaw and pitch with the current rotations
    }

    public void TeleportCamera(Vector3 pos)
    {
        moveTarget.position = pos;
    }
    
    // Update is called once per frame
    void Update()
    {
        
        if(Input.GetMouseButton(2) || GameManager.Instance.currentGameMode == GameMode.Build && Input.GetMouseButton(1))
            HandleRotation();
        else
        {
            yawDelta = Mathf.Lerp(yawDelta, 0, Time.deltaTime * smoothFallOff);
            pitchDelta = Mathf.Lerp(pitchDelta, 0, Time.deltaTime * smoothFallOff);
        }

        Vector3 moveDir = yawTarget.right * Input.GetAxis("Horizontal") + yawTarget.forward * Input.GetAxis("Vertical");


        curMoveSpeed = Input.GetKey(KeyCode.LeftShift) ? moveSpeed * sprintMultiplier : moveSpeed;
        
        moveTarget.Translate(moveDir * Time.deltaTime * curMoveSpeed);

        zoom -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed * (1 + zoom * zoomExponential);
        zoom = Mathf.Max(zoom, 1);

        bodyTransposer.m_FollowOffset = new Vector3(0, 0, -zoom);
        
        // Adjust yaw and pitch based on mouse input
        yaw += yawDelta;
        pitch -= pitchDelta;

        // Clamp the pitch rotation to prevent flipping the camera (e.g., between -90 and 90 degrees)
        pitch = Mathf.Clamp(pitch, -90f, 90f);

        // Apply the rotations to the yawTarget (horizontal rotation) and pitchTarget (vertical rotation)
        yawTarget.localRotation = Quaternion.Euler(0f, yaw, 0f); // Y-axis rotation (yaw)
        pitchTarget.localRotation = Quaternion.Euler(pitch, 0f, 0f); // X-axis rotation (pitch)
    }

    void HandleRotation()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * panSpeed * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * panSpeed * Time.deltaTime;

        // Adjust yaw and pitch based on mouse input
        yawDelta = mouseX;
        pitchDelta = mouseY;


    }
}
