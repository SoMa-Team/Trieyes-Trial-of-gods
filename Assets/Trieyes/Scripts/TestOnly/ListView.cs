using UnityEngine;

namespace OutGame{
    public class ListView : MonoBehaviour
    {
        public virtual void Activate()
        {
            gameObject.SetActive(true);
        }
        public virtual void Deactivate()
        {
            gameObject.SetActive(false);
        }

    }
}