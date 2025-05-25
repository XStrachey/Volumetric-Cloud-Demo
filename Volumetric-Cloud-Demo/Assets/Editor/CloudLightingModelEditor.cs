using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace JSZX.Graphics.VolumetricCloud
{
    [CustomEditor(typeof(CloudLightingModel))]
    public class CloudLightingModelEditor : Editor
    {
        Editor editor;

        public override void OnInspectorGUI()
        {
            CloudLightingModel model = target as CloudLightingModel;

            model.lightingSettings = EditorGUILayout.ObjectField("Lighting Settings", model.lightingSettings, typeof(LightingSettings), false) as LightingSettings;

            DrawSettingsEditor (model.lightingSettings, ref editor);
        }

        void DrawSettingsEditor (UnityEngine.Object settings, ref Editor editor)
        {
            if (settings != null)
            {
                using (var check = new EditorGUI.ChangeCheckScope ())
                {
                    {
                        Editor.CreateCachedEditor (settings, null, ref editor);
                        editor.OnInspectorGUI ();
                    }
                }
            }
        }
    }
}