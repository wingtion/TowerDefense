// On your FreezeArea prefab
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class FreezeAreaAnimator : MonoBehaviour
{
    private Animator _animator;
    private int _freezeHash;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _freezeHash = Animator.StringToHash("Freeze");
    }

    public void PlayFreezeAnimation()
    {
        _animator.SetTrigger(_freezeHash);
    }

    // Animation Event - called at the end of freeze animation
    public void OnFreezeAnimationComplete()
    {
        // Optional: disable or hide the area after animation
        gameObject.SetActive(false);
    }
}