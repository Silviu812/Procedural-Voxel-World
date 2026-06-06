using UnityEngine;

public class InventoryHotbarInput : MonoBehaviour
{
    private readonly KeyCode[] hotbarKeys =
    {
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4,
        KeyCode.Alpha5,
        KeyCode.Alpha6,
        KeyCode.Alpha7,
        KeyCode.Alpha8
    };

    private void Update()
    {
        if (InventoryData.Instance == null)
        {
            return;
        }

        for (int i = 0; i < hotbarKeys.Length; i++)
        {
            if (Input.GetKeyDown(hotbarKeys[i]))
            {
                InventoryData.Instance.SelectSlot(i);
                return;
            }
        }
    }
}