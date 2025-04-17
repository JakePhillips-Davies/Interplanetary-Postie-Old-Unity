using UnityEngine;
using UnityEngine.Events;

public class Activator : MonoBehaviour
{
    public UnityEvent onActivate;

    public void Activate()
    {
        onActivate?.Invoke();
    }
}
