using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class LoadComponents : MonoBehaviour
{
    protected virtual void Awake()
    {
        LoadComponentRuntime();
    }

    protected virtual void OnValidate()
    {
        LoadComponent();
    }

    [Button("Load Components In Edit Mode")]
    /// <summary>
    /// Gán component trực tiếp trên editor mà không cần chạy game, không cần phải kéo tay 
    /// </summary>
    protected abstract void LoadComponent();
    protected abstract void LoadComponentRuntime();
}
