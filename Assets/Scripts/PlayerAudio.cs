using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip footstepClip;
    [SerializeField] private AudioClip hurtClip;

    private void OnEnable()
    {
        playerController.OnFootstep += PlayFootstepSound;
        playerHealth.OnPlayerHurt += PlayHurtSound;
    }

    private void OnDisable()
    {
        playerController.OnFootstep -= PlayFootstepSound;
        playerHealth.OnPlayerHurt -= PlayHurtSound;
    }

    private void PlayFootstepSound()
    {
        audioSource.PlayOneShot(footstepClip, 0.1f);
    }

    private void PlayHurtSound()
    {
        audioSource.PlayOneShot(hurtClip);
    }
}
