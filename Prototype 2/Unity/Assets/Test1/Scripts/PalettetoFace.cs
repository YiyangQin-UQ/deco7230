using UnityEngine;

public class palettefollower : MonoBehaviour
{
    public Transform playerCamera;  
    public Vector3 offset = new Vector3(-0.5f, -0.2f, 1.0f); 
    private bool followEnabled = true;     // 是否跟随
    private Vector3 originalPos;           // 初始位置
    private Quaternion originalRot;        // 初始旋转

    void Start()
    {
        // 记录调色盘的初始位置和旋转
        originalPos = transform.position;
        originalRot = transform.rotation;
    }

    void LateUpdate()
    {
        // 左手扳机按钮切换跟随状态
        if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch))
        {
            followEnabled = !followEnabled;  // 状态切换

            if (!followEnabled)
            {
                // 关闭跟随 → 回到原位
                transform.position = originalPos;
                transform.rotation = originalRot;
                Debug.Log("调色盘已固定在原位");
            }
            else
            {
                Debug.Log("调色盘跟随已开启");
            }
        }

        if (!followEnabled || playerCamera == null) return;

        // 只取相机的水平朝向
        Vector3 forwardFlat = playerCamera.forward;
        forwardFlat.y = 0;
        forwardFlat.Normalize();

        Vector3 rightFlat = Vector3.Cross(Vector3.up, forwardFlat).normalized;

        // 目标位置（水平跟随）
        Vector3 targetPos = playerCamera.position
                            + forwardFlat * offset.z
                            + Vector3.up * offset.y
                            + rightFlat * offset.x;

        transform.position = targetPos;

        // 固定旋转（水平朝向玩家）
        Vector3 lookAtPos = playerCamera.position;
        lookAtPos.y = transform.position.y; 
        transform.LookAt(lookAtPos);
    }
}
