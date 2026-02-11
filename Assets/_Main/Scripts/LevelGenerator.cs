using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Brick Settings")]
    [SerializeField] private GameObject brickPrefab;
    [SerializeField] private int rows = 5;
    [SerializeField] private int columns = 8;
    [SerializeField] private float brickSpacingX = 1.2f;
    [SerializeField] private float brickSpacingY = 0.6f;
    [SerializeField] private Vector3 startPosition = new Vector3(-4.2f, 3f, 0f);

    void Start()
    {
        if (brickPrefab == null)
        {
            Debug.LogError("LevelGenerator: Brick prefab is not assigned!");
            return;
        }

        // Configure pool limit for bricks
        int totalBricks = rows * columns;
        PoolManager.SetPoolLimit(brickPrefab, totalBricks);
        PoolManager.PreSpawn(brickPrefab, totalBricks);

        GenerateLevel();
    }

    void GenerateLevel()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Vector3 position = startPosition + new Vector3(col * brickSpacingX, -row * brickSpacingY, 0f);
                Transform brick = PoolManager.Spawn(brickPrefab, position, Quaternion.identity);

                if (brick == null)
                {
                    Debug.LogWarning($"LevelGenerator: Could not spawn brick at row {row}, column {col}");
                }
            }
        }
    }
}
