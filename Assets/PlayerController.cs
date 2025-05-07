using UnityEngine;
using System.Collections;
using System.Security.Cryptography.X509Certificates;

public class PlayerController : MonoBehaviour
{
    public MazeGenerator mazeGen;
    public CameraController cameraController;
    public UIManager uiManager;
    public float moveDuration = 0.2f; // 1マスの移動にかかる時間
    public float moveInterval = 0.3f; // 連続移動のインターバル（押しっぱなしで繰り返し移動）

    private int[,] maze;
    private Vector2Int currentPos;
    private float cellSize;
    private bool isMoving = false;

    private Vector2Int moveInput = Vector2Int.zero;
    private bool hasMoved = false;//最初の移動フラグ

    public Animator animator;

    public int brokenCount;
    public int maxAttack;
    public bool Attack = false;

    void Start()
    {
        brokenCount = 3;
        maxAttack = 0;
        animator = GetComponent<Animator>();    
        maze = mazeGen.GetMaze();
        cellSize = mazeGen.cellSize;
        currentPos = mazeGen.GetStartPos();
        transform.position = new Vector3(currentPos.x * cellSize, 0f, currentPos.y * cellSize);

        uiManager = FindObjectOfType<UIManager>();  
    }

    void Update()
    {
        if (!isMoving)
        {
            moveInput = GetMoveInput();

            if (moveInput != Vector2Int.zero)
            {
                if (!hasMoved)
                {
                    hasMoved = true;
                    //cameraController.SwitchToFPS(); // 最初の移動でFPSモードに切り替え
                }

                StartCoroutine(MoveContinuous());
            }
        }

        if (Input.GetKeyUp(KeyCode.Q) && brokenCount > 0)
        {
            if(maxAttack < 3)
            {
                animator.SetTrigger("isAttack");
                maxAttack++;
                if(maxAttack == 3)
                {
                    brokenCount--;
                    maxAttack = 0;
                }
            }
        }
    }

    public void OnButtonAChanged(int state)
    {
        if (state == 1 && brokenCount > 0)
        {
            if (maxAttack < 3)
            {
                animator.SetTrigger("isAttack");
                maxAttack++;
                if (maxAttack == 3)
                {
                    brokenCount--;
                    maxAttack = 0;
                }
            }
        }
    }

    // キー入力を取得
    Vector2Int GetMoveInput()
    {
        int x = 0, y = 0;

        // キーボード操作
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) y = 1;
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) y = -1;

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x = 1;
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x = -1;


        return new Vector2Int(x, y);
    }


    // 通路チェック
    bool IsWalkable(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < maze.GetLength(0) &&
               pos.y >= 0 && pos.y < maze.GetLength(1) &&
               maze[pos.x, pos.y] == 0;
    }

    // 連続移動のコルーチン
    IEnumerator MoveContinuous()
    {
        while (moveInput != Vector2Int.zero)
        {
            Vector2Int next = currentPos + moveInput;

            if (IsWalkable(next))
            {
                Vector3 startPos = transform.position;
                Vector3 endPos = new Vector3(next.x * cellSize, 0.5f, next.y * cellSize);

                yield return MoveSmoothly(startPos, endPos, next);
            }

            yield return new WaitForSeconds(moveInterval); // 移動間隔
        }
    }

    // スムーズな移動
    IEnumerator MoveSmoothly(Vector3 start, Vector3 end, Vector2Int nextPos)
    {
        isMoving = true;
        float elapsed = 0f;

        // ★進行方向のベクトルを計算
        Vector3 direction = (end - start).normalized;

        // ★進行方向を向くように回転（Y軸のみ回転）
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation;
        }

        while (elapsed < moveDuration)
        {
            transform.position = Vector3.Lerp(start, end, elapsed / moveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = end;
        currentPos = nextPos;
        isMoving = false;

        if (currentPos == mazeGen.GetGoalPos())
        {
            Debug.Log("ゴールに到達しました！");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            uiManager.Gameover();
            Time.timeScale = 0;
        }
    }
}
