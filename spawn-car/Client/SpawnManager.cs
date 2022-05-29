using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace SpawnCar.Client
{
    public class SpawnManager : BaseScript
    {
        private Vector3? _spawnPosition;

        private TimeSpan? _spawnMaximumSetback;

        private DateTime _nextPositionRecordingTime = DateTime.Now.AddSeconds(30);

        private int? _spawnWantedLevel;

        private string _spawnVehicle;

        public SpawnManager()
        {
            var methods = this.GetType().GetMethods().Where(x => x.GetCustomAttributes<EventHandlerAttribute>(false).Any());

            foreach (var method in methods)
            {
                var parameters = method.GetParameters().Select(p => p.ParameterType).ToArray();
                var returnType = method.ReturnType;
                var actionType = Expression.GetDelegateType(parameters.Concat(new[] { returnType }).ToArray());
                var attributes = method.GetCustomAttributes<EventHandlerAttribute>(false);

                foreach (var attribute in attributes)
                {
                    if (method.IsStatic)
                    {
                        EventHandlers[attribute.Name] += Delegate.CreateDelegate(actionType, method);
                    }
                    else
                    {
                        EventHandlers[attribute.Name] += Delegate.CreateDelegate(actionType, this, method.Name);
                    }
                }
            }
        }

        [EventHandler("mittons:setspawncheckpoint")]
        public void SetSpawnCheckpoint(TimeSpan maximumSetback)
        {
            _spawnMaximumSetback = maximumSetback;
        }

        [EventHandler("mittons:setspawnpoint")]
        [EventHandler("mittons:resetspawnpoint")]
        public void SetSpawnPoint(Vector2? position = default)
        {
            _spawnPosition = position.HasValue ? new Vector3(position.Value.X, position.Value.Y, 0) : default;
        }

        [EventHandler("mittons:setspawnwantedlevel")]
        [EventHandler("mittons:resetspawnwantedlevel")]
        public void SetSpawnWantedLevel(int? level = default)
        {
            _spawnWantedLevel = level;
        }

        [EventHandler("mittons:setspawnvehicle")]
        [EventHandler("mittons:resetspawnvehicle")]
        public void SetSpawnVehicle(string vehicle = default)
        {
            _spawnVehicle = vehicle;
        }

        [EventHandler("playerSpawned")]
        public async Task GameEventTriggered(object temp)
        {
            if (_spawnPosition.HasValue)
            {
                await ClientCommands.MoveToPosition(_spawnPosition.Value);
            }

            if (_spawnWantedLevel.HasValue)
            {
                ClientCommands.SetWantedLevel(_spawnWantedLevel);
            }

            if (!string.IsNullOrWhiteSpace(_spawnVehicle))
            {
                await ClientCommands.SpawnCar(new[] { _spawnVehicle });
            }
        }

        [Tick]
        public Task OnTick()
        {
            if (_spawnMaximumSetback.HasValue && DateTime.Now > _nextPositionRecordingTime)
            {
                _nextPositionRecordingTime = DateTime.Now.Add(_spawnMaximumSetback.Value);
                _spawnPosition = GetEntityCoords(GetPlayerPed(-1), true);
            }

            return Task.FromResult(0);
        }
    }
}
