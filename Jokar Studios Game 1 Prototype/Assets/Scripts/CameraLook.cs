using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLook : MonoBehaviour
{
    public GameObject target; //the game object to rotate around
    public float speed = 10.0f; //rotation speed

    private Vector3 offset; //distance between camera and target

    void Start()
    {
        offset = transform.position - target.transform.position;
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        Debug.Log(horizontal);
        transform.RotateAround(target.transform.position, Vector3.up,speed * Time.deltaTime);
        //transform.position = target.transform.position + offset;
        transform.LookAt(target.transform.position);
    }
}
