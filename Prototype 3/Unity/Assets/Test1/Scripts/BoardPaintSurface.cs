using UnityEngine;

[DisallowMultipleComponent]
public class BoardPaintSurfaceSimple : MonoBehaviour
{
    [Header("Refs")]
    public Renderer boardRenderer;   // 画布的 MeshRenderer
    public Material paintEffect;     // 使用 Brush/MarkPenEffect 的材质
    public Texture baseTexture;      // 初始底图（可空，默认取材质 MainTex）

    RenderTexture cacheRT, tempRT;
    Material boardMat;
    Texture srcTex;

    public static BoardPaintSurfaceSimple Instance { get; private set; }

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(this); return; }
        Instance = this;

        if (!boardRenderer) boardRenderer = GetComponent<MeshRenderer>();
        boardMat = boardRenderer.material;

        // 取底图（优先 BaseTexture，没有就拿材质 MainTex）
        srcTex = baseTexture ? baseTexture : boardMat.mainTexture;
        if (!srcTex) { Debug.LogError("No base texture found on board."); enabled = false; return; }

        // 初始化画布缓存（只这一处 new RT）
        cacheRT = new RenderTexture(srcTex.width, srcTex.height, 0, RenderTextureFormat.ARGB32);
        tempRT  = new RenderTexture(srcTex.width, srcTex.height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(srcTex, cacheRT);
        boardMat.SetTexture("_MainTex", cacheRT);

        if (!paintEffect)
        {
            // 如果忘了拖材质，这里尝试按 Shader 名创建
            var sh = Shader.Find("Brush/MarkPenEffect");
            paintEffect = new Material(sh);
        }
    }

    // 只设置三个属性：_BrushPos、_BrushColor、_BrushSize
    public void DrawDot(Vector2 uv, float size, Color color)
    {
        paintEffect.SetVector("_BrushPos", new Vector4(uv.x, uv.y, 0f, 0f));
        paintEffect.SetColor ("_BrushColor", color);
        paintEffect.SetFloat ("_BrushSize",  size);

        Graphics.Blit(cacheRT, tempRT, paintEffect);
        Graphics.Blit(tempRT,  cacheRT);
    }
}
