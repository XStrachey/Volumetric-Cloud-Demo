using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Slice3DWin : EditorWindow
{
    public ComputeShader slicerCS;
    public ComputeShader slipterCS;
    public int layer;

    public Texture3D tex3D;

    static Slice3D _slice3D;
    static Split2D _split2D;

    [MenuItem("Tools/JSZX/Cloud Toolkit/3D RenderTexture Slicer")]
    public static void OnMenuItem()
    {
        var window = EditorWindow.GetWindow<Slice3DWin>();
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
        slicerCS = EditorGUILayout.ObjectField("纹理分片-计算着色器", slicerCS, typeof(ComputeShader), false) as ComputeShader;
        slipterCS = EditorGUILayout.ObjectField("通道分离-计算着色器", slipterCS, typeof(ComputeShader), false) as ComputeShader;
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox("设置", MessageType.None);
        tex3D = EditorGUILayout.ObjectField("目标纹理", tex3D, typeof(Texture3D), false) as Texture3D;
        if (null != tex3D)
        {
            layer = EditorGUILayout.IntSlider("切层", layer, 0, tex3D.width - 1);
            _split2D.channel = (Channel)EditorGUILayout.Popup("检视通道", (int)_split2D.channel, Enum.GetNames(typeof(Channel)));
        }
        EditorGUILayout.Space();

        if (GUILayout.Button ("Draw"))
        {
            EnsureArgs();
            _slice3D.Slice (tex3D);
            _split2D.Split (_slice3D.slicedLayer);
        }

        GUILayout.Box(_slice3D.slicedLayer);
        GUILayout.Box(_split2D.resultRT);
    }

    void EnsureArgs ()
    {
        if (null == _slice3D)
            if (null != slicerCS)
                _slice3D = new Slice3D(slicerCS);

        if (null == _split2D)
            if (null != slipterCS)
                _split2D = new Split2D(slipterCS);

        _slice3D.layer = this.layer;
    }
}
