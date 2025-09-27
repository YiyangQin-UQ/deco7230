using System.Collections;
using UnityEngine;

public class MarkPenVR : MonoBehaviour
{
    [Header("Refs")]
    public Transform penHead;          // 笔尖（子物体 Tip）
    public Transform board;            // 画布（其材质会被实时写入）
    [Tooltip("Board 所在的物理层（用于笔尖射线检测）")]
    public LayerMask boardMask;
    [Tooltip("Palette 所在的物理层（用于取色）")]
    public LayerMask paletteMask;

    [Header("Brush")]
    public float maxDrawDistance = 0.10f;   // 笔尖到画布可绘制的最大距离
    public float minWidth = 0.005f;         // 最细笔触
    public float maxWidth = 0.02f;          // 最粗笔触
    public Color initialColor = Color.black;

    [Header("Input")]
    public bool requirePressToDraw = true;
    public bool allowHandPinch = true;
    public OVRInput.Button colorPickButton = OVRInput.Button.One; // A/X 取色
    public OVRInput.Controller drawControllers =
        OVRInput.Controller.LTouch | OVRInput.Controller.RTouch | OVRInput.Controller.Hands;

    // --- 内部 ---
    private Texture srcTex;
    public RenderTexture cacheTex;
    private RenderTexture tempTex;
    private Material effectMat;
    private Material boardMat;
    private Vector2 lastUV;
    private bool drawing;
    private Gradient colorGrad;

    // 新增：当前笔触大小
    private float currentBrushSize;

    void Start()
    {
        // 初始化材质与贴图
        effectMat = new Material(Shader.Find("Brush/MarkPenEffect"));
        boardMat = board.GetComponent<MeshRenderer>().material;
        srcTex = boardMat.mainTexture;

        // 初始化画布缓存
        cacheTex = new RenderTexture(srcTex.width, srcTex.height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(srcTex, cacheTex);
        boardMat.SetTexture("_MainTex", cacheTex);

        tempTex = new RenderTexture(srcTex.width, srcTex.height, 0, RenderTextureFormat.ARGB32);

        SetBrushColor(initialColor);
    }

    void Update()
    {
        // --- 笔尖 → 画布绘制 ---
        var ray = new Ray(penHead.position, penHead.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDrawDistance, boardMask))
        {
            // 距离映射到笔触大小
            float t = Mathf.Clamp01(1f - hit.distance / maxDrawDistance);
            currentBrushSize = Mathf.Lerp(minWidth, maxWidth, t);

            bool shouldDraw = requirePressToDraw ? IsDrawingPressed() : true;

            if (shouldDraw)
            {
                if (!drawing)
                {
                    drawing = true;
                    lastUV = hit.textureCoord;
                }
                RenderStroke(hit.textureCoord);
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

        Debug.DrawRay(penHead.position, penHead.forward * 0.2f, Color.green);

        // —— 吸色触发 ——
        if (OVRInput.GetDown(OVRInput.Button.One, drawControllers))
            TryPickColor();

        if (Input.GetKeyDown(KeyCode.Alpha1))
            TryPickColor();
    }

    // --- 输入判断 ---
    bool IsDrawingPressed()
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

        if (!Application.isEditor) return false;
        return Input.GetMouseButton(0);
    }

    // --- 绘制 ---
    void RenderStroke(Vector2 uv)
    {
        Vector2 dir = uv - lastUV;
        float step = currentBrushSize; // 用笔触大小控制插值步长

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
        effectMat.SetVector("_BrushPos", new Vector4(uv.x, uv.y, lastUV.x, lastUV.y));
        effectMat.SetColor("_BrushColor", colorGrad.Evaluate(1f));
        effectMat.SetFloat("_BrushSize", currentBrushSize); // 新增：传递笔触大小

        Graphics.Blit(cacheTex, tempTex, effectMat);
        Graphics.Blit(tempTex, cacheTex);
        boardMat.SetTexture("_MainTex", cacheTex);
    }

    // --- 取色 ---
    void TryPickColor()
    {
        const float pickMaxDistance = 3f;
        var ray = new Ray(penHead.position, penHead.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, pickMaxDistance, paletteMask)) return;

        var rend = hit.collider.GetComponent<Renderer>();
        if (!rend) return;

        var mat = rend.sharedMaterial;
        if (mat == null) return;

        Texture2D tex2D = null;
        Vector2 scale = Vector2.one, offset = Vector2.zero;

        if (mat.HasProperty("_BaseMap") && mat.GetTexture("_BaseMap") is Texture2D baseTex)
        {
            tex2D = baseTex;
            scale = mat.GetTextureScale("_BaseMap");
            offset = mat.GetTextureOffset("_BaseMap");
        }
        else if (mat.HasProperty("_MainTex") && mat.GetTexture("_MainTex") is Texture2D mainTex)
        {
            tex2D = mainTex;
            scale = mat.GetTextureScale("_MainTex");
            offset = mat.GetTextureOffset("_MainTex");
        }

        if (tex2D != null && tex2D.isReadable)
        {
            Vector2 uv = hit.textureCoord;
            uv = uv * scale + offset;
            uv.x -= Mathf.Floor(uv.x);
            uv.y -= Mathf.Floor(uv.y);

            var col = tex2D.GetPixelBilinear(uv.x, uv.y);
            SetBrushColor(col);
            return;
        }

        if (mat.HasProperty("_BaseColor")) { SetBrushColor(mat.GetColor("_BaseColor")); return; }
        if (mat.HasProperty("_Color")) { SetBrushColor(mat.GetColor("_Color")); return; }
        if (mat.IsKeywordEnabled("_EMISSION") && mat.HasProperty("_EmissionColor"))
        {
            var em = mat.GetColor("_EmissionColor");
            SetBrushColor(em);
        }
    }

    public void SetBrushColor(Color c)
    {
        var ck = new GradientColorKey[] { new GradientColorKey(c, 0), new GradientColorKey(c, 1) };
        var ak = new GradientAlphaKey[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) };
        colorGrad = new Gradient { colorKeys = ck, alphaKeys = ak };
    }
}
