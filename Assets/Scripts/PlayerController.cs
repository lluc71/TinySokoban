using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject firstSelectedButton;

    private PlayerInput playerInput;
    private bool isPaused;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    public void OnMove(InputValue value)
    {
        if (isPaused) return;
        if (levelManager == null) return; //TODO: Añadir Log.Error()
        if (levelManager.IsLevelCompleted()) return;

        Vector2 input = value.Get<Vector2>();
        Vector2Int dir = Vector2Int.zero;

        if (input.y > 0.5f)
            dir = Vector2Int.up;
        else if (input.y < -0.5f)
            dir = Vector2Int.down;
        else if (input.x < -0.5f)
            dir = Vector2Int.left;
        else if (input.x > 0.5f)
            dir = Vector2Int.right;

        if (dir != Vector2Int.zero)
        {
            levelManager.TryMovePlayer(dir);
        }
    }

    public void OnPause()
    {
        isPaused = !isPaused;

        if (pausePanel != null)
            pausePanel.SetActive(isPaused);

        if (isPaused)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
    }
}
