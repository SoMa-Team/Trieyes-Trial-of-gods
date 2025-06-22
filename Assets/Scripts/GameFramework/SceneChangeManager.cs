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
            Activate();
        }

        private void OnDestroy()
        {
            Deactivate();
        }

        /// <summary>
        /// 오브젝트 풀링을 위한 활성화 함수
        /// </summary>
        public virtual void Activate()
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

        /// <summary>
        /// 오브젝트 풀링을 위한 비활성화 함수
        /// </summary>
        public virtual void Deactivate()
        {
            // 데이터 정리
            dataToPass = null;
            
            // 싱글톤 참조 정리
            if (Instance == this)
            {
                Instance = null;
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