using UnityEngine;

public sealed class DelayedDestroy : MonoBehaviour
{
    [SerializeField] private float delay;

    public async void Start()
    {
        await Awaitable.WaitForSecondsAsync(delay);
        Destroy(gameObject);
    }
}
