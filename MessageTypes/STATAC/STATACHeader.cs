public class STATACHeader
{
    public string format { get; set; }
    public string statement { get; set; }
    public string bank = "0";
    public string bankCustomerNumber { get; set; }
    public string accountName { get; set; }
    public DateTime broadcastDate { get; set; }
    public string ADP { get; set; }
}
