using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace JSZX.Graphics.VolumetricCloud
{
    [CustomEditor(typeof(AuthoringSystem))]
    public class AuthoringSystemEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            AuthoringSystem system = target as AuthoringSystem;

            system.weatherSettings.weatherTexture = EditorGUILayout.ObjectField("weather map", system.weatherSettings.weatherTexture, typeof(Texture2D), false) as Texture2D;
            
            system.weatherSettings.windDirection = EditorGUILayout.Vector3Field("wind direction", system.weatherSettings.windDirection);
            system.weatherSettings.windMainSpeed = EditorGUILayout.FloatField("wind speed", system.weatherSettings.windMainSpeed);

            system.weatherSettings.coverage = EditorGUILayout.Slider("overall coverage", system.weatherSettings.coverage, 0, 1);
        }
    }
}