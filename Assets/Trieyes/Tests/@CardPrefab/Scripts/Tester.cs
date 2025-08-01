using UnityEngine;
using TMPro;
using CardSystem;
using Utils;

namespace Trieyes.Tests.CardPrefab.Scripts
{
    public class Tester : MonoBehaviour
    {
        public TMP_InputField inputField;
        public TMP_Text logText;
        
        public CardViewTest cardView;

        private void Start()
        {
            inputField.onSubmit.AddListener(OnSubmitCommand);
        }

        private void OnSubmitCommand(string command)
        {
            if (!string.IsNullOrWhiteSpace(command))
            {
                int num = Parser.ParseStrToInt(command);
                cardView.SetCard(CardFactory.Instance.Create(UnityEngine.Random.Range(1, 4), num));

                inputField.text = "";                    // 입력창 초기화
                inputField.ActivateInputField();         // 포커스 재설정
            }
        }

        public void Log(string msg)
        {
            logText.text += msg + "\n";
        }
    }
}