using UnityEngine;
using UnityEngine.SceneManagement;
using CharacterSystem;

namespace GameFramework
{
    public class SceneChangeManager : MonoBehaviour
    {
        public static SceneChangeManager Instance { get; private set; }

        // 씬 전환 시 전달할 데이터 등을 위한 변수 (예시)
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

        // 씬을 로드하는 메서드
        public void LoadScene(string sceneName, object data = null)
        {
            Debug.Log($"Loading scene: {sceneName}");
            dataToPass = data;
            SceneManager.LoadScene(sceneName);
        }

        // 로드된 씬에서 데이터를 가져오는 메서드 (필요시 구현)
        public object GetData()
        {
            return dataToPass;
        }
    }
} 