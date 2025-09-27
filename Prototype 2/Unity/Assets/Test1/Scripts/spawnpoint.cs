using UnityEngine;

public class SpawnAlign : MonoBehaviour
{
    public Transform spawnPoint;     // 拖拽你的 SpawnPoint
    public Transform rig;            // 拖拽 OVRCameraRig

    void Start()
    {
        if (spawnPoint == null || rig == null)
        {
            Debug.LogError("SpawnPoint 或 OVRCameraRig 没有设置！");
            return;
        }

        // 计算头显相对 Rig 的偏移
        Vector3 eyeOffset = rig.GetComponent<OVRCameraRig>().centerEyeAnchor.position - rig.position;

        // 移动 Rig，使得头显位置对齐到 SpawnPoint
        rig.position = spawnPoint.position - eyeOffset;

        // 设置 Rig 朝向和 SpawnPoint 一致（只考虑水平旋转，避免头歪）
        Vector3 forward = spawnPoint.forward;
        forward.y = 0;
        rig.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }
}
