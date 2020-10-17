using GTANetworkAPI;

namespace NeptuneEvo.Core
{
    public class SmoothThrottleAntiReverse : Script
    {
        [ServerEvent(Event.PlayerExitVehicle)]
        public void SmoothThrottleExitEvent(Player player, Vehicle veh)
        {
            NAPI.ClientEvent.TriggerClientEvent(player, "SmoothThrottle_PlayerExitVehicle", veh);
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void SmoothThrottleEnterEvent(Player player, Vehicle veh, sbyte seat)
        {
            NAPI.ClientEvent.TriggerClientEvent(player, "SmoothThrottle_PlayerEnterVehicle", veh, seat);
        }

        //You can call these to change settings on player if you want.
        //Note that these are toggles, you only need to call them once.

        //This disables/enables the smooth throttle
        public static void SetSmoothThrottle(Player player, bool turnedOn)
        {
            NAPI.ClientEvent.TriggerClientEvent(player, "SmoothThrottle_SetSmoothThrottle", turnedOn);
        }

        //This disables/enables anti reverse
        public static void SetAntiReverse(Player player, bool turnedOn)
        {
            NAPI.ClientEvent.TriggerClientEvent(player, "SmoothThrottle_SetAntiReverse", turnedOn);
        }

        //This disables/enables both
        public static void SetSmoothThrottleAntiReverse(Player player, bool turnedOn)
        {
            NAPI.ClientEvent.TriggerClientEvent(player, "SmoothThrottle_SetGlobal", turnedOn);
        }
    }
}