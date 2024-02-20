using LiteDB;
public class STATAC
{
    /// <summary>
    /// Mark this as STATAC, Default is STATAC.
    /// </summary>
    public string format { get; set; }
    // Bank Additional info, can be date, bank data etc.. unrelevant most of the time
    public string statement { get; set; }
    //Bank ID
    public string bank { get; set; }
    //Bank customer number (Like id, not the account. most of the time its branch + account combined)
    public string bankCustomerNumber { get; set; }
    //Name of the account holder (For example "BezeqInt LTD corp 1996")
    public string accountName { get; set; }
    //Date the message has been sent to the customer. Not a transaction date.
    public DateTime broadcastDate { get; set; }
    //Something related to DateTime.. Not really sure what its about
    public string ADP { get; set; }
    //Account type (AV), not really sure what its about..
    public string accountType { get; set; }
    //Account branch, (Mostly 3 numbers)
    public string accountBranch { get; set; }
    //Account number (ranged between 6-9 numbers)
    public string account { get; set; }
    //Additional number (mostly 3 numbers.), Most in use in Leumi.
    public string accountIncluded { get; set; }
    //Currency type (ILS, USD, EUR)
    public string currency { get; set; }
    //Balance remaining/ Before the message. Not been checked
    public long balance { get; set; }
    ///
    ///List of the transactions
    ///
    public List<STATACTransaction> Transaction { get; set; }
}
