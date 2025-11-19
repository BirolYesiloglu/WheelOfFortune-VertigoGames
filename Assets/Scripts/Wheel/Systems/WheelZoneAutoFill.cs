using UnityEngine;
using System.Linq;
using VertigoGames.Wheel.Data;

namespace VertigoGames.Wheel.Systems
{
    public class WheelZoneAutoFill : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] private WheelSliceDatabase _database;

        [Header("Bronze Zone Settings")]
        [SerializeField] private int bronzePointCount = 5;
        [SerializeField] private int bronzeChestCount = 2;
        [SerializeField] private bool bronzeHasBomb = true;

        [Header("Silver Zone Settings (Safe Zones)")]
        [SerializeField] private int silverPointCount = 3;
        [SerializeField] private int silverChestCount = 5;
        [SerializeField] private bool silverHasBomb = false;

        [Header("Gold Zone Settings (Super Zone)")]
        [SerializeField] private int goldHighChestCount = 5;
        [SerializeField] private int goldHighPointCount = 2;
        [SerializeField] private bool goldAddSpecialChest = true;
        [SerializeField] private bool goldHasBomb = false;


        public void FillZone(WheelZoneSO zone)
        {;
            zone.ClearSlices();

            // GOLD ZONE
            if (zone.IsSuperZone)
            {
                FillGoldZone(zone, _database);
                return;
            }

            // SILVER ZONE
            if (zone.IsSafeZone)
            {
                FillSilverZone(zone, _database);
                return;
            }

            // BRONZE ZONE
            FillBronzeZone(zone, _database);
        }


        private void FillBronzeZone(WheelZoneSO zone, WheelSliceDatabase db)
        {
            if (bronzeHasBomb)
                zone.AddSlice(db.BombSlice);

            var points = db.PointSlices.OrderBy(_ => Random.value).Take(bronzePointCount);
            var chests = db.ChestSlices.OrderBy(_ => Random.value).Take(bronzeChestCount);

            zone.AddSlices(points);
            zone.AddSlices(chests);
        }


        private void FillSilverZone(WheelZoneSO zone, WheelSliceDatabase db)
        {
            if (silverHasBomb)
                zone.AddSlice(db.BombSlice);

            var points = db.PointSlices.OrderBy(_ => Random.value).Take(silverPointCount);
            var chests = db.ChestSlices.OrderBy(_ => Random.value).Take(silverChestCount);

            zone.AddSlices(points);
            zone.AddSlices(chests);
        }


        private void FillGoldZone(WheelZoneSO zone, WheelSliceDatabase db)
        {
            if (goldHasBomb)
                zone.AddSlice(db.BombSlice);

            var highChests = db.ChestSlices
                .Where(c => c.RewardValue >= 3)
                .OrderBy(_ => Random.value)
                .Take(goldHighChestCount);

            var highPoints = db.PointSlices
                .Where(p => p.RewardValue >= 15)
                .OrderBy(_ => Random.value)
                .Take(goldHighPointCount);

            zone.AddSlices(highChests);
            zone.AddSlices(highPoints);

            if (goldAddSpecialChest)
            {
                var special = db.ChestSlices.FirstOrDefault(c => c.IsSpecial);
                if (special != null)
                    zone.AddSlice(special);
            }
        }
    }
}