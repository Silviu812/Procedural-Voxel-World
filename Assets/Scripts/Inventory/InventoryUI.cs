using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public InventoryData inventory;
    public InventorySlotUI[] slotViews;

    private void Start()
    {
        if (inventory == null)
            inventory = InventoryData.Instance;

        if (inventory == null)
            return;

        inventory.OnInventoryChanged += Refresh;
        inventory.OnSelectedSlotChanged += RefreshSelectedSlot;

        Refresh();
    }

    private void OnDestroy()
    {
        if (inventory == null)
            return;

        inventory.OnInventoryChanged -= Refresh;
        inventory.OnSelectedSlotChanged -= RefreshSelectedSlot;
    }

    public void Refresh()
    {
        if (inventory == null || slotViews == null)
            return;

        int count = Mathf.Min(slotViews.Length, InventoryData.SlotCount);

        for (int i = 0; i < count; i++)
        {
            if (slotViews[i] != null)
            {
                slotViews[i].Display(
                    inventory.slots[i],
                    i == inventory.selectedSlotIndex
                );
            }
        }
    }

    private void RefreshSelectedSlot(int selectedIndex)
    {
        Refresh();
    }
}