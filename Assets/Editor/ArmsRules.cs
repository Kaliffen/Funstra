using System;
using System.IO;
using Funstra;
using UnityEngine;

public static class ArmsRules
{
    static DistrictState Fresh() {var d=new DistrictState();d.InitializeArms(true);return d;}
    static int Accounted(DistrictState d,int kind)
    {return d.ArmsAmmo(kind)+d.arms.ammoStock[kind]+d.arms.sourceAmmo[kind]+d.arms.spent[kind]+d.arms.confiscated[kind]-d.arms.recovered[kind]+(d.arms.armsInTransit?ArmsState.Manifest(kind):0);}
    static bool MigrationDiagnostic(bool passed,string path,RunState migrated)
    {
        if(passed)return true;
        Directory.CreateDirectory("Evidence");string raw=File.ReadAllText(path);
        File.WriteAllText("Evidence/arms-migration-failure-source.json",raw);
        File.WriteAllText("Evidence/arms-migration-failure-result.json",migrated==null?"null":JsonUtility.ToJson(migrated,true));
        var decoded=JsonUtility.FromJson<RunState>(raw);var d=decoded==null?null:decoded.district;
        File.WriteAllText("Evidence/arms-migration-failure-summary.txt","Load returned "+(migrated==null?"null":"a state")+"\nRaw decoded district: "+(d==null?"null":"schema="+d.armsVersion+"; arms="+(d.arms==null?"null":"object")+"; pistol="+d.ammo+"; shotgun="+d.shotgunAmmo+"; districtValid="+d.Valid())+"\n");
        return false;
    }
    public static void Verify()
    {
        int count=0;Action<bool,string> check=(ok,label)=>{if(!ok)throw new Exception("Arms rule: "+label);count++;};
        var legacy=new DistrictState();check(legacy.arms==null&&legacy.ammo==12&&legacy.shotgunAmmo==8,"Direct legacy fixtures preserve historical equipment");
        legacy.ammo=5;legacy.shotgunAmmo=3;legacy.InitializeArms(false);
        check(legacy.arms.Owns(2)&&legacy.arms.Owns(3)&&!legacy.arms.Owns(4)&&legacy.ammo==5&&legacy.shotgunAmmo==3,"Legacy migration preserves exact remaining ammunition and gun ownership");
        legacy.InitializeArms(true);check(legacy.ammo==5,"Repeated initialization cannot reset a loaded campaign");
        var placeholder=new DistrictState{arms=new ArmsState{sourceAmmo=null}};
        check(placeholder.Valid(),"Legacy validation ignores Unity's unversioned inline arms placeholder");placeholder.InitializeArms(false);
        check(placeholder.armsVersion==1&&placeholder.Valid()&&placeholder.arms.Owns(2),"Explicit schema migration replaces an inline placeholder with legacy ownership");
        var d=Fresh();var run=new RunState{district=d,cash=1000};
        check(d.Valid()&&d.equippedWeapon==1&&d.arms.Owns(1)&&!d.arms.Owns(2)&&d.ammo==0&&d.shotgunAmmo==0&&d.smgAmmo==0,"Fresh campaign starts with fists and no firearm ammunition");
        check(!d.BuyGun(run,1)&&!d.BuyAmmo(run,2)&&run.cash==1000,"Invalid gun and unowned ammunition purchase leave money intact");
        check(d.BuyGun(run,2)&&d.ammo==0&&run.cash==910&&d.arms.dealerMoney==90&&d.arms.gunStock[2]==1,"Gun transaction transfers money and one empty gun");
        check(!d.BuyGun(run,2)&&run.cash==910,"Owned firearm cannot repeatedly consume stock or money");
        check(d.BuyAmmo(run,2)&&d.ammo==6&&d.arms.ammoStock[2]==30&&run.cash==892,"Ammunition purchase transfers the exact finite pack");
        int pistolTotal=Accounted(d,2),shotgunTotal=Accounted(d,3),smgTotal=Accounted(d,4);
        d.InitializeWeapons();check(d.pistolWeapon.Fire(ref d.ammo),"Owned purchased ammunition fires through the real weapon state");d.arms.RecordFired(2);
        check(Accounted(d,2)==pistolTotal&&d.arms.spent[2]==1,"Spent round remains in the conservation ledger");
        d.ConfiscateArms();check(d.ammo==0&&!d.arms.Owns(2)&&d.equippedWeapon==1&&d.arms.confiscated[2]==5&&Accounted(d,2)==pistolTotal,"Confiscation removes ownership and exact remaining rounds");
        d.ConfiscateArms();check(d.arms.confiscated[2]==5,"Repeated confiscation cannot duplicate ledger losses");
        var favor=Fresh();check(favor.BeginArmsFavor()&&!favor.BeginArmsFavor()&&!favor.CompleteArmsFavor(),"Favor starts once and requires a receipt");
        check(favor.CollectArmsReceipt()&&!favor.CollectArmsReceipt()&&favor.TotalMedicine==12,"Receipt transfers once without creating medicine");
        check(favor.CompleteArmsFavor()&&favor.arms.Owns(2)&&favor.ammo==6&&favor.arms.gunStock[2]==1&&favor.arms.ammoStock[2]==30,"Completed favor pays one actual stocked pistol and six rounds");
        check(!favor.CompleteArmsFavor()&&favor.ammo==6,"Favor reward cannot repeat");
        var duplicateFavor=Fresh();duplicateFavor.BuyGun(new RunState{cash=90},2);
        check(!duplicateFavor.BeginArmsFavor()&&duplicateFavor.arms.gunStock[2]==1,"An existing pistol prevents a favor that would discard a duplicate gun");
        duplicateFavor=Fresh();duplicateFavor.BeginArmsFavor();duplicateFavor.CollectArmsReceipt();duplicateFavor.BuyGun(new RunState{cash=90},2);
        duplicateFavor=JsonUtility.FromJson<DistrictState>(JsonUtility.ToJson(duplicateFavor));
        check(duplicateFavor.Valid()&&duplicateFavor.arms.favorStage==1&&duplicateFavor.arms.favorReceipt&&duplicateFavor.arms.Owns(2),"Accepted favor and signed receipt persist when another pistol is acquired");
        var emptyGunShelf=JsonUtility.FromJson<DistrictState>(JsonUtility.ToJson(duplicateFavor));emptyGunShelf.arms.gunStock[2]=0;
        check(emptyGunShelf.CompleteArmsFavor()&&emptyGunShelf.ammo==6&&emptyGunShelf.arms.gunStock[2]==0,"An already armed courier can finish the favor even when the gun shelf is empty");
        check(duplicateFavor.CompleteArmsFavor()&&duplicateFavor.arms.gunStock[2]==1&&duplicateFavor.arms.ammoStock[2]==30&&duplicateFavor.ammo==6,"Acquiring a pistol during the favor awards only six rounds and retains the spare gun");
        duplicateFavor=JsonUtility.FromJson<DistrictState>(JsonUtility.ToJson(duplicateFavor));
        check(duplicateFavor.Valid()&&duplicateFavor.arms.favorStage==2&&!duplicateFavor.CompleteArmsFavor()&&duplicateFavor.ammo==6,"Completed ammunition-only favor persists and cannot pay twice");
        var theft=Fresh();check(theft.TakeDealerGun(3,false)&&!theft.arms.dealerHostile&&theft.arms.dealerTrust==0&&theft.shotgunAmmo==0,"Unseen theft transfers a gun without invented identification or ammunition");
        check(theft.TakeDealerGun(4,true)&&theft.arms.dealerHostile&&theft.arms.dealerTrust==-2,"Witnessed theft creates a persistent dealer refusal");
        check(!theft.BuyGun(new RunState{cash=1000},2)&&!theft.BeginArmsFavor(),"Identified thief cannot buy or start another favor");
        var actor=new DistrictActor("test","TEST",Vector3.zero){ammo=7,combat=new WeaponState{kind=4}};
        check(!d.RecoverWeapon(actor),"Living actors cannot be looted");actor.health=0;
        check(!d.RecoverWeapon(actor),"Unarmed downed residents cannot yield a default weapon");actor.weaponRecoverable=true;
        check(d.RecoverWeapon(actor)&&d.smgAmmo==7&&d.arms.Owns(4)&&actor.ammo==0&&actor.looted&&d.arms.recovered[4]==7,"Recovery transfers actual SMG and remaining rounds from a downed actor");
        check(!d.RecoverWeapon(actor)&&d.smgAmmo==7&&Accounted(d,4)==smgTotal,"Repeated recovery cannot mint equipment or ammunition");
        var saved=JsonUtility.FromJson<DistrictState>(JsonUtility.ToJson(d));
        var savedActor=JsonUtility.FromJson<DistrictActor>(JsonUtility.ToJson(actor));
        check(saved.Valid()&&!saved.RecoverWeapon(savedActor)&&saved.arms.spent[2]==1&&saved.arms.confiscated[2]==5,"Round ledgers and looted corpse survive JSON reload");
        for(int cycle=1;cycle<=3;cycle++)
        {
            check(d.DispatchSupply()&&d.arms.supplyCycle==cycle&&d.TotalMedicine==12+cycle*2&&d.Valid(),"Dispatch consumes finite source and accounts for transit medicine, cycle "+cycle);
            check(!d.DispatchSupply(),"Cannot dispatch over pending consignment, cycle "+cycle);
            var transit=JsonUtility.FromJson<DistrictState>(JsonUtility.ToJson(d));
            check(transit.Valid()&&transit.arms.armsInTransit&&transit.arms.medicineInTransit&&transit.TotalMedicine==d.TotalMedicine,"Transit survives reload without duplication, cycle "+cycle);
            if(cycle==2)
            {
                check(d.DivertArmsSupply()&&d.DivertClinicSupply()&&d.marketStock==0&&d.arms.divertedMedicine==2,"Diverted manifests enter player custody without remote delivery");
                var carried=JsonUtility.FromJson<DistrictState>(JsonUtility.ToJson(d));
                check(carried.Valid()&&carried.arms.divertedMedicine==2&&carried.TotalMedicine==16,"Carried diverted medicine survives reload with its real custody");
                int saleCash=run.cash,marketCash=d.marketMoney;
                check(d.SellDivertedMedicine(run)&&d.marketStock==2&&d.arms.divertedMedicine==0&&run.cash==saleCash+40&&d.marketMoney==marketCash-40,"Mara receives the carried doses and pays from finite market cash");
                check(!d.SellDivertedMedicine(run)&&!d.DonateDivertedMedicine(),"Sold diverted medicine cannot be sold or donated twice");
            }
            else check(d.DeliverArmsSupply()&&d.DeliverClinicSupply(),"Separate dealer and clinic manifests arrive, cycle "+cycle);
            check(!d.DeliverArmsSupply()&&!d.DeliverClinicSupply()&&!d.DivertArmsSupply()&&!d.DivertClinicSupply(),"Delivered or stolen consignment cannot be transferred again, cycle "+cycle);
            check(Accounted(d,2)==pistolTotal&&Accounted(d,3)==shotgunTotal&&Accounted(d,4)==smgTotal&&d.Valid(),"All ammunition and medicine remain conserved, cycle "+cycle);
        }
        check(!d.DispatchSupply()&&d.arms.sourceMedicine==0&&d.arms.sourceAmmo[2]==0&&d.arms.sourceAmmo[3]==0&&d.arms.sourceAmmo[4]==0&&d.trust==1,"Only three source consignments and one goodwill reward exist");
        var donation=Fresh();donation.DispatchSupply();donation.DivertClinicSupply();donation.neri.health=0;
        check(!donation.DonateDivertedMedicine()&&donation.arms.divertedMedicine==2,"Unavailable Neri cannot accept carried medicine");donation.neri.health=100;
        check(donation.DonateDivertedMedicine()&&!donation.DonateDivertedMedicine()&&donation.clinicStock==4&&donation.trust==1&&donation.Valid(),"Donation moves real carried medicine into clinic custody and rewards trust once");
        var confiscation=Fresh();confiscation.DispatchSupply();confiscation.DivertClinicSupply();confiscation.marketMoney=0;
        check(!confiscation.SellDivertedMedicine(run)&&confiscation.arms.divertedMedicine==2,"An empty market purse cannot buy or remove carried medicine");
        confiscation.ConfiscateArms();confiscation.ConfiscateArms();
        check(confiscation.arms.divertedMedicine==0&&confiscation.supplierStock==6&&confiscation.Valid(),"Confiscation returns carried medicine to finite supplier custody exactly once");
        var invalid=JsonUtility.FromJson<DistrictState>(JsonUtility.ToJson(d));invalid.arms.supplyCycle=4;check(!invalid.Valid(),"Save rejects a fabricated fourth consignment");
        invalid=JsonUtility.FromJson<DistrictState>(JsonUtility.ToJson(d));invalid.arms.importedMedicine++;check(!invalid.Valid(),"Save rejects unaccounted imported medicine");
        invalid=JsonUtility.FromJson<DistrictState>(JsonUtility.ToJson(d));invalid.arms.sourceAmmo=null;check(!invalid.Valid(),"Save rejects a missing supply source");
        invalid=Fresh();invalid.arms.favorReceipt=true;check(!invalid.Valid(),"Save rejects a receipt without an accepted favor");
        invalid=Fresh();invalid.arms.favorStage=2;check(!invalid.Valid(),"Save rejects favor completion without its signed receipt");
        string save=Path.Combine(Path.GetTempPath(),"funstra-arms-"+Guid.NewGuid().ToString("N")+".json");
        try
        {
            var oldRun=new RunState();oldRun.district.ammo=4;oldRun.district.shotgunAmmo=2;
            check(oldRun.Save(save),"Legacy save fixture written");
            File.WriteAllText(save,File.ReadAllText(save).Replace("\"armsVersion\"","\"preArmsUnusedField\""));
            var migrated=RunState.Load(save);
            check(MigrationDiagnostic(migrated!=null&&migrated.district.armsVersion==1&&migrated.district.arms!=null&&migrated.district.arms.Owns(2)&&migrated.district.ammo==4&&migrated.district.shotgunAmmo==2,save,migrated),"File load migrates legacy ownership without supplying fresh ammunition");
            var unarmed=new RunState{district=Fresh()};check(unarmed.Save(save),"Fresh unarmed save fixture written");var unarmedLoaded=RunState.Load(save);
            check(unarmedLoaded!=null&&unarmedLoaded.district.armsVersion==1&&!unarmedLoaded.district.arms.Owns(2)&&!unarmedLoaded.district.arms.Owns(3)&&unarmedLoaded.district.ammo==0&&unarmedLoaded.district.equippedWeapon==1,"Fresh unarmed schema cannot regain legacy firearms on load");
            unarmed.cash=108;unarmed.district.BuyGun(unarmed,2);unarmed.district.BuyAmmo(unarmed,2);unarmed.district.ConfiscateArms();
            check(unarmed.Save(save),"Confiscated equipment save fixture written");var confiscatedLoaded=RunState.Load(save);
            check(confiscatedLoaded!=null&&confiscatedLoaded.district.armsVersion==1&&!confiscatedLoaded.district.arms.Owns(2)&&confiscatedLoaded.district.ammo==0&&confiscatedLoaded.district.arms.confiscated[2]==6,"Confiscated firearms stay removed and confiscation ledger persists on load");
            check(run.Save(save),"Arms campaign save written");var loaded=RunState.Load(save);
            check(loaded!=null&&loaded.district.arms.supplyCycle==3&&loaded.district.smgAmmo==d.smgAmmo&&loaded.district.TotalMedicine==18,"Arms campaign file reload retains final supply custody and ownership");
        }
        finally {if(File.Exists(save))File.Delete(save);if(File.Exists(save+".tmp"))File.Delete(save+".tmp");}
        Directory.CreateDirectory("Evidence");File.WriteAllText("Evidence/arms-rules-result.txt","PASS: "+count+" acquisition, custody, supply and migration checks\n");
        Debug.Log("FUNSTRA ARMS RULES PASSED: "+count);
    }
}
