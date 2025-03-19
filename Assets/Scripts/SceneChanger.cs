using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    private void Start()
    {
        // Ensure this object is not destroyed when loading a new scene
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        // Check if the Enter key is pressed
        if (Input.GetKeyDown(KeyCode.Return))
        {
            // Get the current active scene
            int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
            // Calculate the next scene index
            int nextSceneIndex = (currentSceneIndex + 1) % SceneManager.sceneCountInBuildSettings;
            // Load the next scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneIndex);
        }
    }
}
