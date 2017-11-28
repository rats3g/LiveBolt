using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace LiveBolt.Services
{
    public interface IAPNSService
    {
        void SendPushNotifications(IEnumerable<string> deviceTokens, JObject payload);
    }
}