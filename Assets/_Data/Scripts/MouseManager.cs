using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class MouseManager : Singleton<MouseManager>
{
    [SerializeField][Range(0, 100)] private float mouseSensitivity = 30f;
    [SerializeField][Range(0, 1)] private float yMultiplierSensitivity = 1f;
    [SerializeField][Range(0, 1)] private float xMultiplierSensitivity = 1f;
    private InputActions InputActions => InputManager.InputActions;

    public bool IsMouseVisible { get; private set; } = true;
    private MenuType lastMenuState = MenuType.None;

    private void Start()
    {

    }

    private void Update()
    {
        if (UIManager.Instance?.CurrentMenuType != lastMenuState)
        {
            if (UIManager.Instance?.CurrentMenuType == MenuType.GameplayMenu)
            {
                HideMouse();
            }
            else
            {
                ShowMouse();
            }
            lastMenuState = UIManager.Instance.CurrentMenuType;
        }


        if (UIManager.Instance?.CurrentMenuType == MenuType.GameplayMenu)
            RotateCamera();
    }


    // Xoay cam theo chuột
    private void RotateCamera()
    {
        CinemachinePOV pov = CameraManager.Instance?.GetCurrentCameraPOV();
        if (pov == null)
            return;

        // Giá trị trục = giá trị cam hiện tại + giá trị chuột * độ nhạy (độ nhạy * 0.01 để chuyển từ 0-100 về 0-1) * hệ số nhân độ nhạy (tùy chỉnh để tăng giảm độ nhạy theo từng trục)
        float horizontal = pov.m_HorizontalAxis.Value + InputActions.Keyboard.Look.ReadValue<Vector2>().x * (mouseSensitivity * 0.01f) * xMultiplierSensitivity;
        float vertical = pov.m_VerticalAxis.Value + InputActions.Keyboard.Look.ReadValue<Vector2>().y * -(mouseSensitivity * 0.01f) * yMultiplierSensitivity;
        pov.m_HorizontalAxis.Value = horizontal;
        pov.m_VerticalAxis.Value = Mathf.Clamp(vertical, -70f, 70f);
    }

    public void SetMouseSensitivity(float sensitivity)
    {
        mouseSensitivity = sensitivity;
    }
    // Hiển chuột
    public void ShowMouse()
    {
        IsMouseVisible = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // Ẩn chuột
    public void HideMouse()
    {
        IsMouseVisible = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
