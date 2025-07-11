using BattleSystem;
using GameFramework;
using UnityEngine;

public class InitSceneButton : MonoBehaviour
{
    public void OnGameStartButton()
    {
        var mainCharacter = CharacterFactory.Instance.Create(0);
        CharacterFactory.Instance.Deactivate(mainCharacter);
        SceneChangeManager.Instance.StartBattleSceneTest(mainCharacter);
    }
}
