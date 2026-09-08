using System;
using System.Collections.Generic;
using UnityEngine;

namespace Funstra
{
    public sealed class WeaponSpec
    {
        public readonly int kind, magazine, pellets;
        public readonly float damage, speed, range, interval, reload, spread;
        public readonly string name;
        WeaponSpec(int k,string n,int mag,int count,float hit,float velocity,float reach,float cycle,float load,float cone)
        { kind=k;name=n;magazine=mag;pellets=count;damage=hit;speed=velocity;range=reach;interval=cycle;reload=load;spread=cone; }
        public static readonly WeaponSpec Pistol=new WeaponSpec(2,"PISTOL",6,1,28,48,36,.46f,1.5f,0);
        public static readonly WeaponSpec Shotgun=new WeaponSpec(3,"SHOTGUN",2,7,12,40,24,1.05f,2.3f,8);
        public static readonly WeaponSpec SMG=new WeaponSpec(4,"SMG",18,1,15,55,32,.11f,2,2);
        public static readonly WeaponSpec Rifle=new WeaponSpec(5,"RIFLE",5,1,30,86,40,.8f,2.7f,.35f);
        public static WeaponSpec For(int kind) => kind==5?Rifle:kind==4?SMG:kind==3?Shotgun:Pistol;
    }
    [Serializable] public sealed class WeaponState
    {
        public int kind=2,magazine;
        public float reloadRemaining,cooldown;
        public bool initialized;
        public void Initialize(int total,int weaponKind=2)
        { kind=weaponKind;if(!initialized) {magazine=Mathf.Min(WeaponSpec.For(kind).magazine,total);initialized=true;}magazine=Mathf.Clamp(magazine,0,Mathf.Min(total,WeaponSpec.For(kind).magazine)); }
        public bool BeginReload(int total)
        { if(reloadRemaining>0||magazine>=WeaponSpec.For(kind).magazine||total<=magazine)return false;reloadRemaining=WeaponSpec.For(kind).reload;return true; }
        public void CancelReload() { reloadRemaining=0; }
        public void Step(float dt,int total)
        {
            if(dt<=0)return;
            cooldown=Mathf.Max(0,cooldown-dt);magazine=Mathf.Min(magazine,total);
            if(reloadRemaining>0) {reloadRemaining=Mathf.Max(0,reloadRemaining-dt);if(reloadRemaining==0)magazine=Mathf.Min(total,WeaponSpec.For(kind).magazine);}
        }
        public bool Fire(ref int total)
        { if(cooldown>0||reloadRemaining>0||magazine<=0||total<=0)return false;total--;magazine--;cooldown=WeaponSpec.For(kind).interval;return true; }
        public bool Valid(int total) => total>=0&&magazine>=0&&magazine<=Mathf.Min(total,WeaponSpec.For(kind).magazine)&&ProjectileMath.Finite(reloadRemaining)&&reloadRemaining>=0&&reloadRemaining<=3&&ProjectileMath.Finite(cooldown)&&cooldown>=0&&cooldown<=2&&(kind==2||kind==3||kind==4||kind==5);
    }
    [Serializable] public sealed class CombatProjectile
    {
        public Vector3 position,velocity;
        [NonSerialized] public Dictionary<string,Vector3> birthPositions;
        [NonSerialized] public Dictionary<Collider,Bounds> birthObstacles;
        public float remaining,damage;
        public string owner;
        public int kind;
        public bool Valid() => ProjectileMath.Finite(position)&&ProjectileMath.Finite(velocity)&&velocity.sqrMagnitude>1&&velocity.sqrMagnitude<=10000&&ProjectileMath.Finite(remaining)&&remaining>0&&remaining<=40&&ProjectileMath.Finite(damage)&&damage>0&&damage<=30&&!string.IsNullOrEmpty(owner)&&(kind==2||kind==3||kind==4||kind==5);
    }
    public sealed partial class DistrictState
    {
        public CombatSquadState squad=new CombatSquadState();
        public WeaponState pistolWeapon=new WeaponState(),shotgunWeapon=new WeaponState{kind=3},smgWeapon=new WeaponState{kind=4};
        public int shotgunAmmo=8,smgAmmo,equippedWeapon=2;
        public int rifleAmmo;
        public WeaponState rifleWeapon=new WeaponState{kind=5};
        public List<CombatProjectile> projectiles=new List<CombatProjectile>();
        public void InitializeWeapons()
        {
            if(pistolWeapon==null)pistolWeapon=new WeaponState();if(shotgunWeapon==null)shotgunWeapon=new WeaponState{kind=3};if(smgWeapon==null)smgWeapon=new WeaponState{kind=4};
            pistolWeapon.Initialize(ammo,2);shotgunWeapon.Initialize(shotgunAmmo,3);smgWeapon.Initialize(smgAmmo,4);
            if(rifleWeapon==null)rifleWeapon=new WeaponState{kind=5};rifleWeapon.Initialize(rifleAmmo,5);
            if(projectiles==null)projectiles=new List<CombatProjectile>();
        }
        public bool CombatValid()
        {
            if(shotgunAmmo<0||smgAmmo<0||rifleAmmo<0||equippedWeapon<1||equippedWeapon>5||squad!=null&&!squad.Valid())return false;
            if(rifleWeapon!=null&&!rifleWeapon.Valid(rifleAmmo))return false;
            if(pistolWeapon!=null&&!pistolWeapon.Valid(ammo)||shotgunWeapon!=null&&!shotgunWeapon.Valid(shotgunAmmo)||smgWeapon!=null&&!smgWeapon.Valid(smgAmmo))return false;
            if(projectiles!=null) {if(projectiles.Count>128)return false;foreach(var p in projectiles)if(p==null||!p.Valid())return false;}
            return true;
        }
    }
    public static class ProjectileMath
    {
        public const float Radius=.045f;
        public static bool Finite(float x)=>!float.IsNaN(x)&&!float.IsInfinity(x);
        public static bool Finite(Vector3 v)=>Finite(v.x)&&Finite(v.y)&&Finite(v.z);
        // Relative motion prevents a moving actor crossing the traveled segment between snapshots.
        public static bool MovingSphere(Vector3 from,Vector3 to,Vector3 oldCenter,Vector3 newCenter,float radius,out float fraction)
        {
            Vector3 a=from-oldCenter,d=(to-from)-(newCenter-oldCenter);float c=Vector3.Dot(a,a)-radius*radius;
            if(c<=0){fraction=0;return true;}float length=Vector3.Dot(d,d),b=Vector3.Dot(a,d);
            float discriminant=b*b-length*c;
            if(length<.0000001f||discriminant<0){fraction=0;return false;}
            fraction=(-b-Mathf.Sqrt(discriminant))/length;return fraction>=0&&fraction<=1;
        }
        public static bool MovingBounds(Vector3 from,Vector3 to,Bounds previous,Bounds current,float radius,out float fraction)
        {
            Vector3 relativeEnd=to-(current.center-previous.center);Bounds shape=previous;
            shape.size=Vector3.Max(previous.size,current.size)+Vector3.one*(radius*2);
            if(shape.Contains(from)){fraction=0;return true;}
            Vector3 d=relativeEnd-from;float length=d.magnitude;
            if(length>.000001f&&shape.IntersectRay(new Ray(from,d/length),out float distance)&&distance<=length){fraction=distance/length;return true;}
            fraction=0;return false;
        }
    }
}
