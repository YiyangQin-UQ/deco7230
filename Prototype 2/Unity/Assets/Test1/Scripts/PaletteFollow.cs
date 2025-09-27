using UnityEngine;

public class PaletteFollower : MonoBehaviour
{
    public Transform playerCamera;  // 玩家相机
    public Vector3 offset = new Vector3(0, -0.2f, 0.5f); // 相机前方偏移

    void LateUpdate()
    {
        if (playerCamera == null) return;

        // 跟随相机位置 + 偏移
        transform.position = playerCamera.position + playerCamera.forward * offset.z
                             + playerCamera.up * offset.y
                             + playerCamera.right * offset.x;

        // 始终面向玩家
        transform.LookAt(playerCamera);
        transform.Rotate(0, 180, 0); // 让正面朝向玩家
    }
}
