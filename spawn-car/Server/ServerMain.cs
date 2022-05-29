using System;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace SpawnCar.Server
{
    public class ServerMain : BaseScript
    {
        public ServerMain()
        {
            Debug.WriteLine("Hi from SpawnCar.Server!");
        }

        [Command("start_scenario")]
        public async void StartScenario(object[] args)
        {
            var scenario = args.FirstOrDefault().ToString();

            switch (scenario)
            {
                case "policechase":
                    TriggerClientEvent("mittons:setspawnpoint", new Vector2(12, 12));
                    TriggerClientEvent("mittons:setspawnwantedlevel", 5);
                    TriggerClientEvent("mittons:setspawnvehicle", "adder");
                    TriggerClientEvent("playerSpawned");
                    break;
                case "policerace":
                    TriggerClientEvent("mittons:gather", new Vector2(12, 12));
                    TriggerClientEvent("mittons:setspawnpoint", new Vector2(12, 12));
                    TriggerClientEvent("mittons:setspawnwantedlevel", 5);
                    TriggerClientEvent("mittons:setspawnvehicle", "adder");
                    await Task.Delay(TimeSpan.FromSeconds(30));
                    TriggerClientEvent("mittons:setwantedlevel", 5);
                    break;
                default:
                    Debug.WriteLine($"Unknown scenario [{scenario}] requested");
                    break;
            }
        }

        [Command("end_scenario")]
        public void EndScenario()
        {
            TriggerClientEvent("mittons:resetspawnpoint");
            TriggerClientEvent("mittons:resetspawnwantedlevel");
            TriggerClientEvent("mittons:resetspawnvehicle");
        }

        [Command("hello_server")]
        public void HelloServer()
        {
            Debug.WriteLine("Sure, hello.");
        }

        [Command("gather")]
        public void Gather(object[] args)
        {
            Debug.WriteLine("Sure, hello.");
            TriggerClientEvent("mittons:beginscenario", "test", 12, 12);
            TriggerClientEvent("mittons.gather", 12, 12);
            // float.TryParse(args[0].ToString(), out var x);
            // float.TryParse(args[1].ToString(), out var y);
            // var z = GetHeightmapTopZForPosition(x, y);

            // SetEntityCoords(GetPlayerPed(-1), x, y, z, true, false, false, false);

            // Get

            // await BaseScript.Delay(100);

            // GetGroundZFor_3dCoord(x, y, z + 10, ref z, true);

            // SetEntityCoords(GetPlayerPed(-1), x, y, z, true, false, false, false);
        }

        [Command("wanted")]
        public void SetWanted()
        {
            foreach (var player in Players)
            {
                Debug.WriteLine($"{player.Name}...WATCH OUT FOR THE COPS!");
                SetPlayerWantedLevel(player.Handle, 5, false);
            }
        }
    }
}
