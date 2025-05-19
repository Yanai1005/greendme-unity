using UnityEngine;

public class FreeMovement2D : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 5f;

    void Update()
    {
        // 入力を取得
        float horizontalInput = Input.GetAxis("Horizontal"); // A/D キー
        float verticalInput = Input.GetAxis("Vertical");     // W/S キー

        // 移動ベクトルを計算
        Vector2 moveDirection = new Vector2(horizontalInput, verticalInput);

        // 斜め移動時の速度を正規化（斜め移動が速くなりすぎないように）
        moveDirection = moveDirection.normalized;

        // 移動を適用
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

        // キャラクターの向きを変更（左右移動時のみ）
        if (horizontalInput > 0)
            transform.localScale = new Vector3(1, 1, 1); // 右向き
        else if (horizontalInput < 0)
            transform.localScale = new Vector3(-1, 1, 1); // 左向き
    }
}
