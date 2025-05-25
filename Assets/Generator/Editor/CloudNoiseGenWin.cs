using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CloudNoiseGenWin : EditorWindow
{
    public ComputeShader noiseComputeCS;
    public ComputeShader slicerCS;

    const int computeThreadGroupSize = 8;
    public const string detailNoiseName = "DetailNoise";
    public const string shapeNoiseName = "ShapeNoise";

    [Header ("Noise Settings")]
    public int shapeResolution = 64;
    public int detailResolution = 32;

    [Header ("Shape Settings")]
    public ShapeNoiseSettings shapeNoiseSettings;

    [Header ("Detail Settings")]
    public DetailNoiseSettings detailNoiseSettings;

    static CloudNoiseGenerator _cloudNoiseGenerator;
    static Save3D _save3D;

    Editor noiseSettingsEditor;

    public NoiseSettings ActiveSettings
    {
        get
        {
            switch (_cloudNoiseGenerator.activeCloudNoiseType)
            {
                case CloudNoiseType.Shape:
                return shapeNoiseSettings;
                case CloudNoiseType.Detail:
                return detailNoiseSettings;
                default:
                return null;
            }
        }
    }

    [MenuItem("Tools/JSZX/Cloud Toolkit/Cloud Noise Generator")]
    public static void OnMenuItem()
    {
        var window = EditorWindow.GetWindow<CloudNoiseGenWin>();
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
        noiseComputeCS = EditorGUILayout.ObjectField("噪声生成（云）-计算着色器", noiseComputeCS, typeof(ComputeShader), false) as ComputeShader;
        slicerCS = EditorGUILayout.ObjectField("纹理分片-计算着色器", slicerCS, typeof(ComputeShader), false) as ComputeShader;
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox("形状噪声设置", MessageType.None);
        shapeResolution = EditorGUILayout.IntSlider("形状噪声分辨率", shapeResolution, 32, 256);
        shapeNoiseSettings = EditorGUILayout.ObjectField("Shape", shapeNoiseSettings, typeof(ShapeNoiseSettings), false) as ShapeNoiseSettings;
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox("细节噪声设置", MessageType.None);
        detailResolution = EditorGUILayout.IntSlider("细节噪声分辨率", detailResolution, 16, 128);
        detailNoiseSettings = EditorGUILayout.ObjectField("Detail", detailNoiseSettings, typeof(DetailNoiseSettings), false) as DetailNoiseSettings;
        EditorGUILayout.Space();

        _cloudNoiseGenerator.activeCloudNoiseType = (CloudNoiseType)EditorGUILayout.Popup("检视类型", (int)_cloudNoiseGenerator.activeCloudNoiseType, Enum.GetNames(typeof(CloudNoiseType)));

        if (ActiveSettings != null)
        {
            DrawSettingsEditor (ActiveSettings, ref _cloudNoiseGenerator.showSettingsEditor, ref noiseSettingsEditor);
        }

        if (GUILayout.Button ("Update Shape"))
        {
            EnsureArgs();
            _cloudNoiseGenerator.UpdateShape ();
            _save3D.Save(_cloudNoiseGenerator.shapeTexture, shapeNoiseName);
        }

        if (GUILayout.Button ("Update Detail"))
        {
            EnsureArgs();
            _cloudNoiseGenerator.UpdateDetail ();
            _save3D.Save(_cloudNoiseGenerator.detailTexture, detailNoiseName);
        }

        if (GUILayout.Button ("Update & Save"))
        {
            EnsureArgs();
            _cloudNoiseGenerator.UpdateNoise ();
            _save3D.Save(_cloudNoiseGenerator.shapeTexture, shapeNoiseName);
            _save3D.Save(_cloudNoiseGenerator.detailTexture, detailNoiseName);
        }
    }

    void EnsureArgs ()
    {
        if (null == _cloudNoiseGenerator)
            if (null != noiseComputeCS)
                _cloudNoiseGenerator = new CloudNoiseGenerator(noiseComputeCS);

        if (null == _save3D)
            if (null != slicerCS)
                _save3D = new Save3D(slicerCS);

        _cloudNoiseGenerator.noiseComputeCS = this.noiseComputeCS;
        _save3D.slicerCS = this.slicerCS;

        _cloudNoiseGenerator.shapeResolution = this.shapeResolution;
        _cloudNoiseGenerator.detailResolution = this.detailResolution;

        _cloudNoiseGenerator.shapeNoiseSettings = this.shapeNoiseSettings;
        _cloudNoiseGenerator.detailNoiseSettings = this.detailNoiseSettings;
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
