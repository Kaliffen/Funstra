using System;
using UnityEngine;

namespace Funstra
{
    public sealed class CargoSite
    {
        public readonly string name;
        public readonly Vector3 position;
        public readonly int value, weight;
        public readonly float seconds;
        public readonly bool alarm;

        public CargoSite(string name, Vector3 position, int value, int weight, float seconds, bool alarm)
        {
            this.name = name; this.position = position; this.value = value;
            this.weight = weight; this.seconds = seconds; this.alarm = alarm;
        }
    }

    // Cargo is deliberately transient: only extracted earnings enter the save file.
    public sealed class CargoRun
    {
        public static readonly CargoSite[] Sites = {
            new CargoSite("Arcade spares", new Vector3(40, 0, -11), 60, 2, 1.5f, false),
            new CargoSite("Garage tools", new Vector3(-39, 0, 15), 110, 3, 2.5f, false),
            new CargoSite("Bonded cargo", new Vector3(39, 0, 38), 220, 5, 4f, true)
        };

        readonly bool[] taken = new bool[Sites.Length];
        public int Weight { get; private set; }
        public int Value { get; private set; }
        public int Count { get; private set; }
        public float SpeedMultiplier => Weight >= 5 ? .8f : 1f;

        public bool Taken(int index) => index >= 0 && index < taken.Length && taken[index];

        public bool Take(int index, RunState state)
        {
            if (state == null || index < 0 || index >= taken.Length || taken[index]) return false;
            var site = Sites[index];
            if (Weight + site.weight > state.CargoCapacity) return false;
            taken[index] = true;
            Weight += site.weight;
            Value += site.value;
            Count++;
            return true;
        }

        public void Lose()
        {
            Array.Clear(taken, 0, taken.Length);
            Weight = Value = Count = 0;
        }

        public int Bank(RunState state, float heat)
        {
            if (state == null || heat != 0 || Count == 0) return 0;
            int earned = Value;
            state.cash += earned;
            state.cargoRuns++;
            state.cargoEarnings += earned;
            Lose();
            return earned;
        }
    }
}
