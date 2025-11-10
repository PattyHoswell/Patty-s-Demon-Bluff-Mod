using System;
using System.Collections.Generic;
using UnityEngine;

namespace Patty_CustomScenario_MOD.QoL
{
    public static class YieldCache
    {
        public static readonly Lazy<WaitForEndOfFrame> WaitForEndOfFrame = new Lazy<WaitForEndOfFrame>(isThreadSafe: true);
        public static readonly Lazy<WaitForFixedUpdate> WaitForFixedUpdate = new Lazy<WaitForFixedUpdate>(isThreadSafe: true);

        public static readonly Dictionary<float, WaitForSeconds> WaitForSecondsDict = new Dictionary<float, WaitForSeconds>();
        public static readonly Dictionary<float, WaitForSecondsRealtime> WaitForSecondsRealtimeDict = new Dictionary<float, WaitForSecondsRealtime>();

        public static WaitForSeconds GetWaitForSeconds(float seconds)
        {
            WaitForSeconds result = null;
            if (!WaitForSecondsDict.TryGetValue(seconds, out result))
            {
                result = new WaitForSeconds(seconds);
                WaitForSecondsDict[seconds] = result;
            }
            return result;
        }

        public static WaitForSecondsRealtime GetWaitForSecondsRealtime(float seconds)
        {
            WaitForSecondsRealtime result = null;
            if (!WaitForSecondsRealtimeDict.TryGetValue(seconds, out result))
            {
                result = new WaitForSecondsRealtime(seconds);
                WaitForSecondsRealtimeDict[seconds] = result;
            }
            return result;
        }
    }
}
