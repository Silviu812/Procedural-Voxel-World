using System;
using UnityEngine;

[Serializable]
public class InventorySlot
{
    public ItemSO item;

    [Min(0)]
    public int amount;

    public bool IsEmpty()
    {
        return item == null || amount <= 0;
    }

    public void Clear()
    {
        item = null;
        amount = 0;
    }
}

public class InventoryData : MonoBehaviour
{
    public const int SlotCount = 8;

    public static InventoryData Instance { get; private set; }

    [Header("Inventory")]
    public InventorySlot[] slots = new InventorySlot[SlotCount];

    [Header("Selected Slot")]
    [Range(0, SlotCount - 1)]
    public int selectedSlotIndex = 0;

    public event Action OnInventoryChanged;
    public event Action<int> OnSelectedSlotChanged;

    public InventorySlot SelectedSlot
    {
        get { return slots[selectedSlotIndex]; }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        InitializeSlots();

        DontDestroyOnLoad(gameObject);
    }

    private void OnValidate()
    {
        InitializeSlots();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void InitializeSlots()
    {
        if (slots == null || slots.Length != SlotCount)
        {
            InventorySlot[] newSlots = new InventorySlot[SlotCount];

            if (slots != null)
            {
                int slotsToCopy = Mathf.Min(slots.Length, SlotCount);

                for (int i = 0; i < slotsToCopy; i++)
                {
                    newSlots[i] = slots[i];
                }
            }

            slots = newSlots;
        }

        for (int i = 0; i < SlotCount; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = new InventorySlot();
            }
        }

        selectedSlotIndex = Mathf.Clamp(selectedSlotIndex, 0, SlotCount - 1);
    }

    public void SelectSlot(int index)
    {
        if (index < 0 || index >= SlotCount)
        {
            return;
        }

        if (selectedSlotIndex == index)
        {
            return;
        }

        selectedSlotIndex = index;
        OnSelectedSlotChanged?.Invoke(selectedSlotIndex);
    }

    public void SelectSlotByNumber(int number)
    {
        SelectSlot(number - 1);
    }

    public bool TryAddOne(ItemSO itemToAdd)
    {
        return AddItem(itemToAdd, 1) == 1;
    }

    public int AddItem(ItemSO itemToAdd, int amountToAdd)
    {
        if (itemToAdd == null || amountToAdd <= 0)
        {
            return 0;
        }

        InitializeSlots();

        int remainingAmount = amountToAdd;
        int firstEmptySlotIndex = -1;
        int maxStackSize = Mathf.Max(1, itemToAdd.maxStackSize);

        for (int i = 0; i < SlotCount; i++)
        {
            InventorySlot slot = slots[i];

            if (slot.IsEmpty())
            {
                if (firstEmptySlotIndex == -1)
                {
                    firstEmptySlotIndex = i;
                }

                continue;
            }

            if (slot.item != itemToAdd)
            {
                continue;
            }

            if (slot.amount >= maxStackSize)
            {
                continue;
            }

            int availableSpace = maxStackSize - slot.amount;
            int amountMoved = Mathf.Min(availableSpace, remainingAmount);

            slot.amount += amountMoved;
            remainingAmount -= amountMoved;

            if (remainingAmount <= 0)
            {
                OnInventoryChanged?.Invoke();
                return amountToAdd;
            }
        }

        if (firstEmptySlotIndex != -1)
        {
            for (int i = firstEmptySlotIndex; i < SlotCount; i++)
            {
                if (!slots[i].IsEmpty())
                {
                    continue;
                }

                int amountMoved = Mathf.Min(maxStackSize, remainingAmount);

                slots[i].item = itemToAdd;
                slots[i].amount = amountMoved;

                remainingAmount -= amountMoved;

                if (remainingAmount <= 0)
                {
                    break;
                }
            }
        }

        int addedAmount = amountToAdd - remainingAmount;

        if (addedAmount > 0)
        {
            OnInventoryChanged?.Invoke();
        }

        return addedAmount;
    }

    public bool TryRemoveOneFromSelectedSlot(out ItemSO removedItem)
    {
        InitializeSlots();

        InventorySlot slot = SelectedSlot;

        if (slot.IsEmpty())
        {
            removedItem = null;
            return false;
        }

        removedItem = slot.item;
        slot.amount--;

        if (slot.amount <= 0)
        {
            slot.Clear();
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    public ItemSO GetSelectedItem()
    {
        if (SelectedSlot.IsEmpty())
        {
            return null;
        }

        return SelectedSlot.item;
    }

    public int GetSelectedItemAmount()
    {
        if (SelectedSlot.IsEmpty())
        {
            return 0;
        }

        return SelectedSlot.amount;
    }
}