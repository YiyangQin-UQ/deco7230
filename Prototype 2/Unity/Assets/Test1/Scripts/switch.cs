using UnityEngine;

public class ToolWristSwitcherSimple : MonoBehaviour
{
    [Header("Sources")]
    public Transform controller;       // 读取扭腕角的 Transform（RightControllerAnchor 或 笔根）
    public bool isRightHand = true;    // 仅用于震动/输入区分（可不改）
    public MarkPenVR pen;              // 你的绘制脚本（必须填）
    public Renderer penRenderer;       // 笔的渲染器（可选，用来改外观/发光）

    [Header("Switch by Wrist Roll")]
    public float switchAngle = 45f;    // 扭腕超过这个角度触发切换（+正向，−反向）
    public float resetAngle  = 20f;    // 回中滞回（|角度|回到<=此值才允许下一次触发）
    public float cooldown    = 0.6f;   // 切换冷却时间（秒）

    [Header("Colors")]
    public Color normalColor = Color.black; // 普通笔颜色
    public Color eraserColor = Color.white; // “橡皮” = 喷白色

    [Header("Visual (optional)")]
    public bool useEmissionForEraser = true; // 橡皮时让材质发光以示区分
    public float emissionBoost = 1.5f;       // 发光强度系数

    // --- internals ---
    private bool isEraser = false;
    private float neutralRollDeg;   // 校准的中位角
    private bool latchedPos, latchedNeg;
    private float cooldownUntil;

    void Start()
    {
        if (controller == null) controller = transform;
        // 以当前姿态为中位
        neutralRollDeg = GetRollDeg();

        // 初始设为普通笔
        ApplyMode(false, force:true);
    }

    void Update()
    {
        // 键盘测试：T 切换；C 校准
        if (Input.GetKeyDown(KeyCode.T)) Toggle();
        if (Input.GetKeyDown(KeyCode.C)) CalibrateNow();

        // VR：按住食指扳机在画画时，不切换（避免误触；需要的话可注释掉）
        var trigCtrl = isRightHand ? OVRInput.Controller.RTouch : OVRInput.Controller.LTouch;
        if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, trigCtrl) > 0.1f) return;

        // 冷却中
        if (Time.time < cooldownUntil) return;

        // 读取“当前扭腕角”相对中位的偏差
        float delta = Mathf.DeltaAngle(neutralRollDeg, GetRollDeg()); // [-180,180]

        // 超过 +阈值 → 切“下一种”（只有两种：普通/橡皮）
        if (!latchedPos && delta > switchAngle)
        {
            Toggle();
            latchedPos = true; latchedNeg = false;
            cooldownUntil = Time.time + cooldown;
            return;
        }

        // 低于 -阈值 → 切“上一种”（同上，等价于 Toggle）
        if (!latchedNeg && delta < -switchAngle)
        {
            Toggle();
            latchedNeg = true; latchedPos = false;
            cooldownUntil = Time.time + cooldown;
            return;
        }

        // 回中解除锁存
        if (Mathf.Abs(delta) <= resetAngle)
        {
            latchedPos = false;
            latchedNeg = false;
        }
    }

    private void Toggle()
    {
        isEraser = !isEraser;
        ApplyMode(isEraser);
    }

    private void ApplyMode(bool eraser, bool force=false)
    {
        if (pen != null)
        {
            pen.SetBrushColor(eraser ? eraserColor : normalColor);
        }

        // 简单的外观区分：橡皮时开启发光/更亮一些（可选）
        if (penRenderer != null)
        {
            var mat = penRenderer.material; // 实例化材质
            if (useEmissionForEraser)
            {
                if (eraser)
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", eraserColor * emissionBoost);
                }
                else
                {
                    mat.SetColor("_EmissionColor", Color.black);
                    mat.DisableKeyword("_EMISSION");
                }
            }
            else
            {
                // 或者直接改 Albedo 颜色以示区分
                if (mat.HasProperty("_Color"))
                    mat.SetColor("_Color", eraser ? eraserColor : normalColor);
            }
        }

        // 震动反馈（可选）
        var ctrl = isRightHand ? OVRInput.Controller.RTouch : OVRInput.Controller.LTouch;
        OVRInput.SetControllerVibration(0.0001f, 0.35f, ctrl);
        Invoke(nameof(StopHaptics), 0.06f);

        if (!force)
            Debug.Log($"[ToolWristSwitcherSimple] {(eraser ? "橡皮(白色)" : "普通笔")} 模式");
    }

    private void StopHaptics()
    {
        var ctrl = isRightHand ? OVRInput.Controller.RTouch : OVRInput.Controller.LTouch;
        OVRInput.SetControllerVibration(0f, 0f, ctrl);
    }

    // 扭腕角（读 Z 欧拉角；如方向相反可在这里乘以 -1）
    private float GetRollDeg()
    {
        float roll = controller.rotation.eulerAngles.z;
        if (roll > 180f) roll -= 360f;
        return roll;
    }

    // 手动校准“中位”
    public void CalibrateNow()
    {
        neutralRollDeg = GetRollDeg();
        latchedPos = latchedNeg = false;
        var ctrl = isRightHand ? OVRInput.Controller.RTouch : OVRInput.Controller.LTouch;
        OVRInput.SetControllerVibration(0.0001f, 0.2f, ctrl);
        Invoke(nameof(StopHaptics), 0.05f);
        Debug.Log("[ToolWristSwitcherSimple] 已校准当前手腕为中位");
    }
}
