using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    public Image background;
    public Image icon;
    public TMP_Text amountText;

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color selectedColor = new Color(1f, 0.85f, 0.25f, 1f);

    private void Awake()
    {
        if (background == null)
            background = GetComponent<Image>();
    }

    public void Display(InventorySlot slot, bool isSelected)
    {
        if (background != null)
            background.color = isSelected ? selectedColor : normalColor;

        bool hasItem = slot != null && !slot.IsEmpty();

        if (icon != null)
        {
            icon.enabled = hasItem;
            icon.sprite = hasItem ? slot.item.icon : null;
        }

        if (amountText != null)
        {
            amountText.text = hasItem && slot.amount > 1
                ? slot.amount.ToString()
                : "";
        }
    }
}