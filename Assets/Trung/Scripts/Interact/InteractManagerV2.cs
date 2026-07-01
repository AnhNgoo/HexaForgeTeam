using System.Collections.Generic;
using UnityEngine;

public class InteractManagerV2 : MonoBehaviour
{
    public static InteractManagerV2 Instance;

    [Header("Input")]
    [SerializeField]
    private KeyCode interactKey = KeyCode.F;

    [SerializeField]
    private bool enableMouseWheel = true;


    [Header("Debug")]
    [SerializeField]
    private bool debugMode;
    [SerializeField]
private float scrollCooldown = 0.15f;

private float nextScrollTime;

    private readonly List<InteractV2> interactObjects =
        new List<InteractV2>();

    private int currentIndex;
    public bool IsBusy { get; set; }

    public IReadOnlyList<InteractV2> InteractObjects
    {
        get
        {
            return interactObjects;
        }
    }

    public InteractV2 CurrentInteract
    {
        get
        {
            if (interactObjects.Count == 0)
            {
                return null;
            }

            currentIndex =
                Mathf.Clamp(
                    currentIndex,
                    0,
                    interactObjects.Count - 1);

            return interactObjects[currentIndex];
        }
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

    private void Update()
    {
        if (IsBusy)
{
    return;
}
        if (interactObjects.Count == 0)
        {
            return;
        }

        if (enableMouseWheel)
        {
            HandleMouseWheel();
        }

        if (Input.GetKeyDown(interactKey))
        {
            ExecuteCurrent();
        }
    }

    #region Register

    public void Register(
        InteractV2 interact)
    {
        if (interact == null)
        {
            return;
        }

        if (interactObjects.Contains(interact))
        {
            return;
        }

        interactObjects.Add(interact);

if (interactObjects.Count == 1)
{
    currentIndex = 0;
}

RefreshUI();

DebugCurrent();
    }

    public void Unregister(
    InteractV2 interact)
{
    if (interact == null)
    {
        return;
    }

    if (!interactObjects.Remove(interact))
    {
        return;
    }

    if (interactObjects.Count == 0)
    {
        currentIndex = 0;

        RefreshUI();

        return;
    }

    if (currentIndex >= interactObjects.Count)
    {
        currentIndex =
            interactObjects.Count - 1;
    }

    if (currentIndex < 0)
    {
        currentIndex = 0;
    }

    RefreshUI();

    DebugCurrent();
}
    #endregion

    #region Execute

    public void ExecuteCurrent()
    {
        if (CurrentInteract == null)
        {
            return;
        }

        CurrentInteract.Execute();
    }

    #endregion

    #region Mouse Wheel

    private void HandleMouseWheel()
{
    if (Time.unscaledTime < nextScrollTime)
    {
        return;
    }

    float scroll = Input.mouseScrollDelta.y;

    if (scroll > 0f)
    {
        Previous();
        nextScrollTime = Time.unscaledTime + scrollCooldown;
    }
    else if (scroll < 0f)
    {
        Next();
        nextScrollTime = Time.unscaledTime + scrollCooldown;
    }
}

    public void Next()
    {
        if (interactObjects.Count <= 1)
{
    return;
}
        if (interactObjects.Count == 0)
        {
            return;
        }

        currentIndex++;

        if (currentIndex >=
            interactObjects.Count)
        {
            currentIndex = 0;
        }

        RefreshUI();

        DebugCurrent();
    }

    public void Previous()
    {
        if (interactObjects.Count <= 1)
{
    return;
}
        if (interactObjects.Count == 0)
        {
            return;
        }

        currentIndex--;

        if (currentIndex < 0)
        {
            currentIndex =
                interactObjects.Count - 1;
        }

        RefreshUI();

        DebugCurrent();
    }
        #endregion

    

    #region UI

    public void ForceRefresh()
{
    if (interactObjects.Count == 0)
    {
        return;
    }

    currentIndex = Mathf.Clamp(
        currentIndex,
        0,
        interactObjects.Count - 1);

    RefreshUI();
}

    private void RefreshUI()
{
    for (int i = 0;
        i < interactObjects.Count;
        i++)
    {
        interactObjects[i]
            .SetSelected(
                i == currentIndex);
    }

    if (InteractUIV2.Instance == null)
    {
        return;
    }

    InteractUIV2.Instance.Refresh(
        interactObjects,
        currentIndex);
}

    #endregion

    #region Debug

    private void DebugCurrent()
    {
        if (!debugMode)
        {
            return;
        }

        if (CurrentInteract == null)
        {
            Debug.Log(
                "[InteractV2] Empty");

            return;
        }

        Debug.Log(
            "[InteractV2] Current : " +
            CurrentInteract.InteractText);
    }

    #endregion

    #region Public API

    public bool HasInteract()
    {
        return interactObjects.Count > 0;
    }

    public int Count()
    {
        return interactObjects.Count;
    }

    public int CurrentIndex()
    {
        return currentIndex;
    }

    public List<InteractV2> GetObjects()
    {
        return interactObjects;
    }

    #endregion
}