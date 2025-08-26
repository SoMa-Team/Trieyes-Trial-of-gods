using CharacterSystem;

namespace Stats
{
    public sealed class StatManager
    {
        public readonly StatSheet statSheet;

        public StatManager(StatSheet statSheet)
        {
            this.statSheet = statSheet;
        }

        /// Raw(정수, 버프적용후) 값을 그대로 얻고 싶을 때
        public int GetRaw(StatType type) => statSheet[type].Value;

        /// 최종(정규화 후, 게임 사용) 값을 얻을 때 – 이걸 기본으로 사용
        public float Get(StatType type)
        {
            int raw = statSheet[type].Value;
            var ctx = new StatEvalCtx(raw, t => statSheet[t].Value);
            return StatFormulas.Eval(type, ctx);
        }

        public StatManager DeepCopy()
        {
            var copy = new StatManager(statSheet.DeepCopy());
            return copy;
        }
    }
}