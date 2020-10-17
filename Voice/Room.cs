using GTANetworkAPI;
using System.Collections.Generic;

namespace NeptuneEvo.Voice
{
    class Room
    {
        public string Name;
        public List<Player> Players;

        public Dictionary<string, object> MetaData { get { return new Dictionary<string, object> { { "name", Name } }; } }

        public Room(string Name)
        {
            this.Name = Name;

            this.Players = new List<Player>();
        }



        public void OnJoin(Player player)
        {
            if (Players.Contains(player))
            {
                var argsMe = new List<object> { MetaData };
                Players.ForEach(_player => argsMe.Add(_player));


                Trigger.ClientEvent(player, "voice.radioConnect", argsMe.ToArray());
                Trigger.ClientEventToPlayers(Players.ToArray(), "voice.radioConnect", MetaData, player);

                var tempPlayer = player.GetData<VoiceMetaData>("Voip");
                tempPlayer.RadioRoom = Name;

                player.SetData<VoiceMetaData>("Voip", tempPlayer);
                Players.Add(player);
            }
        }

        public void OnQuit(Player player)
        {
            if (Players.Contains(player))
            {
                var argsMe = new List<object> { MetaData };
                Players.ForEach(_player => argsMe.Add(_player));

                Trigger.ClientEvent(player, "voice.radioDisconnect", argsMe.ToArray());
                Trigger.ClientEventToPlayers(Players.ToArray(), "voice.radioDisconnect", MetaData, player);

                player.ResetData("Voip");
                Players.Remove(player);
            }
        }

        public void OnRemove()
        {
            Trigger.ClientEventToPlayers(Players.ToArray(), "voice.radioDisconnect", MetaData);
            Players.Clear();
        }
    }
}