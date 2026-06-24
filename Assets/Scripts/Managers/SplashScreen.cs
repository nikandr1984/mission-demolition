using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SplashScreen : MonoBehaviour
{
    [SerializeField] private float _fadeInDuration = 1f;            // Продолжительность появления заставки
    [SerializeField] private float _splashDuration = 2f;            // Продолжительность показа заставки
    [SerializeField] private float _fadeOutDuration = 1f;           // Продолжительность исчезновения заставки
    [SerializeField] private string _nextSceneName = "MainMenu";    // Имя следующей сцены
    [SerializeField] private Image _logoImage;                      // Ссылка на изображение логотипа

    private CanvasGroup _canvasGroup;                               // Компонент CanvasGroup для управления прозрачностью


    private void Start()
    {
        // 1. Проверка на null
        if (_logoImage == null) return;
        
        // 2. Получаем существующий CanvasGroup
        _canvasGroup = _logoImage.GetComponent<CanvasGroup>();

        // 3. Если его нет, то добавляем
        if (_canvasGroup == null)
        {
            _canvasGroup = _logoImage.gameObject.AddComponent<CanvasGroup>();
        }

        // 4. Запускаем корутину появления/исчезания логотипа
        StartCoroutine(ShowLogoWithFade());

    }


    private IEnumerator ShowLogoWithFade()
    {
        // 1. Плавное появление логотипа        
        float elapsed = 0f;
        while (elapsed < _fadeInDuration)
        {
            _canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed /  _fadeOutDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        _canvasGroup.alpha = 1f;

        // 2. Пауза - держим логотип
        yield return new WaitForSeconds(_splashDuration);

        // 3. Плавное исчезновение логотипа
        elapsed = 0f;

        while (elapsed < _fadeOutDuration)
        {
            _canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / _fadeOutDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        _canvasGroup.alpha = 0f;    

        // 4. Загружаем следующую сцену
        SceneManager.LoadScene(_nextSceneName);
    }
}
