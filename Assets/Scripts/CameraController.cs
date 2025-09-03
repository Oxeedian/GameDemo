using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class CameraController : MonoBehaviour
{
    //private Camera gameCamera;
    [SerializeField] private float movementSpeed = 10;
    [SerializeField] private Camera camera;
    public float turnDuration = 0.2f;

    private float speed;
    private Vector3 moveDirection;

    private Vector3 savedTransformPos;
    private Quaternion savedTransformRot;

    private bool stopUpdate = false;
    

    //void Initialize(Camera camera)
    //{
        //gameCamera = camera;
    //}

    public void UpdateLoop(PlayerUnit players)
    {
        if (stopUpdate)
        {

            return;
        }

        speed = movementSpeed * Time.deltaTime;
        moveDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            moveDirection += transform.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveDirection -= transform.forward;
        }
        if (Input.GetKey(KeyCode.A))
        {
            moveDirection -= transform.right;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveDirection += transform.right;
        }

        transform.position = transform.position + moveDirection * speed;

        if (Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(Rotate90Degrees(Vector3.down));
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            StartCoroutine(Rotate90Degrees(Vector3.up));
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            transform.position = players.transform.position;
        }
    }


    public Camera GetGameCamera()
    {
        return camera;
    }
    private IEnumerator Rotate90Degrees(Vector3 axis)
    {
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(axis * 90);
        float elapsed = 0f;

        while (elapsed < turnDuration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsed / turnDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = endRotation;
    }
    //private IEnumerator RotateTowardsAim()
    //{
    //    // Cache starting values
    //    Vector3 startPos = camera.transform.position;
    //    Quaternion startRot = camera.transform.rotation;

    //    // Target values from your aimTowardsTransform
    //    Vector3 targetPos = aimTowardsTransform.position;
    //    Quaternion targetRot = aimTowardsTransform.rotation;

    //    float elapsed = 0f;

    //    while (elapsed < turnDuration)
    //    {
    //        float t = elapsed / turnDuration;

    //        // Lerp position
    //        transform.position = Vector3.Lerp(startPos, targetPos, t);

    //        // Slerp rotation
    //        transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

    //        elapsed += Time.deltaTime/5;
    //        yield return null;
    //    }

    //    // Snap to final target values
    //    transform.position = targetPos;
    //    transform.rotation = targetRot;
    //}

    private IEnumerator RotateTowardsAim(Vector3 targetPos, Quaternion targetRot)
    {
        Vector3 startPos = camera.transform.position;
        Quaternion startRot = camera.transform.rotation;

        float elapsed = 0f;

        while (elapsed < turnDuration)
        {
            float t = elapsed / turnDuration;

            camera.transform.position = Vector3.Lerp(startPos, targetPos, t);
            camera.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        camera.transform.position = targetPos;
        camera.transform.rotation = targetRot;
    }

    public void SaveCameraMatrix()
    {

        savedTransformPos = camera.transform.position;
        savedTransformRot = camera.transform.rotation;
    }


    public void AimCamera(Vector3 currentPos,Vector3 lookAtPos)
    {
        

        Vector3 lookDir = (lookAtPos - currentPos).normalized;
        Vector3 offset = lookDir;
        Quaternion targetRot = Quaternion.LookRotation(lookDir);


        //aimTowardsPos = currentPos;
        //aimTowardsRot.SetLookRotation(lookDir);

        Vector3 right = Vector3.Cross(Vector3.up, lookDir).normalized;
        Vector3 up = Vector3.up;
        Vector3 back = (lookDir * -1).normalized;
        offset += (right *1.2f);
        offset += (up * 2);
        offset += back * 2;


        stopUpdate = true;
       // camera.transform.position = currentPos;
        StartCoroutine(RotateTowardsAim(currentPos + offset , targetRot));
    }

    public void ResetCamera()
    {
        StartCoroutine(RotateTowardsAim(savedTransformPos, savedTransformRot));
        stopUpdate = false;
    }

    public void SetCameraPosition(Vector3 pos)
    {
        transform.position = pos;
    }


}
