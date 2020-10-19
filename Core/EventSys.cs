using GTANetworkAPI;
using Redage.SDK;
using System;
using System.Collections.Generic;

namespace NeptuneEvo.Core
{
    class EventSys : Script
    {
        private class CustomEvent
        {
            public string Name { get; set; }
            public Player Admin { get; set; }
            public Vector3 Position { get; set; }
            public uint Dimension { get; set; }
            public ushort MembersLimit { get; set; }
            public Player Winner { get; set; }
            public uint Reward { get; set; }
            public ColShape Zone { get; set; } = null;
            public byte EventState { get; set; } = 0; // 0 - МП не создано, 1 - Создано, но не началось, 2 - Создано и началось.
            public DateTime Started { get; set; }
            public uint RewardLimit { get; set; } = 0;
            public List<Player> EventMembers = new List<Player>();
            public List<Vehicle> EventVehicles = new List<Vehicle>();
        }
        private static CustomEvent AdminEvent = new CustomEvent(); // Одновременно можно будет создать только одно мероприятие.
        private static nLog Log = new nLog("EventSys");
        private static Config config = new Config("EventSys");

        public static void Init()
        {
            AdminEvent.RewardLimit = config.TryGet<uint>("RewardLimit", 20000);
        }
        
        private void DeleteClientFromEvent(Player player)
        {
            AdminEvent.EventMembers.Remove(player);
        }

        [ServerEvent(Event.PlayerDisconnected)]
        public void OnPlayerDisconnected(Player player, DisconnectionType type, string reason)
        {
            if (AdminEvent.EventState != 0)
            {
                if (AdminEvent.EventMembers.Contains(player))
                {
                    DeleteClientFromEvent(player);
                    if (AdminEvent.EventState == 2)
                    {
                        if (AdminEvent.EventMembers.Count == 0) CloseAdminEvent(AdminEvent.Admin, 0);
                    }
                }
            }
        }

        [ServerEvent(Event.PlayerDeath)]
        public void OnPlayerDeath(Player player, Player killer, uint reason)
        {
            if (AdminEvent.EventState != 0)
            {
                if (AdminEvent.EventMembers.Contains(player))
                {
                    DeleteClientFromEvent(player);
                    if (AdminEvent.EventState == 2)
                    {
                        if (AdminEvent.EventMembers.Count == 0) CloseAdminEvent(AdminEvent.Admin, 0);
                    }
                }
            }
        }

        [ServerEvent(Event.PlayerExitColshape)]
        public void OnPlayerExitColshape(ColShape colshape, Player player)
        {
            if (AdminEvent.EventState == 2)
            { // Проверяет только после начала мп, когда телепорт закрыт
                if (AdminEvent.Zone != null)
                {
                    if (AdminEvent.EventMembers.Contains(player))
                    {
                        if (colshape == AdminEvent.Zone)
                        {
                            player.Health = 0;
                            player.Armor = 0;
                            player.SendChatMessage("You left the event area.");
                        }
                    }
                }
            }
        }

        [Command("createmp", "Use: /createmp [Participant limit] [Zone radius] [event title]", GreedyArg = true)]
        public void CreateEvent(Player player, ushort members, float radius, string eventname)
        {
            if (Main.Players.ContainsKey(player))
            {
                if (!Group.CanUseCmd(player, "createmp")) return;
                if (AdminEvent.EventState == 0)
                {
                    if (eventname.Length < 50)
                    {
                        if (radius >= 10) AdminEvent.Zone = NAPI.ColShape.CreateSphereColShape(player.Position, radius, player.Dimension);
                        AdminEvent.EventState = 1;
                        AdminEvent.EventMembers = new List<Player>();
                        AdminEvent.EventVehicles = new List<Vehicle>();
                        if (members >= NAPI.Server.GetMaxPlayers()) members = 0;
                        AdminEvent.Started = DateTime.Now;
                        AdminEvent.MembersLimit = members;
                        AdminEvent.Name = eventname;
                        AdminEvent.Winner = null;
                        AdminEvent.Position = player.Position;
                        AdminEvent.Dimension = player.Dimension;
                        AdminEvent.Admin = player;
                        AddAdminEventLog();
                        NAPI.Chat.SendChatMessageToAll("!{#A87C33}Dear players, the event is starting soon '" + eventname + "'!");
                        if (members != 0) NAPI.Chat.SendChatMessageToAll("!{#A87C33}This event has a limit of participants: " + members + ".");
                        else NAPI.Chat.SendChatMessageToAll("!{#A87C33}There is no participant limit for this event.");
                        if (AdminEvent.Zone != null) NAPI.Chat.SendChatMessageToAll("!{#A87C33}The event operates in the zone " + radius + "m from the teleport points.");
                        NAPI.Chat.SendChatMessageToAll("!{#A87C33}To teleport to the event, enter the command /mp");
                    }
                    else player.SendChatMessage("The name of the event is too long, please make it shorter.");
                }
                else player.SendChatMessage("One event has already been created, you cannot create a new one while the old one is active.");
            }
        }

        [Command("startmp")]
        public void StartEvent(Player player)
        {
            if (Main.Players.ContainsKey(player))
            {
                if (!Group.CanUseCmd(player, "startmp")) return;
                if (AdminEvent.EventState == 1)
                {
                    if (AdminEvent.EventMembers.Count >= 1)
                    {
                        AdminEvent.EventState = 2;
                        NAPI.Chat.SendChatMessageToAll("!{#A87C33}Event'" + AdminEvent.Name + "' started, teleport closed!");
                        NAPI.Chat.SendChatMessageToAll("!{#A87C33} Players at the event: " + AdminEvent.EventMembers.Count + ".");
                    }
                    else player.SendChatMessage("It is impossible to start an event without participants.");
                }
                else player.SendChatMessage("The event is either not created or is already running.");
            }
        }

        [Command("stopmp", "Use: /stopmp [ID player] [Reward]")]
        public void MPReward(Player player, int pid, uint reward)
        {
            if (Main.Players.ContainsKey(player))
            {
                if (!Group.CanUseCmd(player, "stopmp")) return;
                if (AdminEvent.EventState == 2)
                {
                    if (reward <= AdminEvent.RewardLimit)
                    {
                        if (AdminEvent.Winner == null)
                        {
                            Player target = Main.GetPlayerByID(pid);
                            if (target != null)
                            {
                                if (AdminEvent.EventMembers.Contains(target) || AdminEvent.Admin == target) CloseAdminEvent(target, reward);
                                else player.SendChatMessage("This player was found, but he is not a participant in the event.");
                            }
                            else player.SendChatMessage("No player with that ID was found. ");
                        }
                        else player.SendChatMessage("The winner has already been nominated.");
                    }
                    else player.SendChatMessage("The reward cannot exceed the set limit: " + AdminEvent.RewardLimit + ".");
                }
                else player.SendChatMessage("The event has either not been created or has not yet started.");
            }
        }

        [Command("mpveh", "Use: /mpveh [Model name] [Colour] [Colour] [Number of cars]")]
        public void CreateMPVehs(Player player, string model, byte c1, byte c2, byte count)
        {
            if (Main.Players.ContainsKey(player))
            {
                if (!Group.CanUseCmd(player, "mpveh")) return;
                if (AdminEvent.EventState >= 1)
                {
                    if (count >= 1 && count <= 10)
                    {
                        VehicleHash vehHash = (VehicleHash)NAPI.Util.GetHashKey(model);
                        if (vehHash != 0)
                        {
                            for (byte i = 0; i != count; i++)
                            {
                                Vehicle vehicle = NAPI.Vehicle.CreateVehicle(vehHash, new Vector3((player.Position.X + (4 * i)), player.Position.Y, player.Position.Z), player.Rotation.Z, c1, c2, "EVENTCAR");
                                vehicle.Dimension = player.Dimension;
                                VehicleStreaming.SetEngineState(vehicle, true);
                                AdminEvent.EventVehicles.Add(vehicle);
                            }
                            AdminEvent.Admin = player;
                        }
                        else player.SendChatMessage("Cars with this name were not found in the database.");
                    }
                    else player.SendChatMessage("You can create from 1 to 10 cars at a time.");
                }
                else player.SendChatMessage("Vehicles can be created only after creation and before the event starts.");
            }
        }

        [Command("mpreward", "Use: /mpreward [New limit]")]
        public void SetMPReward(Player player, uint newreward)
        {
            if (Main.Players.ContainsKey(player))
            {
                if (Main.Players[player].AdminLVL >= 6)
                {
                    if (newreward <= 999999)
                    {
                        AdminEvent.RewardLimit = newreward;
                        try
                        {
                            MySQL.Query($"UPDATE `eventcfg` SET `RewardLimit`={newreward}");
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "You have set a limit on " + newreward, 3000);
                        }
                        catch (Exception e)
                        {
                            Log.Write("EXCEPTION AT \"SetMPReward\":\n" + e.ToString(), nLog.Type.Error);
                        }
                    }
                    else player.SendChatMessage("You have entered too high a limit. Maximum possible limit: 999999");
                }
            }
        }

        [Command("mp")]
        public void TpToMp(Player player)
        {
            if (Main.Players.ContainsKey(player))
            {
                if (Main.Players[player].DemorganTime == 0 && Main.Players[player].ArrestTime == 0 && player.HasData("CUFFED") && player.GetData<bool>("CUFFED") == false && player.HasSharedData("InDeath") && player.GetSharedData<bool>("InDeath") == false)
                {
                    if (AdminEvent.EventState == 1)
                    {
                        if (!AdminEvent.EventMembers.Contains(player))
                        {
                            if (AdminEvent.MembersLimit == 0 || AdminEvent.EventMembers.Count < AdminEvent.MembersLimit)
                            {
                                AdminEvent.EventMembers.Add(player);
                                player.Position = AdminEvent.Position;
                                player.Dimension = AdminEvent.Dimension;
                                player.SendChatMessage("You were teleported to an event '" + AdminEvent.Name + "'.");
                            }
                            else player.SendChatMessage("Unfortunately, the list of participants is full.");
                        }
                        else player.SendChatMessage("You are already on the list of participants.");
                    }
                    else player.SendChatMessage("The teleport is closed. ");
                }
                else player.SendChatMessage("Teleport is not available for you.");
            }
        }

        [Command("mpkick", "Use: /mpkick [ID player]")]
        public void MPKick(Player player, int pid)
        {
            if (Main.Players.ContainsKey(player))
            {
                if (!Group.CanUseCmd(player, "mpkick")) return;
                if (AdminEvent.EventState == 1)
                {
                    Player target = Main.GetPlayerByID(pid);
                    if (target != null)
                    {
                        if (AdminEvent.EventMembers.Contains(target))
                        {
                            AdminEvent.Admin = player;
                            target.Health = 0;
                            target.Armor = 0;
                            player.SendChatMessage("You kicked out " + target.Name + " from the event.");
                        }
                        else player.SendChatMessage("A player with the given ID was found, but he is not a participant in the event.");
                    }
                    else player.SendChatMessage("No player with this ID was found.");
                }
                else player.SendChatMessage("A player can be kicked out only after creation and before the start of the event.");
            }
        }

        [Command("mphp", "Use: /mphp [number HP]")]
        public void MPHeal(Player player, byte health)
        {
            if (Main.Players.ContainsKey(player))
            {
                if (!Group.CanUseCmd(player, "mphp")) return;
                if (AdminEvent.EventState >= 1)
                {
                    if (health >= 1 && health <= 100)
                    {
                        AdminEvent.Admin = player;
                        foreach (Player target in AdminEvent.EventMembers)
                        {
                            NAPI.Player.SetPlayerHealth(target, health);
                        }
                        player.SendChatMessage("You have successfully installed all MP members " + health + " HP.");
                    }
                    else player.SendChatMessage("The amount of HP that can be set is in the range from 1 to 100.");
                }
                else player.SendChatMessage("You can only give HP to players before the event starts.");
            }
        }

        [Command("mpar", "Use: /mpar [number Armor]")]
        public void MPArmor(Player player, byte armor)
        {
            if (Main.Players.ContainsKey(player))
            {
                if (!Group.CanUseCmd(player, "mpar")) return;
                if (AdminEvent.EventState >= 1)
                {
                    if (armor >= 0 && armor <= 100)
                    {
                        AdminEvent.Admin = player;
                        foreach (Player target in AdminEvent.EventMembers)
                        {
                            NAPI.Player.SetPlayerArmor(target, armor);
                        }
                        player.SendChatMessage("You have successfully installed all MP members " + armor + " брони.");
                    }
                    else player.SendChatMessage("The amount of Armor that can be set is in the range from 0 to 100.");
                }
                else player.SendChatMessage("You can only give Armor to players before the event starts.");
            }
        }

        [Command("mpplayers")]
        public void MpPlayerList(Player player)
        {
            if (Main.Players.ContainsKey(player))
            {
                if (!Group.CanUseCmd(player, "mpplayers")) return;
                if (AdminEvent.EventState != 0)
                {
                    short memcount = Convert.ToInt16(AdminEvent.EventMembers.Count);
                    if (memcount > 0)
                    {
                        if (memcount <= 15)
                        {
                            foreach (Player target in AdminEvent.EventMembers)
                            {
                                player.SendChatMessage("ID: " + target.Value + " | Имя: " + target.Name);
                            }
                            player.SendChatMessage("Players at the event: " + memcount);
                        }
                        else player.SendChatMessage("Players at the event: " + memcount);
                    }
                    else player.SendChatMessage("No players found at the event. ");
                }
                else player.SendChatMessage("The event has not yet been created.");
            }
        }
        
        private void AddAdminEventLog()
        {
            try
            {
                GameLog.EventLogAdd(AdminEvent.Admin.Name, AdminEvent.Name, AdminEvent.MembersLimit, MySQL.ConvertTime(AdminEvent.Started));
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"AddAdminEventLog\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

        private void UpdateLastAdminEventLog()
        {
            try
            {
                GameLog.EventLogUpdate(AdminEvent.Admin.Name,AdminEvent.EventMembers.Count,AdminEvent.Winner.Name,AdminEvent.Reward,MySQL.ConvertTime(DateTime.Now),AdminEvent.RewardLimit, AdminEvent.MembersLimit, AdminEvent.Name);
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"UpdateLastAdminEventLog\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

        private void CloseAdminEvent(Player winner, uint reward)
        {
            if (AdminEvent.Zone != null)
            {
                AdminEvent.Zone.Delete();
                AdminEvent.Zone = null;
            }
            if (AdminEvent.EventVehicles.Count != 0)
            {
                foreach (Vehicle vehicle in AdminEvent.EventVehicles)
                {
                    vehicle.Delete();
                }
            }
            AdminEvent.Winner = winner;
            AdminEvent.Reward = reward;
            AdminEvent.EventState = 0;
            UpdateLastAdminEventLog();
            NAPI.Chat.SendChatMessageToAll("!{#A87C33}Event '" + AdminEvent.Name + "' ended, thanks for your participation!");
            if (winner != AdminEvent.Admin)
            {
                if (reward != 0)
                {
                    NAPI.Chat.SendChatMessageToAll("!{#A87C33}Winner " + winner.Name + " received a prize of " + reward + "$.");
                    MoneySystem.Wallet.Change(winner, (int)reward);
                }
                else NAPI.Chat.SendChatMessageToAll("!{#A87C33}Winner: " + winner.Name + ".");
            }
        }
    }
}
