using UnityEngine;

public class SocialLinks : MonoBehaviour
{
    public void OpenVK()
    {
        Application.OpenURL("https://vk.com/gamedev_after_30");
    }

    public void OpenTG()
    {
        Application.OpenURL("https://t.me/gamedev_after_30");
    }

    public void OpenSite()
    {
        Application.OpenURL("https://gamedevafter30.ru/blog");
    }
    
    public void OpenMyGames()
    {
        Application.OpenURL("https://gamedevafter30.ru/projects");
    }
}

