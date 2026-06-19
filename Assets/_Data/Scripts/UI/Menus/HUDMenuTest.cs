using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class HUDMenuTest : MenuBase
{
    public override MenuType menuType => MenuType.HUDMenuTest;

    protected override void LoadComponent()
    {

    }

    protected override void LoadComponentRuntime()
    {

    }

    public override void Open(object data = null)
    {
        base.Open(data);

    }
    public override void Close()
    {
        base.Close();

    }

    private void Start()
    {

    }

    private void OnDestroy()
    {

    }
    private void Update()
    {

    }
}
