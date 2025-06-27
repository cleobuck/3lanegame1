using UnityEngine;
using UnityEngine.SceneManagement;

public class ObstacleColliderTrigger : MonoBehaviour
{
      public void OnTriggerEnter(Collider other)
   {
        SceneManager.LoadScene("GameOverUi");  
   }
   
}