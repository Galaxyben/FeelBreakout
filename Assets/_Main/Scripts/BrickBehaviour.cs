using UnityEngine;

public class BrickBehaviour : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            // Intentar spawnear power-up en la posición del ladrillo
            PowerUpManager powerUpManager = FindObjectOfType<PowerUpManager>();
            if (powerUpManager != null)
            {
                powerUpManager.TrySpawnPowerUp(transform.position);
            }

            PoolManager.Despawn(gameObject);
        }
    }
}
