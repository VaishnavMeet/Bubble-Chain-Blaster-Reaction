using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public void LoadRoomScene()
    {
        SceneManager.LoadScene("Room"); 
    }

    public void LoadSampleScene()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void LoadGameScene()
    {
        SceneManager.LoadScene("GameScene");
    }
}
