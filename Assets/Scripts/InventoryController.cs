using UnityEngine;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
    public static InventoryController Instance;

    [Header("Slot UI Ayarları")]
    public Image[] slotBackgrounds; // Slotların dış kutuları (Slot_0, Slot_1...)
    public Image[] slotIcons;       // İçteki ikonlar (ItemIcon'lar)

    [Header("Seçili Slot Renkleri")]
    public Color normalSlotColor = new Color(0.2f, 0.2f, 0.2f, 0.7f); // Normal koyu renk
    public Color selectedSlotColor = new Color(1f, 0.9f, 0.2f, 0.9f);  // Seçili parlak/sarımtırak renk

    [Header("Eldeki Silah")]
    public GameObject weaponPivot;

    private Sprite[] inventory = new Sprite[4];
    private int selectedSlotIndex = 0; // Şu an seçili olan slot (0-3)

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Başlangıçta ikonları gizle
        for (int i = 0; i < slotIcons.Length; i++)
        {
            if (slotIcons[i] != null)
                slotIcons[i].enabled = false;
        }

        // İlk slotu seçili hale getir
        UpdateSlotHighlight();
        UpdateEquippedItem();
    }

    void Update()
    {
        HandleScrollInput();
    }

    // Farenin tekerleğiyle slot değiştirme
    void HandleScrollInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0f) // Yukarı kaydırma -> Bir önceki slot
        {
            selectedSlotIndex--;
            if (selectedSlotIndex < 0) selectedSlotIndex = inventory.Length - 1;
            UpdateSlotHighlight();
            UpdateEquippedItem();
        }
        else if (scroll < 0f) // Aşağı kaydırma -> Bir sonraki slot
        {
            selectedSlotIndex++;
            if (selectedSlotIndex >= inventory.Length) selectedSlotIndex = 0;
            UpdateSlotHighlight();
            UpdateEquippedItem();
        }

        // İsteğe bağlı: 1, 2, 3, 4 tuşlarıyla da seçebilmek için
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectSlot(3);
    }

    void SelectSlot(int index)
    {
        selectedSlotIndex = index;
        UpdateSlotHighlight();
        UpdateEquippedItem();
    }

    // Seçili slotun çerçeve rengini parlat, diğerlerini normale çek
    void UpdateSlotHighlight()
    {
        for (int i = 0; i < slotBackgrounds.Length; i++)
        {
            if (slotBackgrounds[i] != null)
            {
                slotBackgrounds[i].color = (i == selectedSlotIndex) ? selectedSlotColor : normalSlotColor;
            }
        }
    }

    // Seçili kutuda yay varsa eli aç, boşsa eli kapat
    void UpdateEquippedItem()
    {
        if (weaponPivot != null)
        {
            // Seçili slotta bir eşya varsa eli aktif yap, boşsa kapat
            bool hasItemInHand = (inventory[selectedSlotIndex] != null);
            weaponPivot.SetActive(hasItemInHand);
        }
    }

    public bool AddItem(Sprite itemSprite)
    {
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i] == null)
            {
                inventory[i] = itemSprite;
                slotIcons[i].sprite = itemSprite;
                slotIcons[i].enabled = true;

                // Eğer eşya şu an aktif olan slota geldiyse direkt ele ver
                UpdateEquippedItem();

                return true;
            }
        }

        Debug.Log("Envanter dolu!");
        return false;
    }
}