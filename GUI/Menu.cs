﻿using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using NeptuneEvo.Core;
using Redage.SDK;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace NeptuneEvo.GUI
{
    class MenuManager : Script
    {
        public static Dictionary<Entity, Menu> Menus = new Dictionary<Entity, Menu>();
        private static nLog Log = new nLog("MenuControl");

        public static void Event_OnPlayerDisconnected(Player client, DisconnectionType type, string reason)
        {
            try
            {
                if (Menus.ContainsKey(client))
                    Menus.Remove(client);
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
        }
        #region PhoneCallback
        [RemoteEvent("Phone")]
        public void PhoneCallback(Player client, params object[] arguments)
        {
            if (client == null || !Main.Players.ContainsKey(client)) return;

            try
            {
                string eventName = Convert.ToString(arguments[0]);

                Menu menu = Menus[client];
                switch (eventName)
                {
                    case "navigation":
                        string btn = Convert.ToString(arguments[1]);
                        if (btn == "home")
                        {
                            Close(client, false);
                            Main.OpenPlayerMenu(client).Wait();
                        }
                        else if (btn == "back")
                        {
                            menu.BackButton.Invoke(client, menu);
                        }
                        break;
                    case "callback":
                        if (menu == null) return;
                        string ItemID = Convert.ToString(arguments[1]);
                        string Event = Convert.ToString(arguments[2]);
                        //dynamic data = NAPI.Util.FromJson(arguments[3].ToString());
                        dynamic data = JsonConvert.DeserializeObject(arguments[3].ToString());

                        Menu.Item item = menu.Items.FirstOrDefault(i => i.ID == ItemID);
                        if (item == null) return;
                        //await Log.DebugAsync($"app:{menu.ID}; item:{item.ID};");
                        //await Log.DebugAsync($"json:{Convert.ToString(arguments[3])}");
                        menu.Callback.Invoke(client, menu, item, Event, data);
                        return;
                }
                return;
            }
            catch (Exception e)
            {
                Menu menu = Menus[client];
                Log.Write($"EXCEPTION AT /{menu.ID}/\"PHONE_CALLBACK\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
        #endregion
        #region Menu Open
        public static void Open(Player client, Menu menu, bool force = false)
        {
            try
            {
                if (Menus.ContainsKey(client))
                {
                    Log.Debug($"Player already have opened Menu! id:{Menus[client].ID}", nLog.Type.Warn);
                    if (!force) return;
                    Menus.Remove(client);
                }
                Menus.Add(client, menu);

                //string data = JsonConvert.SerializeObject(menu);
                string data = menu.getJsonStrAsync();

                if (!client.HasData("Phone"))
                {
                    Trigger.ClientEvent(client, "phoneShow");
                    client.SetData("Phone", true);
                }
                Trigger.ClientEvent(client, "phoneOpen", data);
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"MENUCONTROL_OPEN\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
        public static void OpenAsync(Player client, Menu menu, bool force = false)
        {
            try
            {
                lock (Menus)
                {
                    if (Menus.ContainsKey(client))
                    {
                        Log.Debug($"Player already have opened Menu! id:{Menus[client].ID}");
                        if (!force) return;
                        Menus.Remove(client);
                    }
                    Menus.Add(client, menu);
                }
                string data = menu.getJsonStrAsync();

                if (!client.HasData("Phone"))
                {
                    Trigger.ClientEvent(client, "phoneShow");
                    client.SetData("Phone", true);
                }
                Trigger.ClientEvent(client, "phoneOpen", data);
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"MENUCONTROL_OPEN\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
        #endregion
        #region Menu Close
        public static void Close(Player client, bool hidePhone = true)
        {
            try
            {
                if (Menus.ContainsKey(client))
                    Menus.Remove(client);
                if (hidePhone)
                {
                    Trigger.ClientEvent(client, "phoneHide");
                    client.ResetData("Phone");
                }
                Trigger.ClientEvent(client, "phoneClose");
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"MENUCONTROL_CLOSE\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
        public static void CloseAsync(Player client, bool hidePhone = true)
        {
            try
            {
                lock (Menus)
                {
                    if (Menus.ContainsKey(client))
                        Menus.Remove(client);
                }

                if (hidePhone)
                {
                    Trigger.ClientEvent(client, "phoneHide");
                    client.ResetData("Phone");
                }
                Trigger.ClientEvent(client, "phoneClose");
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"MENUCONTROL_CLOSE\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
        #endregion
    }
    class Menu
    {
        public delegate void MenuCallback(Player client, Menu menu, Item item, string eventName, dynamic data);
        public delegate void MenuBack(Player client, Menu menu);

        public string ID { get; internal set; }
        public List<Item> Items { get; internal set; }
        public bool canBack { get; internal set; }
        public bool canHome { get; internal set; }

        [JsonIgnore]
        public MenuCallback Callback { get; set; }
        [JsonIgnore]
        public MenuBack BackButton { get; set; }
        [JsonIgnore]
        private static nLog Log = new nLog("Menu");

        public Menu(string id, bool canback, bool canhome)
        {
            if (string.IsNullOrEmpty(id))
                ID = "";
            else
                ID = id;

            Items = new List<Item>();
            Callback = null;
            BackButton = null;
            canHome = canhome;
            canBack = canback;
        }
        public void Add(Item item)
        {
            Items.Add(item);
        }
        public void Open(Player client)
        {
            MenuManager.Open(client, this, true);
        }
        public void OpenAsync(Player client)
        {
            MenuManager.Open(client, this, true);
        }
        public void Change(Player client, int index, Item newData)
        {
            string data = JsonConvert.SerializeObject(newData.getJsonArr());
            Trigger.ClientEvent(client, "phoneChange", index, data);
        }

        public string getJsonStr()
        {
            JArray items = new JArray();
            foreach (Item i in Items)
            {
                items.Add(i.getJsonArr());
            }
            JArray menuData = new JArray()
            {
                ID,
                items,
                canBack,
                canHome,
            };
            string data = JsonConvert.SerializeObject(menuData);
            //Log.Write(data, nLog.Type.Debug);
            return data;
        }
        public string getJsonStrAsync()
        {
            JArray items = new JArray();
            foreach (Item i in Items)
            {
                items.Add(i.getJsonArr());
            }
            JArray menuData = new JArray()
            {
                ID,
                items,
                canBack,
                canHome,
            };
            string data = JsonConvert.SerializeObject(menuData);
            return data;
        }

        internal class Item
        {
            public string ID { get; internal set; }
            public string Text { get; internal set; }
            public MenuItem Type { get; internal set; }
            public MenuColor Color { get; set; }
            public int Column { get; set; }
            public int Scale { get; set; }
            public bool Checked { get; set; }
            public List<string> Elements { get; set; }

            public Item(string id, MenuItem type)
            {
                if (string.IsNullOrEmpty(id))
                    ID = "";
                else
                    ID = id;
                Type = type;
                Column = 1;
            }
            public JArray getJsonArr()
            {
                JArray elements = new JArray(Elements);
                JArray data = new JArray()
                {
                    ID,
                    Text,
                    Type,
                    Color,
                    Column,
                    Scale,
                    Checked,
                    elements
                };
                return data;
            }
            public JArray getJsonArrAsync()
            {
                JArray elements = new JArray(Elements);
                JArray data = new JArray()
                {
                    ID,
                    Text,
                    Type,
                    Color,
                    Column,
                    Scale,
                    Checked,
                    elements
                };
                return data;
            }
        }
        #region Enums
        public enum MenuItem
        {
            Void,
            Header,
            Card,
            Button,
            Checkbox,
            Input,
            List,

            gpsBtn,
            contactBtn,
            servicesBtn,
            homeBtn,
            grupBtn,
            hotelBtn,
            ilanBtn,
            closeBtn,
            businessBtn,
            adminBtn,
            lockBtn,
            leaveBtn,
            onRadio,
            offRadio,
            promoBtn

        }
        public enum MenuColor
        {
            White,
            Red,
            Green,
            Blue,
            Yellow,
            Orange,
            Teal,
            Cyan,
            Lime
        }
        #endregion
    }
}
