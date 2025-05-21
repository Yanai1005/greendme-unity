using UnityEngine;

public class FreeMovement2D : MonoBehaviour
{
    [Header("移動設定")]
    public float moveUnit = 1f;
    public float moveDuration = 0.2f; // 移動にかかる時間（秒）

    private bool isMoving = false;

    public void MoveRight(int position)
    {
        if (!isMoving)
            StartCoroutine(MoveBy(Vector2.right * position * moveUnit));
    }

    public void MoveLeft(int position)
    {
        if (!isMoving)
            StartCoroutine(MoveBy(Vector2.left * position * moveUnit));
    }

    public void MoveUp(int position)
    {
        if (!isMoving)
            StartCoroutine(MoveBy(Vector2.up * position * moveUnit));
    }

    public void MoveDown(int position)
    {
        if (!isMoving)
            StartCoroutine(MoveBy(Vector2.down * position * moveUnit));
    }

    private System.Collections.IEnumerator MoveBy(Vector2 offset)
    {
        isMoving = true;
        Vector3 start = transform.position;
        Vector3 end = start + (Vector3)offset;

        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            transform.position = Vector3.Lerp(start, end, elapsed / moveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = end;
        isMoving = false;
    }
}
