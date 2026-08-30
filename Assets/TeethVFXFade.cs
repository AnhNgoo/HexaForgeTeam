using UnityEngine;

public class TeethVFXController : MonoBehaviour, IPoolable
{
    [SerializeField] private PoolType poolType;
    [SerializeField] private Animator animator;

    public PoolType PoolType => poolType;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void OnSpawnFromPool()
    {
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
            animator.Play(0, 0, 0f);
        }
    }

    public void OnReturnToPool()
    {
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }
    }
}