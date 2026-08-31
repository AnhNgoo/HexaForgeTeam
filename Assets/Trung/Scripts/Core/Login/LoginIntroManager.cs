using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System;

public class LoginIntroManager : MonoBehaviour
{
    public static LoginIntroManager Instance;

    [Header("Video Intro Components")]
    [SerializeField] private GameObject introPanelRoot;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private Button skipButton;

    private static bool hasPlayedIntroThisSession = false;
    private Action onIntroFinishedCallback;

    // Tự động reset trạng thái mỗi khi Unity Editor bấm Play Mode lại từ đầu
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticData()
    {
        hasPlayedIntroThisSession = false;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public bool TryPlayIntro(Action onFinished)
    {
        onIntroFinishedCallback = onFinished;

        if (hasPlayedIntroThisSession)
        {
            if (introPanelRoot != null) introPanelRoot.SetActive(false);
            return false;
        }

        hasPlayedIntroThisSession = true;

        if (introPanelRoot != null) introPanelRoot.SetActive(true);

        if (skipButton != null)
        {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(SkipIntro);
        }

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.Prepare();
            videoPlayer.Play();
        }
        else
        {
            SkipIntro();
        }

        return true;
    }

    private void Update()
    {
        if (introPanelRoot != null && introPanelRoot.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SkipIntro();
            }
        }
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        SkipIntro();
    }

    public void SkipIntro()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
            videoPlayer.Stop();
        }

        if (introPanelRoot != null)
        {
            introPanelRoot.SetActive(false);
        }

        var callback = onIntroFinishedCallback;
        onIntroFinishedCallback = null;
        callback?.Invoke();
    }
}