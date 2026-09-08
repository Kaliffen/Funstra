using System;
using System.IO;
using UnityEngine;

namespace Funstra
{
    [Serializable]
    public sealed class RunState
    {
        public int version = 2;
        public DistrictState district = new DistrictState();
        public int completed;
        public int cash;
        public int arrests;
        public int perks;
        public bool accepted;
        public bool satchel;
        public int cargoRuns;
        public int cargoEarnings;
        [NonSerialized] public bool carrying;
        public int CargoCapacity => satchel ? 9 : 6;
        public bool Finished => completed >= 3;
        public int PerkCount => ((perks & 1) != 0 ? 1 : 0) + ((perks & 2) != 0 ? 1 : 0) + ((perks & 4) != 0 ? 1 : 0);
        public bool NeedsPerk => PerkCount < Mathf.Min(completed, 2);
        public bool HasPerk(int index) => (perks & (1 << index)) != 0;
        public void Accept() { if (!Finished && !NeedsPerk) accepted = true; }
        public bool Steal()
        {
            if (!accepted || carrying || Finished) return false;
            carrying = true;
            return true;
        }
        public bool Deliver(float heat)
        {
            if (!carrying || heat > 0 || Finished) return false;
            cash += Jobs.All[completed].reward;
            completed++;
            accepted = carrying = false;
            return true;
        }
        public bool ChoosePerk(int index)
        {
            if (index < 0 || index > 2 || !NeedsPerk || HasPerk(index)) return false;
            perks |= 1 << index;
            return true;
        }
        public void Arrest()
        {
            arrests++;
            cash = Mathf.Max(0, cash - 40);
            carrying = false;
        }
        public bool BuySatchel()
        {
            if (satchel || cash < 180) return false;
            cash -= 180;
            satchel = true;
            return true;
        }
        public static RunState Load(string path)
        {
            try
            {
                if (!File.Exists(path)) return null;
                var state = JsonUtility.FromJson<RunState>(File.ReadAllText(path));
                if (state == null || (state.version != 1 && state.version != 2) || state.completed < 0 || state.completed > 3 || state.cash < 0 || state.arrests < 0 || state.perks < 0 || state.perks > 7 || state.PerkCount > Mathf.Min(state.completed, 2) || state.cargoRuns < 0 || state.cargoEarnings < 0) return null;
                if(state.version==1) { state.district=new DistrictState();state.version=2; }
                if(state.district==null||!state.district.Valid())return null;
                state.carrying = false;
                return state;
            }
            catch (Exception) { return null; }
        }
        public bool Save(string path)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                if(File.Exists(path)&&!File.Exists(path+".v1-backup"))
                {
                    var old=JsonUtility.FromJson<RunState>(File.ReadAllText(path));
                    if(old!=null&&old.version==1)File.Copy(path,path+".v1-backup");
                }
                File.WriteAllText(path + ".tmp", JsonUtility.ToJson(this, true));
                if (File.Exists(path)) File.Replace(path + ".tmp", path, null);
                else File.Move(path + ".tmp", path);
                return true;
            }
            catch (Exception e) { Debug.LogWarning("Could not save progress: " + e.Message); return false; }
        }
    }

    public sealed class Job
    {
        public string title, item, district, dialogue;
        public Vector3 position;
        public int reward;
        public float seconds;
        public Job(string title, string item, string district, Vector3 position, int reward, float seconds, string dialogue)
        { this.title = title; this.item = item; this.district = district; this.position = position; this.reward = reward; this.seconds = seconds; this.dialogue = dialogue; }
    }
    public static class Jobs
    {
        public static readonly Vector3 Home = new Vector3(-11, 0, -48);
        public static readonly Vector3 Mara = new Vector3(-17, 0, -35);
        public static readonly Job[] All = {
            new Job("BORROWED TIME", "Courier bag", "THE ARCADE", new Vector3(32, 0, -12), 120, 1.4f,
                "Welcome to Funstra. Someone left my delivery behind the arcade. Bring it here. Keep the blue coats out of it."),
            new Job("THE BACK DOOR", "Garage lockbox", "VICO'S GARAGE", new Vector3(-31, 0, 14), 220, 4f,
                "Vico keeps our money in a box behind his garage. Consider this a withdrawal. If anyone sees you, use the alleys."),
            new Job("PAPER TRAIL", "Depot ledger", "NORTH DOCK", new Vector3(29, 0, 39), 400, 5f,
                "One last favor. The depot ledger has names that shouldn't be there. It has an alarm. Lose the patrols and bring it back. Then we're square.")
        };
    }
}
