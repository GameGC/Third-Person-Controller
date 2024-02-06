using Cinemachine;
using UnityEngine;

public class ImpulseOnStart : MonoBehaviour
{
    private CinemachineImpulseSource _impulseSource;
    private void Awake() => _impulseSource = GetComponent<CinemachineImpulseSource>();

    // Start is called before the first frame update
    private void Start()
    {
        var dir = (transform.position -Camera.main.transform.position).normalized;
        _impulseSource.GenerateImpulseWithVelocity(dir);
    }
}
