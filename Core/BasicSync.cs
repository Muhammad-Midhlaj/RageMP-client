using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Redage.SDK;
using System.Linq;

namespace NeptuneEvo.Core
{
    class BasicSync : Script
    {
        private static nLog Log = new nLog("BasicSync");

        public static void AttachLabelToObject(string text, Vector3 posOffset, NetHandle obj)
        {
            var attachedLabel = new AttachedLabel(text, posOffset);
            switch (obj.Type)
            {
                case EntityType.Player:
                    var player = NAPI.Entity.GetEntityFromHandle<Player>(obj);
                    player.SetSharedData("attachedLabel", JsonConvert.SerializeObject(attachedLabel));
                    Trigger.ClientEventInRange(player.Position, 550, "attachLabel", player);
                    break;
                case EntityType.Vehicle:
                    var vehicle = NAPI.Entity.GetEntityFromHandle<Vehicle>(obj);
                    vehicle.SetSharedData("attachedLabel", JsonConvert.SerializeObject(attachedLabel));
                    Trigger.ClientEventInRange(vehicle.Position, 550, "attachLabel", vehicle);
                    break;
            }
        }

        public static void DetachLabel(NetHandle obj)
        {
            switch (obj.Type)
            {
                case EntityType.Player:
                    var player = NAPI.Entity.GetEntityFromHandle<Player>(obj);
                    player.ResetSharedData("attachedLabel");
                    Trigger.ClientEventInRange(player.Position, 550, "detachLabel");
                    break;
                case EntityType.Vehicle:
                    var vehicle = NAPI.Entity.GetEntityFromHandle<Vehicle>(obj);
                    vehicle.ResetSharedData("attachedLabel");
                    Trigger.ClientEventInRange(vehicle.Position, 550, "detachLabel");
                    break;
            }
        }

        public static void AttachObjectToPlayer(Player player, uint model, int bone, Vector3 posOffset, Vector3 rotOffset)
        {
            var attObj = new AttachedObject(model, bone, posOffset, rotOffset);
            player.SetSharedData("attachedObject", JsonConvert.SerializeObject(attObj));
            Trigger.ClientEventInRange(player.Position, 550, "attachObject", player);
        }

        public static void DetachObject(Player player)
        {
            player.ResetSharedData("attachedObject");
            Trigger.ClientEventInRange(player.Position, 550, "detachObject", player);
        }

        private static string SerializeAttachments(List<uint> attachments)
        {
            return string.Join('|', attachments.Select(hash => hash.ToString("X")));
        }

        [ServerEvent(Event.PlayerConnected)]
        public void OnPlayerConnected(Player player)
        {
            player.SetData("ATTACHMENTS", new List<uint>());
        }

        // TODO: adding attachments by client
        [RemoteEvent("staticAttachments.Add")]
        public static void StaticAttachmentsAdd(Player player, uint hash)
        {

        }

        [RemoteEvent("staticAttachments.Remove")]
        public static void StaticAttachmentsRemove(Player player, uint hash)
        {

        }

        [RemoteEvent("fingerPointer.start")]
        public static void FingerPointerStart(Player player)
        {
            try
            {
                /*if (string.IsNullOrEmpty(playersIdsStr)) playersIdsStr = "[]";
                List<Client> Players = new List<Client>();
                int[] playersIds = JsonConvert.DeserializeObject<int[]>(playersIdsStr);;

                Action<int> action = new Action<int>((int playerId) => {
                    Client targetPlayer = Main.GetPlayerByID(playerId);
                    if (targetPlayer != null) Players.Add(targetPlayer);
                });

                Array.ForEach(playersIds, action);*/

                Trigger.ClientEventInRange(player.Position, 100, "fingerPointer.client.start", player);
                player.SetSharedData("fingerPointerActive", true);
            }
            catch (Exception e)
            {
                Log.Write("FingerPointerStart.Event: " + e.Message + "\n playerIdsStr: ", nLog.Type.Error);
            }
        }

        [RemoteEvent("fingerPointer.stop")]
        public static void FingerPointerStop(Player player)
        {
            try
            {
                /*if (string.IsNullOrEmpty(playersIdsStr)) playersIdsStr = "[]";
                List<Client> Players = new List<Client>();
                int[] playersIds = JsonConvert.DeserializeObject<int[]>(playersIdsStr);

                Action<int> action = new Action<int>((int playerId) => {
                    Client targetPlayer = Main.GetPlayerByID(playerId);
                    if (targetPlayer != null) Players.Add(targetPlayer);
                });

                Array.ForEach(playersIds, action);*/

                Trigger.ClientEventInRange(player.Position, 100, "fingerPointer.client.stop", player);
                player.SetSharedData("fingerPointerActive", false);
            }
            catch (Exception e)
            {
                Log.Write("FingerPointerStop.Event: " + e.Message + "\n playerIdsStr: ", nLog.Type.Error);
            }
        }

        [RemoteEvent("fingerPointer.updateData")]
        public static void FingerPointerUpdateData(Player player, string playersIdsStr, float camPitch, float camHeading, bool fingerIsBlocked, bool fingerIsFirstPerson)
        {
            try
            {
                if (string.IsNullOrEmpty(playersIdsStr)) playersIdsStr = "[]";
                List<Player> Players = new List<Player>();
                int[] playersIds = JsonConvert.DeserializeObject<int[]>(playersIdsStr);

                Action<int> action = new Action<int>((int playerId) => {
                    Player targetPlayer = NeptuneEvo.Main.GetPlayerByID(playerId);
                    if (targetPlayer != null) Players.Add(targetPlayer);
                });

                Array.ForEach(playersIds, action);

                Trigger.ClientEventToPlayers(Players.ToArray(), "fingerPointer.client.updateData", player, camPitch, camHeading, fingerIsBlocked, fingerIsFirstPerson);
            }
            catch (Exception e)
            {
                Log.Write("FingerPointerUpdateData.Event: " + e.Message, nLog.Type.Error);
            }
        }

        [RemoteEvent("invisible")]
        public static void SetInvisible(Player player, bool toggle)
        {
            try
            {
                if (Main.Players[player].AdminLVL == 0) return;
                player.SetSharedData("INVISIBLE", toggle);
                Trigger.ClientEventInRange(player.Position, 550, "toggleInvisible", player, toggle);
            }
            catch (Exception e) { Log.Write("InvisibleEvent: " + e.Message, nLog.Type.Error); }
        }

        public static bool GetInvisible(Player player)
        {
            if (!player.HasSharedData("INVISIBLE") || !player.GetSharedData<bool>("INVISIBLE"))
                return false;
            else
                return true;
        }

        internal class PlayAnimData
        {
            public string Dict { get; set; }
            public string Name { get; set; }
            public int Flag { get; set; }

            public PlayAnimData(string dict, string name, int flag)
            {
                Dict = dict;
                Name = name;
                Flag = flag;
            }
        }

        internal class AttachedObject
        {
            public uint Model { get; set; }
            public int Bone { get; set; }
            public Vector3 PosOffset { get; set; }
            public Vector3 RotOffset { get; set; }

            public AttachedObject(uint model, int bone, Vector3 pos, Vector3 rot)
            {
                Model = model;
                Bone = bone;
                PosOffset = pos;
                RotOffset = rot;
            }
        }

        internal class AttachedLabel
        {
            public string Text { get; set; }
            public Vector3 PosOffset { get; set; }

            public AttachedLabel(string text, Vector3 pos)
            {
                Text = text;
                PosOffset = pos;
            }
        }


    }
}
