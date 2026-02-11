using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [Header("Settings")]
    public PowerUpType powerUpType;
    public float fallSpeed = 3f;
    public Color powerUpColor = Color.yellow;

    [Header("Visual Feedback")]
    public SpriteRenderer spriteRenderer;

    private void Start()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = GetColorForType(powerUpType);
        }
    }

    private void Update()
    {
        // Mover hacia abajo
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

        // Destruir si sale de la pantalla
        if (transform.position.y < -6f)
        {
            PoolManager.Despawn(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Activar el power-up
            PowerUpManager powerUpManager = FindObjectOfType<PowerUpManager>();
            if (powerUpManager != null)
            {
                powerUpManager.ActivatePowerUp(powerUpType);
            }

            // Despawnear el power-up
            PoolManager.Despawn(gameObject);
        }
    }

    private Color GetColorForType(PowerUpType type)
    {
        switch (type)
        {
            case PowerUpType.ExpandPaddle:
                return Color.green;
            case PowerUpType.ShrinkPaddle:
                return Color.red;
            case PowerUpType.MultiBall:
                return Color.cyan;
            case PowerUpType.SplitBall:
                return Color.magenta;
            case PowerUpType.SpeedUp:
                return Color.yellow;
            case PowerUpType.SlowDown:
                return Color.blue;
            default:
                return Color.white;
        }
    }

    public void Initialize(PowerUpType type)
    {
        powerUpType = type;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = GetColorForType(type);
        }
    }
}
