using CafeManagent.dto.response;
using CafeManagent.Models;

namespace CafeManagent.Ulties
{
    public class NotifyUlti
    {
        private readonly List<Notify> StaffNotify = new();
        private readonly List<Notify> ManagerNotify = new();
        private readonly List<int> Viewed = new();
        public void AddStaff(Notify notify)
        {
            Viewed.Clear();
            StaffNotify.Add(notify);
        }
        public void AddManager(Notify notify)
        {
            ManagerNotify.Add(notify);
        }
        public void ClearStaff()
        {
            StaffNotify.Clear();
        }
        public void ClearManager()
        {
            ManagerNotify.Clear();
        }
        public List<Notify> AllStaff()
        {
            return StaffNotify;
        }
        public List<Notify> AllManager()
        {
            return ManagerNotify;
        }
        public bool IsViewded(int id)
        {
            return Viewed.Contains(id);
        }
        public void AddToView(int id)
        {
            Viewed.Add(id);
        }
    }
}
