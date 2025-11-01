using CafeManagent.dto.response;

namespace CafeManagent.Ulties
{
    public class NotifyUlti
    {
        private readonly List<Notify> notifies = new();
        public void Add(Notify notify)
        {
            notifies.Add(notify);
        }
        public void Remove(Notify notify)
        {
            notifies.Remove(notify);
        }
        public void Clear()
        {
            notifies.Clear();
        }
        public List<Notify> All()
        {
            return notifies;
        }

    }
}
