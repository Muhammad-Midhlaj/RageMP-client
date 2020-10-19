using System.Collections.Generic;
using System;
using GTANetworkAPI;
using Newtonsoft.Json;
using NeptuneEvo.GUI;
using Redage.SDK;

namespace NeptuneEvo.Core
{
    class DrivingSchool : Script
    {
        // мотоциклы, легковые машины, грузовые, водный, вертолёты, самолёты
        private static List<int> LicPrices = new List<int>() { 600, 1000, 3000, 6000, 10000, 10000 };
        private static Vector3 enterSchool = new Vector3(-915.0237, -2038.613, 8.604773);
        private static List<Vector3> startCourseCoord = new List<Vector3>()
        {
            new Vector3(-893.7903, -2049.06, 8.886495),
        };
        private static List<Vector3> startCourseRot = new List<Vector3>()
        {
            new Vector3(-0.08991995, -0.000970318, 51.23025),
            new Vector3(-0.08991995, -0.000970318, 51.23025),
            new Vector3(-0.08991995, -0.000970318, 51.23025),
        };
        private static List<Vector3> drivingCoords = new List<Vector3>()
        {
            new Vector3(-922.5455, -2078.952, 8.884938),     //as1
            new Vector3(-943.9007, -2124.576, 8.903489),     //as2
            new Vector3(-1009.972, -2098.968, 12.68621),     //as3
            new Vector3(-1083.344, -1949.253, 12.72149),     //as4
            new Vector3(-893.1658, -1755.668, 18.6328),     //as5
            new Vector3(-802.588, -1660.319, 16.18098),     //as6
            new Vector3(-707.9262, -1529.791, 12.5376),     //as7
            new Vector3(-672.6287, -1470.063, 10.11085),     //as8
            new Vector3(-645.9832, -1395.972, 10.24782),     //as9
            new Vector3(-634.21, -1318.427, 10.24249),     //as10
            new Vector3(-674.2361, -1243.223, 10.25624),     //as11
            new Vector3(-719.2284, -1251.045, 8.799026),     //as12
            new Vector3(-704.8362, -1247.507, 9.798345),     //as13
            new Vector3(-656.5361, -1389.274, 10.16991),     //as14
            new Vector3(-696.2311, -1483.91, 10.59189),     //as15
            new Vector3(-761.0922, -1598.723, 14.0109),     //as16
            new Vector3(-848.3638, -1702.412, 18.45708),     //as17
            new Vector3(-997.0919, -1855.749, 17.40573),     //as18
            new Vector3(-1095.221, -1963.519, 12.68603),     //as19
            new Vector3(-1014.577, -2102.444, 12.75051),     //as20
            new Vector3(-910.9487, -2071.962, 8.884502),     //as21
        };

        private static nLog Log = new nLog("DrivingSc");

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                var shape = NAPI.ColShape.CreateCylinderColShape(enterSchool, 1, 2, 0);
                shape.OnEntityEnterColShape += onPlayerEnterSchool;
                shape.OnEntityExitColShape += onPlayerExitSchool;

                NAPI.Marker.CreateMarker(1, enterSchool - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(0, 255, 255));
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~g~Driving school"), new Vector3(enterSchool.X, enterSchool.Y, enterSchool.Z + 1), 5f, 0.3f, 0, new Color(255, 255, 255));
                var blip = NAPI.Blip.CreateBlip(enterSchool, 0);
                blip.ShortRange = true;
                blip.Name = Main.StringToU16("Driving school");
                blip.Sprite = 545;
                blip.Color = 29;
                for (int i = 0; i < drivingCoords.Count; i++)
                {
                    var colshape = NAPI.ColShape.CreateCylinderColShape(drivingCoords[i], 4, 5, 0);
                    colshape.OnEntityEnterColShape += onPlayerEnterDrive;
                    colshape.SetData("NUMBER", i);
                }
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void Event_OnPlayerExitVehicle(Player player, Vehicle vehicle)
        {
            try
            {
                if (player.HasData("SCHOOLVEH") && player.GetData<Vehicle>("SCHOOLVEH") == vehicle)
                {
                    //player.SetData("SCHOOL_TIMER", Main.StartT(60000, 99999999, (o) => timer_exitVehicle(player), "SCHOOL_TIMER"));
                    player.SetData("SCHOOL_TIMER", Timers.StartOnce(60000, () => timer_exitVehicle(player)));

                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"Если вы не сядете в машину в течение 60 секунд, то провалите экзамен", 3000);
                    return;
                }
            }
            catch (Exception e) { Log.Write("PlayerExitVehicle: " + e.Message, nLog.Type.Error); }
        }

        private void timer_exitVehicle(Player player)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (!Main.Players.ContainsKey(player)) return;
                    if (!player.HasData("SCHOOLVEH")) return;
                    if (player.IsInVehicle && player.Vehicle == player.GetData<Vehicle>("SCHOOLVEH")) return;
                    NAPI.Entity.DeleteEntity(player.GetData<Vehicle>("SCHOOLVEH"));
                    Trigger.ClientEvent(player, "deleteCheckpoint", 12, 0);
                    player.ResetData("IS_DRIVING");
                    player.ResetData("SCHOOLVEH");
                    //Main.StopT(player.GetData<string>("SCHOOL_TIMER"), "timer_36");
                    Timers.Stop(player.GetData<string>("SCHOOL_TIMER"));
                    player.ResetData("SCHOOL_TIMER");
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"You failed exmen", 3000);
                }
                catch (Exception e) { Log.Write("TimerDrivingSchool: " + e.Message, nLog.Type.Error); }
            });
        }

        public static void onPlayerDisconnected(Player player, DisconnectionType type, string reason)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (player.HasData("SCHOOLVEH")) NAPI.Entity.DeleteEntity(player.GetData<Vehicle>("SCHOOLVEH"));
                }
                catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
            }, 0);
        }
        public static void startDrivingCourse(Player player, int index)
        {
            if (player.HasData("IS_DRIVING") || player.GetData<bool>("ON_WORK"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You can't do it now", 3000);
                return;
            }
            if (Main.Players[player].Licenses[index])
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You already have this license", 3000);
                return;
            }
            switch (index)
            {
                case 0:
                    if (Main.Players[player].Money < LicPrices[0])
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You don't have enough money to buy this license", 3000);
                        return;
                    }
                    var vehicle = NAPI.Vehicle.CreateVehicle(VehicleHash.Bagger, startCourseCoord[0], startCourseRot[0], 30, 30);
                    player.SetIntoVehicle(vehicle, 0);
                    player.SetData("SCHOOLVEH", vehicle);
                    vehicle.SetData("ACCESS", "SCHOOL");
                    vehicle.SetData("DRIVER", player);
                    player.SetData("IS_DRIVING", true);
                    player.SetData("LICENSE", 0);
                    Trigger.ClientEvent(player, "createCheckpoint", 12, 1, drivingCoords[0] - new Vector3(0, 0, 2), 4, 0, 255, 0, 0);
                    Trigger.ClientEvent(player, "createWaypoint", drivingCoords[0].X, drivingCoords[0].Y);
                    player.SetData("CHECK", 0);
                    MoneySystem.Wallet.Change(player, -LicPrices[0]);
                    Fractions.Stocks.fracStocks[6].Money += LicPrices[0];
                    GameLog.Money($"player({Main.Players[player].UUID})", $"frac(6)", LicPrices[0], $"buyLic");
                    Core.VehicleStreaming.SetEngineState(vehicle, false);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"To start the vehicle, press B", 3000);
                    return;
                case 1:
                    if (Main.Players[player].Money < LicPrices[1])
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You don't have enough money to buy this license", 3000);
                        return;
                    }
                    vehicle = NAPI.Vehicle.CreateVehicle(VehicleHash.Dilettante, startCourseCoord[0], startCourseRot[0], 30, 30);
                    player.SetIntoVehicle(vehicle, 0);
                    player.SetData("SCHOOLVEH", vehicle);
                    vehicle.SetData("ACCESS", "SCHOOL");
                    vehicle.SetData("DRIVER", player);
                    player.SetData("IS_DRIVING", true);
                    player.SetData("LICENSE", 1);
                    Trigger.ClientEvent(player, "createCheckpoint", 12, 1, drivingCoords[0] - new Vector3(0, 0, 2), 4, 0, 255, 0, 0);
                    Trigger.ClientEvent(player, "createWaypoint", drivingCoords[0].X, drivingCoords[0].Y);
                    player.SetData("CHECK", 0);
                    MoneySystem.Wallet.Change(player, -LicPrices[1]);
                    Fractions.Stocks.fracStocks[6].Money += LicPrices[1];
                    GameLog.Money($"player({Main.Players[player].UUID})", $"frac(6)", LicPrices[1], $"buyLic");
                    Core.VehicleStreaming.SetEngineState(vehicle, false);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"To start the vehicle, press B", 3000);
                    return;
                case 2:
                    if (Main.Players[player].Money < LicPrices[2])
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You don't have enough money to buy this license", 3000);
                        return;
                    }
                    vehicle = NAPI.Vehicle.CreateVehicle(VehicleHash.Flatbed, startCourseCoord[0], startCourseRot[0], 30, 30);
                    player.SetIntoVehicle(vehicle, 0);
                    player.SetData("SCHOOLVEH", vehicle);
                    vehicle.SetData("ACCESS", "SCHOOL");
                    vehicle.SetData("DRIVER", player);
                    player.SetData("IS_DRIVING", true);
                    player.SetData("LICENSE", 2);
                    Trigger.ClientEvent(player, "createCheckpoint", 12, 1, drivingCoords[0] - new Vector3(0, 0, 2), 4, 0, 255, 0, 0);
                    Trigger.ClientEvent(player, "createWaypoint", drivingCoords[0].X, drivingCoords[0].Y);
                    player.SetData("CHECK", 0);
                    MoneySystem.Wallet.Change(player, -LicPrices[2]);
                    Fractions.Stocks.fracStocks[6].Money += LicPrices[2];
                    GameLog.Money($"player({Main.Players[player].UUID})", $"frac(6)", LicPrices[2], $"buyLic");
                    Core.VehicleStreaming.SetEngineState(vehicle, false);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"To start the vehicle, press B", 3000);
                    return;
                case 3:
                    if (Main.Players[player].Money < LicPrices[3])
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You don't have enough money to buy this license", 3000);
                        return;
                    }
                    Main.Players[player].Licenses[3] = true;
                    MoneySystem.Wallet.Change(player, -LicPrices[3]);
                    Fractions.Stocks.fracStocks[6].Money += LicPrices[3];
                    GameLog.Money($"player({Main.Players[player].UUID})", $"frac(6)", LicPrices[3], $"buyLic");
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You have successfully purchased a water transport license", 3000);
                    Dashboard.sendStats(player);
                    return;
                case 4:
                    if (Main.Players[player].Money < LicPrices[4])
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"", 3000);
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You don't have enough money to buy this license", 3000);
                        return;
                    }
                    Main.Players[player].Licenses[4] = true;
                    MoneySystem.Wallet.Change(player, -LicPrices[4]);
                    Fractions.Stocks.fracStocks[6].Money += LicPrices[4];
                    GameLog.Money($"player({Main.Players[player].UUID})", $"frac(6)", LicPrices[4], $"buyLic");
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You have successfully purchased a helicopter flying license", 3000);
                    Dashboard.sendStats(player);
                    return;
                case 5:
                    if (Main.Players[player].Money < LicPrices[5])
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You don't have enough money to buy this license", 3000);
                        return;
                    }
                    Main.Players[player].Licenses[5] = true;
                    MoneySystem.Wallet.Change(player, -LicPrices[5]);
                    Fractions.Stocks.fracStocks[6].Money += LicPrices[5];
                    GameLog.Money($"player({Main.Players[player].UUID})", $"frac(6)", LicPrices[5], $"buyLic");
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You have successfully purchased an aircraft control license", 3000);
                    Dashboard.sendStats(player);
                    return;
            }
        }
        private void onPlayerEnterSchool(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 39);
            }
            catch (Exception e) { Log.Write("onPlayerEnterSchool: " + e.ToString(), nLog.Type.Error); }
        }
        private void onPlayerExitSchool(ColShape shape, Player player)
        {
            NAPI.Data.SetEntityData(player, "INTERACTIONCHECK", 0);
        }
        private void onPlayerEnterDrive(ColShape shape, Player player)
        {
            try
            {
                if (!player.IsInVehicle || player.VehicleSeat != 0) return;
                if (!player.Vehicle.HasData("ACCESS") || player.Vehicle.GetData<string>("ACCESS") != "SCHOOL") return;
                if (!player.HasData("IS_DRIVING")) return;
                if (player.Vehicle != player.GetData<Vehicle>("SCHOOLVEH")) return;
                if (shape.GetData<int>("NUMBER") != player.GetData<int>("CHECK")) return;
                //Trigger.ClientEvent(player, "deleteCheckpoint", 12, 0);
                var check = player.GetData<int>("CHECK");
                if (check == drivingCoords.Count - 1)
                {
                    player.ResetData("IS_DRIVING");
                    var vehHP = player.Vehicle.Health;
                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            NAPI.Entity.DeleteEntity(player.Vehicle);
                        }
                        catch { }
                    });
                    player.ResetData("SCHOOLVEH");
                    if (vehHP < 500)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You failed the exam", 3000);
                        return;
                    }
                    Main.Players[player].Licenses[player.GetData<int>("LICENSE")] = true;
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You passed the exam successfully", 3000);
                    Dashboard.sendStats(player);
                    Trigger.ClientEvent(player, "deleteCheckpoint", 12, 0);
                    return;
                }

                player.SetData("CHECK", check + 1);
                if (check + 2 < drivingCoords.Count)
                    Trigger.ClientEvent(player, "createCheckpoint", 12, 1, drivingCoords[check + 1] - new Vector3(0, 0, 2), 4, 0, 255, 0, 0, drivingCoords[check + 2] - new Vector3(0, 0, 1.12));
                else
                    Trigger.ClientEvent(player, "createCheckpoint", 12, 1, drivingCoords[check + 1] - new Vector3(0, 0, 2), 4, 0, 255, 0, 0);
                Trigger.ClientEvent(player, "createWaypoint", drivingCoords[check + 1].X, drivingCoords[check + 1].Y);
            }
            catch (Exception e)
            {
                Log.Write("ENTERDRIVE:\n" + e.ToString(), nLog.Type.Error);
            }
        }

        #region menu
        public static void OpenDriveSchoolMenu(Player player)
        {
            Menu menu = new Menu("driveschool", false, false);
            menu.Callback = callback_driveschool;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Licenses";
            menu.Add(menuItem);

            menuItem = new Menu.Item("lic_0", Menu.MenuItem.Button);
            menuItem.Text = $"(A)Motorcycles - {LicPrices[0]}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("lic_1", Menu.MenuItem.Button);
            menuItem.Text = $"(B) Cars - {LicPrices[1]}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("lic_2", Menu.MenuItem.Button);
            menuItem.Text = $"(C) Trucks - {LicPrices[2]}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("lic_3", Menu.MenuItem.Button);
            menuItem.Text = $"(V) Water transport - {LicPrices[3]}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("lic_4", Menu.MenuItem.Button);
            menuItem.Text = $"(LV) Helicopters - {LicPrices[4]}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("lic_5", Menu.MenuItem.Button);
            menuItem.Text = $"(LS) Aircraft - {LicPrices[5]}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("close", Menu.MenuItem.Button);
            menuItem.Text = "Close";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_driveschool(Player client, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            MenuManager.Close(client);
            if (item.ID == "close") return;
            var id = item.ID.Split('_')[1];
            startDrivingCourse(client, Convert.ToInt32(id));
            return;
        }
        #endregion
    }
}