using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace SpawnCar.Client
{
    public class ClientMain : BaseScript
    {
        private Vector2? _spawnPosition;

        private int? _spawnWantedLevel;

        private string _spawnVehicle;

        public ClientMain()
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

        [Command("get_coords")]
        public void GetCoords()
        {
            var finalPosition = GetEntityCoords(GetPlayerPed(-1), true);

            TriggerEvent("chat:addMessage", new
            {
                color = new[] { 255, 0, 0 },
                args = new[] { "[CarSpawner]", $"x[{finalPosition.X}] y[{finalPosition.Y}] z[{finalPosition.Z}]" }
            });
        }

        [Command("set_waypoint")]
        public void SetWaypoint(object[] args)
        {
            float.TryParse(args[0].ToString(), out var x);
            float.TryParse(args[1].ToString(), out var y);

            SetNewWaypoint(x, y);
            SetUseWaypointAsDestination(true);
        }

        [EventHandler("mittons.setwaypoint")]
        public void SetWaypoint(float x, float y)
        {
            SetNewWaypoint(x, y);
            SetUseWaypointAsDestination(true);
        }

        [Command("teleport")]
        public async Task Teleport(object[] args)
        {
            float.TryParse(args[0].ToString(), out var x);
            float.TryParse(args[1].ToString(), out var y);
            var z = GetHeightmapTopZForPosition(x, y);

            SetEntityCoords(GetPlayerPed(-1), x, y, z, true, false, false, false);

            await BaseScript.Delay(100);

            GetGroundZFor_3dCoord(x, y, z + 10, ref z, true);

            SetEntityCoords(GetPlayerPed(-1), x, y, z, true, false, false, false);
        }

        // [Tick]
        // public Task OnTick()
        // {
        //     // DrawRect(0.5f, 0.5f, 0.5f, 0.5f, 255, 255, 255, 150);

        //     return Task.FromResult(0);
        // }
    }
}
