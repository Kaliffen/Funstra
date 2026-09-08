using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Funstra
{
    public sealed partial class FunstraGame
    {
        static int requestedFoundation = -1;
        int foundationLevel;
        public bool FoundationMode => foundationLevel > 0;
        bool foundationHelp = true;
        CombatSquad yardSquad;
        readonly List<Transform> yardBodies = new List<Transform>();
        static readonly string[] FoundationNames = { "", "Movement & obstacles", "Weapon handling", "Enemy coordination", "The yard encounter" };
        static readonly string[] FoundationBriefs = {
            "",
            "Walk and sprint around the blocks. Try the narrow passage and the gate. Check corners, camera and stopping precision. Nothing attacks you here.",
            "Two moving targets. Compare pistol and shotgun at different distances. Try firing beside cover, empty a magazine, reload and interrupt it by switching weapons.",
            "Three armed opponents. Let one see you, break sight, then change position. Watch holding, flanking, communication and retreat. SPACE gives you time to inspect.",
            "A compact operation with two approaches. Use the gate or public route, choose your distance, survive contact and withdraw to the mint entrance. Try both weapons."
        };
        void ReadFoundationArguments()
        {
            if(requestedFoundation >= 0) { foundationLevel=requestedFoundation;return; }
            var args=Environment.GetCommandLineArgs();
            for(int i=0;i<args.Length-1;i++)if(args[i]=="--test-level"&&int.TryParse(args[i+1],out int n))foundationLevel=Mathf.Clamp(n,1,4);
        }
        void OpenFoundation(int level)
        {
            if(!FoundationMode&&screen!=ScreenMode.Title)Save();
            requestedFoundation=level;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        void StartFoundation()
        {
            State=new RunState();District.introSeen=true;District.ammo=72;District.bandages=8;
            foreach(var o in City.Targets)o.SetActive(false);
            foreach(var o in cargoProps)o.SetActive(false);
            District.shotgunAmmo=30;InitializeCombat();
            Teleport(City.TestSpawn);cameraSize=16;SelectCombatWeapon(2);Stamina=100;
            screen=ScreenMode.Play;InitializeYard();
        }
        void UpdateFoundation()
        {
            if(foundationScripted)return;
            if(Input.GetKeyDown(KeyCode.F11))Screen.fullScreen=!Screen.fullScreen;
            if(!muteTests&&Input.GetKeyDown(KeyCode.M))mute=!mute;
            AudioListener.volume=mute?0:.65f;
            if(Input.GetKeyDown(KeyCode.F1))foundationHelp=!foundationHelp;
            if(Input.GetKeyDown(KeyCode.F6)) { OpenFoundation(foundationLevel);return; }
            if(Input.GetKeyDown(KeyCode.Escape)||Input.GetKeyDown(KeyCode.Space))screen=Active?ScreenMode.Tactics:ScreenMode.Play;
            if(!Active||District.health<=0)return;
            float dt=Mathf.Min(Time.deltaTime,.05f);Elapsed+=dt;toastTime-=dt;
            cameraSize=Mathf.Clamp(cameraSize-Input.mouseScrollDelta.y*1.5f,12,25);
            City.RefreshProps();UpdatePlayer(dt);UpdateCombatInput();StepCombat(dt);UpdateYard(dt);
            if(District.bleeding)District.health=Mathf.Max(0,District.health-dt*.65f);
            if(Input.GetKeyDown(KeyCode.B))District.BandagePlayer();
        }
        void DrawFoundationSelector()
        {
            Panel(1050,260,486,395);
            Text("FOUNDATION TEST LEVELS",1074,280,440,36,24,paper,FontStyle.Bold);
            Text("Prepared equipment. Quick reset. Your story save is untouched.",1074,324,432,52,16,quiet);
            for(int i=1;i<=4;i++)if(Button(i+" / "+FoundationNames[i].ToUpperInvariant(),1074,390+(i-1)*60,438,49))OpenFoundation(i);
        }
        void DrawFoundation()
        {
            Panel(24,24,570,82);
            Text("TEST "+foundationLevel+" / "+FoundationNames[foundationLevel].ToUpperInvariant(),44,40,530,30,24,paper,FontStyle.Bold);
            Text("F1 checklist   F6 reset   SPACE pause   ESC menu",44,78,530,24,14,quiet);
            Panel(24,120,280,94);
            Text("HEALTH "+Mathf.CeilToInt(District.health)+" / 100",42,136,244,28,20,District.bleeding?CityArt.Red:CityArt.Mint,FontStyle.Bold);
            Text("1 fists   2 pistol   3 shotgun",42,172,244,28,14,quiet);
            if(foundationHelp)
            {
                Panel(24,230,310,220);Text(FoundationBriefs[foundationLevel],44,250,270,184,18,paper);
            }
            Text("WASD move / SHIFT sprint / CTRL sneak   Mouse aim / LMB fire / R reload   B bandage",350,850,1200,32,16,paper);
            if(toastTime>0)Text(toast,430,780,750,55,18,CityArt.Amber,FontStyle.Bold,TextAnchor.MiddleCenter);
            DrawCombatOverlay();
            DrawYardStatus();
            if(!Active||District.health<=0)
            {
                Shade();Panel(480,245,640,385);
                Text(District.health<=0?"INCAPACITATED":"TEST PAUSED",515,277,570,54,40,paper,FontStyle.Bold);
                Text("Test equipment and enemies reset together. Campaign progress is never changed.",515,345,570,66,20,quiet);
                if(District.health>0&&Button("RESUME",515,432,570,48,true))screen=ScreenMode.Play;
                if(Button("RESET THIS TEST",515,490,277,48))OpenFoundation(foundationLevel);
                if(Button("ALL LEVELS / MAIN MENU",806,490,279,48))OpenFoundation(0);
                if(Button("QUIT",515,551,570,45))Application.Quit();
            }
        }
        void DrawCombatOverlay()
        {
            // Campaign maps and tactical instructions start below this persistent readout.
            float top=FoundationMode?110:24;
            Panel(605,top,410,68);
            Text(CombatWeaponName+" / "+CombatAmmoText,625,top+14,370,26,18,CityArt.Amber,FontStyle.Bold,TextAnchor.MiddleCenter);
            Text(CombatReloadRemaining>0?"RELOADING / "+CombatReloadRemaining.ToString("0.0")+"s":weapon==1?"Aim / LMB strike":"LMB fire / R reload / 1 2 3 switch",625,top+42,370,22,14,quiet,FontStyle.Normal,TextAnchor.MiddleCenter);
            if(Active&&weapon!=1&&!showMap)
            {
                float x=Input.mousePosition.x/Screen.width*W,y=(1-Input.mousePosition.y/Screen.height)*H;
                Rect(x-9,y,6,1,CityArt.Mint);Rect(x+4,y,6,1,CityArt.Mint);Rect(x,y-9,1,6,CityArt.Mint);Rect(x,y+4,1,6,CityArt.Mint);
            }
        }
    }
}
