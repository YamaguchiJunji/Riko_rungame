using UnityEngine;

public class strawberry : MonoBehaviour
{
    private AudioSource audioSource_coin;
    private SpriteRenderer spriteRenderer;
    private Collider2D col;

    void Start()
    {
        audioSource_coin = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("player"))
        {
            spriteRenderer.enabled = false;
            col.enabled = false;

            audioSource_coin.Play();

            Destroy(gameObject, audioSource_coin.clip.length);
        }
    }
}