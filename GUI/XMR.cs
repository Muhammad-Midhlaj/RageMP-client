using System;
using System.Collections.Generic;
using GTANetworkAPI;
using Newtonsoft.Json;
class XMR : Script
{
    /*public class XmrEnum : IEquatable<XmrEnum>
    {
        public int id { get; set; }
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public string stream { get; set; }
        public dynamic carid { get; set; }
        public bool car { get; set; }


        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            XmrEnum objAsPart = obj as XmrEnum;
            if (objAsPart == null) return false;
            else return Equals(objAsPart);
        }
        public override int GetHashCode()
        {
            return id;
        }
        public bool Equals(XmrEnum other)
        {
            if (other == null) return false;
            return (this.id.Equals(other.id));
        }
    }

  
    public static List<XmrEnum> xmr_playing = new List<XmrEnum>();
    //public static List<dynamic> xmr_radios = new List<dynamic>();*/


    public static int MAX_RADIOS = 800;

    public class XmrEnum : IEquatable<XmrEnum>
    {
        public int id { get; set; }
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public string stream { get; set; }
        public int carid { get; set; }

        [JsonIgnore]
        public int sql_id { get; set; }

        [JsonIgnore]
        public Entity objeto { get; set; }

        [JsonIgnore]
        public TextLabel label { get; set; }


        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            XmrEnum objAsPart = obj as XmrEnum;
            if (objAsPart == null) return false;
            else return Equals(objAsPart);
        }
        public override int GetHashCode()
        {
            return id;
        }
        public bool Equals(XmrEnum other)
        {
            if (other == null) return false;
            return (this.id.Equals(other.id));
        }
    }

    public static List<XmrEnum> xmr_radios = new List<XmrEnum>();
    public static List<dynamic> list_of_radios = new List<dynamic>();


    public static void XMRs()
    {
        list_of_radios.Add(new { name = "Desligar", url = "" });
        list_of_radios.Add(new { name = "POWERHITZ.COM - #1 FOR HIT MUSIC", url = "http://66.85.88.174:80/hitlist" });
        list_of_radios.Add(new { name = "Radio Hunter - The Hitz Channel", url = "http://listen.shoutcast.com:80/RadioHunter-TheHitzChannel" });
        list_of_radios.Add(new { name = "107.7 Today's Hits", url = "http://rfcmedia.streamguys1.com/MusicPulse.mp3" });
        list_of_radios.Add(new { name = "Funkeiro Life",  url = "http://67.23.242.202:9040/stream" });
        list_of_radios.Add(new { name = "Radio Estilo Retro", url = "http://198.58.98.83:8193/stream" });
        list_of_radios.Add(new { name = "Mundo Rasta FM", url = "http://146.71.124.10:8200/stream" });
        list_of_radios.Add(new { name = "Rock FM", url = "http://54.38.186.143:80/" });
        list_of_radios.Add(new { name = "Metaleiro FM", url = "http://149.56.74.125:20008/stream" });
        list_of_radios.Add(new { name = "House & Festa FM", url = "http://198.15.94.34:8030/stream" });
        list_of_radios.Add(new { name = "Radio Festa", url = "http://94.130.238.52:8016/stream" });
        list_of_radios.Add(new { name = "Fat Boy Rap FM", url = "http://149.56.157.81:5024/stream" });



        for (int i = 0; i < MAX_RADIOS; i++)
        {
            xmr_radios.Add(new XmrEnum { id = i, x = 0, y = 0, z = 0, stream = "null", carid = 0, sql_id = -1 });
        }

    }

    public static void LoadRadiosXMR(Player player)
    {
        player.TriggerEvent("LoadXMR", API.Shared.ToJson(xmr_radios));
    }

    public static int GetPlayerSQLID(Player player)
    {
        return player.GetData<int>("character_sqlid");
    }

    public static void CreateMenu(Player player, string callback, string title, string description, bool freezePlayer, string json, bool resetBack, string BackgroundSprite = "none", string BackgroundColor = "none", int CurrentSelect = 0, bool MouseControl = false)
    {

        player.TriggerEvent("menu_handler_create_menu_generic", callback, title, description, freezePlayer, json, resetBack, BackgroundSprite, BackgroundColor, CurrentSelect, MouseControl);
    }

    public static void OnPlayerDisconnect(Player player)
    {
        for (int i = 0; i < MAX_RADIOS; i++)
        {
            if (xmr_radios[i].sql_id == XMR.GetPlayerSQLID(player) && xmr_radios[i].carid == 0)
            {
                xmr_radios[i].sql_id = -1;
                xmr_radios[i].carid = 0;
                xmr_radios[i].x = 0;
                xmr_radios[i].y = 0;
                xmr_radios[i].z = 0;
                xmr_radios[i].stream = "null";
                xmr_radios[i].objeto.Delete();
                xmr_radios[i].label.Delete();
                player.TriggerEvent("RemoveRadio", i);
            }
        }
    }


    [Command("setstation")]
    public static void CMD_setstation(Player player)
    {
        List<dynamic> menu_item_list = new List<dynamic>();

        foreach (var radio in list_of_radios)
        {
            menu_item_list.Add(new { Type = 1, Name = radio.name, Description = "" });
        }

        if (player.IsInVehicle && player.Vehicle.HasSharedData("vehicle_radio_id"))
        {
            XMR.CreateMenu(player, "RESPONSE_RADIO_MENU", "SK Radio", "Radio do Veículo", false, JsonConvert.SerializeObject(menu_item_list), false, BackgroundColor: "Green");
        }
        else
        {
            int radio_id = -1;
            for (int i = 0; i < MAX_RADIOS; i++)
            {
                if (xmr_radios[i].sql_id == XMR.GetPlayerSQLID(player) && xmr_radios[i].carid == 0)
                {
                    radio_id = i;
                    break;
                }
            }

            if (radio_id == -1)
            {
                return;
            }

            XMR.CreateMenu(player, "RESPONSE_RADIO_MENU", "SK Radio", "Boombox - Radio List", false, JsonConvert.SerializeObject(menu_item_list), false, BackgroundColor: "Green");
        }

    }

    public static void SelectMenuResponse(Player player, String callbackId, int selectedIndex, String objectName, String valueList)
    {
        if(callbackId == "RESPONSE_RADIO_MENU")
        {
            if (player.IsInVehicle)
            {
                foreach (var radio in list_of_radios)
                {
                    if (radio.name == objectName)
                    {
                        SetStationVehicle(player, radio.url);
                        return;
                    }
                }
            }
            else
            {

                int radio_id = -1;

                for (int i = 0; i < MAX_RADIOS; i++)
                {
                    if (xmr_radios[i].sql_id == XMR.GetPlayerSQLID(player) && xmr_radios[i].carid == 0)
                    {
                        radio_id = i;
                        break;
                    }
                }

                if (radio_id == -1)
                {
                    return;
                }

                if (objectName == "Desligar")
                {
                    for (int i = 0; i < MAX_RADIOS; i++)
                    {
                        if (xmr_radios[i].sql_id == XMR.GetPlayerSQLID(player) && xmr_radios[i].carid == 0)
                        {
                            xmr_radios[i].sql_id = -1;
                            xmr_radios[i].carid = 0;
                            xmr_radios[i].x = 0;
                            xmr_radios[i].y = 0;
                            xmr_radios[i].z = 0;
                            xmr_radios[i].stream = "null";
                            xmr_radios[i].objeto.Delete();
                            xmr_radios[i].label.Delete();

                            foreach (var target in API.Shared.GetAllPlayers())
                            {
                                target.TriggerEvent("RemoveRadio", i);
                            }
                            return;
                        }
                    }
                }
                foreach (var radio in list_of_radios)
                {
                    if (radio.name == objectName)
                    {
                        SetBoombox(player, radio.url);
                        return;
                    }

                }
            }
        }
    }


    public static void SetStationVehicle(Player player, string stream)
    {
        if (player.IsInVehicle)
        {
            int radio_id = -1;
            for (int i = 0; i < MAX_RADIOS; i++)
            {
                if (xmr_radios[i].carid == player.Vehicle.GetSharedData<int>("vehicle_radio_id") && xmr_radios[i].sql_id == -1)
                {
                    radio_id = i;

                    break;
                }
            }

            if (radio_id == -1)
            {
                for (int i = 0; i < MAX_RADIOS; i++)
                {
                    if (xmr_radios[i].carid == 0 && xmr_radios[i].sql_id == -1)
                    {
                        radio_id = i;
                        break;
                    }
                }
            }

            if (radio_id == -1)
            {
                return;
            }

            if (stream == "Desligar")
            {
                int i = radio_id;
                xmr_radios[i].sql_id = -1;
                xmr_radios[i].carid = 0;
                xmr_radios[i].x = 0;
                xmr_radios[i].y = 0;
                xmr_radios[i].z = 0;
                xmr_radios[i].stream = "null";

                foreach (var target in API.Shared.GetAllPlayers())
                {
                    target.TriggerEvent("RemoveRadio", i);
                }

                return;
            }

            if (xmr_radios[radio_id].stream == "null")
            {
                xmr_radios[radio_id].carid = player.Vehicle.GetSharedData<int>("vehicle_radio_id");
                xmr_radios[radio_id].x = player.Position.X;
                xmr_radios[radio_id].y = player.Position.Y;
                xmr_radios[radio_id].z = player.Position.Z;
                xmr_radios[radio_id].stream = stream;

                foreach (var target in API.Shared.GetAllPlayers())
                {
                    target.TriggerEvent("AddRadio", radio_id, player.Position.X, player.Position.Y, player.Position.Z, xmr_radios[radio_id].stream, xmr_radios[radio_id].carid);
                }

            }
            else
            {

                xmr_radios[radio_id].carid = player.Vehicle.GetSharedData<int>("vehicle_radio_id");
                xmr_radios[radio_id].x = player.Position.X;
                xmr_radios[radio_id].y = player.Position.Y;
                xmr_radios[radio_id].z = player.Position.Z;
                xmr_radios[radio_id].stream = stream;

                foreach (var target in API.Shared.GetAllPlayers())
                {
                    target.TriggerEvent("EditRadio", radio_id, stream);
                }

            }

            

            List<Player> proxPlayers = NAPI.Player.GetPlayersInRadiusOfPlayer(45, player);
            foreach (Player target in proxPlayers)
            {
            }

        }
    }

    [Command("boombox")]
    public static void CMD_Boombox(Player player)
    {
        //2079380440

        if(player.GetData<int>("character_boombox") == 0)
        {
            return;
        }
        int radio_id = -1;
        for (int i = 0; i < MAX_RADIOS; i++)
        {
            if (xmr_radios[i].sql_id == XMR.GetPlayerSQLID(player) && xmr_radios[i].carid == 0)
            {
                radio_id = i;
                break;
            }
        }
        if (radio_id == -1)
        {
            for (int i = 0; i < MAX_RADIOS; i++)
            {
                if (xmr_radios[i].sql_id == -1 && xmr_radios[i].carid == 0)
                {
                    radio_id = i;
                    break;
                }
            }
        }
        if (radio_id == -1)
        {
            return;
        }

        if (xmr_radios[radio_id].stream == "null")
        {

            xmr_radios[radio_id].carid = 0;
            xmr_radios[radio_id].sql_id = XMR.GetPlayerSQLID(player);
            xmr_radios[radio_id].x = player.Position.X;
            xmr_radios[radio_id].y = player.Position.Y;
            xmr_radios[radio_id].z = player.Position.Z;
            xmr_radios[radio_id].stream = "http://66.85.88.174:80/hitlist";

            foreach(var target in API.Shared.GetAllPlayers())
            {
                target.TriggerEvent("AddRadio", radio_id, player.Position.X, player.Position.Y, player.Position.Z, xmr_radios[radio_id].stream, xmr_radios[radio_id].carid);
            }
       

            xmr_radios[radio_id].objeto = API.Shared.CreateObject(2079380440, player.Position - new Vector3(0,0, 1.0f), new Vector3(0,0,0), 0);
            xmr_radios[radio_id].label = API.Shared.CreateTextLabel("~y~[ ~w~Boombox~y~ ]~n~~b~((~w~ Use ~r~/setstation~w~ para alterar a estação ~b~))", xmr_radios[radio_id].objeto.Position + new Vector3(0, 0, 0.45f), 8.0f, 0.29f, 1, new Color(255,255,255,255));
        }
        else
        {
            List<dynamic> menu_item_list = new List<dynamic>();
            foreach (var radio in list_of_radios)
            {
                menu_item_list.Add(new { Type = 1, Name = radio.name, Description = "" });
            }
            XMR.CreateMenu(player, "RESPONSE_RADIO_MENU", "SK Radio", "Lista de Radios", false, JsonConvert.SerializeObject(menu_item_list), false, BackgroundColor: "Green");
        }
    }


    public static void SetBoombox(Player player, string stream)
    {

        int radio_id = -1;

        for (int i = 0; i < MAX_RADIOS; i++)
        {
            if (xmr_radios[i].sql_id == XMR.GetPlayerSQLID(player) && xmr_radios[i].carid == 0)
            {
                radio_id = i;
                break;
            }
        }

        if(radio_id == -1)
        {
            return;
        }
        if (xmr_radios[radio_id].stream != "null")
        {
            xmr_radios[radio_id].carid = 0;
            xmr_radios[radio_id].sql_id = XMR.GetPlayerSQLID(player);
            xmr_radios[radio_id].x = player.Position.X;
            xmr_radios[radio_id].y = player.Position.Y;
            xmr_radios[radio_id].z = player.Position.Z;
            xmr_radios[radio_id].stream = stream;


            foreach (var target in API.Shared.GetAllPlayers())
            {
                target.TriggerEvent("EditRadio", radio_id, stream);
            }
          

            List<Player> proxPlayers = NAPI.Player.GetPlayersInRadiusOfPlayer(45, player);
            foreach (Player target in proxPlayers)
            {
                if (target.GetData<bool>("status") == true)
                {
                }
            }
        }
    }




    /*[Command("setstation2")]
    public static void Command(Client player)
    {
        PlayVehicleRadio(player, "http://tunein4.streamguys1.com/hhbeafree5");wwww

    }*/

    /*
    public static void UpdateRadio(Client player)
    {
        List<dynamic> xmr_radios = new List<dynamic>();
        for (int i = 0; i < MAX_RADIOS; i++)
        {
            if (xmr_playing[i].stream != null)
            {

                if(xmr_playing[i].car == true)
                {
                    xmr_radios.Add(new { id = i, x = player.Position.X, y = player.Position.Y, z = player.Position.Z, stream = xmr_playing[i].stream, carid = player.Vehicle.GetSharedData("vehicle_radio_id") });
                }
                else
                {
                    xmr_radios.Add(new { id = i, x = player.Position.X, y = player.Position.Y, z = player.Position.Z, stream = xmr_playing[i].stream, carid = "null" });
                }
            }
        }
        player.TriggerEvent("LoadXMR", API.Shared.ToJson(xmr_radios));

    }

    public static void PlayVehicleRadio(Client player, string stream)
    {
        if(player.IsInVehicle)
        {
            for(int i = 0; i < MAX_RADIOS; i++)
            {
                if (xmr_playing[i].stream == null)
                {
                    xmr_playing[i].stream = stream;
                    player.TriggerEvent("AddRadio", i, player.Position.X, player.Position.Y, player.Position.Z, stream, player.Vehicle.Handle);
                    xmr_playing.Add(new XmrEnum { id = i, x = player.Position.X, y = player.Position.Y, z = player.Position.Z, stream = stream, carid = player.Vehicle.GetSharedData("vehicle_radio_id"), car = true });
                    UpdateRadio(player);
                    return;
                }
            }
        }
        else
        {
            for (int i = 0; i < MAX_RADIOS; i++)
            {
                if (xmr_playing[i].stream == null)
                {
                    xmr_playing[i].stream = stream;
                    player.TriggerEvent("AddRadio", i, player.Position.X, player.Position.Y, player.Position.Z, stream, null);
                    xmr_playing.Add(new XmrEnum { id = i, x = player.Position.X, y = player.Position.Y, z = player.Position.Z, stream = stream, carid = null, car = false });
                    UpdateRadio(player);
                    return;
                }
            }
        }
    }

    [RemoteEvent("SelectionRadioStationByName")]
    public static void SelectionRadioStationByName(Client player, string text)
    {

        PlayVehicleRadio(player, "Teste");
    }*/
}

