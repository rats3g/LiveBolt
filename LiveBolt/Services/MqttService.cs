using System;
using System.Threading.Tasks;
using LiveBolt.Data;
using LiveBolt.Models;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Core;
using MQTTnet.Core.Client;
using MQTTnet.Core.ManagedClient;

namespace LiveBolt.Services
{
    public class MqttService : IMqttService
    {
        private readonly IRepository _repository;
        private readonly ILogger _logger;

        public MqttService(IRepository repository, ILogger<MqttService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async void RegisterDLM(Guid moduleId, string homeId, string homePassword, string nickname)
        {
            var home = _repository.GetHomeByNameAndPassword(homeId, homePassword);
            if (home == null)
            {
                return;
            }

            var dlm = await _repository.GetDLMByGuid(moduleId);
            if (dlm != null)
            {
                return;
            }

            var newDlm = new DLM
            {
                Id = moduleId,
                IsLocked = false,
                AssociatedHomeId = home.Id,
                Nickname = nickname
            };

            _repository.AddDLM(newDlm);

            home.DLMs.Add(newDlm);

            await _repository.Commit();
        }

        public async void UpdateDLMStatus(Guid moduleId, bool isLocked)
        {
            var dlm = await _repository.GetDLMByGuid(moduleId);
            if (dlm == null)
            {
                return;
            }

            dlm.IsLocked = isLocked;

            await _repository.Commit();
        }

        public async void RegisterIDM(Guid moduleId, string homeId, string homePassword, string nickname)
        {
            var home = _repository.GetHomeByNameAndPassword(homeId, homePassword);
            if (home == null)
            {
                return;
            }

            var idm = await _repository.GetIDMByGuid(moduleId);
            if (idm != null)
            {
                return;
            }

            var newIdm = new IDM
            {
                Id = moduleId,
                IsClosed = false,
                AssociatedHomeId = home.Id,
                Nickname = nickname
            };

            _repository.AddIDM(newIdm);

            home.IDMs.Add(newIdm);

            await _repository.Commit();
        }

        public async void UpdateIDMStatus(Guid moduleId, bool isClosed)
        {
            var idm = await _repository.GetIDMByGuid(moduleId);
            if (idm == null)
            {
                return;
            }

            idm.IsClosed = isClosed;

            await _repository.Commit();
        }

        public async Task<bool> PublishLockCommand(Guid moduleId, bool isLocked)
        {
            var dlm = await _repository.GetDLMByGuid(moduleId);
            if (dlm == null)
            {
                return false;
            }

            var mqttOptions = new MqttClientOptionsBuilder()
                .WithClientId("LiveboltServer")
                .WithTcpServer("localhost")
                .WithCredentials("livebolt", "livebolt")
                .Build();

            var mqttClient = new MqttFactory().CreateMqttClient();

            await mqttClient.ConnectAsync(mqttOptions);

            var message = new MqttApplicationMessageBuilder()
                .WithTopic($"dlm/lock/{moduleId}")
                .WithPayload((isLocked ? 1 : 0).ToString())
                .WithExactlyOnceQoS()
                .Build();

            await mqttClient.PublishAsync(message);

            await mqttClient.DisconnectAsync();

            return true;
        }

        public async Task PublishRemoveDLMCommand(Guid moduleId)
        {
            var mqttOptions = new MqttClientOptionsBuilder()
                .WithClientId("LiveboltServer")
                .WithTcpServer("localhost")
                .WithCredentials("livebolt", "livebolt")
                .Build();

            var mqttClient = new MqttFactory().CreateMqttClient();

            await mqttClient.ConnectAsync(mqttOptions);

            var message = new MqttApplicationMessageBuilder()
                .WithTopic($"dlm/remove/{moduleId}")
                .WithPayload(1.ToString())
                .WithExactlyOnceQoS()
                .Build();

            await mqttClient.PublishAsync(message);

            await mqttClient.DisconnectAsync();
        }

        public async Task PublishRemoveIDMCommand(Guid moduleId)
        {
            var mqttOptions = new MqttClientOptionsBuilder()
                .WithClientId("LiveboltServer")
                .WithTcpServer("localhost")
                .WithCredentials("livebolt", "livebolt")
                .Build();

            var mqttClient = new MqttFactory().CreateMqttClient();

            await mqttClient.ConnectAsync(mqttOptions);

            var message = new MqttApplicationMessageBuilder()
                .WithTopic($"idm/remove/{moduleId}")
                .WithPayload(1.ToString())
                .WithExactlyOnceQoS()
                .Build();

            await mqttClient.PublishAsync(message);

            await mqttClient.DisconnectAsync();
        }

        public async void RemoveDLM(Guid moduleId, string homeId)
        {
            var dlm = await _repository.GetDLMByGuid(moduleId);
            if (dlm == null)
            {
                Console.WriteLine($"No dlm by guid: {moduleId}");
                return;
            }

            var home = await _repository.GetHomeByName(homeId);
            if (home == null)
            {
                Console.WriteLine($"No home by name: {homeId}");
                return;
            }

            home.DLMs.Remove(dlm);

            _repository.RemoveDlm(dlm);

            await _repository.Commit();
        }

        public async void RemoveIDM(Guid moduleId, string homeId)
        {
            var idm = await _repository.GetIDMByGuid(moduleId);
            if (idm == null)
            {
                Console.WriteLine($"No idm by guid: {moduleId}");
                return;
            }

            var home = await _repository.GetHomeByName(homeId);
            if (home == null)
            {
                Console.WriteLine($"No home by name: {homeId}");
                return;
            }

            home.IDMs.Remove(idm);

            _repository.RemoveIdm(idm);

            await _repository.Commit();
        }
    }
}
