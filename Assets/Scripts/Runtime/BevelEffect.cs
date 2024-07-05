using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class BevelEffect : MonoBehaviour
{

    [TextArea(3, 5)]
    public string matrix_str = "";



    private Matrix3x3 matrix
    {
        get
        {
            var s = matrix_str.Split(',');
            if (s.Length == 9)
            {
                return new Matrix3x3(
                    (float)double.Parse(s[0]),
                    -(float)double.Parse(s[1]),
                    (float)double.Parse(s[2]),
                    -(float)double.Parse(s[3]),
                    (float)double.Parse(s[4]),
                    (float)double.Parse(s[5]),
                    (float)double.Parse(s[6]),
                    (float)double.Parse(s[7]),
                    (float)double.Parse(s[8])
                );
            }
            else
            {
                return new Matrix3x3(1, 0, 0, 0, 1, 0, 0, 0, 1);
            }
        }
    }

    public TextMeshProUGUI tmp => GetComponent<TextMeshProUGUI>();

    
    private void OnPreRenderText(TMP_TextInfo obj)
    {
        var mesh = obj.meshInfo;
        float xmin = 0, ymin = 0, xmax = 0, ymax = 0f;
        foreach (var mi in mesh)
        {
            foreach (var v in mi.vertices)
            {
                xmin = Math.Min(xmin, v.x);
                ymin = Math.Min(ymin, v.y);
                xmax = Math.Max(xmax, v.x);
                ymax = Math.Max(ymax, v.y);
            }
        }
        var c = new Vector2((xmin + xmax) / 2f, (ymin + ymax) / 2f);
        var rect = GetComponent<RectTransform>().rect;
        var center = rect.center;
        var mat = new Matrix3x3(1, 0, 0, 0, 1, 0, center.x - c.x, center.y - c.y, 1);
        
        if (mesh.Length > 0)
        {
            var info = mesh[0];
            var vertices = info.vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                var v = vertices[i];
                var nv = new Vector3(v.x, v.y, 1) * mat * matrix;
                vertices[i] = new Vector3(nv.x, nv.y, v.z);
            }
        }
    }

    protected void OnEnable()
    {
        if (tmp != null)
        {
            tmp.OnPreRenderText += OnPreRenderText;
            tmp.SetAllDirty();
            // OnPreRenderText(tmp.textInfo);
        }
    }

    protected void OnDisable()
    {
        if (tmp != null)
        {
            tmp.OnPreRenderText -= OnPreRenderText;
            tmp.SetAllDirty();
        }
    }

    private void OnValidate()
    {
        tmp.SetAllDirty();
    }
}
