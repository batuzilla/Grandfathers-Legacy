using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Sprite itemSprite; // Bu eşyanın resmi (Yay veya Ok sprite'ı)
    private bool playerInRange = false;

    void Update()
    {
        // Karakter menzildeyken E tuşuna basarsa al
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            bool added = InventoryController.Instance.AddItem(itemSprite);
            if (added)
            {
                Destroy(gameObject); // Yerden sil
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}