using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Funstra
{
    public sealed partial class FunstraGame : MonoBehaviour
    {
        public CityArt City { get; private set; }
        public RunState State { get; private set; } = new RunState();
        public Transform Player { get; private set; }
        public Camera View { get; private set; }
        public readonly List<TownAgent> Agents = new List<TownAgent>();
        public float Heat { get; private set; }
        public Vector3 LastSeen;
        public float Elapsed { get; private set; }
        public bool Sneaking { get; private set; }
        public bool Hidden { get; private set; }
        public bool Stealing { get; private set; }
        public bool Active => screen == ScreenMode.Play;
        public float Stamina = 100;
        public bool Smoke;
        enum ScreenMode { Title, Play, Pause, Talk, Perk, Ending, ConfirmRestart, Safehouse, Intro, Clinic, Collector, Journal, Tactics, Recovery, MedicineSale, Refuge, Conversation }
        ScreenMode screen;
        CharacterController controller;
        Transform figure;
        GameObject playerRing;
        float theftProgress, arrestProgress, playerPhase, lastSight, toastTime, stepTime, cameraSize = 17;
        string toast = "", prompt = "";
        bool muteTests;
        bool mute, showMap, sprinting, sprintExhausted;
        readonly List<float> buildingRevealUntil = new List<float>();
        string savePath;
        AudioSource audioSource;
        AudioClip pickupSound, alertSound, cashSound, stepSound;
        Vector3 cameraVelocity;
        Vector3 cameraOffset = new Vector3(19, 32, -23);
        Vector3? autoMove;
        bool autoInteract;
        readonly List<string> smokeResults = new List<string>();

        void Awake()
        {
            ReadFoundationArguments();
            muteTests=Array.IndexOf(Environment.GetCommandLineArgs(),"--mute-tests")>=0;mute=muteTests;
            if(muteTests)AudioListener.volume=0;
            policeTest=Array.IndexOf(Environment.GetCommandLineArgs(),"--police-test")>=0;
            Application.targetFrameRate = 60; QualitySettings.vSyncCount = 1;
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"--30fps")>=0) { Application.targetFrameRate=30;QualitySettings.vSyncCount=0; }
            visualCheck = Array.IndexOf(Environment.GetCommandLineArgs(), "--visual-check") >= 0;
            streetsTest = Array.IndexOf(Environment.GetCommandLineArgs(), "--streets-test") >= 0;
            environmentTest = Array.IndexOf(Environment.GetCommandLineArgs(), "--environment-test") >= 0;
            bandageTest = streetsTest||environmentTest||Array.IndexOf(Environment.GetCommandLineArgs(), "--bandage-test") >= 0;
            bandageVisual = Array.IndexOf(Environment.GetCommandLineArgs(), "--bandage-visual") >= 0;
            captureScreens = visualCheck || Array.IndexOf(Environment.GetCommandLineArgs(), "--capture-screens") >= 0;
            Smoke = policeTest || visualCheck || bandageTest || bandageVisual || Array.IndexOf(Environment.GetCommandLineArgs(), "--smoke-test") >= 0;
            captureScreens |= bandageVisual;
            savePath = Path.Combine(Application.persistentDataPath, bandageTest||bandageVisual?"bandage-smoke.json":Smoke ? "smoke-save.json" : "lights-progress.json");
            if(!Smoke)savePath=Path.Combine(Application.persistentDataPath,"streets-progress.json");
            City = new CityArt(); if(FoundationMode)City.BuildTestLevel(foundationLevel);else {City.Build();BuildCargoArt();}
            SetupLighting();
            var player = new GameObject("Player"); Player = player.transform; Player.position = Jobs.Home + Vector3.up*.15f;
            controller = player.AddComponent<CharacterController>(); controller.height = 1.85f; controller.radius = .38f; controller.center = Vector3.up*.95f; controller.stepOffset = .3f;
            figure = City.Human("Player visual", Vector3.zero, CityArt.Mint); figure.SetParent(Player,false);
            playerRing = City.Ring("Player circle",Vector3.zero,.72f,CityArt.Mint); playerRing.transform.SetParent(Player,false);
            var cam = new GameObject("Main Camera"); View = cam.AddComponent<Camera>(); cam.tag = "MainCamera";
            View.orthographic = true; View.orthographicSize = cameraSize; View.nearClipPlane = .1f; View.farClipPlane = 250;
            View.backgroundColor = CityArt.Hex("222D43"); View.clearFlags = CameraClearFlags.SolidColor;
            cam.AddComponent<AudioListener>(); SnapCamera();
            if(!FoundationMode)SpawnAgents(); SetupAudio();
            if(FoundationMode)pistol=City.Box("Player firearm",new Vector3(.43f,.95f,.3f),new Vector3(.12f,.15f,.45f),CityArt.Hex("303848"),figure);
            else BuildDistrict();
            screen = ScreenMode.Title;
            if(FoundationMode) {StartFoundation();if(Array.IndexOf(Environment.GetCommandLineArgs(),"--foundation-test")>=0)StartCoroutine(FoundationRun());}
            if (Smoke) StartCoroutine(SmokeRun());
        }
        void SetupLighting()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = CityArt.Hex("687D9A"); RenderSettings.ambientEquatorColor = CityArt.Hex("4F6078"); RenderSettings.ambientGroundColor = CityArt.Hex("303648");
            RenderSettings.fog = true; RenderSettings.fogColor = CityArt.Hex("53637C"); RenderSettings.fogMode = FogMode.Linear; RenderSettings.fogStartDistance = 75; RenderSettings.fogEndDistance = 180;
            var sun = new GameObject("Late evening sun").AddComponent<Light>(); sun.type = LightType.Directional; sun.color = CityArt.Hex("FFD0A0"); sun.intensity = .95f;
            sun.transform.rotation = Quaternion.Euler(32,-35,0); sun.shadows = LightShadows.Soft; sun.shadowStrength = .82f;
            QualitySettings.shadowDistance = 100; QualitySettings.shadows = ShadowQuality.All; QualitySettings.shadowResolution = ShadowResolution.High; QualitySettings.antiAliasing = 4; QualitySettings.pixelLightCount = 4;
        }
        void SpawnAgents()
        {
            AddAgent(true,new[]{ new Vector3(8,0,-13),new Vector3(42,0,-13),new Vector3(45,0,-38),new Vector3(8,0,-38) });
            AddAgent(true,new[]{ new Vector3(-43,0,13),new Vector3(-7,0,13),new Vector3(-7,0,-13),new Vector3(-43,0,-13) });
            AddAgent(true,new[]{ new Vector3(8,0,39),new Vector3(44,0,39),new Vector3(46,0,13),new Vector3(8,0,13) });
            for(int i=0;i<8;i++)
            {
                int row = i%3; float z = -39+row*26; float side = i%2==0 ? 1 : -1;
                AddAgent(false,new[]{new Vector3(side*(10+i*2),0,z+3),new Vector3(side*45,0,z+3),new Vector3(side*45,0,z+22),new Vector3(side*7,0,z+22),new Vector3(side*7,0,z+3)});
            }
        }
        void AddAgent(bool police,Vector3[] route)
        {
            var a = new TownAgent { Police = police, Route = route, Phase = Agents.Count*.73f };
            a.Body = City.Human(police ? "Patrol officer" : "Resident", route[0], police ? CityArt.Hex("577FC4") : CityArt.Hex(new[]{"CC947C","B8AD83","9A7D9F","D1B992"}[Agents.Count%4]),police);
            if(police)City.Box("Police pistol",new Vector3(.4f,.95f,.3f),new Vector3(.12f,.15f,.45f),CityArt.Hex("303848"),a.Body);
            a.Body.rotation = Quaternion.LookRotation(route[1]-route[0]); Agents.Add(a);
        }
        void Update()
        {
            if(FoundationMode) { UpdateFoundation();return; }
            if(Input.GetKeyDown(KeyCode.F2))compactHUD=!compactHUD;
            if (Input.GetKeyDown(KeyCode.F11)) Screen.fullScreen = !Screen.fullScreen;
            if (!muteTests && Input.GetKeyDown(KeyCode.M)) mute = !mute;
            AudioListener.volume = mute ? 0 : .65f;
            if(DistrictEnabled&&Input.GetKeyDown(KeyCode.F5)&&screen!=ScreenMode.Title) { Save();Notify("Saved here, including wounds, witnesses, crew and carried goods."); }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if(screen == ScreenMode.Play) { Save();screen = ScreenMode.Pause; }
                else if(screen != ScreenMode.Title && screen != ScreenMode.Perk && screen != ScreenMode.Ending && screen != ScreenMode.ConfirmRestart) screen = ScreenMode.Play;
                else if(screen == ScreenMode.ConfirmRestart) screen = ScreenMode.Title;
            }
            if(DistrictEnabled&&Input.GetKeyDown(KeyCode.Space))
            {
                if(screen==ScreenMode.Play) { Save();screen=ScreenMode.Tactics; }
                else if(screen==ScreenMode.Tactics)screen=ScreenMode.Play;
            }
            if(DistrictEnabled&&Input.GetKeyDown(KeyCode.J))
            { if(screen==ScreenMode.Play) { Save();screen=ScreenMode.Journal; }else if(screen==ScreenMode.Journal)screen=ScreenMode.Play; }
            if(!Active)
            {
                if(screen==ScreenMode.Tactics)
                { SelectDistrictTarget();if(Input.mouseScrollDelta.y!=0)cameraSize=Mathf.Clamp(cameraSize-Input.mouseScrollDelta.y*1.5f,14,29); }
                Stealing = false;medicineProgress=0; ResetCargoInteraction(); return;
            }
            float dt = Mathf.Min(Time.deltaTime,.05f); Elapsed += dt; toastTime -= dt;
            if(Input.GetKeyDown(KeyCode.Tab)) showMap = !showMap;
            if(Input.mouseScrollDelta.y != 0) cameraSize = Mathf.Clamp(cameraSize-Input.mouseScrollDelta.y*1.5f,14,29);
            City.RefreshProps();City.StepTraffic(this,dt);
            UpdatePlayer(dt);
            if(DistrictEnabled) { UpdateDistrict(dt);if(!Active)return; UpdateYard(dt); }
            StepPoliceResponse(dt);
            bool seen = false;
            foreach(var a in Agents) { if(!smokeFreezeAgents)a.Step(this,dt); if(a.Police && a.SeesPlayer && a.Pursuing) seen = true; }
            if(Heat > 0)
            {
                if(Police.identifiedGunman)Heat=Police.searchRemaining;
                else if(!seen) Heat = Mathf.Max(0,Heat-dt*(Hidden ? 2.2f : 1));
                if(Heat == 0) Notify("Heat cleared. Mara will buy the goods.");
            }
            bool nearOfficer = false;
            foreach(var a in Agents) if(Heat>0 && a.Police && a.Pursuing && a.SeesPlayer && Vector3.Distance(a.Position,Player.position)<1.65f) nearOfficer = true;
            arrestProgress = Mathf.Clamp01(arrestProgress + (nearOfficer ? dt*.85f : -dt*1.5f));
            if(arrestProgress >= 1) Busted();
            UpdateInteraction(dt);
            for(int i=0;i<City.Targets.Count;i++)
            {
                City.Targets[i].SetActive(i>=State.completed && !(i==State.completed && State.carrying));
                City.Targets[i].transform.position = Jobs.All[i].position+Vector3.up*(.75f+Mathf.Sin(Elapsed*2.4f+i)*.1f);
                City.Targets[i].transform.rotation = Quaternion.Euler(0,Elapsed*22,0);
            }
        }
        void UpdatePlayer(float dt)
        {
            Vector3 movement;
            if(autoMove.HasValue) movement = Vector3.ClampMagnitude(autoMove.Value-Player.position,1);
            else
            {
                float x = (Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)?1:0)-(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow)?1:0);
                float z = (Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow)?1:0)-(Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.DownArrow)?1:0);
                var forward = View.transform.forward; forward.y=0; forward.Normalize();
                movement = Vector3.ClampMagnitude(View.transform.right*x+forward*z,1);
            }
            movement.y=0;
            Sneaking = Input.GetKey(KeyCode.LeftControl)||Input.GetKey(KeyCode.C);
            float capacity = State.HasPerk(1) ? 135 : 100;
            // Exhaustion must not alternate run/walk speed every other frame at two stamina.
            if(Stamina<=2)sprintExhausted=true;
            if(Stamina>=capacity)sprintExhausted=false;
            sprinting = !sprintExhausted && !Sneaking && movement.sqrMagnitude>.01f && (Input.GetKey(KeyCode.LeftShift)||autoMove.HasValue) && Stamina>2;
            Stamina = Mathf.Clamp(Stamina + dt*(sprinting ? -19 : 15),0,capacity);
            float speed = Sneaking ? (State.HasPerk(0)?3.1f:2.3f) : sprinting ? (State.HasPerk(1)?8.4f:7) : 4.2f;
            speed *= Cargo.SpeedMultiplier;
            if(DistrictEnabled) speed *= (District.health<40?.8f:1)*(District.Carrying?.9f:1);
            controller.Move((movement*speed+Vector3.down*8)*dt);
            var p=Player.position; p.x=Mathf.Clamp(p.x,City.Nav.Min.x,City.Nav.Max.x); p.z=Mathf.Clamp(p.z,City.Nav.Min.y,City.Nav.Max.y); Player.position=p;
            if(movement.sqrMagnitude>.01f) figure.rotation = Quaternion.Slerp(figure.rotation,Quaternion.LookRotation(movement),dt*14);
            playerPhase += dt*(Sneaking?.65f:1.1f); CityArt.Animate(figure,playerPhase,movement.magnitude*speed);
            figure.localScale = new Vector3(1,Sneaking?.8f:1,1);
            if(movement.sqrMagnitude>.01f && (stepTime-=dt)<=0) { PlayFootstep(Sneaking?.13f:.27f); stepTime=sprinting?.25f:.4f; }
            Hidden = false;
            if(Sneaking && !Stealing && Elapsed-lastSight>1.2f)
                foreach(var h in City.Hides) if(Vector3.Distance(Player.position,h)<2.5f) Hidden=true;
        }
        void UpdateInteraction(float dt)
        {
            prompt=""; Stealing=false;
            bool pressed=Input.GetKeyDown(KeyCode.E), held=Input.GetKey(KeyCode.E)||autoInteract;
            if(UpdateMedicalInteraction(dt,pressed,held)) { theftProgress=0;ResetCargoInteraction();return; }
            if(UpdateCargoInteraction(dt,held)) { theftProgress=0; return; }
            if(Vector3.Distance(Player.position,Jobs.Mara)<3.2f)
            {
                theftProgress=0;
                prompt=Heat>0?"LOSE THE POLICE BEFORE RETURNING":"E  /  TALK TO MARA";
                if(pressed && Heat<=0) Talk();
                return;
            }
            if(State.accepted && !State.Finished && !State.carrying && Vector3.Distance(Player.position,Jobs.All[State.completed].position)<2.6f)
            {
                prompt="HOLD E  /  TAKE "+Jobs.All[State.completed].item.ToUpperInvariant();
                if(held)
                {
                    Stealing=true; Hidden=false;
                    theftProgress+=dt/(Jobs.All[State.completed].seconds*(State.HasPerk(2)?.6f:1));
                    if(theftProgress>=1)
                    {
                        State.Steal(); theftProgress=0; Stealing=false; prompt="GOODS SECURED  /  RETURN TO MARA"; Sound(pickupSound);
                        if(State.completed==2) RaiseAlarm("Depot alarm! Break sight, then hide.",Player.position);
                        else Notify("Got the "+Jobs.All[State.completed].item.ToLowerInvariant()+". Bring it to Mara.");
                    }
                }
                else theftProgress=Mathf.Max(0,theftProgress-dt*.8f);
                return;
            }
            theftProgress=0;
            foreach(var h in City.Hides) if(Vector3.Distance(Player.position,h)<2.5f) prompt=Hidden?"HIDDEN  /  STAY LOW UNTIL HEAT CLEARS":"HOLD CTRL  /  HIDE WHEN OUT OF SIGHT";
        }
        public void ReportSight(Vector3 position)
        {
            if(Heat<=0) { Sound(alertSound); Notify("Spotted! Break line of sight in the alleys."); }
            Heat=Police.identifiedGunman?45:12; lastSight=Elapsed; LastSeen=position;Police.Sight(position);
        }
        public void RaiseAlarm(string message,Vector3 position)
        { bool newlyWanted=Heat<=0;Heat=12; LastSeen=position; lastSight=Elapsed;if(newlyWanted)Sound(alertSound); Notify(message); }
        void Busted()
        {
            if(DistrictEnabled&&District.Carrying) { District.shipmentOwner=District.clock>=DistrictState.SaleTime?"buyer":"collector";District.released=false;District.Record("confiscated","police","Police returned the medical shipment to its owner. It remains recoverable."); }
            State.Arrest(); Cargo.Lose(); RefreshCargoArt(); ResetCargoInteraction(); Heat=arrestProgress=theftProgress=0; Stealing=false;
            Teleport(Jobs.Home); ResetPolice(); Save(); Notify("Busted. Goods confiscated; up to $40 fined. The job is still available."); Sound(alertSound);
        }
        void ResetPolice(bool clearResponse=true)
        {
            if(clearResponse)ResetPoliceResponse();else ClearPoliceRuntime();
            foreach(var a in Agents) if(a.Police) { a.Body.position=a.Route[0]; if(a.Record!=null)a.Record.position=a.Body.position; a.Pursuing=a.SeesPlayer=false; a.Suspicion=a.FireDelay=0; a.Path.Clear(); a.Repath=0; a.Stop=0; }
        }
        public void Teleport(Vector3 p) { controller.enabled=false; Player.position=City.Nav.SafePoint(p)+Vector3.up*.12f; controller.enabled=true; SnapCamera(); }
        void Talk()
        {
            if(State.Finished) { Notify("Mara: We're square. Run cargo from the purple crates and bank it at home."); return; }
            if(State.carrying && State.Deliver(Heat))
            {
                Save(); Sound(cashSound); screen=State.Finished?ScreenMode.Ending:ScreenMode.Perk;
            }
            else screen=State.NeedsPerk?ScreenMode.Perk:ScreenMode.Talk;
        }
        void StartRun(bool resume)
        {
            State=resume?LoadCurrentRun()??new RunState():new RunState();
            City.SetServiceGate(District.yardGateClosed,null);
            Cargo.Lose(); RefreshCargoArt(); ResetCargoInteraction();
            Heat=Elapsed=theftProgress=arrestProgress=0; Stamina=100; Hidden=Stealing=false; Teleport(Jobs.Home); ResetPolice(false);
            screen=State.NeedsPerk?ScreenMode.Perk:ScreenMode.Play;
            if(DistrictEnabled)
            {
                if(resume)
                {
                    for(int i=0;i<CargoRun.Sites.Length;i++)if((District.cargoTakenMask&(1<<i))!=0)Cargo.Take(i,State);
                    State.carrying=District.savedJobCarrying&&State.accepted&&!State.Finished;RefreshCargoArt();
                }
                if(resume&&District.hasPosition) { Teleport(District.playerPosition);Heat=District.savedHeat;LastSeen=District.savedLastSeen; }
                ResetDistrictRuntime();
                InitializeYard();
                InitializePoliceResponse();
                if(!District.introSeen)screen=ScreenMode.Intro;
            }
            Notify(DistrictEnabled?"Neri's clinic is marked cyan. TAB map / J history / L switches tracked story.":State.accepted?"Your job is waiting. Check the amber marker.":"Meet Mara at the mint circle. Press E to talk.");
            if(!resume) Save();
        }
        void Save() { if(FoundationMode)return; DistrictCheckpoint();if(!State.Save(savePath)) Notify("Progress could not be saved. This run can still continue."); }
        RunState LoadCurrentRun()
        {
            if(File.Exists(savePath))return RunState.Load(savePath);
            if(Smoke)return null;
            return RunState.Load(Path.Combine(Application.persistentDataPath,"lights-progress.json"))??RunState.Load(Path.Combine(Application.persistentDataPath,"district-progress.json"))??RunState.Load(Path.Combine(Application.persistentDataPath,"progress.json"));
        }
        void Notify(string message) { toast=message; toastTime=6; }
        void SnapCamera() { if(!View)return; cameraVelocity=Vector3.zero;View.transform.position=Player.position+cameraOffset; View.transform.LookAt(Player.position+Vector3.up); }
        void LateUpdate()
        {
            if(!View)return;
            Vector3 focus = Player.position;
            if(screen==ScreenMode.Title || screen==ScreenMode.ConfirmRestart) focus=new Vector3(3,0,-15);
            Vector3 desired=focus+cameraOffset;
            View.transform.position=Vector3.SmoothDamp(View.transform.position,desired,ref cameraVelocity,.16f);
            View.transform.rotation=Quaternion.LookRotation((focus+Vector3.up)-desired);
            View.orthographicSize=Mathf.Lerp(View.orthographicSize,screen==ScreenMode.Title?29:cameraSize,Time.unscaledDeltaTime*5);
            // Cut away a building only when it actually obscures the player's upper body.
            var ray=new Ray(View.transform.position,(Player.position+Vector3.up-View.transform.position).normalized);
            float length=Vector3.Distance(View.transform.position,Player.position+Vector3.up);
            while(buildingRevealUntil.Count<City.Buildings.Count)buildingRevealUntil.Add(0);
            for(int i=0;i<City.Buildings.Count;i++)
            {
                bool cut=screen!=ScreenMode.Title && City.Buildings[i].IntersectRay(ray,out float distance) && distance<length-1;
                if(DistrictEnabled&&Vector3.Distance(Player.position,DistrictState.Clinic)<6)
                {
                    Vector3 focusPoint=TallyPosition+Vector3.up*.4f;
                    var clinicRay=new Ray(View.transform.position,(focusPoint-View.transform.position).normalized);
                    if(City.Buildings[i].IntersectRay(clinicRay,out float clinicDistance)&&clinicDistance<Vector3.Distance(View.transform.position,focusPoint)-.5f)cut=true;
                }
                // Keep a revealed facade out briefly after the ray clears its edge. Small
                // controller/camera changes should not flash the whole building on and off.
                if(cut)buildingRevealUntil[i]=Time.unscaledTime+.18f;
                bool revealed=cut || (screen!=ScreenMode.Title&&Time.unscaledTime<buildingRevealUntil[i]);
                foreach(var r in City.BuildingRenderers[i]) r.enabled=!revealed;
            }
            if(!FoundationMode)City.UpdateClinicCutaway(View,Player.position);
        }
        void SetupAudio()
        {
            audioSource=gameObject.AddComponent<AudioSource>(); audioSource.spatialBlend=0;
            LoadPortAudio();
        }
        void Sound(AudioClip clip) { audioSource.PlayOneShot(clip); }
    }
}
