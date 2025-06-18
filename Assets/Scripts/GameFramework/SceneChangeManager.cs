using UnityEngine;
using UnityEngine.SceneManagement;
using CharacterSystem;

namespace GameFramework
{
    public class SceneChangeManager : MonoBehaviour
    {
        // ===== [기능 1] 싱글턴 및 데이터 전달 =====
        public static SceneChangeManager Instance { get; private set; }
        public object dataToPass;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        public object GetData()
        {
            return dataToPass;
        }

        // ===== [기능 2] 씬 전환 =====
        public void LoadScene(string sceneName, object data = null)
        {
            Debug.Log($"Loading scene: {sceneName}");
            dataToPass = data;
            SceneManager.LoadScene(sceneName);
        }
    }
} 