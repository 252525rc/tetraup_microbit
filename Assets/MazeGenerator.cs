using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class MazeGenerator : MonoBehaviour
{
    public int width = 62;  // 奇数推奨
    public int height = 62;
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public float cellSize = 2f;
    public Transform playerTransform;

    public GameObject startMarkerPrefab;
    public GameObject goalMarkerPrefab;
    public GameObject enemyPrefab;

    public Vector2Int startPos;
    private Vector2Int goalPos;
    private List<Vector2Int> enemyPositions = new List<Vector2Int>();

    public static int[,] maze;

    public int[,] GetMaze() => maze;
    public Vector2Int GetStartPos() => startPos;
    public Vector2Int GetGoalPos() => goalPos;
    private List<Vector2Int> corners = new List<Vector2Int>();

    public ConnectButton connectButton;

    private void Awake()
    {
        GenerateMaze();
        SetStartGoalAndEnemies();
        DrawMaze3D();

    }

    void Start()
    {

    }

    void GenerateMaze()
    {
        maze = new int[width, height];

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                maze[x, y] = 1; // 全部壁にして初期化

        RecursiveDig(1, 1);
    }

    void RecursiveDig(int x, int y)
    {
        maze[x, y] = 0; // 通路

        List<Vector2Int> directions = new List<Vector2Int>
        {
            new Vector2Int(2, 0),
            new Vector2Int(-2, 0),
            new Vector2Int(0, 2),
            new Vector2Int(0, -2)
        };
        Shuffle(directions);

        foreach (var dir in directions)
        {
            int nx = x + dir.x;
            int ny = y + dir.y;

            if (IsInBounds(nx, ny) && maze[nx, ny] == 1)
            {
                maze[x + dir.x / 2, y + dir.y / 2] = 0; // 壁を壊す
                RecursiveDig(nx, ny);
            }
        }
    }

    void DrawMaze3D()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(x * cellSize, 0, y * cellSize);

                if (floorPrefab != null)
                {
                    Instantiate(floorPrefab, pos + Vector3.down * 1.5f, Quaternion.identity, transform);
                }

                if (maze[x, y] == 1) // 壁
                {
                    GameObject wall = Instantiate(wallPrefab, pos, Quaternion.identity, transform);

                    WallColor script = wall.GetComponent<WallColor>();
                    if (script != null)
                    {
                        script.x = x;
                        script.y = y;
                    }
                }

            }
        }

        // スタートマーカーをプレイヤーとして生成
        if (startMarkerPrefab != null)
        {
            Vector3 spos = new Vector3(startPos.x * cellSize, -1f, startPos.y * cellSize);
            GameObject player = Instantiate(startMarkerPrefab, spos, Quaternion.identity, transform);
            playerTransform = player.transform;

            // PlayerControllerを追加し、MazeGeneratorの参照を渡す
            PlayerController playerController = player.AddComponent<PlayerController>();
            playerController.mazeGen = this;
            connectButton.target = player;
            
        }

        // ゴールマーカー
        if (goalMarkerPrefab != null)
        {
            Vector3 gpos = new Vector3(goalPos.x * cellSize, 0.5f, goalPos.y * cellSize);
            Instantiate(goalMarkerPrefab, gpos, Quaternion.identity, transform);
        }

        // 敵の生成
        foreach (Vector2Int enemyPos in enemyPositions)
        {
            Vector3 epos = new Vector3(enemyPos.x * cellSize, 0.5f, enemyPos.y * cellSize);
            GameObject enemy = Instantiate(enemyPrefab, epos, Quaternion.identity, transform);

            var controller = enemy.GetComponent<EnemyController>();
            if (controller == null)
            {
                controller = enemy.AddComponent<EnemyController>();
            }

            controller.mazeGen = this;
            controller.player = playerTransform; 
        }

    }

    void Shuffle(List<Vector2Int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            var temp = list[i];
            list[i] = list[rand];
            list[rand] = temp;
        }
    }

    bool IsInBounds(int x, int y)
    {
        return x > 0 && x < width - 1 && y > 0 && y < height - 1;
    }

    void SetStartGoalAndEnemies()
    {
        corners.Clear();
        corners.AddRange(new List<Vector2Int>
        {
            new Vector2Int(1, 1),
            new Vector2Int(1, height - 2),
            new Vector2Int(width - 2, 1),
            new Vector2Int(width - 2, height - 2)
        });


        Shuffle(corners);
        startPos = corners[0];
        goalPos = corners[1];

        // 残りの2つの角に敵を配置
        enemyPositions.Add(corners[2]);
        enemyPositions.Add(corners[3]);

        // 通路にする
        maze[startPos.x, startPos.y] = 0;
        maze[goalPos.x, goalPos.y] = 0;
        foreach (var pos in enemyPositions)
        {
            maze[pos.x, pos.y] = 0;
        }
    }
}
