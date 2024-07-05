using System;
using System.Collections.Generic;
using System.IO;
using PDNWrapper;
using UnityEngine;
using Unity.Collections;
using System.Linq;
using System.Reflection;
using UnityEditor.AssetImporters;
using UnityEditor.U2D.Common;
using UnityEditor.U2D.Sprites;
using UnityEngine.U2D;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UI;
using TMPro;

#if ENABLE_2D_ANIMATION
using UnityEditor.U2D.Animation;
using UnityEngine.U2D.Animation;
#endif

namespace UnityEditor.U2D.PSD
{
    /// <summary>
    /// ScriptedImporter to import Photoshop files
    /// </summary>
    // Version using unity release + 5 digit padding for future upgrade. Eg 2021.2 -> 21200000
    [ScriptedImporter(23100002, new string[] { "psb & psd" }, new[] { "psb", "psd" }, AllowCaching = true)]
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.2d.psdimporter@latest")]
    public class PSDImporterUGUI : PSDImporter
    {
        [SerializeField]
        internal bool m_ImportAsUGUI = false;

        internal override GameObject OnProducePrefab(AssetImportContext ctx, string assetname, Sprite[] sprites)
        {
            return m_ImportAsUGUI
                ? OnProducePrefabUGUI(ctx, assetname, sprites)
                : base.OnProducePrefab(ctx, assetname, sprites);
        }

        internal override void BuildGroupGameObject(List<PSDLayer> psdGroup, int index, Transform root)
        {
            base.BuildGroupGameObject(psdGroup, index, root);
            var psdData = psdGroup[index];
            if (psdData.gameObject != null)
            {
                psdData.gameObject.transform.SetAsFirstSibling();
            }
        }

        GameObject OnProducePrefabUGUI(AssetImportContext ctx, string assetname, Sprite[] sprites)
        {
            GameObject root = null;
            if (sprites != null && sprites.Length > 0)
            {
                var spriteImportData = GetSpriteImportData();
                root = new GameObject();
                root.transform.SetSiblingIndex(0);
                root.name = assetname + "_GO";

                GetOrAddComponent<RectTransform>(root);
                GetOrAddComponent<Canvas>(root);
                GetOrAddComponent<GraphicRaycaster>(root);

#if ENABLE_2D_ANIMATION
                var currentCharacterData = characterData;

                var contextObjects = new List<UnityEngine.Object>();
                ctx.GetObjects(contextObjects);
                var spriteLib =
                    contextObjects.Find(x => x.GetType() == typeof(SpriteLibraryAsset)) as SpriteLibraryAsset;

                if (spriteLib != null)
                    root.AddComponent<SpriteLibrary>().spriteLibraryAsset = spriteLib;

                CharacterData? characterSkeleton = inCharacterMode
                    ? new CharacterData?(GetDataProvider<ICharacterDataProvider>().GetCharacterData())
                    : null;
#endif

                var psdLayers = GetPSDLayers();
                for (var i = 0; i < psdLayers.Count; ++i)
                {
                    BuildGroupGameObject(psdLayers, i, root.transform);
                }

                var boneGOs = CreateBonesGO(root.transform);
                for (var i = 0; i < psdLayers.Count; ++i)
                {
                    var l = psdLayers[i];
                    var layerSpriteID = l.spriteID;
                    var sprite = sprites.FirstOrDefault(x => x.GetSpriteID() == layerSpriteID);
                    var spriteMetaData = spriteImportData.FirstOrDefault(x => x.spriteID == layerSpriteID);

                    if (sprite != null && spriteMetaData != null && l.gameObject != null)
                    {
                        var graphic = ProduceGraphic(l, sprite);
                        var rtf = graphic.rectTransform;
                        rtf.pivot = new Vector2(0.5f, 0.5f);
                        rtf.anchorMax = new Vector2(0.5f, 0.5f);
                        rtf.anchorMin = new Vector2(0.5f, 0.5f);
                        rtf.sizeDelta = new Vector2(l.width, l.height);

                        var pivot = spriteMetaData.pivot;
                        pivot.x *= spriteMetaData.rect.width;
                        pivot.y *= spriteMetaData.rect.height;

                        var spritePosition = spriteMetaData.spritePosition;
                        spritePosition.x += pivot.x;
                        spritePosition.y += pivot.y;
                        spritePosition *= (definitionScale);
                        // spritePosition *= (definitionScale / sprite.pixelsPerUnit);

                        rtf.position = new Vector3(spritePosition.x, spritePosition.y, 0f);

#if ENABLE_2D_ANIMATION
                        if (characterSkeleton != null)
                        {
                            var part = characterSkeleton.Value.parts.FirstOrDefault(x =>
                                x.spriteId == spriteMetaData.spriteID.ToString());
                            if (part.bones != null && part.bones.Length > 0)
                            {
                                var spriteSkin = l.gameObject.AddComponent<SpriteSkin>();
                                var img = graphic as Image;
                                if (img != null && img.sprite != null && img.sprite.GetBindPoses().Length > 0)
                                {
                                    var spriteBones = currentCharacterData.parts
                                        .FirstOrDefault(x => new GUID(x.spriteId) == img.sprite.GetSpriteID()).bones
                                        .Where(x => x >= 0 && x < boneGOs.Length).Select(x => boneGOs[x]);
                                    if (spriteBones.Any())
                                    {
                                        spriteSkin.SetRootBone(root.transform);
                                        spriteSkin.SetBoneTransforms(spriteBones.Select(x => x.go.transform).ToArray());
                                        if (spriteSkin.isValid)
                                            spriteSkin.CalculateBounds();
                                    }
                                }
                            }
                        }

                        GetSpriteLibLabel(layerSpriteID.ToString(), out var category, out var labelName);
                        if (!string.IsNullOrEmpty(category) && !string.IsNullOrEmpty(labelName))
                        {
                            var resolver = l.gameObject.AddComponent<SpriteResolver>();
                            resolver.SetCategoryAndLabel(category, labelName);
                            resolver.ResolveSpriteToSpriteRenderer();
                        }
#endif
                    }
                }

                // var prefabBounds = new Rect(0, 0, importData.documentSize.x / pixelsPerUnit, importData.documentSize.y / pixelsPerUnit);
                var prefabBounds = new Rect(0, 0, importData.documentSize.x, importData.documentSize.y);
                var documentPivot = (Vector3)ImportUtilities.GetPivotPoint(prefabBounds, m_DocumentAlignment, m_DocumentPivot);
                for (var i = 0; i < psdLayers.Count; ++i)
                {
                    var l = psdLayers[i];
                    if (l.gameObject == null || l.gameObject.GetComponent<Graphic>() == null)
                        continue;
                    var rtf = l.gameObject.GetComponent<RectTransform>();
                    rtf.anchoredPosition3D -= documentPivot;
                }

                for (int i = 0; i < boneGOs.Length; ++i)
                {
                    if (boneGOs[i].go.transform.parent != root.transform)
                        continue;
                    var p = boneGOs[i].go.transform.position;
                    p -= documentPivot;
                    boneGOs[i].go.transform.position = p;
                }
            }

            return root;
        }

        private Graphic ProduceGraphic(PSDLayer layer, Sprite sprite)
        {
            var obj = layer.gameObject;
            if (obj == null)
            {
                return null;
            }

            var textData = layer.GetLayerTextData(out var ttos);
            var effectData = layer.GetLayerEffectData();
            GetOrAddComponent<RectTransform>(obj);
            var graphic = obj.GetComponent<Graphic>();
            if (graphic)
            {
                DestroyImmediate(graphic);
            }

            if (textData != null)
            {
                var td = textData.Value;
                var tmp = GetOrAddComponent<TextMeshProUGUI>(obj);
                tmp.text = td.text;
                tmp.fontSize = td.fontSize;
                tmp.color = new Color(td.color.r, td.color.g, td.color.b, td.color.a * layer.bitmapLayer.Opacity);
                tmp.fontStyle = td.fontStyles;
                tmp.alignment = td.aligment;
                tmp.textWrappingMode = TextWrappingModes.PreserveWhitespaceNoWrap;

                var tfs = ttos.Transform;
                var matrix = new Matrix3x3((float)tfs[0], -(float)tfs[1], 0, -(float)tfs[2], (float)tfs[3], 0, 0, 0, 1);
                if (!matrix.Equals(Matrix3x3.identity))
                {
                    var affine = GetOrAddComponent<AffineEffect>(obj);
                    affine.matrix = matrix;
                }

                return tmp;
            }
            else
            {
                var img = GetOrAddComponent<Image>(obj);
                img.sprite = sprite;
                var color = img.color;
                color.a = layer.bitmapLayer.Opacity;
                img.color = color;

                if (effectData != null && effectData.Value.shadowEnabled)
                {
                    var ed = effectData.Value;
                    var shadow = GetOrAddComponent<Shadow>(obj);
                    shadow.effectColor = ed.shadowColor;
                    var y = ed.shadowDistance * Mathf.Sin(ed.shadowAngle / Mathf.Rad2Deg);
                    var x = ed.shadowDistance * Mathf.Cos(ed.shadowAngle / Mathf.Rad2Deg);
                    shadow.effectDistance = new Vector2(-x, -y);
                }

                return img;
            }
        }

        private static T GetOrAddComponent<T>(GameObject go) where T : Component
        {
            var t = go.GetComponent<T>();
            return t == null ? go.AddComponent<T>() : t;
        }
    }
}