using System.Collections.Generic;
using Character;

namespace Attack
{
    public class Attack
    {
        public AttackData attackData;
        public Pawn attacker;
        public List<AttackComponent> components = new List<AttackComponent>();

        public void Execute()
        {
            foreach (var comp in components)
                comp.Execute(this);
        }
    }
} 