using UnityEngine;

namespace OutGame{
    public class Popup : MonoBehaviour
    {
        public ListView ListView;

        public virtual void Activate()
        {
            if (ListView != null)
            {
                ListView.Activate();
            }
        }
        public virtual void Deactivate()
        {
            if (ListView != null)
            {
                ListView.Deactivate();
            }
        }
    }
}