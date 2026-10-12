using UnityEngine;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
    public static InventoryController Instance;

    [Header("Slot UI Ayarları")]
    public Image[] slotBackgrounds;
    public Image[] slotIcons;

    [Header("Seçili Slot Renkleri")]
    public Color normalSlotColor = new Color(0.2f, 0.2f, 0.2f, 0.7f);
    public Color selectedSlotColor = new Color(1f, 0.9f, 0.2f, 0.9f);

    [Header("Eldeki Silah")]
    public GameObject weaponPivot;

    private Sprite[] inventory = new Sprite[4];
    private int selectedSlotIndex = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        for (int i = 0; i < slotIcons.Length; i++)
        {
            if (slotIcons[i] != null)
                slotIcons[i].enabled = false;
        }

        UpdateSlotHighlight();
        UpdateEquippedItem();
    }

    void Update()
    {
        HandleScrollInput();
    }

    void HandleScrollInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0f)
        {
            selectedSlotIndex--;
            if (selectedSlotIndex < 0) selectedSlotIndex = inventory.Length - 1;
            UpdateSlotHighlight();
            UpdateEquippedItem();
        }
        else if (scroll < 0f)
        {
            selectedSlotIndex++;
            if (selectedSlotIndex >= inventory.Length) selectedSlotIndex = 0;
            UpdateSlotHighlight();
            UpdateEquippedItem();
        }

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

    void UpdateEquippedItem()
    {
        if (weaponPivot != null)
        {
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
                UpdateEquippedItem();
                return true;
            }
        }

        Debug.Log("Envanter dolu!");
        return false;
    }
}