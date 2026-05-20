using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssignCameraForCanvas : LoadComponents
{
    [SerializeField] private Canvas canvas;
    protected override void LoadComponent()
    {
        if (canvas == null)
            canvas = GetComponent<Canvas>();
        if (canvas != null)
            canvas.worldCamera = Camera.main;
    }

    protected override void LoadComponentRuntime()
    {

    }

    private void OnEnable()
    {
        if (canvas != null)
            canvas.worldCamera = Camera.main;
    }

    private void OnDisable()
    {
        if (canvas != null)
            canvas.worldCamera = null;
    }
}
