using System;
using UnityEngine;

namespace Funstra
{
    public sealed partial class FunstraGame
    {
        AudioClip[] concreteSteps;
        AudioClip bodyImpactReport, reloadFinishReport, shotgunActionReport;
        AudioSource footstepSource;
        AudioSource rifleReportSource;
        int footstepIndex;
        float nextImpactAudio;

        static AudioClip PortClip(string name)
        {
            var clip = Resources.Load<AudioClip>("Audio/" + name);
            if (!clip) throw new InvalidOperationException("Missing recorded sound asset: " + name);
            return clip;
        }
        void LoadPortAudio()
        {
            pickupSound=PortClip("pickup");cashSound=PortClip("cash");alertSound=PortClip("alert");
            concreteSteps=new[]{PortClip("step-01"),PortClip("step-02"),PortClip("step-03"),PortClip("step-04")};
            stepSound=concreteSteps[0];
            footstepSource=gameObject.AddComponent<AudioSource>();
            footstepSource.playOnAwake=false;footstepSource.spatialBlend=0;footstepSource.priority=180;
        }
        void LoadCombatAudio()
        {
            pistolReport=PortClip("pistol");shotgunReport=PortClip("shotgun");
            impactReport=PortClip("impact-cover");bodyImpactReport=PortClip("impact-body");
            reloadReport=PortClip("reload-start");reloadFinishReport=PortClip("reload-finish");
            shotgunActionReport=PortClip("shotgun-action");
            if(!rifleReportSource)
            {
                rifleReportSource=gameObject.AddComponent<AudioSource>();
                rifleReportSource.playOnAwake=false;rifleReportSource.spatialBlend=0;
                // A separate pitched recorded report gives the rifle a sharper crack
                // without changing pitch on concurrently playing pistol or impact sounds.
                rifleReportSource.pitch=1.3f;
            }
        }
        void PlayFootstep(float volume)
        {
            if (!footstepSource || concreteSteps==null) return;
            // Deterministic variation does not consume the gameplay random stream.
            int index=footstepIndex++%concreteSteps.Length;
            footstepSource.pitch=index%2==0?.98f:1.02f;
            footstepSource.PlayOneShot(concreteSteps[index],volume);
        }
        void PlayWeaponReport(int gun,Vector3 position)
        {
            if (!audioSource) return;
            float distance=Player?Vector3.Distance(Player.position,position):0;
            if(gun==5&&rifleReportSource){rifleReportSource.PlayOneShot(shotgunReport,.85f/(1+distance*.045f));return;}
            audioSource.PlayOneShot(gun==3?shotgunReport:pistolReport,1/(1+distance*.055f));
        }
        void PlayImpactAudio(Vector3 position,bool cover)
        {
            // Seven pellets can hit in one frame. One physical impact accent prevents an
            // artificial sevenfold gain spike while all seven damage/collision events remain.
            if (!audioSource || Time.unscaledTime<nextImpactAudio) return;
            nextImpactAudio=Time.unscaledTime+.055f;
            float distance=Player?Vector3.Distance(Player.position,position):0;
            audioSource.PlayOneShot(cover?impactReport:bodyImpactReport,.45f/(1+distance*.08f));
        }
        void PlayReloadFinished()
        { if(audioSource)audioSource.PlayOneShot(weapon==3?shotgunActionReport:reloadFinishReport,.8f); }
    }
}
