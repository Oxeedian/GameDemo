using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class CameraController : MonoBehaviour
{
    //private Camera gameCamera;
    [SerializeField] private float movementSpeed = 10;
    [SerializeField] private Camera camera;

    private float speed;
    private Vector3 moveDirection;
    

    //void Initialize(Camera camera)
    //{
        //gameCamera = camera;
    //}

    public void UpdateLoop(PlayerUnit players)
    {
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

    public float turnDuration = 0.2f; // seconds

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

    public void SetCameraPosition(Vector3 pos)
    {
        transform.position = pos;
    }
}
