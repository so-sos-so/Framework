﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Framework;
using Framework.Assets;
using Framework.Asynchronous;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public class Test : MonoBehaviour
{
    [Button]
    private void Start()
    {
        var per = typeof(Addressables).GetProperty("Instance", BindingFlags.Static| BindingFlags.NonPublic);
        var obj = per.GetValue(null);
        Dictionary<object, AsyncOperationHandle> dic = obj.GetType().GetField("m_resultToHandle", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(obj) as
            Dictionary<object, AsyncOperationHandle>;
        foreach (var key in dic.Keys)
        {
            if (key != null)
            {
                Addressables.Release(key);
            }
        }
    }

    [Button]
    private void LoadCube()
    {
        Addressables.InstantiateAsync("cube").Completed += handle => print("load cube");
    }

    private SpriteLoader Single;
    [Button]
    private async void LoadSingle()
    {
        Single = new SpriteLoader();
        var sp = await Single.LoadSprite("single");
        print(sp);
    }

    private SpriteLoader MulSprite;
    
    [Button]
    public async void LoadMul()
    {
        MulSprite = new SpriteLoader();
        var sp = await MulSprite.LoadSprite("sheet/sprite_sheet_0");
        print(sp);
    }

    [Button]
    public void ReleaseMul()
    {
        MulSprite.Release();
    }

    [Button]
    public void ReleaseSingle()
    {
        Single.Release();
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(200, 200, 100, 50), "LoadSingle"))
        {
            LoadSingle();
        }
        if (GUI.Button(new Rect(200, 300, 100, 50), "LoadMul"))
        {
            LoadMul();
        }
        if (GUI.Button(new Rect(200, 400, 100, 50), "ReleaseSingle"))
        {
            ReleaseSingle();
        }
        if (GUI.Button(new Rect(200, 500, 100, 50), "ReleaseMul"))
        {
            ReleaseMul();
        }
        if (GUI.Button(new Rect(200, 600, 100, 50), "ResourcesLoad"))
        {
            Resources.Load<Sprite>("回锅肉");
        }
        if (GUI.Button(new Rect(200, 700, 100, 50), "Unloadall"))
        {
            Resources.UnloadUnusedAssets();
        }
    }
}
