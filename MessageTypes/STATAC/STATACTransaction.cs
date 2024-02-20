public class STATACTransaction
{
    public string refernce { set; get; }
    public string fullTransactionDetails { set; get; }
    public string transactionDetails { set; get; }
    public string transactionCode { set; get; }
    //public long transaction { set; get; }
    public long operationValue { set; get; }
    public bool isPlusOperation { set; get; }
    public long balance { set; get; }
    public DateOnly ValueDate { set; get; }
    public DateOnly ProccessingDate { set; get; }
    public DateOnly Date { set; get; }
}
