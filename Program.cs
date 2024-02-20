///////////////////////////////
// STATAC extractor server 2024 DevNoam.
// ------------------------------------------------------------------------------------------
// THIS SOFTWARE ONLY CONVERTS STATAC FILES INTO JSON.
// TO GET STATAC FILE USER MUST USE 'CTB' SOFTWARE 'CATAV' TO CONVERT .MSU TO .MSQ
// ------------------------------------------------------------------------------------------
///////////////////////////////
using System.Text;
using System.Text.RegularExpressions;


public class Program
{ 
    public static Encoding hebrewEncoding;
    static void Main(string[] args)
    {
        //Initial app startup 
        Console.WriteLine(@"+--------------------------------------------------------------------------------------------------------------------+
|                                                                                                                    |
|  ▓█████▄ ▓█████ ██▒   █▓ ███▄    █  ▒█████   ▄▄▄       ███▄ ▄███▓                                                  |
|  ▒██▀ ██▌▓█   ▀▓██░   █▒ ██ ▀█   █ ▒██▒  ██▒▒████▄    ▓██▒▀█▀ ██▒                                                  |
|  ░██   █▌▒███   ▓██  █▒░▓██  ▀█ ██▒▒██░  ██▒▒██  ▀█▄  ▓██    ▓██░                                                  |
|  ░▓█▄   ▌▒▓█  ▄  ▒██ █░░▓██▒  ▐▌██▒▒██   ██░░██▄▄▄▄██ ▒██    ▒██                                                   |
|  ░▒████▓ ░▒████▒  ▒▀█░  ▒██░   ▓██░░ ████▓▒░ ▓█   ▓██▒▒██▒   ░██▒                                                  |
|   ▒▒▓  ▒ ░░ ▒░ ░  ░ ▐░  ░ ▒░   ▒ ▒ ░ ▒░▒░▒░  ▒▒   ▓▒█░░ ▒░   ░  ░                                                  |
|   ░ ▒  ▒  ░ ░  ░  ░ ░░  ░ ░░   ░ ▒░  ░ ▒ ▒░   ▒   ▒▒ ░░  ░      ░                                                  |
|   ░ ░  ░    ░       ░░     ░   ░ ░ ░ ░ ░ ▒    ░   ▒   ░      ░                                                     |
|     ░       ░  ░     ░           ░     ░ ░        ░  ░       ░                                                     |
|   ░                 ░                                                                                              |
|    ▄████  ▒█████   ██▓    ▓█████▄  ███▄    █ ▓█████▄▄▄█████▓     ██████ ▓█████  ██▀███   ██▒   █▓▓█████  ██▀███    |
|   ██▒ ▀█▒▒██▒  ██▒▓██▒    ▒██▀ ██▌ ██ ▀█   █ ▓█   ▀▓  ██▒ ▓▒   ▒██    ▒ ▓█   ▀ ▓██ ▒ ██▒▓██░   █▒▓█   ▀ ▓██ ▒ ██▒  |
|  ▒██░▄▄▄░▒██░  ██▒▒██░    ░██   █▌▓██  ▀█ ██▒▒███  ▒ ▓██░ ▒░   ░ ▓██▄   ▒███   ▓██ ░▄█ ▒ ▓██  █▒░▒███   ▓██ ░▄█ ▒  |
|  ░▓█  ██▓▒██   ██░▒██░    ░▓█▄   ▌▓██▒  ▐▌██▒▒▓█  ▄░ ▓██▓ ░      ▒   ██▒▒▓█  ▄ ▒██▀▀█▄    ▒██ █░░▒▓█  ▄ ▒██▀▀█▄    |
|  ░▒▓███▀▒░ ████▓▒░░██████▒░▒████▓ ▒██░   ▓██░░▒████▒ ▒██▒ ░    ▒██████▒▒░▒████▒░██▓ ▒██▒   ▒▀█░  ░▒████▒░██▓ ▒██▒  |
|   ░▒   ▒ ░ ▒░▒░▒░ ░ ▒░▓  ░ ▒▒▓  ▒ ░ ▒░   ▒ ▒ ░░ ▒░ ░ ▒ ░░      ▒ ▒▓▒ ▒ ░░░ ▒░ ░░ ▒▓ ░▒▓░   ░ ▐░  ░░ ▒░ ░░ ▒▓ ░▒▓░  |
|    ░   ░   ░ ▒ ▒░ ░ ░ ▒  ░ ░ ▒  ▒ ░ ░░   ░ ▒░ ░ ░  ░   ░       ░ ░▒  ░ ░ ░ ░  ░  ░▒ ░ ▒░   ░ ░░   ░ ░  ░  ░▒ ░ ▒░  |
|  ░ ░   ░ ░ ░ ░ ▒    ░ ░    ░ ░  ░    ░   ░ ░    ░    ░         ░  ░  ░     ░     ░░   ░      ░░     ░     ░░   ░   |
|        ░     ░ ░      ░  ░   ░             ░    ░  ░                 ░     ░  ░   ░           ░     ░  ░   ░       |
|                            ░                                                                 ░                     |
|                                                                                                                    |
+--------------------------------------------------------------------------------------------------------------------+");
        ///////////////////////////////
        // Change and prepare encoding
        ///////////////////////////////
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        hebrewEncoding = Encoding.GetEncoding("DOS-862");

        //Change encoding for the console
        Console.OutputEncoding = Encoding.GetEncoding("Windows-1255");

        ///////////////////////////////
        // Connect to DB
        ///////////////////////////////
        //Connect to LiteDB (MongoDB in the future?), Mode is reserved for later development (SHARED/DIRECT Connection)
        DBManager.InstantiateDBEngine(Directory.GetCurrentDirectory());

        //Fetch banks from DB, important for the Listener instead of fetching each time.
        BanksManager.FetchBanksFromDB();

        //Push bank information.
        //DBManager.PushBank();


        ///////////////////////////////
        // Start web server process
        ///////////////////////////////
        // Reserved


        ///////////////////////////////
        // Start the listener server
        ///////////////////////////////
        //Start the message listening, and start working as software
        MessageListener listener = new MessageListener();
        listener.StartListen();

        Console.SetOut(new StreamWriter(Directory.GetCurrentDirectory() + @"\Output.txt"));
    }


    public static string ConvertUnicode(string unicode)
    {
        var result = Regex.Replace(string.Concat(unicode.Reverse()), @"[0-9]*\.[0-9]+-?", match => string.Concat(match.Value.Reverse()));
        return result;
    }
}
