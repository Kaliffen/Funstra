using UnityEngine;

namespace Funstra
{
    public sealed partial class DistrictState
    {
        public bool refuge, reserveMedicine, tallyPetted, neriStoryHeard;
        public int refusedPatients, clinicContributions;
        public bool HasIncident(string kind) => incidents.Exists(e=>e.kind==kind);
        public bool CanUseRefuge => refuge&&trust>=3&&neri.health>0;
        public bool OpenRefuge(RunState run)
        {
            if(refuge||!recruited||trust<3||neri.health<=0||run.cash<120)return false;
            run.cash-=120;marketMoney+=60;clinicMoney+=60;clinicContributions+=60;refuge=true;
            Record("refuge","neri","You and Neri repaired the spare room. $60 bought repairs; $60 stays in the clinic fund. You have a shared refuge.");return true;
        }
        public bool FundClinic(RunState run)
        {
            if(!CanUseRefuge||run.cash<30)return false;
            run.cash-=30;clinicMoney+=30;clinicContributions+=30;
            Record("fund","player","You put $30 into the clinic fund. It can buy medicine while you are away.");return true;
        }
        public bool BuyClinicReserve()
        {
            if(!CanUseRefuge||supplierStock<2||clinicMoney<30)return false;
            supplierStock-=2;clinicStock+=2;clinicMoney-=30;supplierMoney+=30;shortageKnown=false;
            Record("supply","neri","You authorized two doses from the remaining supplier reserve for $30. No new stock was created.");return true;
        }
        public bool SetClinicPolicy(bool reserve)
        {
            if(!CanUseRefuge||reserveMedicine==reserve)return false;
            reserveMedicine=reserve;
            Record("policy","neri",reserve?"You reserved the last two clinic doses for the crew. Public patients will be turned away at that threshold.":"You reopened the shelf to public patients. The last dose may go to a stranger before you come home.");return true;
        }
        public bool RefugeRest()
        {
            if(!CanUseRefuge||bleeding||neri.bleeding||(health>=80&&(neri.health>=80||Vector3.Distance(neri.position,Clinic)>4)))return false;
            health=Mathf.Max(80,health);if(Vector3.Distance(neri.position,Clinic)<=4)neri.health=Mathf.Max(80,neri.health);
            Record("rest","neri","A meal and a clean bed restored up to 80 health for those at the clinic. No debt, no medicine spent. Serious wounds still need treatment.");return true;
        }
        public string NeriWords
        {
            get
            {
                if(trust<0)return "I can still put a needle through a vein. I can't put trust back where it was. Stand on the other side of the counter.";
                if(refuge&&reserveMedicine&&refusedPatients>0)return "Edda came after her shift. I told her the shelf was empty, with two doses behind my back. If this is our place, that lie belongs to both of us.";
                if(refuge&&clinicStock==0)return "We have a key and an empty shelf. The room is still yours. A refuge isn't much of one if we only open the door when things are easy.";
                if(refuge)return "I used to sleep with my shoes on. Last night I left them under the bed. Don't laugh. That's what your name beside mine on the door bought me.";
                if(incidents.Exists(e=>e.kind=="defeat"&&e.observer=="neri")&&recruited)return "You were heavier than you look. I kept talking on the way home so I'd know if you stopped answering. Next time, answer sooner.";
                if(shipmentOwner=="market")return "Mara found a buyer, then. I won't pretend I don't understand rent. But tonight I have to choose which patient hears 'come back tomorrow'.";
                if(trust>=3&&(guard.health<80||incidents.Exists(e=>e.kind=="incapacitated"&&e.observer=="rook")))return "The doses are here. So is Rook, in every flinch when a door bangs. I'll use what you brought. Don't ask me to celebrate how it came.";
                if(trust>=3&&released)return "You paid a debt that wasn't yours. My mother used to call that foolish. Then she would set another bowl on the table. There's room here, if you want it.";
                if(trust>=3)return "No receipt. I know what that means; I won't ask you to make up a nicer story. Six people get another morning. I can be grateful and afraid.";
                if(Carrying)return "Set it down gently. Those little bottles have crossed the port twice while Edda has been waiting on this step. You still get to choose where they end up.";
                return "Edda cleans the ferry engines. Her hand won't close. Ivo holds our six doses against a debt my mother signed after the flood. I keep her bed here. These days it belongs to whoever needs it.";
            }
        }
        public string IvoTitle => identified?"I REMEMBER YOU":HasIncident("settlement")?"SETTLED ISN'T FORGOTTEN":released?"YOUR NAME IS ON THE RECEIPT":discovered?"SIX EMPTY SPACES":"THE DEBT HAS A BUYER";
        public string IvoWords
        {
            get
            {
                if(identified&&guard.health<=0)return "Rook has a daughter who waits up for him. Spare me the speech about the clinic. Sixty settles my claim. It doesn't stand him back up.";
                if(identified)return "Rook gave me your face, not a description of your conscience. Sixty dollars withdraws his orders. The shipment is a separate debt.";
                if(HasIncident("settlement"))return "The account is settled. That's what the money buys: an end to collection. It doesn't buy the conversation we had before.";
                if(released&&shipmentOwner=="clinic")return "Neri got the bottles. I got the payment. You got to decide whose debt to carry. My father called that freedom. He never had to buy any.";
                if(released)return "Keep the receipt dry. Rook reads the stamp before he reads faces. The stock will wait now; your payment cancelled the buyer's claim.";
                if(discovered)return "Six spaces on the shelf. Someone will be asked to cover that loss. If you hear a name, bring me a name. I don't have yours in that column.";
                return "My father kept this ledger through the flood. People thanked him for writing down what they owed. Nobody told their children the ink wouldn't wash out. The buyer comes at 21:52.";
            }
        }
    }
}
