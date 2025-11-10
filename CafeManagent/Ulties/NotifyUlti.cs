using CafeManagent.dto.response;

namespace CafeManagent.Ulties
{
    public class NotifyUlti
    {
        private readonly List<Notify> StaffNotify = new();
        private readonly List<Notify> ManagerNotify = new();
        public void AddStaff(Notify notify)
        {
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


    }
}
