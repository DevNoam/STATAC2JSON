using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class BanksManager
{
    //Contains data of each bank about how to parse its STATAC file.

    public static List<STATACBankTemplate> banks;

    public static STATACBankTemplate GetBank(string bankID)
    {

        foreach (var bank in banks)
        {
            if (bank.BankID == bankID)
            { 
                return bank;
            }
        }
        return null;
    }

    public static void FetchBanksFromDB() => banks = DBManager.GetBanksTemplate();
}
