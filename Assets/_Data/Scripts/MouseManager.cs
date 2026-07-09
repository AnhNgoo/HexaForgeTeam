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

    private bool isMouseVisible = true;

    private void Start()
    {

    }

    private void Update()
    {
        if (InputActions.Keyboard.Escape.triggered && !isMouseVisible)
        {
            ShowMouse();
        }
        else if (InputActions.Keyboard.LeftMouse.triggered && isMouseVisible)
        {
            HideMouse();
        }

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
        Debug.Log("Show Mouse");
        isMouseVisible = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // Ẩn chuột
    public void HideMouse()
    {
        Debug.Log("Hide Mouse");
        isMouseVisible = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
