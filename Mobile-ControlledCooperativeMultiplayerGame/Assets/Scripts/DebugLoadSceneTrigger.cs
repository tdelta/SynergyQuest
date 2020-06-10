using UnityEngine;

public class DebugLoadSceneTrigger : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            SceneController.Instance.LoadSceneByName(sceneToLoad);
        }
    }
}
