///////////////////////////////
// Database Manager 2024 DevNoam.
//
//This class handles DB communication.
///////////////////////////////
using LiteDB;

public class DBManager
{
    static LiteDatabase db;


    /// <summary>
    /// Create new Database connection
    /// </summary>
    public static void InstantiateDBEngine(string path)
    { 
        //Connection can be Shared or Direct. Shared will not lock the database, direct will lock and prevent other users to access to the DB.
        db = new LiteDatabase($@"Filename={path}\database.db;Connection=shared");
        if (db == null)
            Console.WriteLine("Couldn't start a connection with the DB!");
        else
            Console.WriteLine("DB Connection succeed!");

        //Console.WriteLine(DeleteBank("65d463b98bf8ba0b4ae313d3"));
    }


    /// <summary>
    /// Push transaction to the Database
    /// </summary>
    /// <param name="statacMessage"></param>
    /// <returns></returns>
    public static bool PushTransaction(STATAC statacMessage)
    {
        var col = db.GetCollection<STATAC>("STATACTransactions");

        // Insert new customer document (Id will be auto-incremented)
        BsonValue id = col.Insert(statacMessage);
        if (id != null)
        {
            Console.WriteLine("Message uploaded to DB!");
            return true;
        }
        else
        { 
            Console.WriteLine("Error: Message not uploaded.");
            return false;
        }

    }

    /// <summary>
    /// Delete bank from the Database, requires ObjectID.
    /// This function done in-Code only.
    /// </summary>
    private static bool DeleteBank(string bank)
    {
        var collection = db.GetCollection<STATACBankTemplate>("STATACBankstemplates");
        var objectId = new ObjectId(bank);
        return collection.Delete(objectId);
    }
    /// <summary>
    /// Push bank to the DB, internally used. this information is fixed and rarely updates.
    /// Console function haven't been implemented, must be done in code.
    /// </summary>
    public static void PushBank()
    {
        var collection = db.GetCollection<STATACBankTemplate>("STATACBankstemplates");
        var Bank = new STATACBankTemplate
        {
            BankID = "11",
            BankName = "בנק דיסקונט",
            MaskID = "BBBAAAAAAAAAAAXXX"
            /// B = B (Branch)
            /// A = A (Account)
            /// S = I (Included)
            /// F = C (Coin)
            /// X = X (Skip)
        };
        //var objectId = new ObjectId("6597308bb1caeb0171d2bef1");
        //Console.WriteLine(collection.Delete(objectId));
        collection.Insert(Bank);
    }

    /// <summary>
    /// This function returns the list of all the banks that present in the DB for message processing.
    /// </summary>
    /// <returns></returns>
    public static List<STATACBankTemplate> GetBanksTemplate()
    { 
        var collection = db.GetCollection<STATACBankTemplate>("STATACBankstemplates");
        List<STATACBankTemplate> Banks = collection.FindAll().ToList();
        if(Banks != null)
            Console.WriteLine("Banks have been fetched from DB!");

        return Banks;
    }
}
