using System;
using System.Threading.Tasks;

namespace LiveBolt.Services
{
    public interface IMqttService
    {
        void RegisterDLM(Guid moduleId, string homeId, string homePassword, string nickname);
        void UpdateDLMStatus(Guid moduleId, bool isLocked);
        void RegisterIDM(Guid moduleId, string homeId, string homePassword, string nickname);
        void UpdateIDMStatus(Guid moduleId, bool isClosed);
        Task<bool> PublishLockCommand(Guid moduleId, bool isLocked);
    }
}