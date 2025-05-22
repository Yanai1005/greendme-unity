using UnityEngine;

public class SmoothMovement2D : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 5f;
    public float smoothTime = 0.1f;
    public float deadZone = 0.1f;

    [Header("境界設定")]
    public float boundaryLeft = -8f;
    public float boundaryRight = 8f;
    public float boundaryTop = 4f;
    public float boundaryBottom = -4f;

    private Vector2 targetVelocity = Vector2.zero;
    private Vector2 currentVelocity = Vector2.zero;
    private Vector2 velocitySmoothing = Vector2.zero;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        currentVelocity = Vector2.SmoothDamp(
            currentVelocity,
            targetVelocity,
            ref velocitySmoothing,
            smoothTime
        );

        Vector2 newPosition = rb.position + currentVelocity * Time.fixedDeltaTime;
        newPosition.x = Mathf.Clamp(newPosition.x, boundaryLeft, boundaryRight);
        newPosition.y = Mathf.Clamp(newPosition.y, boundaryBottom, boundaryTop);

        rb.MovePosition(newPosition);
    }
    public void SetMovementDirection(string directionData)
    {
        try
        {
            MovementData data = JsonUtility.FromJson<MovementData>(directionData);

            float x = Mathf.Abs(data.x) > deadZone ? data.x : 0f;
            float y = Mathf.Abs(data.y) > deadZone ? data.y : 0f;

            targetVelocity = new Vector2(x, y) * moveSpeed;
        }
        catch (System.Exception e)
        {
            Debug.LogError("移動データの解析エラー: " + e.Message);
        }
    }

    public void MoveRight(int position)
    {
        targetVelocity = Vector2.right * moveSpeed;
    }

    public void MoveLeft(int position)
    {
        targetVelocity = Vector2.left * moveSpeed;
    }

    public void MoveUp(int position)
    {
        targetVelocity = Vector2.up * moveSpeed;
    }

    public void MoveDown(int position)
    {
        targetVelocity = Vector2.down * moveSpeed;
    }

    // 移動停止
    public void StopMovement()
    {
        targetVelocity = Vector2.zero;
    }

    void OnDrawGizmos()
    {
        // 境界を表示
        Gizmos.color = Color.red;
        Vector3 center = new Vector3((boundaryLeft + boundaryRight) / 2, (boundaryTop + boundaryBottom) / 2, 0);
        Vector3 size = new Vector3(boundaryRight - boundaryLeft, boundaryTop - boundaryBottom, 0);
        Gizmos.DrawWireCube(center, size);

        // 現在の速度ベクトルを表示
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, (Vector3)currentVelocity);
        }
    }
}

[System.Serializable]
public class MovementData
{
    public float x;
    public float y;
}
