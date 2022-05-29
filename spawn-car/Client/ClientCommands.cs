using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace SpawnCar.Client
{
    public class ClientCommands : BaseScript
    {
        public ClientCommands()
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

        [EventHandler("mittons:gather")]
        public static async Task MoveToPosition(Vector3 position)
        {
            var z = GetHeightmapTopZForPosition(position.X, position.Y);

            SetEntityCoords(GetPlayerPed(-1), position.X, position.Y, z, true, false, false, false);

            await BaseScript.Delay(100);

            GetGroundZFor_3dCoord(position.X, position.Y, z + 10, ref z, true);

            SetEntityCoords(GetPlayerPed(-1), position.X, position.Y, z, true, false, false, false);
        }

        [EventHandler("mittons:setwantedlevel")]
        public static void SetWantedLevel(int? level = default)
        {
            SetPlayerWantedLevel(PlayerId(), level.Value, false);
            SetPlayerWantedLevelNow(PlayerId(), false);
        }

        [Command("car")]
        public static async Task SpawnCar(object[] args)
        {
            var model = args.Length > 0 ? args[0].ToString() : "adder";

            var hash = (uint)GetHashKey(model);

            if (!IsModelInCdimage(hash) || !IsModelAVehicle(hash))
            {
                TriggerEvent("chat:addMessage", new
                {
                    color = new[] { 255, 0, 0 },
                    args = new[] { "[CarSpawner]", $"It might have been a good thing that you tried to spawn a {model}. Who even wants their spawning to actually ^*succeed?" }
                });
                return;
            }

            var vehicle = await World.CreateVehicle(model, Game.PlayerPed.Position, Game.PlayerPed.Heading);

            Game.PlayerPed.SetIntoVehicle(vehicle, VehicleSeat.Driver);

            TriggerEvent("chat:addMessage", new
            {
                color = new[] { 255, 0, 0 },
                args = new[] { "[CarSpawner]", $"Woohoo! Enjoy your new ^*{model}!" }
            });
        }
    }
}
