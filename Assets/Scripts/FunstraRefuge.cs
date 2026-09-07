using UnityEngine;

namespace Funstra
{
    public sealed partial class FunstraGame
    {
        GameObject refugeSign,refugeLamp;
        bool conversationIvo;
        float petTime;
        static readonly Vector3 TallyPosition=DistrictState.Clinic+new Vector3(1.6f,0,-1.8f);
        void BuildRefugeArt()
        {
            refugeSign=City.Box("Shared refuge / repaired door",DistrictState.Clinic+new Vector3(-2.6f,1.5f,-3.6f),new Vector3(.18f,2.5f,1.25f),medical);
            refugeLamp=City.Box("Shared refuge / porch light",DistrictState.Clinic+new Vector3(-2.4f,2.7f,-3.6f),new Vector3(.25f,.3f,.45f),CityArt.Amber,null,false,true);
            City.Sign("TALLY",TallyPosition+Vector3.up*1.1f,CityArt.Amber,.09f);
        }
        void SyncRefugeArt()
        { refugeSign.SetActive(District.refuge);refugeLamp.SetActive(District.CanUseRefuge); }
        void UpdateRefugeInteraction()
        {
            if(petTime>0) { petTime-=Time.deltaTime;figure.Find("Right arm").localRotation=Quaternion.Euler(-65+Mathf.Sin(petTime*10)*12,0,0);catTail.localRotation=Quaternion.Euler(0,65+Mathf.Sin(petTime*12)*15,0); }
            if(Vector3.Distance(Player.position,TallyPosition)<3&&Input.GetKeyDown(KeyCode.P))PetTally();
            if(Vector3.Distance(Player.position,DistrictState.Clinic)<4&&Input.GetKeyDown(KeyCode.F))screen=ScreenMode.Refuge;
        }
        void PetTally()
        {
            if(!District.tallyPetted)District.Record("tally","neri","Tally put his head into your hand. Neri: He only does that to people who sit still long enough.");
            petTime=1.5f;Vector3 towards=TallyPosition-Player.position;towards.y=0;if(towards.sqrMagnitude>.01f)figure.rotation=Quaternion.LookRotation(towards);Sound(stepSound);
            District.tallyPetted=true;Save();Notify("Tally leans into your hand, purring. For a moment, nobody needs anything.");
            catTail.localRotation=Quaternion.Euler(0,85,0);
        }
        void DrawRefuge()
        {
            bool here=Vector3.Distance(Player.position,DistrictState.Clinic)<4;
            DistrictPanel("REPAIR / A SMALL FOOTHOLD","KEEP THE LIGHTS ON",!District.refuge?"Neri: The spare room leaks. Help me fix it and put something in the fund. A bed without a creditor. A door with both our names. Then we decide who gets the last dose.":District.NeriWords);
            Text("FUND $"+District.clinicMoney+"   SHELF "+District.clinicStock+"   SUPPLIER "+District.supplierStock+"\n"+(District.reserveMedicine?"RESERVE LAST 2 FOR CREW":"PUBLIC CARE / USE EVERY DOSE")+"   |   "+District.refusedPatients+" turned away by policy",395,408,805,67,18,medical,FontStyle.Bold);
            if(!District.refuge)
            {
                Text("$120: $60 repairs + $60 clinic fund. Requires Neri's partnership.\nBenefit: recover to 80 HP here without debt or medicine; bandage bleeding first.\nThe supplier has only four reserve doses for this demo. Money cannot conjure stock.",395,499,805,113,19,paper);
                DistrictAction("REPAIR & SHARE THE ROOM / $120",here&&District.recruited&&District.trust>=3&&State.cash>=120,395,613,805,()=>District.OpenRefuge(State));
            }
            else
            {
                bool allowed=here&&District.CanUseRefuge;
                DistrictAction("ADD $30 TO FUND",allowed&&State.cash>=30,395,501,395,()=>District.FundClinic(State));
                DistrictAction("BUY 2 DOSES / FUND $30",allowed&&District.supplierStock>=2&&District.clinicMoney>=30,806,501,395,()=>District.BuyClinicReserve());
                DistrictAction(District.reserveMedicine?"OPEN SHELF TO EVERYONE":"RESERVE LAST 2 FOR CREW",allowed,395,562,395,()=>District.SetClinicPolicy(!District.reserveMedicine));
                DistrictAction("REST TO 80 / NO DEBT",allowed&&!District.bleeding&&!District.neri.bleeding&&(District.health<80||District.neri.health<80),806,562,395,()=>District.RefugeRest());
                Text("Public care uses 1 dose every 90s. Reserve policy stops it at 2.\nAutomatic supply: 2 doses / $30 every 4 minutes if shelf <2. No offline clock.",395,625,805,49,16,quiet);
            }
            DistrictAction("BACK TO NERI",true,395,702,805,()=>screen=ScreenMode.Clinic);
        }
        void DrawConversation()
        {
            DistrictPanel(conversationIvo?"IVO / AFTER THE NUMBERS":"NERI / AFTER THE SHIFT",conversationIvo?District.IvoTitle:"THE BED WAS MY MOTHER'S",conversationIvo?District.IvoWords:District.NeriWords);
            Text(conversationIvo?"\"I could burn the ledger,\" he says, pressing a bent corner flat. \"The Combine has another copy. I tell myself that's why I haven't.\"":"\"She repaired pumps. Never trained as a doctor. When the water came, people brought her everything broken. Engines. Hands. Children.\"\n\nNeri folds the same clean towel twice. \"I inherited the room and the debt. I kept the door open. I don't always know which part of that was a choice.\"",395,423,805,205,22,paper);
            DistrictAction("STAY A MOMENT, THEN GO BACK",true,395,680,805,()=>{if(!conversationIvo&&!District.neriStoryHeard){District.neriStoryHeard=true;District.Record("conversation","neri","Neri told you about their mother, the flood and the room they kept open.");}screen=conversationIvo?ScreenMode.Collector:ScreenMode.Clinic;});
        }
    }
}
