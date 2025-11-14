using UnityEngine;

public class SonidoChisagua : MonoBehaviour
{
    public AudioSource audioSource;
    public Animator animator;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (audioSource != null && !audioSource.isPlaying)
            {
                audioSource.Play();
                if (animator != null)
                {
                    animator.SetBool("anim1", true);
                    Debug.Log("Animación activada: anim1 = true");
                    Invoke("ResetAnimation", audioSource.clip.length); // Vuelve a quieto después de que termine el audio
                }
            }
        }
    }

    void ResetAnimation()
    {
        animator.SetBool("anim1", false);
        Debug.Log("Animación finalizada: anim1 = false");
    }
}
