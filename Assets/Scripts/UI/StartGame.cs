using UnityEngine;
using UnityEngine.UI;
public class StartGame : MonoBehaviour
{
    public World world;
    public PlayerCharacter playerController;
    public GameObject mainMenu;
    public GameObject inventory;
    public BaseAlgo baseAlgo;
    public InputField seedInput;

    private void Awake()
    {
        ShowMenuCursor();
    }

    private void Start()
    {
        if (playerController != null)
            playerController.DisableControl();

        if (mainMenu != null)
            mainMenu.SetActive(true);

        if (inventory != null)
            inventory.SetActive(false);

        ShowMenuCursor();
    }

    public void PlayGame()
    {
        playerController.SpawnOnTerrain();
        world.GenerateWorld();
        
        
       
        playerController.EnableControl();

        if (mainMenu != null)
            mainMenu.SetActive(false);

        if (inventory != null)
            inventory.SetActive(true);
    }

    private void ShowMenuCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void PlayGameWithSeed()
    {
        ApplyCustomSeed();
        PlayGame();
    }

    public void ApplyCustomSeed()
    {
        if (baseAlgo == null || seedInput == null)
            return;

        string seedText = seedInput.text;

        if (string.IsNullOrWhiteSpace(seedText))
            return;

        int parsedSeed;

        if (!int.TryParse(seedText, out parsedSeed))
        {
            seedInput.text = "";
            return;
        }

        if (parsedSeed < 0)
        {
            seedInput.text = "";
            return;
        }

        baseAlgo.seed = parsedSeed;
    }
}