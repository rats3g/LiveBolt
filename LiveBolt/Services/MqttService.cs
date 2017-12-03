using System;
using System.Threading.Tasks;
using LiveBolt.Data;
using LiveBolt.Models;
using MQTTnet;
using MQTTnet.Core;
using MQTTnet.Core.Client;
using MQTTnet.Core.ManagedClient;

namespace LiveBolt.Services
{
    public class MqttService : IMqttService
    {
        private readonly IRepository _repository;

        public MqttService(IRepository repository)
        {
            _repository = repository;
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
    }
}
