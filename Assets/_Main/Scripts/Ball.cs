using System;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public float minY = -5f;
    public float maxSpeed = 15f;
    public GameObject ballPrefab;
    public int splitCount = 2;

    private Rigidbody2D rb;
    private bool splitOnNextHit = false;
    private bool hasAlreadySplit = false;

    void Start()
    {
        rb =  GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Clamp velocity
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Dividir pelota si el efecto está activo
        if (splitOnNextHit && !hasAlreadySplit)
        {
            SplitBall();
            hasAlreadySplit = true;
            splitOnNextHit = false;
        }
    }

    private void SplitBall()
    {
        if (ballPrefab == null)
        {
            // Si no hay prefab asignado, intentar usar el gameObject actual como referencia
            ballPrefab = gameObject;
        }

        for (int i = 0; i < splitCount; i++)
        {
            GameObject newBall = Instantiate(ballPrefab, transform.position, Quaternion.identity);
            Rigidbody2D newRb = newBall.GetComponent<Rigidbody2D>();

            if (newRb != null)
            {
                // Calcular ángulo de división
                float baseAngle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
                float offsetAngle = (i - splitCount / 2f) * 30f; // Separar 30 grados entre pelotas
                float finalAngle = (baseAngle + offsetAngle) * Mathf.Deg2Rad;

                Vector2 direction = new Vector2(Mathf.Cos(finalAngle), Mathf.Sin(finalAngle));
                newRb.linearVelocity = direction * rb.linearVelocity.magnitude * 0.8f;

                // Asegurar que la nueva pelota no se divida inmediatamente
                Ball newBallScript = newBall.GetComponent<Ball>();
                if (newBallScript != null)
                {
                    newBallScript.hasAlreadySplit = true;
                    newBallScript.splitOnNextHit = false;
                }
            }
        }
    }

    public void EnableSplitOnNextHit()
    {
        splitOnNextHit = true;
        hasAlreadySplit = false;
    }

    public void DisableSplitOnNextHit()
    {
        splitOnNextHit = false;
    }

    public void SetMaxSpeed(float newMaxSpeed)
    {
        maxSpeed = newMaxSpeed;
    }
}