using System;
using System.Collections.Generic;
using UnityEngine;

namespace Funstra
{
    public sealed partial class FunstraGame
    {
        sealed class CombatTarget {public DistrictActor actor;public Transform body;}
        readonly List<CombatTarget> extraCombatTargets=new List<CombatTarget>();
        readonly Dictionary<string,Vector3> previousCombatPositions=new Dictionary<string,Vector3>();
        readonly Dictionary<Collider,Bounds> previousCombatObstacles=new Dictionary<Collider,Bounds>();
        readonly List<CombatTarget> combatTargets=new List<CombatTarget>();
        readonly List<Collider> combatObstacles=new List<Collider>();
        readonly List<Collider> knownCombatObstacles=new List<Collider>();
        readonly List<LineRenderer> projectileLines=new List<LineRenderer>();
        readonly List<Transform> impactMarks=new List<Transform>();
        readonly List<float> impactTimes=new List<float>();
        Material projectileMaterial;
        LineRenderer aimLine;
        AudioClip pistolReport,shotgunReport,impactReport,reloadReport;
        float recoil,impactClock;
        Vector3 combatAim;
        public event Action<DistrictActor,string> CombatActorHit;
        public string LastCombatImpact {get;private set;}="";
        public int CombatShotCount {get;private set;}
        public int CombatHitCount {get;private set;}
        public int CombatCoverHitCount {get;private set;}
        public string CombatWeaponName=>weapon==1?"FISTS":WeaponSpec.For(weapon).name;
        public WeaponState CurrentWeapon=>weapon==3?District.shotgunWeapon:District.pistolWeapon;
        public int CombatTotalAmmo=>weapon==3?District.shotgunAmmo:District.ammo;
        public string CombatAmmoText=>weapon==1?"MELEE":CurrentWeapon.magazine+" / "+(CombatTotalAmmo-CurrentWeapon.magazine)+" reserve";
        public float CombatReloadRemaining=>weapon==1?0:CurrentWeapon.reloadRemaining;
        public int ActiveProjectileCount=>District.projectiles.Count;
        public Vector3 CombatAimPoint=>combatAim;
        public void RegisterCombatActor(DistrictActor actor,Transform body)
        {
            extraCombatTargets.RemoveAll(t=>t.actor.id==actor.id);extraCombatTargets.Add(new CombatTarget{actor=actor,body=body});
            if(actor.combat==null)actor.combat=new WeaponState();actor.combat.Initialize(actor.ammo,actor.combat.kind);
            previousCombatPositions[actor.id]=actor.position;
        }
        public void ClearExtraCombatActors(){extraCombatTargets.Clear();previousCombatPositions.Clear();}
        public void InitializeCombat()
        {
            District.InitializeWeapons();weapon=District.equippedWeapon;recoil=0;
            previousCombatPositions.Clear();previousCombatObstacles.Clear();
            if(projectileMaterial==null)
            {
                projectileMaterial=new Material(Shader.Find("Sprites/Default"));
                aimLine=MakeCombatLine("Aim / intended direction",.025f);aimLine.startColor=CityArt.Mint;aimLine.endColor=new Color(.4f,.9f,.8f,.1f);
                LoadCombatAudio();
            }
            RefreshCombatTargets();foreach(var t in combatTargets)previousCombatPositions[t.actor.id]=t.actor.position;
            previousCombatPositions["player"]=Player.position;
            CacheCombatObstacles();RefreshCombatObstacles();foreach(var c in combatObstacles)previousCombatObstacles[c]=c.bounds;
            RenderProjectiles();
        }
        void RefreshCombatTargets()
        {
            combatTargets.Clear();
            if(!FoundationMode)
            {
                combatTargets.Add(new CombatTarget{actor=District.guard,body=guardBody});combatTargets.Add(new CombatTarget{actor=District.collector,body=collectorBody});combatTargets.Add(new CombatTarget{actor=District.neri,body=neriBody});
                foreach(var a in Agents)if(a.Record!=null)combatTargets.Add(new CombatTarget{actor=a.Record,body=a.Body});
            }
            combatTargets.AddRange(extraCombatTargets);
        }
        public void CacheCombatObstacles()
        {
            knownCombatObstacles.Clear();
            foreach(var c in UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsInactive.Include))
                if(c.gameObject.layer==8&&!c.isTrigger)knownCombatObstacles.Add(c);
        }
        void RefreshCombatObstacles()
        {
            Physics.SyncTransforms();combatObstacles.Clear();
            foreach(var c in knownCombatObstacles)if(c&&c.enabled&&c.gameObject.activeInHierarchy)combatObstacles.Add(c);
        }
        public void SelectCombatWeapon(int kind)
        {
            if(kind<1||kind>3||kind==weapon)return;
            District.pistolWeapon.CancelReload();District.shotgunWeapon.CancelReload();weapon=District.equippedWeapon=kind;recoil=0;
            if(pistol){pistol.SetActive(weapon!=1);pistol.transform.localScale=weapon==3?new Vector3(.14f,.17f,.85f):new Vector3(.12f,.15f,.45f);}
        }
        public bool ReloadPlayer()
        {
            if(weapon==1)return false;
            bool started=CurrentWeapon.BeginReload(CombatTotalAmmo);
            if(started){Sound(reloadReport);Notify("Reloading "+CombatWeaponName.ToLower()+". Switching weapon or taking a hit interrupts.");}
            return started;
        }
        public void UpdateCombatInput()
        {
            // Guided runs drive combat APIs explicitly; desktop input must not alter their evidence.
            if(Smoke) { if(aimLine)aimLine.enabled=false; return; }
            if(Input.GetKeyDown(KeyCode.Alpha1))SelectCombatWeapon(1);
            if(Input.GetKeyDown(KeyCode.Alpha2))SelectCombatWeapon(2);
            if(Input.GetKeyDown(KeyCode.Alpha3))SelectCombatWeapon(3);
            if(Input.GetKeyDown(KeyCode.R)&&!Input.GetKey(KeyCode.LeftShift)&&!Input.GetKey(KeyCode.RightShift))ReloadPlayer();
            Ray ray=View.ScreenPointToRay(Input.mousePosition);var plane=new Plane(Vector3.up,Vector3.up*1.1f);
            if(plane.Raycast(ray,out float distance))combatAim=ray.GetPoint(distance);
            bool pointerInWorld=!showMap&&Input.mousePosition.x/Screen.width>.27f&&Input.mousePosition.x/Screen.width<.74f&&Input.mousePosition.y/Screen.height>.18f&&Input.mousePosition.y/Screen.height<.85f;
            if(compactHUD)pointerInWorld=!showMap&&Input.mousePosition.y/Screen.height>.18f&&Input.mousePosition.y/Screen.height<.85f;
            if(FoundationMode)pointerInWorld=!showMap&&Input.mousePosition.y/Screen.height>.17f;
            if(pointerInWorld&&weapon!=1)
            {
                Vector3 heading=combatAim-(Player.position+Vector3.up*1.1f);heading.y=0;
                if(heading.sqrMagnitude>.02f)figure.rotation=Quaternion.LookRotation(heading);
            }
            aimLine.enabled=pointerInWorld&&weapon!=1;
            if(aimLine.enabled)
            {
                Vector3 from=Player.position+Vector3.up*1.1f,delta=combatAim-from;delta.y=0;
                Vector3 to=from+delta.normalized*Mathf.Min(3,delta.magnitude);
                bool blocked=Physics.SphereCast(from,ProjectileMath.Radius,delta.normalized,out var hit,Mathf.Min(3,delta.magnitude),1<<8,QueryTriggerInteraction.Ignore);
                if(blocked)to=hit.point;
                aimLine.startColor=blocked?CityArt.Red:CityArt.Mint;aimLine.SetPosition(0,from);aimLine.SetPosition(1,to);
            }
            if(pointerInWorld&&Input.GetMouseButton(0)) {if(weapon==1)TryMeleeAt(combatAim);else FirePlayerAt(combatAim);}
        }
        public bool TryMeleeAt(Vector3 aim)
        {
            if(District.health<=0||attackCooldown>0)return false;
            RefreshCombatTargets();Vector3 facing=aim-Player.position;facing.y=0;
            if(facing.sqrMagnitude<.001f)facing=figure.forward;else facing.Normalize();
            CombatTarget nearest=null;float best=2.8f;
            foreach(var t in combatTargets)
            {
                if(t.actor.health<=0||!t.body||!t.body.gameObject.activeInHierarchy)continue;
                Vector3 delta=t.actor.position-Player.position;delta.y=0;float distance=delta.magnitude;
                if(distance<best&&Vector3.Dot(facing,delta.normalized)>.25f&&City.Nav.Sight(Player.position,t.actor.position)) {best=distance;nearest=t;}
            }
            if(nearest==null)return false;
            attackCooldown=.7f;figure.rotation=Quaternion.LookRotation(facing);
            ApplyCombatHit(nearest.actor,nearest.body,18,"player",false);Impact(nearest.actor.position+Vector3.up,false);return true;
        }
        public bool FirePlayerAt(Vector3 target)
        {
            if(weapon==1||District.health<=0)return false;
            District.InitializeWeapons();var state=CurrentWeapon;int total=CombatTotalAmmo;
            if(District.projectiles.Count+WeaponSpec.For(weapon).pellets>128)return false;
            if(!state.Fire(ref total))
            {if(state.magazine==0&&state.reloadRemaining==0&&state.cooldown==0){Notify(total>0?"Magazine empty. R reload / 1 fists / retreat.":"Out of ammunition. Switch weapon or retreat.");state.cooldown=.35f;}return false;}
            if(weapon==3)District.shotgunAmmo=total;else District.ammo=total;
            Vector3 from=Player.position+Vector3.up*1.1f;target.y=from.y;
            Vector3 heading=target-from;if(heading.sqrMagnitude>.001f)figure.rotation=Quaternion.LookRotation(heading);
            SpawnShot("player",from,target,weapon,recoil);recoil=Mathf.Min(3,recoil+(weapon==3?2.4f:.7f));
            if(!FoundationMode)
            {
                Hidden=false;ReportPoliceNoise(Player.position);
                string witness=ViolenceWitness();if(witness!=null)ReportPoliceViolence(Player.position,0,witness);
            }
            return true;
        }
        public void StepActorWeapon(DistrictActor actor,float dt)
        {
            if(actor.combat==null)actor.combat=new WeaponState();actor.combat.Initialize(actor.ammo,actor.combat.kind);
            if(actor.health<=0){actor.combat.CancelReload();return;}
            actor.combat.Step(dt,actor.ammo);
            if(actor.combat.magazine==0&&actor.ammo>0)actor.combat.BeginReload(actor.ammo);
        }
        public bool FireActorAt(DistrictActor actor,Transform body,Vector3 target,int gun=2)
        {
            if(actor.health<=0)return false;if(actor.combat==null)actor.combat=new WeaponState();actor.combat.Initialize(actor.ammo,gun);
            if(District.projectiles.Count+WeaponSpec.For(gun).pellets>128||!actor.combat.Fire(ref actor.ammo))return false;
            Vector3 from=actor.position+Vector3.up*1.1f;target.y=from.y;Vector3 heading=target-from;
            if(body&&heading.sqrMagnitude>.001f)body.rotation=Quaternion.LookRotation(heading);
            SpawnShot(actor.id,from,target,gun,0);return true;
        }
        void SpawnShot(string owner,Vector3 from,Vector3 target,int gun,float kick)
        {
            var spec=WeaponSpec.For(gun);Vector3 heading=(target-from).normalized;if(heading.sqrMagnitude<.1f)heading=Vector3.forward;
            // Do not put the projectile on the far side of close cover when the muzzle protrudes.
            Vector3 muzzle=from+heading*.5f;Physics.SyncTransforms();
            if(Physics.CheckSphere(from,ProjectileMath.Radius,1<<8,QueryTriggerInteraction.Ignore)||Physics.SphereCast(from,ProjectileMath.Radius,heading,out _, .5f,1<<8,QueryTriggerInteraction.Ignore))muzzle=from;
            RefreshCombatTargets();
            var birthPositions=new Dictionary<string,Vector3>{{"player",Player.position}};
            foreach(var t in combatTargets)birthPositions[t.actor.id]=t.actor.position;
            var birthObstacles=new Dictionary<Collider,Bounds>();
            foreach(var obstacle in knownCombatObstacles)if(obstacle&&obstacle.enabled&&obstacle.gameObject.activeInHierarchy)birthObstacles[obstacle]=obstacle.bounds;
            for(int i=0;i<spec.pellets;i++)
            {
                float angle=spec.pellets==1?kick*Mathf.Sin(CombatShotCount*2.4f):(i-(spec.pellets-1)*.5f)*spec.spread/(spec.pellets-1);
                var velocity=Quaternion.Euler(0,angle,0)*heading*spec.speed;
                District.projectiles.Add(new CombatProjectile{position=muzzle,velocity=velocity,remaining=spec.range,damage=spec.damage,owner=owner,kind=gun,birthPositions=birthPositions,birthObstacles=birthObstacles});
            }
            CombatShotCount++;PlayWeaponReport(gun,from);
        }
        public void StepCombat(float dt)
        {
            if(dt<=0)return;attackCooldown=Mathf.Max(0,attackCooldown-dt);District.InitializeWeapons();
            float oldReload=CurrentWeapon.reloadRemaining;District.pistolWeapon.Step(dt,District.ammo);District.shotgunWeapon.Step(dt,District.shotgunAmmo);
            if(oldReload>0&&CurrentWeapon.reloadRemaining==0)PlayReloadFinished();
            recoil=Mathf.MoveTowards(recoil,0,dt*4);impactClock+=dt;
            if(pistol){pistol.transform.localPosition=new Vector3(.43f,.95f,.3f-recoil*.05f);pistol.transform.localRotation=Quaternion.Euler(-recoil*9,0,0);}
            RefreshCombatTargets();RefreshCombatObstacles();
            // Sweeps cover the actual traveled segment and relative actor/cover motion across this update.
            for(int i=District.projectiles.Count-1;i>=0;i--)
            {
                var p=District.projectiles[i];float travel=Mathf.Min(p.remaining,p.velocity.magnitude*dt);Vector3 from=p.position,to=from+p.velocity.normalized*travel;
                float best=1.0001f;CombatTarget victim=null;bool playerHit=false,cover=false;
                if(Physics.CheckSphere(from,ProjectileMath.Radius,1<<8,QueryTriggerInteraction.Ignore)){best=0;cover=true;}
                if(travel>0&&Physics.SphereCast(from,ProjectileMath.Radius,p.velocity.normalized,out var hit,travel,1<<8,QueryTriggerInteraction.Ignore)&&hit.distance/travel<best) {best=hit.distance/travel;cover=true;}
                foreach(var obstacle in combatObstacles)
                {
                    Bounds now=obstacle.bounds;
                    if((p.birthObstacles??previousCombatObstacles).TryGetValue(obstacle,out var before)&&(now.center-before.center).sqrMagnitude>.000001f&&ProjectileMath.MovingBounds(from,to,before,now,ProjectileMath.Radius,out float f)&&f<best){best=f;cover=true;}
                }
                foreach(var t in combatTargets)
                {
                    if(t.actor.id==p.owner||t.actor.health<=0||!t.body||!t.body.gameObject.activeInHierarchy)continue;
                    Vector3 old=(p.birthPositions??previousCombatPositions).TryGetValue(t.actor.id,out var previous)?previous:t.actor.position;
                    if(ProjectileMath.MovingSphere(from,to,old+Vector3.up*1.1f,t.actor.position+Vector3.up*1.1f,.44f,out float f)&&f<best){best=f;victim=t;playerHit=false;cover=false;}
                }
                if(p.owner!="player"&&District.health>0)
                {
                    Vector3 old=(p.birthPositions??previousCombatPositions).TryGetValue("player",out var previous)?previous:Player.position;
                    if(ProjectileMath.MovingSphere(from,to,old+Vector3.up*1.1f,Player.position+Vector3.up*1.1f,.43f,out float f)&&f<best){best=f;victim=null;playerHit=true;cover=false;}
                }
                if(best<=1)
                {
                    Vector3 at=Vector3.Lerp(from,to,best);Impact(at,cover);
                    if(cover){CombatCoverHitCount++;LastCombatImpact="Cover stopped a round";}
                    else if(playerHit){District.health=Mathf.Max(0,District.health-p.damage);District.bleeding=true;CurrentWeapon.CancelReload();CombatHitCount++;LastCombatImpact="Player hit";Notify("Hit! B bandage / SPACE pause / break sight.");}
                    else if(victim!=null)ApplyCombatHit(victim.actor,victim.body,p.damage,p.owner,true);
                    District.projectiles.RemoveAt(i);
                }
                else {p.position=to;p.remaining-=travel;p.birthPositions=null;p.birthObstacles=null;if(p.remaining<=.0001f)District.projectiles.RemoveAt(i);}
            }
            previousCombatPositions["player"]=Player.position;foreach(var t in combatTargets)previousCombatPositions[t.actor.id]=t.actor.position;
            previousCombatObstacles.Clear();foreach(var c in combatObstacles)previousCombatObstacles[c]=c.bounds;
            for(int i=0;i<impactMarks.Count;i++)impactMarks[i].gameObject.SetActive(impactTimes[i]>impactClock);
            RenderProjectiles();
        }
        void ApplyCombatHit(DistrictActor target,Transform body,float damage,string owner,bool bullet)
        {
            if(target.health<=0)return;
            string policeWitness=owner=="player"&&!FoundationMode&&DistrictEnabled?ViolenceWitness(target.health>damage?target:null):null;
            target.health=Mathf.Max(0,target.health-damage);if(bullet)target.bleeding=true;
            target.combat?.CancelReload();PoseActor(body,target);CombatHitCount++;LastCombatImpact=target.name+" hit";CombatActorHit?.Invoke(target,owner);
            if(owner=="player"&&!FoundationMode)
            {
                if(policeWitness!=null)ReportPoliceViolence(Player.position,target.health<=0?3:1,policeWitness);
                if(target==District.neri){District.trust=-3;District.recruited=false;District.Record("betrayal","neri","You attacked Neri. The partnership is broken.");}
                if(target==District.guard||target==District.collector||MedicalWitness())District.Identify("An attack was witnessed and reported to Ivo.");
                if(District.citizens.Contains(target)){target.order="Flee";District.Record("assault",target.id,"You attacked "+target.name+"."+(policeWitness!=null?" The attack was reported.":" The attacker was not identified."));}
                if(target.health==0){District.Record("incapacitated",target.id,target.name+" is incapacitated. Their inventory and relationships remain.");Notify(target.name+" down. Withdraw or help them with E and a bandage.");}
            }
        }
        void RenderProjectiles()
        {
            for(int i=0;i<District.projectiles.Count;i++)
            {
                if(i>=projectileLines.Count)projectileLines.Add(MakeCombatLine("Traveling projectile",.04f));
                var line=projectileLines[i];var p=District.projectiles[i];line.enabled=true;line.startColor=p.owner=="player"?CityArt.Amber:CityArt.Red;line.endColor=Color.white;
                line.SetPosition(0,p.position-p.velocity.normalized*.5f);line.SetPosition(1,p.position);
            }
            for(int i=District.projectiles.Count;i<projectileLines.Count;i++)projectileLines[i].enabled=false;
        }
        LineRenderer MakeCombatLine(string name,float width)
        {var line=new GameObject(name).AddComponent<LineRenderer>();line.sharedMaterial=projectileMaterial;line.positionCount=2;line.startWidth=width;line.endWidth=width*.5f;line.enabled=false;return line;}
        void Impact(Vector3 at,bool cover)
        {
            int slot=impactTimes.FindIndex(t=>t<=impactClock);
            if(slot<0&&impactMarks.Count<32){var mark=GameObject.CreatePrimitive(PrimitiveType.Sphere);mark.name="Projectile impact";Destroy(mark.GetComponent<Collider>());mark.GetComponent<Renderer>().sharedMaterial=City.Mat(CityArt.Amber,true);impactMarks.Add(mark.transform);impactTimes.Add(0);slot=impactMarks.Count-1;}
            if(slot>=0){impactMarks[slot].position=at;impactMarks[slot].localScale=Vector3.one*(cover?.14f:.22f);impactMarks[slot].gameObject.SetActive(true);impactTimes[slot]=impactClock+.16f;}
            PlayImpactAudio(at,cover);
        }
        void OnDestroy()
        {
            if(projectileMaterial)Destroy(projectileMaterial);
            // Imported Resources clips are shared assets, not generated clips to destroy.
        }
    }
}
