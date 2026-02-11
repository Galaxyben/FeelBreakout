using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 10f;
    [SerializeField] private float screenLimit = 8f;
    private float movementInput;

    public void OnMove(InputValue value)
    {
        movementInput = value.Get<float>();
    }

    void Update()
    {
        float moveDistance = movementInput * speed * Time.deltaTime;
        transform.Translate(new Vector3(moveDistance, 0, 0));
    }

    private void LateUpdate()
    {
        float xPos = Mathf.Clamp(transform.position.x, -screenLimit, screenLimit);
        transform.position = new Vector3(xPos, transform.position.y, transform.position.z);
    }
}
