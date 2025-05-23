using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    [Header("エフェクト設定")]
    public float pulseSpeed = 2f;
    public float pulseScale = 1.2f;

    private Vector3 originalScale;
    private SpriteRenderer spriteRenderer;
    private bool goalTriggered = false; // 重複防止フラグ

    void Start()
    {
        Debug.Log("GoalTrigger Start() called");
        originalScale = transform.localScale;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // ゴールが光るエフェクト
        StartCoroutine(PulseEffect());
    }

    void Update()
    {
        // ゴール判定が既に実行されている場合は何もしない
        if (goalTriggered) return;

        // デバッグ用：プレイヤーとの距離を監視
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);

            if (distance <= 0)
            {
                Debug.Log("Player reached goal by distance! Distance: " + distance);
                goalTriggered = true; // 重複防止

                // ゴール判定を実行
                MazeGameManager gameManager = FindObjectOfType<MazeGameManager>();
                if (gameManager != null)
                {
                    gameManager.OnGoalReached();
                }
            }
            else if (distance < 1.0f) // 1ユニット以内の場合
            {
                Debug.Log("Player is close to goal! Distance: " + distance);
            }
        }

        // デバッグ用：Gキーでゴール判定を手動実行
        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("Manual goal test triggered!");
            goalTriggered = true; // 重複防止
            MazeGameManager gameManager = FindObjectOfType<MazeGameManager>();
            if (gameManager != null)
            {
                gameManager.OnGoalReached();
            }
        }
    }

    System.Collections.IEnumerator PulseEffect()
    {
        while (true)
        {
            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
            transform.localScale = Vector3.Lerp(originalScale, originalScale * pulseScale, pulse);

            // 色の変化も追加
            Color baseColor = Color.yellow;
            Color brightColor = Color.white;
            spriteRenderer.color = Color.Lerp(baseColor, brightColor, pulse);

            yield return null;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("OnTriggerEnter2D called with: " + other.name + ", tag: " + other.tag);

        if (other.CompareTag("Player"))
        {
            Debug.Log("Player reached goal!");

            // ゲームクリア処理
            MazeGameManager gameManager = FindObjectOfType<MazeGameManager>();
            if (gameManager != null)
            {
                gameManager.OnGoalReached();
            }
            else
            {
                Debug.LogError("MazeGameManager not found!");
            }
        }
    }

    // Rigidbody2Dがない場合はOnTriggerStay2Dも使用
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player staying on goal");
        }
    }
}
