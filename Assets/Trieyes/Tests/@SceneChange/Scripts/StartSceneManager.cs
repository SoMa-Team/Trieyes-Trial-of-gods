using UnityEngine;
using UnityEngine.UI;
using CharacterSystem;
using GameFramework;
using System.Threading.Tasks;
using RelicSystem;
using AttackSystem;

public class StartSceneManager : MonoBehaviour
{
    public Button startButton;
    public AttackData attackData;

    void Awake()
    {
        startButton.onClick.AddListener(GameStart);
    }

    void Start()
    {
        _Start();
    }

    async Task _Start()
    {
        await RelicDataBase.InitializeAsync();
    }

    void GameStart()
    {
        SceneChangeManager.Instance.StartBattleSceneTest();
    }
}
