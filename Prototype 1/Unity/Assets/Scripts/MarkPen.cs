using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkPen : MonoBehaviour
{
    // 取色相关
    public LayerMask paletteMask;            // Inspector 勾选 Palette 层
    public float pickMaxDistance = 1f;
    public KeyCode pickKey = KeyCode.Mouse1; // 右键取色；改成别的键也行


    private Texture tex;
    public RenderTexture cacheTex;
    RenderTexture currentTex;
    private float brushMaxSize;

    public float brushSize = 0.01f;
    public Color brushCol = Color.red;

    private Material effectMat;

    private Material renderMat;


    public Transform penHead;
    public Transform board;

    private Vector2 lastuv;
    private bool isDown;
    // Start is called before the first frame update
    void Start()
    {
        Initialized();
    }

    // Update is called once per frame
    void Update()
    {

        Ray ray = new Ray(penHead.position, penHead.forward);


        if (Physics.Raycast(ray, out RaycastHit raycastHit, 1, LayerMask.GetMask("Board")))
        {
            if (raycastHit.distance < 1f)
            {
                if (!isDown)
                {
                    isDown = true;
                    lastuv = raycastHit.textureCoord;
                }
                // 压感映射：离纸近（≈0）→ brushMaxSize；离纸远（≥maxDistance）→ 0
                float maxDistance = 0.1f; // 允许的最大绘制距离（10cm）
                float t = Mathf.Clamp01(1 - (raycastHit.distance / maxDistance));
                brushSize = t * brushMaxSize;

                RenderBrushToBoard(raycastHit);
                lastuv = raycastHit.textureCoord;
            }
            else
            {
                isDown = false;
            }

        }
        else
        {
            isDown = false;
        }
        if (Physics.Raycast(ray, out RaycastHit hit, 1, LayerMask.GetMask("Board")))
        {
            Debug.Log("Hit " + hit.collider.name + " distance=" + hit.distance);
        }
        else
        {
            Debug.Log("No Hit");
        }

        Debug.DrawRay(penHead.position, penHead.forward * 2, Color.green);


        if (Input.GetKeyDown(pickKey))
        {
            Ray pickRay = new Ray(penHead.position, penHead.forward);
            if (Physics.Raycast(pickRay, out RaycastHit phit, pickMaxDistance, paletteMask))
            {
                var rend = phit.collider.GetComponent<Renderer>();
                if (rend != null)
                {
                    Material mat = rend.sharedMaterial; // 或 rend.material（会实例化）
                    Texture2D tex2D = null;
                    Vector2 scale = Vector2.one, offset = Vector2.zero;

                    // ① 先试 URP 的 _BaseMap
                    if (mat.HasProperty("_BaseMap"))
                    {
                        tex2D = mat.GetTexture("_BaseMap") as Texture2D;
                        scale = mat.GetTextureScale("_BaseMap");
                        offset = mat.GetTextureOffset("_BaseMap");
                    }
                    // ② 否则用 Standard 的 _MainTex
                    else if (mat.HasProperty("_MainTex"))
                    {
                        tex2D = mat.GetTexture("_MainTex") as Texture2D;
                        scale = mat.GetTextureScale("_MainTex");
                        offset = mat.GetTextureOffset("_MainTex");
                    }

                    if (tex2D != null)
                    {
                        Vector2 uv = phit.textureCoord;     // 必须 MeshCollider 才靠谱
                        uv = uv * scale + offset;           // 应用平铺/偏移
                        uv.x -= Mathf.Floor(uv.x);          // wrap 到 0~1
                        uv.y -= Mathf.Floor(uv.y);

                        // 贴图需要 Read/Write Enabled
                        Color sampled = tex2D.GetPixelBilinear(uv.x, uv.y);
                        brushCol = sampled;

                        Debug.Log($"Picked {sampled} from {mat.shader.name}  tex={tex2D.name}  uv={uv}");
                    }
                    else
                    {
                        Debug.LogWarning("取色失败：材质没有可用贴图（_BaseMap/_MainTex）。");
                    }
                }
            }
        }




    }

    private void RenderBrushToBoard(RaycastHit hit)
    {

        Vector2 dir = hit.textureCoord - lastuv;

        if (Vector3.SqrMagnitude(dir) > brushSize * brushSize)
        {
            int length = Mathf.CeilToInt(dir.magnitude / brushSize);

            for (int i = 0; i < length; i++)
            {
                RenderToMatTex(lastuv + dir.normalized * i * brushSize);
            }

        }
        RenderToMatTex(hit.textureCoord);
    }

    private void RenderToMatTex(Vector2 uv)
    {
        effectMat.SetVector("_BrushPos", new Vector4(uv.x, uv.y, lastuv.x, lastuv.y));
        effectMat.SetColor("_BrushColor", brushCol);
        effectMat.SetFloat("_BrushSize", brushSize);
        Graphics.Blit(cacheTex, currentTex, effectMat);
        renderMat.SetTexture("_MainTex", currentTex);
        Graphics.Blit(currentTex, cacheTex);
        Debug.Log("Blit at uv = " + uv);
        renderMat.SetTexture("_MainTex", cacheTex);



    }

    private void Initialized()
    {
        brushMaxSize = brushSize;
        effectMat = new Material(Shader.Find("Brush/MarkPenEffect"));
        Material boardMat = board.GetComponent<MeshRenderer>().material;
        tex = boardMat.mainTexture;

        renderMat = boardMat;

        cacheTex = new RenderTexture(tex.width, tex.height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(tex, cacheTex);
        renderMat.SetTexture("_MainTex", cacheTex);

        currentTex = new RenderTexture(tex.width, tex.height, 0, RenderTextureFormat.ARGB32);

        Debug.Log("effectMat = " + effectMat);


    }


}
