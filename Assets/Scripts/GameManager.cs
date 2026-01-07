using TMPro;
using UnityEngine;

public enum  GameMode 
{
    idle,
    playing,
    levelEnd
}


public class GameManager : MonoBehaviour
{
   private static GameManager Instance;

    [Header("Set in Inspector")]
    public TextMeshPro uitLevel;  // Ссылка на UI элемент для отображения уровня
    public TextMeshPro uitShots;  // Ссылка на UI элемент для отображения количества выстрелов
    public TextMeshPro uitButton; // Ссылка на UI элемент для кнопки
    public Vector3 castlePos;     // Позиция замка
    public GameObject[] castles;  // Массив префабов замков


    [Header("Set Dynamically")]
    public int level;                         // Текущий уровень игры
    public int levelMax;                      // Количество уровней в игре
    public int shotsTaken;                    // Количество выстрелов, сделанных игроком
    public GameObject castle;                 // Текущий замок
    public GameObject viewBoth;               // Ссылка на объект для обзора обоих - рогатки и замка 
    public Goal goal;                         // Ссылка на цель
    public GameMode mode = GameMode.idle;     // Текущий режим игры
    public string showing = "Show Slingshot"; // Режим FollowCam


    private void Start()
    {
        Instance = this;
        level = 0;
        levelMax = castles.Length;
        StartLevel();
    }


    private void Update()
    {
        UpdateGUI();

        // Проверить завершение уровня
        if ((mode == GameMode.playing) && goal.GoalMet)
        {
            mode = GameMode.levelEnd; // Уровень завершен

            // Уменьшить масштаб
            SwitchView("Show Both");

            // Запустить следующий уровень через 2 секунды
            Invoke("NextLevel", 2f);
        }
    }


    void StartLevel()
    {
        // Уничтожаем предыдущий замок, если он существует
        if (castle != null)
        {
            Destroy(castle);
        }

        // Уничтожаем прежние снаряды, если они существуют
        GameObject[] gos = GameObject.FindGameObjectsWithTag("Projectile");
        foreach (GameObject pTemp in gos)
        {
            Destroy(pTemp);
        }

        // Создаем новый замок
        castle = Instantiate<GameObject>(castles[level]);
        castle.transform.position = castlePos;
        shotsTaken = 0;

        // Переустановить  камеру в начальную позицию
        SwitchView("Show Both");
        ProjectileLine.Instance.Clear();

        // Сбросить цель
        //Goal.goalMet = false;

        UpdateGUI();

        mode = GameMode.playing;
    }


    public void SwitchView(string eView = "")
    {
        if (eView == "")
        {
            eView = uitButton.text;
        }

        showing = eView;

        switch (showing)
        {
            case "Show Slingshot":
                FollowCam.Instance.ClearPointOfInterest();
                uitButton.text = "Show Castle";
                break;

            case "Show Castle":
                FollowCam.Instance.SetPointOfInterest(castle);
                uitButton.text = "Show Both";
                break;

            case "Show Both":
                FollowCam.Instance.SetPointOfInterest(viewBoth);
                uitButton.text = "Show Slingshot";
                break;
        }
    }


    private void UpdateGUI()
    {
       uitLevel.text = "Level: " + (level + 1) + " of " + levelMax;
       uitShots.text = "Shots Taken: " + shotsTaken;
    }


    private void NextLevel()
    {
        level++;
        
        if (level == levelMax)
        {
            level = 0;
        }

        StartLevel();
    }

    public static void ShotFired()
    {
        Instance.shotsTaken++;
    }
}
