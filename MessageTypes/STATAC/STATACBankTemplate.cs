using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class STATACBankTemplate
{
    //Bank ID (ex: 10 (Leumi))
    public string BankID { get; set; }
    
    //Name (Ex: Leumi)
    public string BankName { get; set; }

    //MASK for account. b = Branch, a = account, S = ADDITIONAL, f = Refered as Coin?, x = skip.
    //Example: bbbsssaaaaaaxxxxx
    public string MaskID { get; set; }
    
}
