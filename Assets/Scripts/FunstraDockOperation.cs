using UnityEngine;

namespace Funstra
{
    public sealed partial class FunstraGame
    {
        GameObject dockComponent,dockComponentRing,dockWater,dockPump;
        Transform dockArt;
        float dockPickupProgress;
        string dockPickupWorker="",dockDialog="";
        public bool DockDialogOpen {get;private set;}
        public bool DockEnabled=>CrewEnabled&&!FoundationMode&&District.dock!=null;
        public Vector3 DockComponentPosition=>District.dock.componentOwner=="yard"?DockOperationState.ComponentPost:
            District.dock.componentOwner=="rell-workshop"?DockOperationState.RepairPost:CrewPosition(District.dock.componentOwner);
        public string DockObjective=>!DockEnabled?"":District.dock.stakeResolved?"Rell's pump is back. Keep your crew alive.":
            DockOperationState.IsCarrier(District.dock.componentOwner)?"Pump component: "+District.dock.componentOwner+". Return it to Rell at the east workshop.":
            District.dock.auxiliaryRepaired?"Rell joined after the repair. His impounded pump remains in Vale's yard.":
            "Rell needs his impounded pump. Find him at the east workshop; Vale's public counter is on the quay.";
        public void CloseDockDialog(){DockDialogOpen=false;dockDialog="";}
        public bool DockYardEngaged(Vector3 position)=>!DockEnabled?position.x>8&&position.z>29:
            District.dock.yardHostile||(!District.dock.released&&position.x>18&&position.x<46&&position.z>29&&position.z<46);

        public void InitializeDockOperation()
        {
            CloseDockDialog();dockPickupProgress=0;dockPickupWorker="";
            District.InitializeDockState();
            if(dockArt)dockArt.gameObject.SetActive(DockEnabled);
            if(!DockEnabled)return;
            // Reload interrupts unfinished work, but never recreates a used gasket or component.
            District.dock.InterruptRepair();
            if(!dockArt)
            {
                dockArt=new GameObject("Rell / finite dock operation").transform;
                dockComponent=City.Box("Impounded pump impeller / unique custody",Vector3.zero,new Vector3(.85f,.7f,.6f),CityArt.Hex("D3AD67"),dockArt);
                City.Shape("Pump shaft",PrimitiveType.Cylinder,new Vector3(0,.15f,0),new Vector3(.4f,1,.4f),CityArt.Hex("718D91"),dockComponent.transform);
                dockComponentRing=City.Ring("Pump component",DockOperationState.ComponentPost,1.2f,CityArt.Amber);dockComponentRing.transform.SetParent(dockArt,true);
                dockPump=City.Box("Workshop auxiliary pump",DockOperationState.RepairPost+new Vector3(0,.6f,1),new Vector3(1.2f,1.2f,.8f),CityArt.Hex("657D79"),dockArt);
                City.Box("Repair crank",new Vector3(.8f,.2f,0),new Vector3(.5f,.15f,.15f),CityArt.Amber,dockPump.transform);
                dockWater=City.Box("Flooded workshop sump",DockOperationState.RepairPost+new Vector3(0,.09f,0),new Vector3(3.5f,.02f,2.5f),CityArt.Hex("3E666A"),dockArt);
                City.Sign("RELL / PUMPS & REPAIRS",DockOperationState.Workshop+Vector3.up*2.9f,CityArt.Amber,.13f,dockArt);
                City.Sign("VALE / RELEASE PAPERS / $90",DockOperationState.PublicCounter+Vector3.up*2.8f,CityArt.Amber,.11f,dockArt);
                City.Ring("Vale / public counter",DockOperationState.PublicCounter,1.1f,CityArt.Amber).transform.SetParent(dockArt,true);
            }
            CombatActorHit-=DockActorHit;CombatActorHit+=DockActorHit;SyncDockArt();
        }
        void DockActorHit(DistrictActor actor,string owner)
        {
            if(!DockEnabled||!DockOperationState.IsCarrier(owner)||actor==null||actor.id==null||!actor.id.StartsWith("yard-"))return;
            if(!District.dock.yardHostile)District.Record("yard attacked",owner,"Vale's people saw their crew attacked. Release papers no longer buy safe passage.");
            District.dock.yardHostile=true;District.dock.yardAttacked=true;
        }
        public void StepDockOperation(float dt)
        {
            if(!DockEnabled)return;
            if(District.dock.repairProgress>0&&(!CrewAlive(ControlledCrewId)||!DockNear(DockOperationState.RepairPost,2.4f)||District.dock.repairWorker!=ControlledCrewId))District.dock.InterruptRepair();
            SyncDockArt();
        }
        void SyncDockArt()
        {
            if(!dockComponent)return;
            dockComponent.transform.position=DockComponentPosition+Vector3.up*(DockOperationState.IsCarrier(District.dock.componentOwner)?1.05f:.55f);
            dockComponentRing.SetActive(District.dock.componentOwner=="yard");dockWater.SetActive(!District.dock.WorkshopDrained);
            if(District.dock.WorkshopDrained)dockPump.transform.rotation=Quaternion.Euler(0,Mathf.Sin(District.clock*8)*3,0);
        }
        bool DockNear(Vector3 target,float range)=>Vector3.Distance(ControlledPosition,target)<range&&City.Nav.Sight(ControlledPosition,target)&&City.Nav.ClearWalk(ControlledPosition,target);
        DistrictActor DockVale=>District.squad?.members?.Find(m=>m.actor!=null&&m.actor.id=="yard-0")?.actor;
        bool DockValeAtCounter=>DockVale!=null&&DockVale.health>0&&Vector3.Distance(DockVale.position,DockOperationState.PublicCounter)<9&&City.Nav.Sight(DockVale.position,DockOperationState.PublicCounter);
        bool DockWitness()
        {
            if(yardSquad==null)return false;
            foreach(var m in yardSquad.Members)
                if(m.Body&&ActorSees(m.Actor,m.Body,ControlledPosition,Sneaking?8:18))return true;
            return false;
        }
        public bool UpdateDockInteraction(float dt,bool pressed,bool held)
        {
            if(!DockEnabled||!CrewAlive(ControlledCrewId))return false;
            var d=District.dock;string actor=ControlledCrewId;
            if(d.repairProgress>0&&(!held||!DockNear(DockOperationState.RepairPost,2.4f)))d.InterruptRepair();
            if(d.componentOwner=="yard"&&DockNear(DockOperationState.ComponentPost,2.4f))
            {
                prompt="HOLD E / "+(d.released?"COLLECT RELEASED PUMP":"UNFASTEN IMPOUNDED PUMP")+" / "+Mathf.FloorToInt(dockPickupProgress*100)+"%";
                if(!held){dockPickupProgress=0;dockPickupWorker="";return true;}
                if(dockPickupWorker!=actor){dockPickupProgress=0;dockPickupWorker=actor;}
                Stealing=!d.released;dockPickupProgress+=dt/(d.released?1:4);
                if(!d.released&&DockWitness())
                {
                    if(!d.theftWitnessed)District.Record("pump theft witnessed",actor,"The yard watch saw "+actor+" unfasten Rell's impounded pump.");
                    d.theftWitnessed=true;d.yardHostile=true;yardSquad?.Alert(ControlledPosition);
                }
                if(dockPickupProgress>=1&&d.TryTake(actor,DockWitness(),d.approach))
                {dockPickupProgress=0;Save();SyncDockArt();Notify("Pump carried by "+actor+". Bring it to Rell's workshop.");}
                return true;
            }
            dockPickupProgress=0;dockPickupWorker="";
            if(d.componentOwner=="yard"&&ControlledPosition.z<34&&ControlledPosition.z>27&&ControlledPosition.x>25.5f&&ControlledPosition.x<34.5f&&!City.GateClosed)d.approach="service gate";
            if(DockOperationState.IsCarrier(d.componentOwner)&&d.componentOwner!=actor&&DockNear(CrewPosition(d.componentOwner),2.5f))
            {
                string from=d.componentOwner;prompt="E / TAKE PUMP FROM "+from.ToUpperInvariant();
                if(pressed&&d.TryTransfer(from,actor)){District.Record("pump transfer",actor,from+" passed custody of the pump to "+actor+".");Save();Notify("Pump now carried by "+actor+".");}return true;
            }
            if(d.componentOwner==actor&&CrewAlive("rell")&&DockNear(DockOperationState.Workshop,3)&&Vector3.Distance(District.crew.rell.position,DockOperationState.Workshop)<4)
            {
                prompt="E / RETURN PUMP / GET RELL'S WORKSHOP RUNNING";
                if(pressed&&d.TryReturn(actor,true))
                {District.Record("pump returned",actor,"Rell fitted the impeller. His workshop can drain the dock cellars again.");RecruitDockMechanic("You brought my work back. I'll stand with you.");Save();SyncDockArt();}return true;
            }
            if(d.repairAccepted&&!d.auxiliaryRepaired&&DockNear(DockOperationState.RepairPost,2.4f))
            {
                prompt="HOLD E / RESEAL AUXILIARY PUMP / "+Mathf.CeilToInt(DockOperationState.RepairSeconds-d.repairProgress)+"s / 1 GASKET";
                if(held&&CrewAlive("rell")&&d.WorkRepair(actor,dt))
                {
                    if(!d.repairPracticeAwarded){d.repairPracticeAwarded=true;District.crew.mechanicsPractice++;}
                    District.Record("workshop repaired",actor,actor+" spent the workshop's last gasket and resealed the auxiliary pump. The flooded sump is draining.");
                    RecruitDockMechanic("You stayed for the dirty work. I'll come with you. Vale still has my main pump.");Save();SyncDockArt();
                }
                else if(!held)d.InterruptRepair();return true;
            }
            if(DockNear(DockOperationState.PublicCounter,2.7f)&&d.componentOwner=="yard")
            {
                prompt=DockValeAtCounter?"E / VALE / PUMP RELEASE TERMS":"VALE IS AWAY OR DOWN / NO RELEASE CLERK";
                if(pressed&&DockValeAtCounter){dockDialog="vale";DockDialogOpen=true;}return true;
            }
            if(CrewAlive("rell")&&actor!="rell"&&DockNear(District.crew.rell.position,2.8f))
            {
                prompt="E / RELL / WORKSHOP & CREW";
                if(pressed){d.metRell=true;dockDialog="rell";DockDialogOpen=true;Save();}return true;
            }
            return false;
        }
        void RecruitDockMechanic(string words)
        {
            if(!District.crew.rellRecruited)
            {
                District.crew.rellRecruited=true;District.crew.rell.order="Follow";
                var order=District.crew.For("rell");order.order="Follow";order.memory=words;
                District.Record("partnership","rell","Rell joined after you helped his workshop. "+words);
            }
            Notify("Rell: "+words);
        }
        public void DrawDockOperationUI()
        {
            if(!DockEnabled||!DockDialogOpen)return;
            var d=District.dock;
            if(dockDialog=="vale")
            {
                DistrictPanel("PUBLIC QUAY / VALE / CASH $"+State.cash,"THE PUMP HAS A PRICE",d.yardHostile?"Your crew attacked mine. I'm not signing you through.":d.released?"Paid and signed. Collect the impeller from the loading bay. My watch will let you through.":"Ninety dollars clears Rell's impeller. Pay here, then collect it. Walk past the winch into my yard without papers and the watch will stop you.");
                Text("ONE COMPONENT / $90 TO VALE\nA release covers the yard while your crew keeps the peace. Vale must be here and conscious to sign.",395,425,805,105,20,paper);
                DistrictAction(d.released?"RELEASE PAID":"PAY $90 / SAFE COLLECTION",DockValeAtCounter&&!d.released&&!d.yardHostile&&State.cash>=90,395,575,805,()=>
                {if(d.TryPayRelease(State,true)){District.Record("pump release","vale","Paid Vale $90 for Rell's one impounded impeller. Safe collection is cleared.");yardSquad?.ClearContact();CloseDockDialog();Notify("Release paid. Collect the pump inside the yard.");}});
            }
            else
            {
                DistrictPanel("EAST WORKSHOP / RELL",d.stakeResolved?"THE WATER IS MOVING":"MY TOOLS. THEIR LOCK.",d.stakeResolved?"That impeller keeps the cellar pumps alive. You got my work back. If someone leaves you bleeding out there, I'll be the one coming back.":d.auxiliaryRepaired?"The auxiliary pump is holding. You earned a place beside me. My main impeller is still locked in Vale's yard.":"Vale impounded my pump for a debt I already paid. Without it, the dock cellars stay flooded. Bring it home and I'll come with you. Or help me get this auxiliary pump working first.");
                Text(d.auxiliaryRepaired?"AUXILIARY RUNNING / LAST GASKET USED\nThe workshop sump is drained. Your completed repair earned mechanics practice.":"Two ways into the same yard: public quay past the winch, or the service gate from the south. Vale sells release papers for $90 at the public counter.\nRepair work: hold E at the pump for 12 seconds. It uses our last gasket and drains the workshop sump.",395,414,805,135,19,paper);
                DistrictAction(d.auxiliaryRepaired?"AUXILIARY PUMP REPAIRED":d.repairAccepted?"RETURN TO THE REPAIR PUMP":"I'LL HELP RESEAL THE AUXILIARY PUMP",!d.auxiliaryRepaired,395,575,805,()=>{d.BeginRepair();CloseDockDialog();Notify("At the workshop pump, hold E for 12 seconds to fit the last gasket.");});
            }
            DistrictAction("BACK TO THE STREET",true,395,680,805,CloseDockDialog);
        }
    }
}
