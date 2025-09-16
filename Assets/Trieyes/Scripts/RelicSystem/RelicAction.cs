using Utils;

namespace RelicSystem
{
    public class RelicAction: IEventHandler
    {
        protected Relic _relic;

        public void AttachTo(Relic relic)
        {
            _relic = relic;
        }
        
        public virtual bool OnEvent(EventType eventType, object param)
        {
            return false;
        }
    }
}