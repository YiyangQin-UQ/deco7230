using UnityEngine;
using System.Collections.Generic;
using System.Collections;


[RequireComponent(typeof(Renderer))]
public class MinimalMixBall : MonoBehaviour
{
    [Header("Links")]
    public PaletteSlots palette;          // 拖 PaletteZone（挂 PaletteSlots 的对象）
    public Renderer targetRenderer;       // 拖这个小球的 MeshRenderer

    [Header("Mix Progress")]
    public float degreesPerFullMix = 720f;      // 累计多少度算混合完成（两圈=720）
    public bool requirePenInZoneToMix = true;   // 只在笔在盘里时才累积
    public bool keyboardTest = true;            // 编辑器里用 ←/→ 测试

    [Header("Look")]
    [Range(0f, 1f)] public float alphaWhenUnmixed = 0.45f; // 未混合时的透明度
    [Range(0f, 1f)] public float alphaWhenMixed = 1.0f;  // 完全混合时透明度

    [Header("Color Rules")]
    public Color threeColorMix = new Color(0.45f, 0.35f, 0.25f); // 三色结果

    // runtime
    float accumulatedDeg;
    float lastStickAng;
    readonly List<Color> activeColors = new List<Color>(3);
    Material mat;

    public float MixT => Mathf.Clamp01(degreesPerFullMix <= 0 ? 1 : accumulatedDeg / degreesPerFullMix);

    void Awake()
    {
        if (!targetRenderer) targetRenderer = GetComponent<Renderer>();
        mat = targetRenderer.material; // 实例化材质（可改 _Color/_BaseColor）
        ApplyVisual();                 // 初次应用一次
    }

    void Update()
    {
        if (!palette) return;

        // 集合当前槽位颜色
        activeColors.Clear();
        for (int i = 0; i < 3; i++)
        {
            var c = palette.slotColors[i];
            if (c.HasValue) activeColors.Add(c.Value);
        }

        // 少于两色：显示隐藏/变回默认
        if (activeColors.Count < 2)
        {
            SetColorAlpha(GetFallbackColor(), 0f); // 隐掉（或你喜欢的外观）
            accumulatedDeg = 0f; lastStickAng = 0f;
            return;
        }

        // 累计旋转（左摇杆/键盘）
        bool mayAcc = !requirePenInZoneToMix || palette.penInZone;
        if (mayAcc) AccumulateLeftStick();
        if (keyboardTest) AccumulateWithKeyboard();

        // 应用外观：颜色 + 透明度
        ApplyVisual();
    }

    // 强制发光（取色反馈）：发光期间 alpha=1，不透明
    [Header("Pick Glow")]
    public float pickGlowDuration = 0.35f;
    public float pickGlowIntensity = 3f;

    bool glowingOverride = false;

    public void TriggerPickGlow(Color c)
    {
        StartCoroutine(PickGlow(c));
    }

    IEnumerator PickGlow(Color c)
    {
        glowingOverride = true;

        // 打开发光并放大亮度
        if (mat != null)
        {
            mat.EnableKeyword("_EMISSION");
            if (mat.HasProperty("_EmissionColor"))
                mat.SetColor("_EmissionColor", c * pickGlowIntensity);
        }

        // 发光期间保持不透明
        float t = pickGlowDuration;
        while (t > 0f)
        {
            t -= Time.deltaTime;
            yield return null;
        }

        // 恢复到温和发光（可选）
        if (mat != null)
        {
            Color baseC = c;
            if (mat.HasProperty("_BaseColor")) baseC = mat.GetColor("_BaseColor");
            else if (mat.HasProperty("_Color")) baseC = mat.GetColor("_Color");

            if (mat.HasProperty("_EmissionColor"))
                mat.SetColor("_EmissionColor", baseC * 0.25f);
        }

        glowingOverride = false;
        ApplyVisual(); // 让外观回到按混合进度控制
    }


    void ApplyVisual()
    {
        Color mixed = GetFullyMixedColor(activeColors);
        // 发光时强制不透明；其余按混合进度控制透明度
        float a = glowingOverride ? 1f : Mathf.Lerp(alphaWhenUnmixed, alphaWhenMixed, MixT);
        SetColorAlpha(mixed, a);
    }


    // —— 给画笔取色的接口 —— //
    // 混合没完成时：稍微向完全混合色靠拢，但保留一点“原色平均”的味道（假装夹色）
    // 返回当前混合阶段的“伪夹色”
    public Color GetSampledColor()
    {
        if (activeColors.Count == 0) return Color.white;

        // 完全混合：返回最终色
        if (MixT >= 0.99f)
            return GetFullyMixedColor(activeColors);

        // ----------------------------
        // 1) 先得到最终混合色 fully
        // ----------------------------
        Color fully = GetFullyMixedColor(activeColors);

        // ----------------------------
        // 2) 构造“夹色扰动”：
        //    在 early 阶段，叠加原色波动；
        //    在 late 阶段，波动频率高、振幅小。
        // ----------------------------
        float t = Mathf.Clamp01(MixT);

        // 颜色扰动强度：前期强，后期弱
        float amp = Mathf.Lerp(0.5f, 0.05f, t);
        // 颜色扰动频率：前期低，后期高
        float freq = Mathf.Lerp(1.5f, 6f, t);

        // 基于时间和混合进度的伪噪声，让颜色动态变化
        float n = Mathf.Sin(Time.time * freq * 3.14f) * 0.5f + 0.5f;

        // 从原色中挑一组权重
        Color jitter = Color.black;
        float sumW = 0f;
        for (int i = 0; i < activeColors.Count; i++)
        {
            float w = Mathf.Abs(Mathf.Sin((Time.time + i) * freq + i * 1.1f));
            jitter += activeColors[i] * w;
            sumW += w;
        }
        if (sumW > 0f) jitter /= sumW;

        // ----------------------------
        // 3) 综合出“夹色”
        // ----------------------------
        // 原理：结果色 ± 原色扰动 * 振幅
        Color mixed = Color.Lerp(fully, jitter, amp);

        // 稍微调亮一点，让颜色更“湿润”
        mixed = Color.Lerp(mixed, Color.white, 0.05f * (1 - t));

        return mixed;
    }


    // —— 输入累计 —— //
    void AccumulateLeftStick()
    {
        Vector2 s = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.LTouch);
        if (s.sqrMagnitude < 0.25f) { lastStickAng = 0f; return; }
        float a = Mathf.Atan2(s.y, s.x) * Mathf.Rad2Deg;
        if (lastStickAng == 0f) { lastStickAng = a; return; }
        float delta = Mathf.DeltaAngle(lastStickAng, a);
        lastStickAng = a;
        accumulatedDeg += Mathf.Abs(delta);
    }
    void AccumulateWithKeyboard()
    {
        float add = 0f;
        if (Input.GetKey(KeyCode.LeftArrow)) add += 180f * Time.deltaTime;
        if (Input.GetKey(KeyCode.RightArrow)) add += 180f * Time.deltaTime;
        accumulatedDeg += Mathf.Abs(add);
    }

    // —— 结果色规则 —— //
    Color GetFullyMixedColor(List<Color> cols)
    {
        if (cols.Count == 2) return MixRYB(cols[0], cols[1]);
        if (cols.Count >= 3) return threeColorMix;

        return cols.Count == 1 ? cols[0] : Color.white;
    }
    Color MixRYB(Color a, Color b)
    {
        int ia = Dominant(a), ib = Dominant(b);
        if (ia == ib) return (a + b) * 0.5f;
        if ((ia == 0 && ib == 1) || (ia == 1 && ib == 0)) return new Color(1, 1, 0); // 黄
        if ((ia == 0 && ib == 2) || (ia == 2 && ib == 0)) return new Color(1, 0, 1); // 品
        return new Color(0, 1, 1); // 青
    }
    int Dominant(Color c) { if (c.r >= c.g && c.r >= c.b) return 0; if (c.g >= c.r && c.g >= c.b) return 1; return 2; }

    // —— 材质颜色/透明 —— //
    void SetColorAlpha(Color c, float a)
    {
        if (mat == null) return;
        c.a = Mathf.Clamp01(a);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", c);
    }

    Color GetFallbackColor() => new Color(1, 1, 1, 0); // 没有颜色时
    public void ResetMix() { accumulatedDeg = 0f; lastStickAng = 0f; ApplyVisual(); }


}
