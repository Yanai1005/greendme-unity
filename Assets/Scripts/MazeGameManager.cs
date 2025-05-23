using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MazeGameManager : MonoBehaviour
{
    [Header("UI要素")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI congratsText;
    public Button restartButton;

    [Header("ゲーム設定")]
    public float gameOverDelay = 1f;

    private MazeGenerator mazeGenerator;
    private MazePlayerMovement playerMovement;
    private bool gameCompleted = false;

    void Start()
    {
        mazeGenerator = FindObjectOfType<MazeGenerator>();
        playerMovement = FindObjectOfType<MazePlayerMovement>();

        // UI初期化
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        // プレイヤーにPlayerタグを設定
        if (playerMovement != null)
        {
            playerMovement.gameObject.tag = "Player";
            Debug.Log("Player tag set on: " + playerMovement.gameObject.name);
        }
    }

    public void OnGoalReached()
    {
        if (gameCompleted) return;

        gameCompleted = true;
        Debug.Log("ゴール到達！");

        // ゲームクリア処理
        StartCoroutine(ShowGameOverScreen());
    }

    IEnumerator ShowGameOverScreen()
    {
        yield return new WaitForSeconds(gameOverDelay);

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (congratsText != null)
        {
            congratsText.text = "迷路クリア！\nおめでとうございます！";
        }
    }

    public void RestartGame()
    {
        // ゲーム再開処理
        gameCompleted = false;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // 新しい迷路を生成
        if (mazeGenerator != null)
        {
            // 既存の迷路オブジェクトを削除
            foreach (Transform child in mazeGenerator.transform)
            {
                DestroyImmediate(child.gameObject);
            }

            // 新しい迷路を生成
            mazeGenerator.Start();
        }

        // プレイヤーを開始位置に戻す
        if (playerMovement != null && mazeGenerator != null)
        {
            Vector2Int startPos = mazeGenerator.PlayerStartPosition;
            playerMovement.transform.position = mazeGenerator.GridToWorldPosition(startPos.x, startPos.y);
        }
    }

    // Unity WebGLからの呼び出し用メソッド（キーボード制御）
    public void OnMoveRight()
    {
        if (playerMovement != null && !playerMovement.IsUsingBodyControl())
            playerMovement.MoveRight();
    }

    public void OnMoveLeft()
    {
        if (playerMovement != null && !playerMovement.IsUsingBodyControl())
            playerMovement.MoveLeft();
    }

    public void OnMoveUp()
    {
        if (playerMovement != null && !playerMovement.IsUsingBodyControl())
            playerMovement.MoveUp();
    }

    public void OnMoveDown()
    {
        if (playerMovement != null && !playerMovement.IsUsingBodyControl())
            playerMovement.MoveDown();
    }

    // 体の傾き制御用メソッド（Reactから呼び出される）
    public void SetMovementDirection(string directionData)
    {
        if (playerMovement != null)
        {
            playerMovement.SetMovementDirection(directionData);
        }
    }

    // 体の傾き制御の無効化
    public void DisableBodyControl()
    {
        if (playerMovement != null)
        {
            playerMovement.DisableBodyControl();
        }
    }

    // デバッグ用：現在の制御モードを取得
    public bool IsUsingBodyControl()
    {
        return playerMovement != null ? playerMovement.IsUsingBodyControl() : false;
    }
}
