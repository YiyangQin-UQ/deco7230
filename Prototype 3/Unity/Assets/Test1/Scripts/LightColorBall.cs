using UnityEngine;

public class PigmentBall : MonoBehaviour
{
    [Header("Pigment Settings")]
    public Color pigmentColor = Color.white;  // 颜料颜色
    public float glowDuration = 0.3f;         // 发光持续时间
    public float glowIntensity = 3.0f;        // 发光强度（越大越亮）

    private Material mat;
    private Color baseEmission;
    private bool glowing = false;

    void Start()
    {
        mat = GetComponent<Renderer>().material;
        baseEmission = mat.GetColor("_EmissionColor");
    }

    public void TriggerGlow()
    {
        if (!glowing)
            StartCoroutine(GlowOnce());
    }

    private System.Collections.IEnumerator GlowOnce()
    {
        glowing = true;
        // 临时设置发光颜色（更亮）
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", pigmentColor * glowIntensity);
        yield return new WaitForSeconds(glowDuration);
        // 还原
        mat.SetColor("_EmissionColor", baseEmission);
        glowing = false;
    }
}
