using UnityEngine;
using UnityEngine.SceneManagement;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class menu_manager : MonoBehaviour
{
    [Header("Scene Configuration")]
    [Tooltip("Name of the game scene to load when starting (default: game_scene)")]
    public string gameSceneName = "game_scene";

    void Update()
    {
        bool startPressed = false;

#if ENABLE_INPUT_SYSTEM
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            startPressed = keyboard.anyKey.wasPressedThisFrame;
        }
#else
        startPressed = Input.anyKeyDown;
#endif

        if (startPressed)
        {
            StartGame();
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }
}
