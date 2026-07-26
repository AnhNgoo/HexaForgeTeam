using UnityEngine;
using UnityEngine.UI;

public class UIPanelCloseButton : LoadComponents
{
    [SerializeField]
    private Button button;

    protected override void LoadComponent()
    {
        if (button == null)
        {
            button =
                GetComponent<Button>();
        }
    }

    protected override void LoadComponentRuntime()
    {
    }

    private void Start()
    {
        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(
            ClosePanel);
    }

    // BƯỚC THÊM MỚI: Bắt sự kiện phím ESC khi nút Close này đang hoạt động
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePanel();
        }
    }

   private void ClosePanel()
    {
        if (UIManager.Instance == null)
        {
            return;
        }

        UIManager.Instance.CloseAllMenus();

        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance.IsBusy = false;
            InteractManagerV2.Instance.ForceRefresh();
        }
    }
}