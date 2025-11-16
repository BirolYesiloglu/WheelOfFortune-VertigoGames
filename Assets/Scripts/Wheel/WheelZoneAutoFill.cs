using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using VertigoGames.Wheel.Data;

namespace VertigoGames.Wheel.Systems
{
    public static class WheelZoneAutoFill
    {
        public static void FillZone(WheelZoneSO zone)
        {
            var db = WheelSliceDatabase.Instance;

            if (db == null)
            {
                Debug.LogError("Slice Database not found! Please add a WheelSliceDatabase to the scene.");
                return;
            }

            zone.ClearSlices();

            // SUPER ZONE
            if (zone.IsSuperZone)
            {
                var highChests = db.ChestSlices.Where(c => c.RewardValue >= 3)
                                               .OrderBy(_ => Random.value)
                                               .Take(4);

                var highPoints = db.PointSlices.Where(p => p.RewardValue >= 15)
                                               .OrderBy(_ => Random.value)
                                               .Take(3);

                zone.AddSlices(highChests);
                zone.AddSlices(highPoints);

                var special = db.ChestSlices.FirstOrDefault(c => c.IsSpecial);
                if (special != null)
                    zone.AddSlice(special);

                return;
            }

            // SAFE ZONE
            if (zone.IsSafeZone)
            {
                var chests = db.ChestSlices.OrderBy(_ => Random.value).Take(2);
                var points = db.PointSlices.OrderBy(_ => Random.value).Take(6);

                zone.AddSlices(chests);
                zone.AddSlices(points);
                return;
            }

            // NORMAL ZONE
            zone.AddSlice(db.BombSlice);

            var pool = new List<WheelSliceSO>();
            pool.AddRange(db.PointSlices);
            pool.AddRange(db.ChestSlices);

            var selected = pool.OrderBy(_ => Random.value).Take(7);
            zone.AddSlices(selected);
        }
    }
}