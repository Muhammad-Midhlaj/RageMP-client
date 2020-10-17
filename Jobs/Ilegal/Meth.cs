using GTANetworkAPI;
using System;
using System.Linq;
using System.Collections.Generic;
using NeptuneEvo.Core;
using Redage.SDK;
using NeptuneEvo.GUI;
using Newtonsoft.Json;
using NeptuneEvo;

class Meth : Script
{
    public class MethEnum : IEquatable<MethEnum>

    {
        public int id { get; set; }

        public Entity objectHandle { get; set; }
        public TextLabel textLabel { get; set; }
        public Vector3 position { get; set; }
        public TimerEx timer { get; set; }
        public int stage { get; set; }
        public int downtime { get; set; }
        public override int GetHashCode()
        {
            return id;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            MethEnum objAsPart = obj as MethEnum;
            if (objAsPart == null) return false;
            else return Equals(objAsPart);
        }

        public bool Equals(MethEnum other)
        {
            if (other == null) return false;
            return (this.id.Equals(other.id));
        }
    }

    public static List<MethEnum> MethList = new List<MethEnum>();

    [ServerEvent(Event.ResourceStart)]
    public void onResourceStart()
    {

        Blip temp_blip = NAPI.Blip.CreateBlip(new Vector3(3392.58, 5499.587, 24.23199));
        NAPI.Blip.SetBlipName(temp_blip, "Урожай опиума");
        NAPI.Blip.SetBlipSprite(temp_blip, 468);
        NAPI.Blip.SetBlipColor(temp_blip, 59);
        NAPI.Blip.SetBlipScale(temp_blip, 1.0f);
        NAPI.Blip.SetBlipShortRange(temp_blip, true);

        API.Shared.DeleteWorldProp(-596943609, new Vector3(3391.689, 5504.36, 23.9412), 40f); // tenda

        API.Shared.DeleteWorldProp(-1572018818, new Vector3(3391.689, 5504.36, 23.9412), 40f); // table


        //23
        MethList.Add(new MethEnum { position = new Vector3(-1730.396, 2662.546, 2.999999), stage = 0 }); // 0, 0, 266.1345
        MethList.Add(new MethEnum { position = new Vector3(-1760.848, 2583.384, 3.248314), stage = 0 }); // 0, 0, 248.2544
        MethList.Add(new MethEnum { position = new Vector3(-2557.167, 3337.08, 30.4498), stage = 0 }); // 0, 0, 323.119
        MethList.Add(new MethEnum { position = new Vector3(-2058.068, 5256.291, 16.99559), stage = 0 }); // 0, 0, 36.05629
        MethList.Add(new MethEnum { position = new Vector3(-1238.909, 4812.666, 200.6745), stage = 0 }); // 0, 0, 70.98997
        MethList.Add(new MethEnum { position = new Vector3(-623.3401, 5305.432, 60.24689), stage = 0 }); // 0, 0, 10.06372
        MethList.Add(new MethEnum { position = new Vector3(-330.4768, 5297.932, 150.1182), stage = 0 }); // 0, 0, 279.5106
        MethList.Add(new MethEnum { position = new Vector3(120.1758, 6049.529, 185.5762), stage = 0 }); // 0, 0, 264.478
        MethList.Add(new MethEnum { position = new Vector3(1507.984, 6301.293, 36.26267), stage = 0 }); // 0, 0, 72.4735
        MethList.Add(new MethEnum { position = new Vector3(2605.425, 6583.155, 16.37092), stage = 0 }); // 0, 0, 109.7814
        MethList.Add(new MethEnum { position = new Vector3(3151.438, 5562.319, 170.3736), stage = 0 }); // 0, 0, 162.2165
        MethList.Add(new MethEnum { position = new Vector3(1893.786, 5057.833, 50.65946), stage = 0 }); // 0, 0, 120.3747
        MethList.Add(new MethEnum { position = new Vector3(1639.568, 4783.887, 46.26814), stage = 0 }); // 0, 0, 42.05631
        /*MethList.Add(new MethEnum { position = new Vector3(3379.939, 5508.705, 22.44706), stage = 0 }); // 0, 0, 108.2324
        MethList.Add(new MethEnum { position = new Vector3(3384.936, 5501.328, 23.84397), stage = 0 }); // 0, 0, 224.2606
        MethList.Add(new MethEnum { position = new Vector3(3377.716, 5524.121, 21.26247), stage = 0 }); // 0, 0, 36.17857
        MethList.Add(new MethEnum { position = new Vector3(3388.63, 5531.151, 18.53586), stage = 0 }); // 0, 0, 305.4675
        MethList.Add(new MethEnum { position = new Vector3(3399.502, 5532.924, 16.88271), stage = 0 }); // 0, 0, 264.2496
        MethList.Add(new MethEnum { position = new Vector3(3408.657, 5536.813, 15.08692), stage = 0 }); // 0, 0, 295.8766
        MethList.Add(new MethEnum { position = new Vector3(3419.742, 5532.726, 16.50706), stage = 0 }); // 0, 0, 246.5864
        MethList.Add(new MethEnum { position = new Vector3(3436.655, 5517.308, 20.10886), stage = 0 }); // 0, 0, 233.5347
        MethList.Add(new MethEnum { position = new Vector3(3451.692, 5514.224, 18.76247), stage = 0 }); // 0, 0, 268.9109
        MethList.Add(new MethEnum { position = new Vector3(3462.616, 5511.243, 17.67923), stage = 0 }); // 0, 0, 239.8521
        MethList.Add(new MethEnum { position = new Vector3(3463.126, 5494.436, 16.78993), stage = 0 }); //*/

        API.Shared.CreateObject(-1814952641, new Vector3(3391.689, 5504.36, 23.9412 - 1.2), new Vector3(0, 0, 0), 255, 0);

        foreach (var weed in MethList)
        {
            weed.downtime = 10 * 60;
            weed.timer = null;
            weed.objectHandle = API.Shared.CreateObject(-2093428068, new Vector3(weed.position.X, weed.position.Y, weed.position.Z - 1.2f), new Vector3(), 255, 0);
            weed.textLabel = API.Shared.CreateTextLabel("~h~~g~-~y~Листья коки~g~ -~w~~n~~n~Нажмите ~y~Y~w~ чтобы забрать", new Vector3(weed.position.X, weed.position.Y, weed.position.Z - 0.4f), 11.0f, 0.3f, 4, new Color(255, 255, 255, 255), false, 0);
        }

        ColShape opium = NAPI.ColShape.CreatCircleColShape(3392.58f, 5499.587f, 100, 0);
        opium.OnEntityEnterColShape += (s, ent) =>
        {
            Player player;

            if ((player = NAPI.Player.GetPlayerFromHandle(ent)) != null)
            {
                if (player.GetData<bool>("status") == false) return;
                int index = 0;
                foreach (var weed in MethList)
                {
                    if (weed.stage == 0)
                    {
                        player.TriggerEvent("blip_create_ext", "opium_" + index + "", weed.position, 59, 0.5f, 468, true, "Folhas de Coca");
                    }
                    index++;
                }
                player.SetData("in_meth_area", true);
            }
        };

        opium.OnEntityExitColShape += (s, ent) =>
        {
            Player player;

            if ((player = NAPI.Player.GetPlayerFromHandle(ent)) != null)
            {
                if (player.GetData<bool>("status") == false) return;
                int index = 0;
                foreach (var weed in MethList)
                {
                    player.TriggerEvent("blip_remove", "opium_" + index + "");
                    index++;
                }
                player.SetData("in_meth_area", false);
            }
        };
    }



    /*public static void KeyPressY(Player player)
    {
        int index = 0;
        foreach (var weed in MethList)
        {
            if (Main.IsInRangeOfPoint(player.Position, weed.position, 1.0f) && weed.stage == 0)

            {

                weed.stage = 1;

                nInventory.Add(player, new nItem(ItemType.Coca, 1));


                player.TriggerEvent("createNewHeadNotificationAdvanced", "Voce colheu ~y~1~y~ folha de coca~w~ !");

                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Você colheu " + "1" + " " + "folhas de coca" + ".", 3000);

                Main.SendNotificationBrowser(player, "", "<string>+ 1</strong> La hoja se ingreso al inventario!", "success");

                NAPI.Player.PlayPlayerAnimation(player, (int)(Main.AnimationFlags.Loop), "anim@mp_snowball", "pickup_snowball");

                NAPI.Task.Run(() =>
                {
                    player.StopAnimation();
                    weed.objectHandle.Position = new Vector3(weed.position.X, weed.position.Y, weed.position.Z - 2.8f);

                    foreach (var target in NAPI.Pools.GetAllPlayers()) if (target.GetData<bool>("status") == true && target.GetData("in_meth_area") == true) target.TriggerEvent("blip_remove", "opium_" + index + "");
                }, delayTime: 1500);


                weed.timer = TimerEx.SetTimer(() => {
                    weed.downtime--;

                    weed.textLabel.Text = "~h~~g~-~y~Hojas de coca~g~ -~w~~n~~n~Tiempo de sembrado em: ~y~" + weed.downtime;

                    switch (weed.downtime)
                    {
                        //Sould be done clientside to 1.1
                        case 590: weed.objectHandle.MovePosition(new Vector3(weed.position.X, weed.position.Y, weed.position.Z - 2.5f), 1000); break;
                        case 550: weed.objectHandle.MovePosition(new Vector3(weed.position.X, weed.position.Y, weed.position.Z - 2.4f), 1000); break;
                        case 500: weed.objectHandle.MovePosition(new Vector3(weed.position.X, weed.position.Y, weed.position.Z - 2.3f), 1000); break;
                        case 450: weed.objectHandle.MovePosition(new Vector3(weed.position.X, weed.position.Y, weed.position.Z - 2.2f), 1000); break;
                        case 400: weed.objectHandle.MovePosition(new Vector3(weed.position.X, weed.position.Y, weed.position.Z - 2.1f), 1000); break;
                        case 350: weed.objectHandle.MovePosition(new Vector3(weed.position.X, weed.position.Y, weed.position.Z - 2.0f), 1000); break;
                        case 300: weed.objectHandle.MovePosition(new Vector3(weed.position.X, weed.position.Y, weed.position.Z - 1.9f), 1000); break;
                        case 250: weed.objectHandle.MovePosition(new Vector3(weed.position.X, weed.position.Y, weed.position.Z - 1.8f), 1000); break;
                        case 200: weed.objectHandle.MovePosition(new Vector3(weed.position.X, weed.position.Y, weed.position.Z - 1.7f), 1000); break;
                        case 150: weed.objectHandle.MovePosition(new Vector3(weed.position.X, weed.position.Y, weed.position.Z - 1.6f), 1000); break;
                        case 100: weed.objectHandle.MovePosition(new Vector3(weed.position.X, weed.position.Y, weed.position.Z - 1.5f), 1000); break;
                        case 50: weed.objectHandle.MovePosition(new Vector3(weed.position.X, weed.position.Y, weed.position.Z - 1.4f), 1000); break;
                        case 30: weed.objectHandle.MovePosition(new Vector3(weed.position.X, weed.position.Y, weed.position.Z - 1.3f), 1000); break;
                        case 20: weed.objectHandle.MovePosition(new Vector3(weed.position.X, weed.position.Y, weed.position.Z - 1.2f), 1000); break;
                        case 10: weed.objectHandle.MovePosition(new Vector3(weed.position.X, weed.position.Y, weed.position.Z - 1.1f), 1000); break;
                        case 0: weed.objectHandle.MovePosition(new Vector3(weed.position.X, weed.position.Y, weed.position.Z - 1.0f), 1000); break;
                    }

                    if (weed.downtime == 0)
                    {
                        weed.downtime = 10 * 60;
                        weed.stage = 0;
                        weed.textLabel.Text = "~h~~g~-~y~Hojas de coca Coca~g~ -~w~~n~~n~Presione ~y~Y~w~ para recoger";
                        weed.timer.Kill();

                        foreach (var target in NAPI.Pools.GetAllPlayers()) if (target.GetData<bool>("status") == true && target.GetData("in_meth_area") == true) target.TriggerEvent("blip_create_ext", "opium_" + index + "", weed.position, 59, 0.5f, 468, true, "Folhas de Coca");
                    }

                }, 1000, 0);
                return;
            }
            index++;

        }
    }*/


}

