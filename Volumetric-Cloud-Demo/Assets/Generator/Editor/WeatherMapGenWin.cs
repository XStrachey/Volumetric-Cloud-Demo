using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WeatherMapGenWin : EditorWindow
{
    const int computeThreadGroupSize = 16;

    public ComputeShader noiseComputeCS;
    public int resolution = 512;
    public WeatherNoiseSettings settings;

    public string name;

    static WeatherMapGen _weatherMapGen;

    Editor noiseSettingsEditor;

    [MenuItem("Tools/JSZX/Cloud Toolkit/Weather Map Generator")]
    public static void OnMenuItem()
    {
        var window = EditorWindow.GetWindow<WeatherMapGenWin>();
        window.minSize = new Vector2(400, 640);
        window.Show();
    }

    void OnEnable()
    {
        EnsureArgs();
    }

    void OnGUI()
    {
        EditorGUILayout.HelpBox("计算着色器", MessageType.None);
        noiseComputeCS = EditorGUILayout.ObjectField("噪声生成（天气图）-计算着色器", noiseComputeCS, typeof(ComputeShader), false) as ComputeShader;
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox("设置", MessageType.None);
        name = EditorGUILayout.TextField("天气图存储名", name);
        resolution = EditorGUILayout.IntSlider("天气图分辨率", resolution, 128, 2048);
        settings = EditorGUILayout.ObjectField("噪声属性", settings, typeof(WeatherNoiseSettings), false) as WeatherNoiseSettings;
        EditorGUILayout.Space();

        DrawSettingsEditor (_weatherMapGen.settings, ref _weatherMapGen.showSettingsEditor, ref noiseSettingsEditor);

        if (GUILayout.Button ("Update Weather Map"))
        {
            EnsureArgs();
            _weatherMapGen.UpdateMap();
        }

        if (GUILayout.Button ("Save"))
        {
            if (null != _weatherMapGen.weatherMap)
            {
                var path = EditorUtility.SaveFilePanel(
                    "Save texture as PNG",
                    "",
                    _weatherMapGen.weatherMap + ".png",
                    "png");

                if (path.Length != 0)
                {
                    Texture2D outputTex = _weatherMapGen.ConvertFromRenderTexture(_weatherMapGen.weatherMap);
                    var pngData = outputTex.EncodeToPNG();
                    if (pngData != null)
                        File.WriteAllBytes(path, pngData);
                }
            }
        }

        GUILayout.Box(_weatherMapGen.weatherMap);
        GUILayout.Box(_weatherMapGen.weatherMapTex);
    }

    void EnsureArgs ()
    {
        if (null == _weatherMapGen)
            if (null != noiseComputeCS)
                _weatherMapGen = new WeatherMapGen(noiseComputeCS);

        _weatherMapGen.noiseComputeCS = this.noiseComputeCS;
        _weatherMapGen.resolution = this.resolution;
        _weatherMapGen.settings = this.settings;
        _weatherMapGen.name = this.name;
    }

    void DrawSettingsEditor (UnityEngine.Object settings, ref bool foldout, ref Editor editor)
    {
        if (settings != null)
        {
            foldout = EditorGUILayout.InspectorTitlebar (foldout, settings);
            using (var check = new EditorGUI.ChangeCheckScope ())
            {
                if (foldout)
                {
                    Editor.CreateCachedEditor (settings, null, ref editor);
                    editor.OnInspectorGUI ();
                }
            }
        }
    }
}
