using UnityEngine;

namespace Funstra
{
    public sealed partial class DistrictState
    {
        public int crewArmsVersion;
        public void InitializeCrewArms()
        {
            if(crewArmsVersion!=0||arms==null)return;
            System.Array.Resize(ref arms.gunStock,6);System.Array.Resize(ref arms.ammoStock,6);System.Array.Resize(ref arms.sourceAmmo,6);
            System.Array.Resize(ref arms.spent,6);System.Array.Resize(ref arms.confiscated,6);System.Array.Resize(ref arms.recovered,6);
            arms.gunStock[5]=1;arms.ammoStock[5]=30;crewArmsVersion=1;
        }
        public WeaponState GunState(int kind)=>kind==5?rifleWeapon:kind==4?smgWeapon:kind==3?shotgunWeapon:pistolWeapon;
        public void SetGunState(int kind,WeaponState value)
        {if(kind==5)rifleWeapon=value;else if(kind==4)smgWeapon=value;else if(kind==3)shotgunWeapon=value;else pistolWeapon=value;}
        public void SetGunAmmo(int kind,int value)
        {if(kind==5)rifleAmmo=value;else if(kind==4)smgAmmo=value;else if(kind==3)shotgunAmmo=value;else ammo=value;}
    }
    public sealed partial class FunstraGame
    {
        public bool TransferCrewGun(string from,string to)
        {
            if(!CrewEnabled||from==to||!IsCrewId(from)||!CrewAlive(from)||!IsCrewId(to)||Vector3.Distance(CrewPosition(from),CrewPosition(to))>2.6f||!City.Nav.Sight(CrewPosition(from),CrewPosition(to)))return false;
            int kind=from=="player"?weapon:CrewActor(from).combat.kind;
            bool donor=from=="player"?kind>1&&District.arms.Owns(kind):District.crew.For(from).armed;
            bool occupied=to=="player"?District.arms.Owns(kind):District.crew.For(to).armed;
            if(!donor||occupied)return false;
            WeaponState gun=from=="player"?District.GunState(kind):CrewActor(from).combat;
            int rounds=from=="player"?District.ArmsAmmo(kind):CrewActor(from).ammo;
            // Move the weapon object itself, including its magazine and unfinished reload.
            if(from=="player")
            {District.arms.ownedMask&=~(1<<kind);District.SetGunAmmo(kind,0);District.SetGunState(kind,new WeaponState{kind=kind,initialized=true});weapon=District.equippedWeapon=1;}
            else
            {CrewActor(from).ammo=0;CrewActor(from).combat=new WeaponState{kind=2,initialized=true};District.crew.For(from).armed=false;}
            if(to=="player")
            {District.arms.ownedMask|=1<<kind;District.SetGunAmmo(kind,rounds);District.SetGunState(kind,gun);}
            else {CrewActor(to).ammo=rounds;CrewActor(to).combat=gun;District.crew.For(to).armed=true;}
            Save();Notify("Transferred "+WeaponSpec.For(kind).name+" and "+rounds+" rounds to "+to.ToUpper()+".");return true;
        }
        void UpdateCrewCombatInput()
        {
            if(Smoke)return;
            string id=ControlledCrewId;var actor=CrewActor(id);if(actor==null||!CrewAlive(id))return;
            if(Input.GetKeyDown(KeyCode.Alpha1))District.crew.For(id).holstered=true;
            if(Input.GetKeyDown(KeyCode.Alpha2)||Input.GetKeyDown(KeyCode.Alpha3)||Input.GetKeyDown(KeyCode.Alpha4)||Input.GetKeyDown(KeyCode.Alpha5))District.crew.For(id).holstered=false;
            if(Input.GetKeyDown(KeyCode.R)&&District.crew.For(id).armed)actor.combat.BeginReload(actor.ammo);
            Ray ray=View.ScreenPointToRay(Input.mousePosition);var plane=new Plane(Vector3.up,Vector3.up*1.1f);
            if(!plane.Raycast(ray,out float distance))return;
            var point=ray.GetPoint(distance);bool world=!showMap&&!CrewPointerBlocked(Input.mousePosition)&&Input.mousePosition.y/Screen.height>.18f&&Input.mousePosition.y/Screen.height<.70f;
            if(!world||!Input.GetMouseButton(0))return;
            if(CrewGunDrawn(id))
            {
                FireActorAt(actor,CrewBody(id),point,actor.combat.kind);
            }
            else if(attackCooldown<=0)
            {
                RefreshCombatTargets();foreach(var target in combatTargets)
                    if(!IsCrewId(target.actor.id)&&target.actor.health>0&&Vector3.Distance(actor.position,target.actor.position)<2.6f&&Vector3.Distance(point,target.actor.position+Vector3.up*1.1f)<2&&City.Nav.Sight(actor.position,target.actor.position))
                    {attackCooldown=.7f;ApplyCombatHit(target.actor,target.body,18,id,false);break;}
            }
        }
        string CrewViolenceWitness(Vector3 position)
        {
            foreach(var a in Agents)if(a.Record!=null&&a.Record.health>0&&Vector3.Distance(a.Position,position)<18&&City.Nav.Sight(a.Position,position))return a.Record.id;
            return null;
        }
    }
}
