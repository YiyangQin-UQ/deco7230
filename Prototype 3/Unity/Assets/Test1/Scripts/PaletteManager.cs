using UnityEngine;

public class PaletteSlots : MonoBehaviour
{
    [Header("Drag 3 slot objects here (set them Inactive initially)")]
    public GameObject slot1;
    public GameObject slot2;
    public GameObject slot3;

    [Header("State (debug)")]
    public bool penInZone = false;              // 笔尖是否在盘内
    public Color?[] slotColors = new Color?[3]; // 槽位记录（null=空）

    void Reset()
    {
        penInZone = false;
        slotColors = new Color?[3];
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PenTip")) penInZone = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PenTip")) penInZone = false;
    }

    // ========= 放色：有空位就按 0→1→2 填；满了就把最早的顶掉（FIFO） =========
    public bool PlaceOrOverwriteOldest(Color c)
    {
        // 1) 有空位：按序填入
        for (int i = 0; i < 3; i++)
        {
            if (slotColors[i] == null)
            {
                ApplyToSlot(GetSlotObj(i), i, c);
                return true;
            }
        }

        // 2) 满了：左移一格，最后一格放新色
        slotColors[0] = slotColors[1];
        slotColors[1] = slotColors[2];
        slotColors[2] = c;

        var s0 = GetSlotObj(0);
        var s1 = GetSlotObj(1);
        var s2 = GetSlotObj(2);

        CopyColorBetweenObjs(s1, s0);
        CopyColorBetweenObjs(s2, s1);
        ApplyColorToObj(s2, c);

        if (s0 && !s0.activeSelf) s0.SetActive(true);
        if (s1 && !s1.activeSelf) s1.SetActive(true);
        if (s2 && !s2.activeSelf) s2.SetActive(true);

        return true;
    }

    // ========= 你提到的：ApplyToSlot =========
    void ApplyToSlot(GameObject slotObj, int idx, Color c)
    {
        slotColors[idx] = c;
        if (slotObj != null)
        {
            if (!slotObj.activeSelf) slotObj.SetActive(true);
            ApplyColorToObj(slotObj, c);
            slotObj.transform.localScale *= 1.02f; // 轻微反馈
        }
    }

    // 把颜色涂到某个 slot 物体上
    void ApplyColorToObj(GameObject slotObj, Color c)
    {
        var mr = slotObj.GetComponent<MeshRenderer>();
        if (!mr) return;

        var mat = mr.material;
        c.a = 1f;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c); // URP/HDRP
        if (mat.HasProperty("_Color"))     mat.SetColor("_Color", c);     // 标准
        if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", c * 0.25f);
    }

    // 复制一个 slot 的当前可见颜色到另一个
    void CopyColorBetweenObjs(GameObject from, GameObject to)
    {
        if (!from || !to) return;
        var mrFrom = from.GetComponent<MeshRenderer>();
        var mrTo   = to.GetComponent<MeshRenderer>();
        if (!mrFrom || !mrTo) return;

        Color c = Color.white;
        var matF = mrFrom.material;
        if (matF.HasProperty("_BaseColor")) c = matF.GetColor("_BaseColor");
        else if (matF.HasProperty("_Color")) c = matF.GetColor("_Color");

        ApplyColorToObj(to, c);
    }

    GameObject GetSlotObj(int i)
    {
        if (i == 0) return slot1;
        if (i == 1) return slot2;
        return slot3;
    }

    // 一键清空
    public void ClearAll()
    {
        slotColors = new Color?[3];
        if (slot1) slot1.SetActive(false);
        if (slot2) slot2.SetActive(false);
        if (slot3) slot3.SetActive(false);
    }

    public int FilledCount()
    {
        int n = 0;
        for (int i = 0; i < 3; i++) if (slotColors[i] != null) n++;
        return n;
    }
}
