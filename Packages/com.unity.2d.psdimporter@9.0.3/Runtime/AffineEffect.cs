using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[ExecuteAlways]
public class AffineEffect : MonoBehaviour
{
    [SerializeField]
    // [HideInInspector]
    public Matrix3x3 matrix;

    public TextMeshProUGUI textMeshProUGUI => GetComponent<TextMeshProUGUI>();


    private void OnPreRenderText(TMP_TextInfo obj)
    {
        var meshes = obj.meshInfo;
        float xmin = 0, ymin = 0, xmax = 0, ymax = 0f;
        foreach (var mi in meshes)
        {
            foreach (var v in mi.vertices)
            {
                xmin = Mathf.Min(xmin, v.x);
                ymin = Mathf.Min(ymin, v.y);
                xmax = Mathf.Max(xmax, v.x);
                ymax = Mathf.Max(ymax, v.y);
            }
        }

        var c = new Vector2((xmin + xmax) / 2f, (ymin + ymax) / 2f);
        var rect = GetComponent<RectTransform>().rect;
        var center = rect.center;
        var m = new Matrix3x3(1, 0, 0, 0, 1, 0, center.x - c.x, center.y - c.y, 1);
        foreach (var mesh in meshes)
        {
            var vertices = mesh.vertices;
            for (var i = 0; i < vertices.Length; i++)
            {
                var v = vertices[i];
                var nv = new Vector3(v.x, v.y, 1) * m * matrix;
                vertices[i] = new Vector3(nv.x, nv.y, v.z);
            }
        }
    }

    protected void OnEnable()
    {
        if (textMeshProUGUI)
        {
            textMeshProUGUI.OnPreRenderText += OnPreRenderText;
            textMeshProUGUI.SetAllDirty();
        }
    }

    protected void OnDisable()
    {
        if (textMeshProUGUI != null)
        {
            textMeshProUGUI.OnPreRenderText -= OnPreRenderText;
            textMeshProUGUI.SetAllDirty();
        }
    }


#if UNITY_EDITOR
    private void OnValidate()
    {
        if (textMeshProUGUI != null)
        {
            textMeshProUGUI.SetAllDirty();
        }
    }
#endif
}