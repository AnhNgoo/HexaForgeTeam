using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MobileCamera : LoadComponents
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private CinemachinePOV pov;

    [SerializeField] private float horizontalSensitivity = 5f;
    [SerializeField] private float verticalSensitivity = 2f;
    [SerializeField] private bool ignoreUI = false;

    private Vector2 lastTouchPosition;
    private bool isDragging = false;
    private int lookFingerId = -1;

    private float targetXAxis;
    private float targetYAxis;

    private void Update()
    {
        HandleTouchInput();
        ApplyCamera();
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount <= 0)
        {
            isDragging = false;
            lookFingerId = -1;
            return;
        }

        if (lookFingerId == -1)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch startTouch = Input.GetTouch(i);
                if (startTouch.phase != TouchPhase.Began)
                    continue;

                if (startTouch.position.x <= Screen.width * 0.5f)
                    continue; //Chỉ bắt đầu xoay khi chạm ở nửa phải màn hình

                if (ignoreUI && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(startTouch.fingerId))
                    continue; //Không bắt đầu kéo nếu chạm vào UI

                lookFingerId = startTouch.fingerId;
                isDragging = true;
                lastTouchPosition = startTouch.position;
                break;
            }
        }

        if (lookFingerId == -1)
            return;

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            if (touch.fingerId != lookFingerId)
                continue;

            switch (touch.phase)
            {
                case TouchPhase.Moved:
                    if (!isDragging) return;

                    Vector2 delta = touch.position - lastTouchPosition;

                    targetXAxis += delta.x * horizontalSensitivity * Time.deltaTime;
                    targetYAxis -= delta.y * verticalSensitivity * Time.deltaTime;

                    targetYAxis = Mathf.Clamp(targetYAxis, -70, 70);

                    lastTouchPosition = touch.position;
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    isDragging = false;
                    lookFingerId = -1;
                    break;
            }

            return;
        }

        isDragging = false;
        lookFingerId = -1;
    }

    private void ApplyCamera()
    {
        pov.m_HorizontalAxis.Value = targetXAxis;
        pov.m_VerticalAxis.Value = targetYAxis;
    }

    public void SetAxis(float horizontal, float vertical)
    {
        targetXAxis = horizontal;
        targetYAxis = Mathf.Clamp(vertical, -70f, 70f);
        ApplyCamera();
    }

    public void SetSensitivity(float horizontal, float vertical)
    {
        horizontalSensitivity = horizontal;
        verticalSensitivity = vertical;
    }

    protected override void LoadComponent()
    {
        if (pov == null)
            pov = virtualCamera.GetCinemachineComponent<CinemachinePOV>();
        if (virtualCamera == null)
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    protected override void LoadComponentRuntime()
    {

    }
}
