using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace JSZX.Graphics.VolumetricCloud
{
    [CustomEditor(typeof(CloudDensityModel))]
    public class CloudDensityModelEditor : Editor
    {
        Editor editor;

        public override void OnInspectorGUI()
        {
            CloudDensityModel model = target as CloudDensityModel;

            model.shapeSettings = EditorGUILayout.ObjectField("Shape Settings", model.shapeSettings, typeof(ShapeSettings), false) as ShapeSettings;

            DrawSettingsEditor (model.shapeSettings, ref editor);
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