using CharacterSystem;

namespace Stats
{
    public sealed class StatManager
    {
        private readonly StatSheet _sheet;
        private readonly Pawn _owner;

        public StatManager(Pawn owner, StatSheet sheet)
        {
            _owner = owner;
            _sheet = sheet;
            _sheet.setOwner(owner);
        }

        /// Raw(정수, 버프적용후) 값을 그대로 얻고 싶을 때
        public int GetRaw(StatType type) => _sheet[type].Value;

        /// 최종(정규화 후, 게임 사용) 값을 얻을 때 – 이걸 기본으로 사용
        public float Get(StatType type)
        {
            int raw = _sheet[type].Value;
            var ctx = new StatEvalCtx(_owner, raw, t => _sheet[t].Value);
            return StatFormulas.Eval(type, ctx);
        }
    }
}