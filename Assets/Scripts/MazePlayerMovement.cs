using UnityEngine;
using System.Collections;

public class MazePlayerMovement : MonoBehaviour
{
    [Header("移動設定")]
    public float moveUnit = 1f;
    public float moveDuration = 0.2f;

    [Header("体の傾き制御設定")]
    public float inputThreshold = 0.3f; // 移動を開始する傾きの閾値
    public float continuousMoveCooldown = 0.3f; // 連続移動の間隔

    private MazeGenerator mazeGenerator;
    private bool isMoving = false;
    private Vector2Int currentGridPosition;

    // 体の傾き制御用
    private Vector2 currentInputDirection = Vector2.zero;
    private float lastMoveTime = 0f;
    private bool useBodyControl = false;

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
        // キーボード入力による移動（体の傾き制御が無効な場合のみ）
        if (!isMoving && !useBodyControl)
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

        // 体の傾き制御による移動処理
        if (useBodyControl && !isMoving && Time.time - lastMoveTime > continuousMoveCooldown)
        {
            ProcessBodyInput();
        }
    }

    // Reactからの体の傾きデータを受信するメソッド
    public void SetMovementDirection(string directionData)
    {
        try
        {
            useBodyControl = true;
            MovementData data = JsonUtility.FromJson<MovementData>(directionData);

            // 入力方向を更新
            currentInputDirection = new Vector2(data.x, data.y);

            Debug.Log($"Body input received: X={data.x:F2}, Y={data.y:F2}");
        }
        catch (System.Exception e)
        {
            Debug.LogError("移動データの解析エラー: " + e.Message);
        }
    }

    // 体の傾き入力を処理して移動コマンドに変換
    void ProcessBodyInput()
    {
        // 閾値を超えた場合のみ移動
        if (Mathf.Abs(currentInputDirection.x) > inputThreshold || Mathf.Abs(currentInputDirection.y) > inputThreshold)
        {
            // より強い傾きの軸を優先
            if (Mathf.Abs(currentInputDirection.x) > Mathf.Abs(currentInputDirection.y))
            {
                // 水平移動
                if (currentInputDirection.x > inputThreshold)
                {
                    Debug.Log("Body control: Moving Right");
                    MoveRight();
                }
                else if (currentInputDirection.x < -inputThreshold)
                {
                    Debug.Log("Body control: Moving Left");
                    MoveLeft();
                }
            }
            else
            {
                // 垂直移動（Yの方向を調整）
                if (currentInputDirection.y > inputThreshold)
                {
                    Debug.Log("Body control: Moving Down");
                    MoveDown(); // 画面座標系では正のYが下向き
                }
                else if (currentInputDirection.y < -inputThreshold)
                {
                    Debug.Log("Body control: Moving Up");
                    MoveUp();
                }
            }

            lastMoveTime = Time.time;
        }
    }

    // キーボード制御に戻すメソッド
    public void DisableBodyControl()
    {
        useBodyControl = false;
        currentInputDirection = Vector2.zero;
        Debug.Log("Body control disabled, switched to keyboard control");
    }

    // 既存の移動メソッド群
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
                Debug.Log("Hit wall - movement stopped");
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

    public bool IsUsingBodyControl()
    {
        return useBodyControl;
    }
}

[System.Serializable]
public class MovementData
{
    public float x;
    public float y;
}
