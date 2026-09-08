using System;
using System.Collections.Generic;
namespace Funstra
{
    [Serializable] public sealed class ProjectileHitReceipt
    {public string owner,victim;public int kind;public float damage,time;}
    public sealed partial class FunstraGame
    {
        public readonly List<ProjectileHitReceipt> ProjectileHits=new List<ProjectileHitReceipt>();
        void RecordProjectileHit(string owner,string victim,int kind,float damage)
        {
            InterruptCrewAid(victim);
            ProjectileHits.Add(new ProjectileHitReceipt{owner=owner,victim=victim,kind=kind,damage=damage,time=District.clock});
            if(ProjectileHits.Count>512)ProjectileHits.RemoveAt(0);
        }
    }
}
