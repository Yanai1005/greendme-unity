using UnityEngine;
using System.Collections;

public class MazePlayerMovement : MonoBehaviour
{
    [Header("移動設定")]
    public float moveUnit = 1f;
    public float moveDuration = 0.2f;

    private MazeGenerator mazeGenerator;
    private bool isMoving = false;
    private Vector2Int currentGridPosition;

    void Start()
    {
        Debug.Log("MazePlayerMovement Start() called");

        // MazeGeneratorを取得
        mazeGenerator = FindObjectOfType<MazeGenerator>();

        if (mazeGenerator != null)
        {
            Debug.Log("MazeGenerator found!");
            // 少し待ってから位置を設定（迷路生成完了を待つ）
            StartCoroutine(SetInitialPosition());
        }
        else
        {
            Debug.LogError("MazeGenerator not found!");
        }
    }

    System.Collections.IEnumerator SetInitialPosition()
    {
        Debug.Log("SetInitialPosition started...");

        // より長く待機して、迷路の生成を確実に待つ
        yield return new WaitForSeconds(0.5f);

        Debug.Log("Setting initial position...");
        Debug.Log("Player start position: " + mazeGenerator.PlayerStartPosition);

        // プレイヤーを開始位置に配置
        SetPlayerPosition(mazeGenerator.PlayerStartPosition);

        Debug.Log("Player positioned at: " + transform.position);
        Debug.Log("Current grid position: " + currentGridPosition);
    }

    void SetPlayerPosition(Vector2Int gridPos)
    {
        Debug.Log("SetPlayerPosition called with: " + gridPos);
        currentGridPosition = gridPos;
        Vector3 worldPos = mazeGenerator.GridToWorldPosition(gridPos.x, gridPos.y);
        Debug.Log("Calculated world position: " + worldPos);
        transform.position = worldPos;
        Debug.Log("Transform.position set to: " + transform.position);
    }

    void Update()
    {
        // WASDキーでの移動入力処理
        if (!isMoving)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                Debug.Log("W key pressed - Moving Up");
                MoveUp();
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                Debug.Log("S key pressed - Moving Down");
                MoveDown();
            }
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Debug.Log("A key pressed - Moving Left");
                MoveLeft();
            }
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                Debug.Log("D key pressed - Moving Right");
                MoveRight();
            }
        }
    }

    public void MoveRight(int steps = 1)
    {
        if (!isMoving)
            StartCoroutine(MoveToDirection(Vector2Int.right, steps));
    }

    public void MoveLeft(int steps = 1)
    {
        if (!isMoving)
            StartCoroutine(MoveToDirection(Vector2Int.left, steps));
    }

    public void MoveUp(int steps = 1)
    {
        if (!isMoving)
            StartCoroutine(MoveToDirection(Vector2Int.up, steps));
    }

    public void MoveDown(int steps = 1)
    {
        if (!isMoving)
            StartCoroutine(MoveToDirection(Vector2Int.down, steps));
    }

    IEnumerator MoveToDirection(Vector2Int direction, int steps)
    {
        isMoving = true;

        for (int i = 0; i < steps; i++)
        {
            Vector2Int newGridPos = currentGridPosition + direction;

            // 壁判定
            if (mazeGenerator.IsPath(newGridPos.x, newGridPos.y))
            {
                // 移動可能な場合
                Vector3 startPos = transform.position;
                Vector3 endPos = mazeGenerator.GridToWorldPosition(newGridPos.x, newGridPos.y);

                // スムーズな移動アニメーション
                float elapsed = 0f;
                while (elapsed < moveDuration)
                {
                    transform.position = Vector3.Lerp(startPos, endPos, elapsed / moveDuration);
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                transform.position = endPos;
                currentGridPosition = newGridPos;
            }
            else
            {
                // 壁にぶつかった場合は移動を停止
                break;
            }

            // 複数ステップの場合は少し待機
            if (i < steps - 1)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }

        isMoving = false;
    }

    public Vector2Int GetCurrentGridPosition()
    {
        return currentGridPosition;
    }

    public bool IsMoving()
    {
        return isMoving;
    }
}
