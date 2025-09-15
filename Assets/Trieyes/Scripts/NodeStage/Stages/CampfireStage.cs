using UnityEngine;
using UnityEngine.UI;
using CardViews;
using Utils;
using PrimeTween;

namespace NodeStage
{
    public class CampfireStage : EventStage<CampfireStage>
    {
        [Header("UI")]
        [SerializeField] private GameObject Background;
        [SerializeField] private Button btnOption1;
        [SerializeField] private Button btnOption2;
        [SerializeField] private Button btnOption3;

        [Header("Popup")]
        [SerializeField] private DeckView deckViewLocal; // 부모의 deckView와 같다면 생략 가능

        private enum Mode { None, HpOption1, HpOption2, HpOption3 }
        private AllIn1SpriteShaderHandler shaderHandler;

        protected override void OnActivated()
        {
            btnOption1?.onClick.RemoveAllListeners();
            btnOption2?.onClick.RemoveAllListeners();
            btnOption3?.onClick.RemoveAllListeners();

            btnOption1?.onClick.AddListener(() => Option1Function(Mode.HpOption1));
            btnOption2?.onClick.AddListener(() => Option2Function(Mode.HpOption2));
            btnOption3?.onClick.AddListener(() => Option3Function(Mode.HpOption3));

            // AllIn1SpriteShaderHandler 초기화
            shaderHandler = new AllIn1SpriteShaderHandler();
            if (Background != null)
            {
                Material mat = Background.GetComponent<Image>().material;
                shaderHandler.SetObject(mat);
                AnimationBackgroundShow();
            }
        }

        private void AnimationBackgroundShow()
        {
            if (Background != null)
            {
                Background.SetActive(true);
            }
        }
        
        private void AnimationBackgroundHide(System.Action onComplete = null)
        {
            if (shaderHandler?.mat == null)
            {
                onComplete?.Invoke();
                return;
            }

            shaderHandler.mat.EnableKeyword("FADE_ON");
            shaderHandler.mat.SetFloat("_FadeAmount", -0.1f);

            Tween.Custom(-0.1f, 1f, 1f, (value) =>
                {
                    shaderHandler.mat.SetFloat("_FadeAmount", value);
                })
                .OnComplete(() =>
                {
                    shaderHandler.mat.DisableKeyword("FADE_ON");
                    shaderHandler.mat.SetFloat("_FadeAmount", -0.1f);
                    onComplete?.Invoke(); // 애니메이션 끝난 후 콜백 실행
                });
        }

        private void Option1Function(Mode mode)
        {
            // 30% 회복
            int hp = Mathf.RoundToInt(mainCharacter.maxHp * 0.3f);
            mainCharacter.ChangeHP(hp);
            if (Background != null)
            {
                AnimationBackgroundHide(() => base.NextStage());
            }
        }

        private void Option2Function(Mode mode)
        {
            // 50% 회복
            int hp = Mathf.RoundToInt(mainCharacter.maxHp * 0.5f);
            mainCharacter.ChangeHP(hp);
            if (Background != null)
            {
                AnimationBackgroundHide(() => base.NextStage());
            }
        }

        private void Option3Function(Mode mode)
        {
            // 70% 회복
            int hp = Mathf.RoundToInt(mainCharacter.maxHp * 0.7f);
            mainCharacter.ChangeHP(hp);
            if (Background != null)
            {
                AnimationBackgroundHide(() => base.NextStage());
            }
        }
    }
}
