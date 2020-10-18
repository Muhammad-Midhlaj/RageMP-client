using GTANetworkAPI;
using Redage.SDK;

namespace NeptuneEvo.Core
{
    class AdminSP : Script
    {
        [RemoteEvent("SpectateSelect")]
        public static void SpectatePrevNext(Player player, bool state)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (!Group.CanUseCmd(player, "sp")) return;
            int target = player.GetData<int>("spclient"); // It is better to call GetData <object> once than several times. SetData / GetData <object> is slow.
            if (target != -1)
            {
                int id = 0;
                if (!state)
                {
                    id = (target - 1);
                    if (id == player.Value) id--; // We skip our ID, because we cannot look after ourselves
                }
                else
                {
                    id = (target + 1);
                    if (id == player.Value) id++; // We skip our ID, because we cannot look after ourselves
                }
                Spectate(player, id);
            }
            else player.SendChatMessage("Unable to switch to another player.");
        }
        
        public static void Spectate(Player player, int id)
        {
            if (Main.Players.ContainsKey(player))
            {
                if (id >= 0 && id < NAPI.Server.GetMaxPlayers())
                {
                    Player target = Main.GetPlayerByID(id);
                    if (target != null)
                    {
                        if (target != player)
                        {
                            if (Main.Players.ContainsKey(target))
                            {
                                if (target.GetData<bool>("spmode") == false)
                                {
                                    if (player.GetData<bool>("spmode") == false)
                                    { // We do not save new position data if we are already in tracking mode
                                        player.SetData("sppos", player.Position);
                                        player.SetData("spdim", player.Dimension);
                                    }
                                    else NAPI.ClientEvent.TriggerClientEvent(player, "spmode", null, false); // If SP is already sewn for someone and then on another, then first deattach
                                    player.SetSharedData("INVISIBLE", true); // Your variable from your invisibility system so that players do not see the nickname over their heads
                                    player.SetData("spmode", true);
                                    player.SetData("spclient", target.Value);
                                    player.Transparency = 0; // Сначала устанавливаем игроку полную прозрачность, а только потом телепортируем к игроку
                                    player.Dimension = target.Dimension;
                                    player.Position = new Vector3(target.Position.X, target.Position.Y, (target.Position.Z + 3)); // Сначала телепортируем к игроку, чтобы он загрузился
                                    NAPI.ClientEvent.TriggerClientEvent(player, "spmode", target, true); // И только потом аттачим админа к игроку
                                    player.SendChatMessage("Are you watching " + target.Name + " [ID: " + target.Value + "].");
                                }
                            }
                            else player.SendChatMessage("The player with this ID has not logged in yet.");
                        }
                    }
                    else player.SendChatMessage("Player ID " + id + " absent.");
                }
                else player.SendChatMessage("Player ID is invalid (less than 0 or more slots).");
            }
        }

        [RemoteEvent("UnSpectate")]
        public static void RemoteUnSpectate(Player player)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (!Group.CanUseCmd(player, "sp")) return;
            UnSpectate(player);
        }
        
        public static void UnSpectate(Player player)
        {
            if (Main.Players.ContainsKey(player))
            {
                if (player.GetData<bool>("spmode") == true)
                {
                    NAPI.ClientEvent.TriggerClientEvent(player, "spmode", null, false);
                    player.SetData("spclient", -1);
                    Timers.StartOnce(400, () => {
                        player.Dimension = player.GetData<uint>("spdim");
                        player.Position = player.GetData<Vector3>("sppos"); // Сначала возвращаем игрока на исходное местоположение, а только потом восстанавливаем прозрачность
                        player.Transparency = 255;
                        player.SetSharedData("INVISIBLE", false); // Включаем видимость ника и отключаем отображение хп всех игроков рядом
                        player.SetData("spmode", false);
                        player.SendChatMessage("You are out of spectator mode.");
                    });
                }
                else player.SendChatMessage("You are not in spectator mode.");
            }
        }
    }
}
