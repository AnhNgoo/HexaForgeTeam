using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UIAutoSoundManager : MonoBehaviour
{
    public static UIAutoSoundManager Instance;

    [Header("Audio Source")]
    [SerializeField] private AudioSource uiAudioSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip buttonClickClip;
    [SerializeField] private AudioClip buttonHoverClip;

    [Header("Settings")]
    [Range(0f, 1f)] [SerializeField] private float clickVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float hoverVolume = 0.5f;
    [SerializeField] private float hoverCooldown = 0.05f; // Tránh spam âm thanh khi lướt chuột qua quá nhanh

    private float lastHoverTime = 0f;
    private HashSet<Button> registeredButtons = new HashSet<Button>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (uiAudioSource == null)
        {
            uiAudioSource = GetComponent<AudioSource>();
            if (uiAudioSource == null)
            {
                uiAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        uiAudioSource.playOnAwake = false;
        uiAudioSource.spatialBlend = 0f; // 2D Sound

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        ScanAndRegisterButtons();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        registeredButtons.Clear();
        ScanAndRegisterButtons();
    }

    /// <summary>
    /// Quét toàn bộ Button trong Scene và gán tự động sự kiện âm thanh
    /// </summary>
    public void ScanAndRegisterButtons()
    {
        Button[] buttons = Resources.FindObjectsOfTypeAll<Button>();

        for (int i = 0; i < buttons.Length; i++)
        {
            Button btn = buttons[i];
            if (btn == null || registeredButtons.Contains(btn)) continue;
            if (!btn.gameObject.scene.isLoaded) continue; // Bỏ qua Prefab assets trong Project

            RegisterButtonSound(btn);
        }
    }

    public void RegisterButtonSound(Button btn)
    {
        if (btn == null || registeredButtons.Contains(btn)) return;

        // 1. Gán âm thanh Click
        btn.onClick.AddListener(() => PlayButtonClickSound());

        // 2. Gán âm thanh Hover qua EventTrigger
        EventTrigger trigger = btn.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = btn.gameObject.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry hoverEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        hoverEntry.callback.AddListener((data) =>
        {
            if (btn.interactable)
            {
                PlayButtonHoverSound();
            }
        });
        trigger.triggers.Add(hoverEntry);

        registeredButtons.Add(btn);
    }

    public void PlayButtonClickSound()
    {
        if (buttonClickClip != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(buttonClickClip, clickVolume);
        }
    }

    public void PlayButtonHoverSound()
    {
        if (Time.unscaledTime - lastHoverTime < hoverCooldown) return;

        if (buttonHoverClip != null && uiAudioSource != null)
        {
            lastHoverTime = Time.unscaledTime;
            uiAudioSource.PlayOneShot(buttonHoverClip, hoverVolume);
        }
    }
}