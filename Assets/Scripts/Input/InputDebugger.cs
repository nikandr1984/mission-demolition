using UnityEngine;

public class InputDebugger : MonoBehaviour
{
    [SerializeField] private GameInput _gameInput;

    private void OnEnable()
    {
        _gameInput.OnFireStarted += () => Debug.Log("Огонь! (Нажатие)");
        _gameInput.OnFireCanceled += () => Debug.Log("Отбой! (Отпускание)");
        _gameInput.OnAimPositionChanged += (pos) => Debug.Log($"Прицел: {pos}");
    }

    private void OnDisable()
    {
        _gameInput.OnFireStarted -= () => Debug.Log("Огонь!");
        _gameInput.OnFireCanceled -= () => Debug.Log("Отбой!");
        _gameInput.OnAimPositionChanged -= (pos) => Debug.Log($"Прицел: {pos}");
    }
}
