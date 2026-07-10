using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayBossHealthUI : LoadComponents
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text bossNameText;
    [SerializeField] private Slider healthSlider;

    private EnemyBase _boss;

    protected override void Awake()
    {
        base.Awake();
        Hide();
    }

    protected override void LoadComponent()
    {
        if (root == null)
            root = transform.Find("BossHealthRoot")?.gameObject;

        if (bossNameText == null)
            bossNameText = transform.Find("BossHealthRoot/BossNameText")?.GetComponent<TMP_Text>();

        if (healthSlider == null)
            healthSlider = transform.Find("BossHealthRoot")?.GetComponent<Slider>();
    }

    protected override void LoadComponentRuntime()
    {
        if (root == null)
            root = transform.Find("BossHealthRoot")?.gameObject;

        if (bossNameText == null)
            bossNameText = transform.Find("BossHealthRoot/BossNameText")?.GetComponent<TMP_Text>();

        if (healthSlider == null)
            healthSlider = transform.Find("BossHealthRoot")?.GetComponent<Slider>();
    }

    private void Update()
    {
        if (_boss == null || _boss.Health.CurrentHealth <= 0f)
        {
            Hide();
            return;
        }

        healthSlider.value = Mathf.Clamp01(_boss.Health.CurrentHealth / _boss.Data.maxHealth);
    }

    public void Show(EnemyBase boss)
    {
        if (boss == null || boss.Data == null || !boss.Data.isBoss)
            return;

        _boss = boss;
        root.SetActive(true);
        bossNameText.text = boss.Data.bossDisplayName;
        healthSlider.value = Mathf.Clamp01(boss.Health.CurrentHealth / boss.Data.maxHealth);
    }

    public void Hide()
    {
        _boss = null;

        if (root != null)
            root.SetActive(false);
    }
}