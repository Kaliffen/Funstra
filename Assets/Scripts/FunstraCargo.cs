using System.Collections.Generic;
using UnityEngine;

namespace Funstra
{
    public sealed partial class FunstraGame
    {
        public CargoRun Cargo { get; private set; } = new CargoRun();
        public bool Extracting { get; private set; }
        float cargoProgress, extractionProgress;
        int cargoTarget = -1;
        Vector3 extractionAnchor;
        readonly List<GameObject> cargoProps = new List<GameObject>();
        readonly Color cargoColor = CityArt.Hex("C8A2FF");

        void BuildCargoArt()
        {
            foreach(var site in CargoRun.Sites)
            {
                var group = new GameObject("Hot cargo / " + site.name);
                group.transform.position = site.position;
                City.Box("Pallet", new Vector3(0,.12f,0), new Vector3(1.65f,.22f,1.3f), CityArt.Hex("66524C"),group.transform);
                City.Prop("Sealed cargo", new Vector3(0,.65f,0), new Vector3(1.25f,.85f,1), CityArt.Hex("675579"),group.transform);
                for(int side=-1;side<=1;side+=2)
                    City.Box("Cargo band",new Vector3(side*.4f,.65f,0),new Vector3(.08f,.9f,1.04f),cargoColor,group.transform,false,true);
                if(site.alarm) City.Box("Alarm beacon",new Vector3(0,1.18f,0),new Vector3(.25f,.16f,.25f),CityArt.Red,group.transform,false,true);
                var ring=City.Ring("Cargo pickup",site.position,1.8f,cargoColor);
                ring.transform.SetParent(group.transform,true);
                cargoProps.Add(group);
            }
            City.Ring("Safehouse extraction",Jobs.Home,2.7f,CityArt.Mint);
            City.Solid("Safehouse drop chest",Jobs.Home+new Vector3(0,.55f,2.5f),new Vector3(1.6f,1.1f,.8f),CityArt.Hex("304C50"));
            City.Box("Safehouse chest stripe",Jobs.Home+new Vector3(0,.95f,2.06f),new Vector3(1.4f,.14f,.05f),CityArt.Mint,null,false,true);
            City.Sign("HOME",Jobs.Home+new Vector3(0,1.5f,2.5f),CityArt.Mint,.13f);
        }

        void RefreshCargoArt()
        { for(int i=0;i<cargoProps.Count;i++)cargoProps[i].SetActive(!Cargo.Taken(i));City.RefreshProps(); }

        void ResetCargoInteraction()
        { cargoProgress=extractionProgress=0; cargoTarget=-1; Extracting=false; }

        bool UpdateCargoInteraction(float dt,bool held)
        {
            Extracting=false;
            if(Vector3.Distance(Player.position,Jobs.Home)<2.8f)
            {
                cargoTarget=-1; cargoProgress=0;
                if(Heat>0) { extractionProgress=0; prompt="SAFEHOUSE CLOSED  /  LOSE THE HEAT FIRST"; return true; }
                if(Input.GetKeyDown(KeyCode.F)) { OpenSafehouse(); return true; }
                if(Cargo.Value==0) { extractionProgress=0; prompt="F  /  SAFEHOUSE & SATCHEL   |   PURPLE CRATES = OPTIONAL CARGO"; return true; }
                prompt="HOLD E  /  BANK $"+Cargo.Value+"   |   STAND STILL 3s   |   F  UPGRADES";
                if(!held) { extractionProgress=0; return true; }
                if(extractionProgress==0)extractionAnchor=Player.position;
                if(Vector3.Distance(extractionAnchor,Player.position)>.2f) { extractionProgress=0; return true; }
                Extracting=true; extractionProgress+=dt/3f;
                if(extractionProgress>=1)
                {
                    int secured=Cargo.Bank(State,Heat);
                    ResetCargoInteraction(); RefreshCargoArt(); Save(); Sound(cashSound);
                    Notify("$"+secured+" secured. Cargo restocked. F at home to upgrade your satchel.");
                }
                return true;
            }
            extractionProgress=0;
            for(int i=0;i<CargoRun.Sites.Length;i++)
            {
                var site=CargoRun.Sites[i];
                if(Cargo.Taken(i)||Vector3.Distance(Player.position,site.position)>=2.6f)continue;
                if(cargoTarget!=i) { cargoTarget=i; cargoProgress=0; }
                if(Cargo.Weight+site.weight>State.CargoCapacity)
                { cargoProgress=0;prompt="BAG TOO FULL  /  NEED "+site.weight+" SPACE   |   BANK YOUR HAUL AT HOME";return true; }
                prompt="HOLD E  /  "+site.name.ToUpperInvariant()+"   $"+site.value+"   "+site.weight+" LOAD"+(site.alarm?"   /   ALARMED":"");
                if(!held) { cargoProgress=0; return true; }
                Stealing=true; Hidden=false;
                cargoProgress+=dt/(site.seconds*(State.HasPerk(2)?.6f:1));
                if(cargoProgress>=1 && Cargo.Take(i,State))
                {
                    cargoProgress=0; Stealing=false; RefreshCargoArt(); Sound(pickupSound);
                    if(site.alarm)RaiseAlarm("Bonded cargo alarm! Heavy bag: sprint, break sight, then hide.",Player.position);
                    else Notify(site.name+" packed. $"+Cargo.Value+" at risk. Bank it at home or take another crate.");
                }
                return true;
            }
            cargoTarget=-1;cargoProgress=0;return false;
        }

        void OpenSafehouse()
        {
            if(Heat>0 || Vector3.Distance(Player.position,Jobs.Home)>=2.8f)return;
            ResetCargoInteraction(); screen=ScreenMode.Safehouse;
        }

        void BuyCargoSatchel()
        {
            if(Heat>0 || Vector3.Distance(Player.position,Jobs.Home)>=2.8f)return;
            if(State.BuySatchel()) { Save();Sound(cashSound);Notify("Larger satchel fitted. Capacity 9. Heavy loads still slow you down."); }
        }

        void ContinueFreeroam()
        { screen=State.NeedsPerk?ScreenMode.Perk:ScreenMode.Play;Notify("Debt settled. Work for yourself: purple cargo out, safehouse money home."); }
    }
}
