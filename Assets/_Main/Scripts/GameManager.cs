using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player Stats")]
    public int lives = 3;
    public int score = 0;

    [Header("References")]
    public Camera mainCamera;
    public Transform ball;
    public Transform paddle;
    public Ball ballScript;
    public PowerUpManager powerUpManager;

    [Header("Colors")]
    public Color backgroundColor = Color.black;
    public Color brickColor = Color.white;

    private Vector3 ballStartPosition = Vector3.zero;
    private Vector3 paddleStartPosition = new Vector3(0, -4, 0);

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (mainCamera != null)
        {
            mainCamera.backgroundColor = backgroundColor;
        }

        ApplyBrickColors();
    }

    void Update()
    {
        if (ball != null && ballScript != null)
        {
            if (ball.position.y < ballScript.minY)
            {
                LoseLife();
            }
        }
    }

    void LoseLife()
    {
        lives--;
        Debug.Log("Lives: " + lives);

        ResetBallAndPaddle();

        if (lives <= 0)
        {
            GameOver();
        }
    }

    void ResetBallAndPaddle()
    {
        if (ball != null)
        {
            ball.position = ballStartPosition;
            Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
            if (ballRb != null)
            {
                ballRb.linearVelocity = Vector2.zero;
            }
        }

        if (paddle != null)
        {
            paddle.position = paddleStartPosition;
        }
    }

    public void AddScore(int points)
    {
        score += points;
        Debug.Log("Score: " + score);
    }

    void ApplyBrickColors()
    {
        GameObject[] bricks = GameObject.FindGameObjectsWithTag("Brick");
        foreach (GameObject brick in bricks)
        {
            SpriteRenderer sr = brick.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = brickColor;
            }
        }
    }

    void GameOver()
    {
        Debug.Log("Game Over! Final Score: " + score);
        // Aquí puedes agregar lógica adicional para game over
    }
}
