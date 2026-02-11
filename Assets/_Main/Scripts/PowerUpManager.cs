using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    [Header("Power-Up Settings")]
    [SerializeField] private GameObject powerUpPrefab;
    [SerializeField] [Range(0f, 1f)] private float dropChance = 0.3f;
    [SerializeField] private float effectDuration = 10f;

    [Header("Paddle Settings")]
    [SerializeField] private float expandedScale = 1.5f;
    [SerializeField] private float shrunkScale = 0.5f;
    [SerializeField] private float scaleTransitionSpeed = 5f;

    [Header("Ball Settings")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private float speedMultiplierFast = 1.5f;
    [SerializeField] private float speedMultiplierSlow = 0.6f;
    [SerializeField] private int multiBallCount = 2;

    [Header("References")]
    [SerializeField] private Transform paddle;

    private Dictionary<PowerUpType, Coroutine> activeEffects = new Dictionary<PowerUpType, Coroutine>();
    private Vector3 originalPaddleScale;
    private float originalBallMaxSpeed;
    private bool splitBallActive = false;

    private void Start()
    {
        if (paddle != null)
        {
            originalPaddleScale = paddle.localScale;
        }

        // Configurar pool de power-ups
        if (powerUpPrefab != null)
        {
            PoolManager.SetPoolLimit(powerUpPrefab, 20);
            PoolManager.PreSpawn(powerUpPrefab, 10);
        }
    }

    public void TrySpawnPowerUp(Vector3 position)
    {
        if (powerUpPrefab == null) return;

        if (Random.value <= dropChance)
        {
            PowerUpType randomType = (PowerUpType)Random.Range(0, System.Enum.GetValues(typeof(PowerUpType)).Length);
            SpawnPowerUp(randomType, position);
        }
    }

    private void SpawnPowerUp(PowerUpType type, Vector3 position)
    {
        Transform powerUpTransform = PoolManager.Spawn(powerUpPrefab, position, Quaternion.identity);

        if (powerUpTransform != null)
        {
            PowerUp powerUp = powerUpTransform.GetComponent<PowerUp>();
            if (powerUp != null)
            {
                powerUp.Initialize(type);
            }
        }
    }

    public void ActivatePowerUp(PowerUpType type)
    {
        Debug.Log($"Power-Up activado: {type}");

        // Cancelar efecto anterior del mismo tipo si existe
        if (activeEffects.ContainsKey(type) && activeEffects[type] != null)
        {
            StopCoroutine(activeEffects[type]);
        }

        switch (type)
        {
            case PowerUpType.ExpandPaddle:
                activeEffects[type] = StartCoroutine(PaddleScaleEffect(expandedScale));
                break;

            case PowerUpType.ShrinkPaddle:
                activeEffects[type] = StartCoroutine(PaddleScaleEffect(shrunkScale));
                break;

            case PowerUpType.MultiBall:
                SpawnMultipleBalls();
                break;

            case PowerUpType.SplitBall:
                activeEffects[type] = StartCoroutine(SplitBallEffect());
                break;

            case PowerUpType.SpeedUp:
                activeEffects[type] = StartCoroutine(BallSpeedEffect(speedMultiplierFast));
                break;

            case PowerUpType.SlowDown:
                activeEffects[type] = StartCoroutine(BallSpeedEffect(speedMultiplierSlow));
                break;
        }
    }

    private IEnumerator PaddleScaleEffect(float targetScale)
    {
        if (paddle == null) yield break;

        Vector3 targetScaleVector = new Vector3(targetScale, originalPaddleScale.y, originalPaddleScale.z);

        // Animar hacia la escala objetivo
        while (Vector3.Distance(paddle.localScale, targetScaleVector) > 0.01f)
        {
            paddle.localScale = Vector3.Lerp(paddle.localScale, targetScaleVector, Time.deltaTime * scaleTransitionSpeed);
            yield return null;
        }

        paddle.localScale = targetScaleVector;

        // Esperar duración
        yield return new WaitForSeconds(effectDuration);

        // Animar de vuelta a la escala original
        while (Vector3.Distance(paddle.localScale, originalPaddleScale) > 0.01f)
        {
            paddle.localScale = Vector3.Lerp(paddle.localScale, originalPaddleScale, Time.deltaTime * scaleTransitionSpeed);
            yield return null;
        }

        paddle.localScale = originalPaddleScale;
    }

    private void SpawnMultipleBalls()
    {
        Ball[] existingBalls = FindObjectsOfType<Ball>();

        if (existingBalls.Length == 0) return;

        Ball originalBall = existingBalls[0];
        Vector3 spawnPosition = originalBall.transform.position;

        for (int i = 0; i < multiBallCount; i++)
        {
            if (ballPrefab != null)
            {
                GameObject newBall = Instantiate(ballPrefab, spawnPosition, Quaternion.identity);
                Rigidbody2D rb = newBall.GetComponent<Rigidbody2D>();

                if (rb != null)
                {
                    // Dar velocidad en dirección diferente
                    float angle = Random.Range(30f, 150f);
                    Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                    rb.linearVelocity = direction * originalBall.maxSpeed * 0.7f;
                }
            }
        }
    }

    private IEnumerator SplitBallEffect()
    {
        splitBallActive = true;

        Ball[] balls = FindObjectsOfType<Ball>();
        foreach (Ball ball in balls)
        {
            ball.EnableSplitOnNextHit();
        }

        yield return new WaitForSeconds(effectDuration);

        splitBallActive = false;

        balls = FindObjectsOfType<Ball>();
        foreach (Ball ball in balls)
        {
            ball.DisableSplitOnNextHit();
        }
    }

    private IEnumerator BallSpeedEffect(float multiplier)
    {
        Ball[] balls = FindObjectsOfType<Ball>();

        // Guardar velocidades originales y aplicar multiplicador
        Dictionary<Ball, float> originalSpeeds = new Dictionary<Ball, float>();
        foreach (Ball ball in balls)
        {
            originalSpeeds[ball] = ball.maxSpeed;
            ball.SetMaxSpeed(ball.maxSpeed * multiplier);
        }

        yield return new WaitForSeconds(effectDuration);

        // Restaurar velocidades originales
        foreach (var kvp in originalSpeeds)
        {
            if (kvp.Key != null)
            {
                kvp.Key.SetMaxSpeed(kvp.Value);
            }
        }
    }

    public bool IsSplitBallActive()
    {
        return splitBallActive;
    }
}
