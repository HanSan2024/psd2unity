using System;
using System.Collections.Generic;
using System.IO;
using PhotoshopFile;
using UnityEditor.AssetImporters;
using UnityEditor.U2D.Common;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UIElements;

#if ENABLE_2D_ANIMATION
using UnityEngine.U2D.Animation;
using UnityEditor.U2D.Animation;
#endif

namespace UnityEditor.U2D.PSD
{
    /// <summary>
    /// Inspector for PSDImporter
    /// </summary>
    [CustomEditor(typeof(PSDImporterUGUI))]
    [MovedFrom("UnityEditor.Experimental.AssetImporters")]
    [CanEditMultipleObjects]
    public class PSDImporterEditorUGUI : PSDImporterEditor
    {
        public readonly GUIContent importAsUGUI = EditorGUIUtility.TrTextContent("Import as UGUI", "");
        SerializedProperty m_ImportAsUGUI;

        public override void OnEnable()
        {
            base.OnEnable();
            m_ImportAsUGUI = serializedObject.FindProperty("m_ImportAsUGUI");
        }

        internal override void DoSpriteInspector()
        {
            base.DoSpriteInspector();
            EditorGUILayout.PropertyField(m_ImportAsUGUI, importAsUGUI);
            GUILayout.Space(5);
        }
    }

}