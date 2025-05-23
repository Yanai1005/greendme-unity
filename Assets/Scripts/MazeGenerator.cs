using UnityEngine;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour
{
    [Header("迷路設定")]
    public int width = 21;  // 奇数で設定（壁込みのサイズ）
    public int height = 21; // 奇数で設定（壁込みのサイズ）
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject goalPrefab;

    [Header("スプライト設定")]
    public Sprite wallSprite;
    public Sprite floorSprite;
    public Color wallColor = Color.black;
    public Color floorColor = Color.white;
    public Color goalColor = Color.yellow;

    private int[,] maze;
    private int mazeWidth;
    private int mazeHeight;

    // 0: 通路, 1: 壁
    private const int PATH = 0;
    private const int WALL = 1;

    public Vector2Int PlayerStartPosition { get; private set; }
    public Vector2Int GoalPosition { get; private set; }

    public void Start()
    {
        GenerateMaze();
        CreateMazeVisual();
    }

    void GenerateMaze()
    {
        mazeWidth = width;
        mazeHeight = height;
        maze = new int[mazeWidth, mazeHeight];

        // 全体を壁で初期化
        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                maze[x, y] = WALL;
            }
        }

        // 棒倒し法で迷路生成
        CreateMazeWithStickMethod();

        // プレイヤー開始位置とゴール位置を設定
        SetStartAndGoalPositions();
    }

    void CreateMazeWithStickMethod()
    {
        // 深さ優先探索で迷路生成
        CreateMazeWithDFS();
    }

    void CreateMazeWithDFS()
    {
        // 訪問済みかどうかを記録する配列
        bool[,] visited = new bool[mazeWidth, mazeHeight];

        // DFS用のスタック
        Stack<Vector2Int> stack = new Stack<Vector2Int>();

        // 開始位置（左上角の通路部分）
        Vector2Int start = new Vector2Int(1, 1);
        stack.Push(start);
        visited[start.x, start.y] = true;
        maze[start.x, start.y] = PATH;

        // 方向ベクトル（上下左右、2つずつ移動）
        Vector2Int[] directions = {
            new Vector2Int(0, -2), // 上
            new Vector2Int(2, 0),  // 右
            new Vector2Int(0, 2),  // 下
            new Vector2Int(-2, 0)  // 左
        };

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Peek();
            List<Vector2Int> neighbors = new List<Vector2Int>();

            // 移動可能な隣接セルを探す
            foreach (Vector2Int dir in directions)
            {
                Vector2Int next = current + dir;

                // 境界チェックと訪問済みチェック
                if (IsInBounds(next.x, next.y) && !visited[next.x, next.y])
                {
                    neighbors.Add(next);
                }
            }

            if (neighbors.Count > 0)
            {
                // ランダムに隣接セルを選択
                Vector2Int chosen = neighbors[Random.Range(0, neighbors.Count)];

                // 選択したセルを訪問済みにする
                visited[chosen.x, chosen.y] = true;
                maze[chosen.x, chosen.y] = PATH;

                // 現在位置と選択したセルの間の壁を除去
                Vector2Int wall = current + (chosen - current) / 2;
                maze[wall.x, wall.y] = PATH;

                // 選択したセルをスタックにプッシュ
                stack.Push(chosen);
            }
            else
            {
                // 移動できる場所がない場合はバックトラック
                stack.Pop();
            }
        }

        // 角の通路を確保（必要に応じて）
        EnsureCornerAccess();

        // 行き止まりを減らすために追加の通路を作成（オプション）
        CreateAdditionalPaths();
    }

    void EnsureCornerAccess()
    {
        // 四隅の角への通路を確保
        Vector2Int[] corners = {
            new Vector2Int(1, 1),                           // 左上
            new Vector2Int(mazeWidth - 2, 1),               // 右上  
            new Vector2Int(1, mazeHeight - 2),              // 左下
            new Vector2Int(mazeWidth - 2, mazeHeight - 2)   // 右下
        };

        foreach (Vector2Int corner in corners)
        {
            maze[corner.x, corner.y] = PATH;

            // 各角から内側への通路を確保
            if (corner.x == 1) // 左側の角
            {
                maze[corner.x + 1, corner.y] = PATH;
            }
            else // 右側の角
            {
                maze[corner.x - 1, corner.y] = PATH;
            }

            if (corner.y == 1) // 上側の角
            {
                maze[corner.x, corner.y + 1] = PATH;
            }
            else // 下側の角
            {
                maze[corner.x, corner.y - 1] = PATH;
            }
        }
    }

    void CreateAdditionalPaths()
    {
        // ランダムに追加の通路を作成（迷路を少し簡単にする）
        int additionalPaths = Random.Range(3, 8);

        for (int i = 0; i < additionalPaths; i++)
        {
            int x = Random.Range(1, mazeWidth - 1);
            int y = Random.Range(1, mazeHeight - 1);

            // 壁の場合のみ通路に変更
            if (maze[x, y] == WALL)
            {
                // 隣接する通路が2つ以下の場合のみ通路を作成
                int adjacentPaths = CountAdjacentPaths(x, y);
                if (adjacentPaths <= 2)
                {
                    maze[x, y] = PATH;
                }
            }
        }
    }

    int CountAdjacentPaths(int x, int y)
    {
        int count = 0;
        Vector2Int[] directions = {
            new Vector2Int(0, -1), // 上
            new Vector2Int(1, 0),  // 右
            new Vector2Int(0, 1),  // 下
            new Vector2Int(-1, 0)  // 左
        };

        foreach (Vector2Int dir in directions)
        {
            int newX = x + dir.x;
            int newY = y + dir.y;

            if (IsInBounds(newX, newY) && maze[newX, newY] == PATH)
            {
                count++;
            }
        }

        return count;
    }

    void SetStartAndGoalPositions()
    {
        // プレイヤーを左上角に配置
        PlayerStartPosition = new Vector2Int(1, 1);

        // ゴールを中央に配置
        int centerX = mazeWidth / 2;
        int centerY = mazeHeight / 2;

        // 中央が壁の場合は通路にする
        maze[centerX, centerY] = PATH;
        GoalPosition = new Vector2Int(centerX, centerY);
    }

    void CreateMazeVisual()
    {
        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                Vector3 position = new Vector3(x - mazeWidth / 2f, -(y - mazeHeight / 2f), 0);

                if (maze[x, y] == WALL)
                {
                    CreateTile(position, wallSprite, wallColor, "Wall");
                }
                else
                {
                    CreateTile(position, floorSprite, floorColor, "Floor");

                    // ゴール位置にゴールオブジェクトを配置
                    if (x == GoalPosition.x && y == GoalPosition.y)
                    {
                        CreateGoal(position);
                    }
                }
            }
        }
    }

    void CreateTile(Vector3 position, Sprite sprite, Color color, string name)
    {
        GameObject tile = new GameObject(name);
        tile.transform.parent = transform;
        tile.transform.position = position;

        SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
        sr.sprite = sprite ? sprite : CreateSquareSprite();
        sr.color = color;

        if (name == "Wall")
        {
            // 壁にコライダーを追加
            BoxCollider2D collider = tile.AddComponent<BoxCollider2D>();
            collider.isTrigger = false;
        }
    }

    void CreateGoal(Vector3 position)
    {
        GameObject goal = new GameObject("Goal");
        goal.transform.parent = transform;
        goal.transform.position = position;
        goal.transform.localScale = Vector3.one * 0.8f;

        SpriteRenderer sr = goal.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color = goalColor;
        sr.sortingOrder = 1;

        // ゴールにトリガーコライダーを追加
        BoxCollider2D collider = goal.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        // ゴール判定スクリプトを追加
        goal.AddComponent<GoalTrigger>();
    }

    Sprite CreateSquareSprite()
    {
        Texture2D texture = new Texture2D(64, 64);
        Color[] pixels = new Color[64 * 64];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }
        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
    }

    bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < mazeWidth && y >= 0 && y < mazeHeight;
    }

    public bool IsWall(int x, int y)
    {
        if (!IsInBounds(x, y)) return true;
        return maze[x, y] == WALL;
    }

    public bool IsPath(int x, int y)
    {
        if (!IsInBounds(x, y)) return false;
        return maze[x, y] == PATH;
    }

    public Vector3 GridToWorldPosition(int x, int y)
    {
        return new Vector3(x - mazeWidth / 2f, -(y - mazeHeight / 2f), 0);
    }

    public Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x + mazeWidth / 2f);
        int y = Mathf.RoundToInt(-worldPos.y + mazeHeight / 2f);
        return new Vector2Int(x, y);
    }
}
