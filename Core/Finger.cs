using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeptuneEvo.Core
{
    class Fingerpointing : Script
    {
        [RemoteEvent("fpsync.update")]
        public void FingerSyncUpdate(Player client, float camPitch, float camHeading)
        {
            NAPI.ClientEvent.TriggerClientEventInRange(client.Position, 100f, "fpsync.update", client.Value, camPitch, camHeading);
        }
        [RemoteEvent("pointingStop")]
        public void FingerStop(Player client)
        {
            client.StopAnimation();
            client.StopAnimation();
        }
    }
}
