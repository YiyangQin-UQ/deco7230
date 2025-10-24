// MarkPenVRSimple.cs
using UnityEngine;

public class MarkPenVRSimple : MonoBehaviour
{
    [Header("Refs")]
    public Transform penHead;        // 笔尖（forward 指向画布）
    public LayerMask boardMask;      // 只勾选“画布”Layer
    public Transform board;


    [Tooltip("可选的第二笔头（用于双头橡皮）")]
    public Transform penHeadAlt;

    [Header("Pigment Detection")]
    public PigmentBall[] pigmentBalls;   // 拖你的三个颜料团
    public float pigmentRadius = 0.05f;  // 半径（检测范围，建议与模型大小接近）

    [Header("Palette Hook (Step 1 only)")]
    public PaletteSlots paletteSlots;  // 拖入 Palette


    [Header("Mix Ball Pickup")]
    public MinimalMixBall mixBall;     // 拖中间的小球（挂了 MinimalMixBall 的那个）
    public float mixPickRadius = 0.04f; // 吸色判定半径（米）
    public bool pickFromMixRequireTrigger = false; // 是否需要扣一次扳机才吸色

    // —— Palette 放色防连发 —— 
    [Header("Palette place guard")]
    private bool placeLatch = false;          // 按住时只触发一次
    private float placeCooldown = 0.15f;      // 保险冷却（秒）
    private float placeCooldownTimer = 0f;

    [Header("Fake Stripes (夹色)")]
    public bool enableFakeStripes = true;     // 开关：是否启用伪夹色
    public float stripeFreqMin = 4f;          // 混合很少时的条纹频率（越小条纹越宽）
    public float stripeFreqMax = 24f;         // 混合接近完成时的条纹频率（越大越细）
    public float stripeAmpMax = 0.45f;       // 混合很少时的摆动幅度（夹色强）
    public float stripeAmpMin = 0.05f;       // 混合接近完成时的摆动幅度（夹色弱）
    private float stripePhase = 0f;          // 条纹相位（沿笔迹推进）








    [Header("Brush")]
    public float maxDrawDistance = 0.10f; // 可落笔最大距离
    public float minWidth = 0.005f;
    public float maxWidth = 0.02f;
    public Color currentColor = Color.black;

    [Header("Input")]
    public bool requirePressToDraw = true;
    public bool allowHandPinch = false;
    public OVRInput.Controller drawControllers =
        OVRInput.Controller.LTouch | OVRInput.Controller.RTouch;

    Vector2 lastUV;
    bool drawing;
    float currentBrushSize;

    void Update()
    {
        if (!BoardPaintSurfaceSimple.Instance || !penHead) return;

        const float maxPenetration = 0.05f; // 允许穿透 5cm
        RaycastHit hit;
        bool hitFound = false;
        Transform activeTip = penHead;

        // --- ① 主笔头检测 ---
        Ray ray1 = new Ray(penHead.position, penHead.forward);
        if (Physics.Raycast(ray1, out hit, maxDrawDistance, boardMask))
        {
            hitFound = true;
            activeTip = penHead;
        }
        else if (penHeadAlt) // --- ② 副笔头检测 ---
        {
            Ray ray2 = new Ray(penHeadAlt.position, penHeadAlt.forward);
            if (Physics.Raycast(ray2, out hit, maxDrawDistance, boardMask))
            {
                hitFound = true;
                activeTip = penHeadAlt;
            }
        }

        // --- ③ 命中或穿透时绘制 ---
        if (hitFound || penHead || penHeadAlt)
        {
            // 使用几何平面计算笔尖到画板的有符号距离
            Plane boardPlane = new Plane(board.forward, board.position);
            float signedDist = boardPlane.GetDistanceToPoint(activeTip.position);

            // 构造从平面背后一点出发 → 朝向法线的射线
            Vector3 normal = boardPlane.normal;
            Vector3 start = activeTip.position - normal * (maxPenetration + 0.01f);
            Vector3 dir = normal;
            float maxLen = maxDrawDistance + 2f * maxPenetration;

            bool hitBoard = Physics.Raycast(start, dir, out hit, maxLen, boardMask);

            // ✅ 如果射线命中，或者虽然没命中但笔头已在板内（穿透≤5cm），都允许绘制
            if (hitBoard || (signedDist < 0f && Mathf.Abs(signedDist) <= maxPenetration))
            {
                // --- 笔粗逻辑 ---


                // --- 笔刷粗细计算（反向版，靠近=粗，远离=细）---
                float sd = Mathf.Clamp(signedDist, -maxPenetration, maxDrawDistance);

                // 归一化：近(=-maxPenetration)->1，远(=maxDrawDistance)->0
                float tRaw = (sd + maxPenetration) / (maxDrawDistance + maxPenetration);
                tRaw = Mathf.Clamp01(1f - tRaw);  // 反向映射，近=1，远=0

                // 计算目标笔粗
                float targetSize = Mathf.Lerp(minWidth, maxWidth, tRaw);

                // 平滑过渡
                float alpha = 1f - Mathf.Exp(-20f * Time.deltaTime);
                currentBrushSize = currentBrushSize + (targetSize - currentBrushSize) * alpha;



                // --- 绘制逻辑 ---
                bool shouldDraw = requirePressToDraw ? IsPressed() : true;
                if (shouldDraw)
                {
                    if (!drawing)
                    {
                        drawing = true;
                        lastUV = hit.textureCoord;
                    }
                    RenderStroke(hit.textureCoord);
                }
                else drawing = false;
            }
            else
            {
                drawing = false;
            }
        }
        else
        {
            drawing = false;
        }


        // --- ④ 调试线 ---
        Debug.DrawRay(penHead.position, penHead.forward * 0.2f, Color.green);
        if (penHeadAlt)
            Debug.DrawRay(penHeadAlt.position, penHeadAlt.forward * 0.2f, Color.yellow);

        // --- ⑤ 颜料检测 ---
        if (pigmentBalls != null && pigmentBalls.Length > 0)
        {
            foreach (var ball in pigmentBalls)
            {
                if (!ball) continue;

                // 以“球心半径”判定是否插入（dist < pigmentRadius）
                float dist = Vector3.Distance(penHead.position, ball.transform.position);
                if (dist < pigmentRadius)
                {
                    // 方向判定：确保是“插入/接触”而不是擦边离开
                    Vector3 dirCenterToTip = (penHead.position - ball.transform.position).normalized;
                    float signed = Vector3.Dot(dirCenterToTip, penHead.forward);

                    // 允许“插入方向向内”时触发；如果你希望只要进入就取色，可直接去掉这个 if
                    if (signed < 0f)
                    {
                        // ✅ 用现有方法，别造新名字
                        SetColor(ball.pigmentColor);

                        // 颜料团闪一下作为反馈
                        ball.TriggerGlow();
                    }
                }
            }
            HandlePlaceToPalette();

        }

        HandlePlaceToPalette();
        TryPickFromMixBall();
        HandleClearPaletteShortcut();



    }

    // --- ⑥ 调色盘槽检测） ---
    // 只在笔尖在盘里时，且这帧“刚按下”Trigger，才放一个颜色
    void HandlePlaceToPalette()
    {
        if (placeCooldownTimer > 0f) placeCooldownTimer -= Time.deltaTime;

        if (!paletteSlots) return;
        if (!paletteSlots.penInZone) return;

        // 只认“按下边沿”，且一次按压只触发一次
        bool trigDownL = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch);
        bool trigDownR = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
        bool trigUpL = OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch);
        bool trigUpR = OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);

        // 编辑器测试键
        bool keyDown = Input.GetKeyDown(KeyCode.Space);
        bool keyUp = Input.GetKeyUp(KeyCode.Space);

        bool pressedDown = trigDownL || trigDownR || keyDown;
        bool released = trigUpL || trigUpR || keyUp;

        // 触发条件：这次“刚按下”，且没有在锁定，且不在冷却
        if (pressedDown && !placeLatch && placeCooldownTimer <= 0f)
        {
            // 只放一个槽：有空位就放；满了则覆写最早的（你当前 PaletteSlots 的逻辑）
            paletteSlots.PlaceOrOverwriteOldest(currentColor);

            // 上锁并进入冷却，防止同一帧或多脚本重复触发
            placeLatch = true;
            placeCooldownTimer = placeCooldown;

            // 调试用：确认只触发一次
            // Debug.Log("[Palette] Placed ONE color");
        }

        // 松手后解除锁
        if (released)
        {
            placeLatch = false;
        }
    }


    // 从中间混色球吸色：靠近即可（可选：需要扣扳机）
    void TryPickFromMixBall()
    {
        if (!mixBall || !penHead) return;

        // 若调色盘里不足两种颜色，就不吸（避免误触）
        if (paletteSlots && paletteSlots.FilledCount() < 2) return;

        // 距离判定
        float dist = Vector3.Distance(penHead.position, mixBall.transform.position);

        // 是否需要按键
        bool okToPick = true;
        if (pickFromMixRequireTrigger)
        {
            okToPick =
                OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch) ||
                OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) ||
                Input.GetKeyDown(KeyCode.M); // 编辑器可用 M 键测试
        }

        if (dist < mixPickRadius && okToPick)
        {
            // 从混色球取“夹色感”的颜色
            Color sampled = mixBall.GetSampledColor();
            SetColor(sampled);
            mixBall.TriggerPickGlow(sampled);
        }
    }

    // 随时清空调色盘：不要求笔尖靠近
    void HandleClearPaletteShortcut()
    {
        if (!paletteSlots) return;

        // 手柄 A 或 B 键、或键盘 C 键 都可以触发
        bool clearDown =
            OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch) ||
            OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch) ||
            Input.GetKeyDown(KeyCode.C);

        if (clearDown)
        {
            paletteSlots.ClearAll();
            // 同时把混色进度也重置（如果有混色球）
            if (mixBall) mixBall.ResetMix();

            // 小反馈（可选）
            Debug.Log("[Palette] Cleared all slots manually.");
        }
    }


    // 返回当前笔触用的“伪夹色” —— 基于调色盘已放的原色 + 混合进度，做幅度/频率控制
    Color SampleStripedColor()
    {
        // 条件不足时，直接用 currentColor
        if (!enableFakeStripes || paletteSlots == null || paletteSlots.FilledCount() < 2)
            return currentColor;

        // 读取调色盘里的原色
        Color[] cols = new Color[3];
        int n = 0;
        for (int i = 0; i < 3; i++)
        {
            var c = paletteSlots.slotColors[i];
            if (c.HasValue) { cols[n++] = c.Value; }
        }
        if (n == 0) return currentColor;

        // 进度：有 mixBall 就用它的 MixT（0..1），没有就当 0
        float t = (mixBall != null) ? mixBall.MixT : 0f;

        // 幅度：越未混合，越“夹色”；越接近完成，越弱
        float amp = Mathf.Lerp(stripeAmpMax, stripeAmpMin, t);

        // 频率：越接近完成，条纹越细（频率越高）
        float freq = Mathf.Lerp(stripeFreqMin, stripeFreqMax, t);

        // 计算一组随相位变化的权重（0..1），并给每个原色不同相位偏移
        Color jitter = Color.black;
        float sumW = 0f;

        // 使用黄金角偏移让分布更均匀
        const float PHASE_STEP = 2.399963f; // 约等于 137.5°（弧度）
        float sharp = Mathf.Lerp(1.5f, 3.0f, t); // 后期更尖锐→更“细”

        for (int i = 0; i < n; i++)
        {
            // 0..1 的波形（随相位推进）
            float w = Mathf.Sin(stripePhase + i * PHASE_STEP) * 0.5f + 0.5f;
            w = Mathf.Pow(Mathf.Clamp01(w), sharp);
            jitter += cols[i] * w;
            sumW += w;
        }
        if (sumW > 0f) jitter /= sumW;

        // 把“结果色”(currentColor) 与 “原色扰动” 混合，得到伪夹色
        Color outC = Color.Lerp(currentColor, jitter, amp);

        // 轻微提亮（可选）：初期更“湿润”
        outC = Color.Lerp(outC, Color.white, 0.03f * (1f - t));

        // 推进相位：与笔粗/步长和频率相关，确保随笔迹推进而非时间
        float phaseStep = currentBrushSize * freq * 10f;
        stripePhase += phaseStep;

        return outC;
    }








    bool IsPressed()
    {
        float rt = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
        float lt = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.LTouch);
        if (rt > 0.75f || lt > 0.75f) return true;

        if (allowHandPinch)
        {
            bool rPinch = OVRInput.Get(OVRInput.NearTouch.PrimaryIndexTrigger, OVRInput.Controller.RHand)
                       || OVRInput.Get(OVRInput.Touch.PrimaryIndexTrigger, OVRInput.Controller.RHand);
            bool lPinch = OVRInput.Get(OVRInput.NearTouch.PrimaryIndexTrigger, OVRInput.Controller.LHand)
                       || OVRInput.Get(OVRInput.Touch.PrimaryIndexTrigger, OVRInput.Controller.LHand);
            if (rPinch || lPinch) return true;
        }
        return false;
    }

    void RenderStroke(Vector2 uv)
    {
        // 简单插值连线，逐点 Blit
        Vector2 dir = uv - lastUV;
        float step = currentBrushSize;

        if (dir.sqrMagnitude > step * step)
        {
            int n = Mathf.CeilToInt(dir.magnitude / step);
            Vector2 unit = dir.normalized * step;
            for (int i = 0; i < n; i++)
                BlitDot(lastUV + unit * i);
        }
        BlitDot(uv);
        lastUV = uv;
    }

    void BlitDot(Vector2 uv)
    {
        // 用伪夹色采样代替固定的 currentColor
        Color drawC = SampleStripedColor();
        BoardPaintSurfaceSimple.Instance.DrawDot(uv, currentBrushSize, drawC);
    }


    // 供外部（调色盘）改笔色
    public void SetColor(Color c) => currentColor = c;
}
