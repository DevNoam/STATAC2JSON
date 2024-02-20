///////////////////////////////
// Message Unpacker 2024 DevNoam.
//
//This class processes the STATAC message into Json data and prepares it to DB uploading.
///////////////////////////////

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

public class MessageUnpacker
{
    //Get bank information (For example bank 10, relevant for MaskID)
    STATACBankTemplate theBankTemplate = new STATACBankTemplate();
    //Main function
    public STATAC ParseData(string path)
    {
        //Read the file and convert into array of strings
        string[] lines = null;
        try
        {
            lines = File.ReadAllLines($@"{path}", Program.hebrewEncoding);
        }
        catch (Exception)
        {
            Console.WriteLine("Canceled: Error reading the file");
        }
        lines = lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
        if (lines.Length <= 0)
            return null;

        //Create props
        STATAC STATACMessage = new STATAC(); // This is the data we return back
        STATACHeader STATACheader = new STATACHeader(); // Header line
        STATACSubHeader STATACsubheader = new STATACSubHeader(); // Subheader line
        STATACMessage.Transaction = new List<STATACTransaction>(); // Transactions line, this is a list containing all the transactions


        //Check the line
        foreach (string line in lines)
        {
            //The header line
            if (line.StartsWith("031000000"))
            {
                STATACheader = ParseHeaderLine(line);
                //Get the bank data from the list of banks we loaded into the app.
                theBankTemplate = BanksManager.GetBank(STATACheader.bank);
                if (theBankTemplate == null)
                { 
                    Console.WriteLine("This bank does not exist in the database, operation canceled.");
                    return null;
                }
            }
            //The subheader line
            else if (line.StartsWith("032000000"))
            {
                STATACsubheader = ParseSubHeaderLine(line, STATACheader.bank);
            }
            //Transaction line
            else if (line.StartsWith("033000000"))
            {
                STATACTransaction transaction = ParseTransaction(line);
                //Move the transaction to the transactions list
                STATACMessage.Transaction.Add(transaction);
            }
        }


        //Attach values to the STATAC message (Header & Subheader):
        STATACMessage.format = STATACheader.format;
        STATACMessage.statement = STATACheader.statement;
        STATACMessage.bank = STATACheader.bank;
        STATACMessage.bankCustomerNumber = STATACheader.bankCustomerNumber;
        STATACMessage.accountName = STATACheader.accountName;
        STATACMessage.broadcastDate = STATACheader.broadcastDate;
        STATACMessage.ADP = STATACheader.ADP;
        STATACMessage.accountType = STATACsubheader.accountType;
        STATACMessage.accountBranch = STATACsubheader.accountBranch;
        STATACMessage.account = STATACsubheader.account;
        STATACMessage.accountIncluded = STATACsubheader.accountIncluded;
        STATACMessage.currency = STATACsubheader.currency;
        STATACMessage.balance = STATACsubheader.balance;
        //Note that we dont attaching Transactions since we appended them before to the list.
        return STATACMessage;
    }

    // Header line Parser
    private STATACHeader ParseHeaderLine(string line)
    {
        //Create header prop that we will return later to 
        STATACHeader header = new STATACHeader();
        // Extracting data using substring
        string Format = line.Substring(09, 6).Trim(); //Tells if this is a STATAC Format.
        string Bank = line.Substring(15, 2).Trim(); // Checks for bank ID.
        string StatementData = line.Substring(17, 16).Trim(); //Beggining of bank included data, irelevant.
        string bankCustomerNumber = line.Substring(33, 12); // Bank customer number, relevant as the bank ID for classification in the panel.

        string accountName = line.Substring(45, 35).Trim(); // Name of the account, for displaying in the web.
        
        //Extract broadcast date (When the bank pushed this message)
        DateTime date;
        if (DateTime.TryParseExact(line.Substring(80, 12).Trim(), "yyyyMMddhhmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
        {
            //OK
            //Console.WriteLine("Parsed date: " + date.ToString("yyyy-MM-dd:hh-mm")); // Output in a different format if needed
        }
        else
        {
            Console.WriteLine("Invalid date format");
        }
        //Something related to DateTime, ADP seems like its Night, ADY = Day
        string ADP = line.Substring(92).Trim();

        //Push data
        header.format = Format;
        header.bank = Bank;
        header.statement = StatementData;
        header.accountName = Program.ConvertUnicode(accountName);
        header.broadcastDate = date;
        header.bankCustomerNumber = bankCustomerNumber;
        header.ADP = ADP;
        return header;
    }
    private STATACSubHeader ParseSubHeaderLine(string line, string banksId)
    {
        //Create subheader prop that we will return later to 
        STATACSubHeader subHeader = new STATACSubHeader();
        //Get the account type, mostly AV, not sure what its stand for..
        string AccountType = line.Substring(9, 3).Trim(); 

        //Get the MaskID from the bank we've fetched in the header.
        string MaskID = theBankTemplate.MaskID;
        //Get the account string (String that contains account, branch included...)
        string accountString = line.Substring(12, 17);
        string branch = null;
        string account = null;
        string additional = null;
        string coin = null;

        //If the MaskID is null, cancel
        if (MaskID == null) {
            Console.WriteLine("MaskID for bank: " + banksId + " is null");
        } 
        //If the MaskID is not equ to 17, cancel
        else if (MaskID.Length != 17)
        { 
            Console.WriteLine("MaskID for bank: " + banksId + " is insufficient length");
        }
        else if(accountString.Length == MaskID.Length)
        {
            //Iterat on all the MaskID characters.
            for (int i = 0; i < MaskID.Length; i++)
            {
                //If found specific character, for example A, push the accountString data based on the index.
                char character = MaskID[i];
                switch (character)
                {
                    case 'X':
                        // Continue, this is a skip character
                        break;
                    case 'B':
                        // This is the branch
                        branch += accountString[i];
                        break;
                    case 'A':
                        // This is the account
                        account += accountString[i];
                        break;
                    case 'I':
                        // This is the included data
                        additional += accountString[i];
                        break;
                    case 'C':
                        // This is Coin field, used mostly in discount bank.
                        coin += accountString[i];
                        break;
                    default:
                        // ERROR WITH THIS ACCOUNT STRING
                        Console.WriteLine($"Unexpected character '{character}' in the MaskID");
                        break;
                }
            }
        }
        //This is the balance after all the data has been procceeded.
        string currency = line.Substring(29, 3).Trim();
        //Converting it to a long..
        long balance = long.Parse(new string(line.Substring(32).Where(c => !char.IsWhiteSpace(c)).ToArray()));


        subHeader.accountType = AccountType;
        subHeader.accountBranch = branch;
        subHeader.account = account;
        subHeader.accountIncluded = additional;
        subHeader.accountCoin = coin;
        subHeader.currency = currency;
        subHeader.balance = balance;
        return subHeader;
    }

    //Parse Transaction, the most advanced function out of the 3
    private STATACTransaction ParseTransaction(string line)
    {
        STATACTransaction transaction = new STATACTransaction();
        //Get the referance data, (Asmachta)
        string referanceData = line.Substring(9, 32).Trim(); //Get reference data.
        
        // Operation code (Code peola) for the bank operation (Like a check, import, export).
        //We take 5 characters
        string codeDataVar = line.Substring(41, 5); //Code transaction data.
        //Most of the banks use 3 characters, some use 5. the code is Numeric numbers. Since sometimes we can pull non-numeric numbers. we clean the code to accept numbers only 0-9 using Regex.
        string codeData = Regex.Replace(codeDataVar, "[^0-9]", "");

        // we take the index 40 and start counting by each char. We start before the CodeData.
        int lastDigitIndex = 40;
        ////////////////////////////
        //Get transaction name.
        ////////////////////////////
        StringBuilder fullTransactionDetailsBuilder = new StringBuilder();

        // Extract characters until "+"
        // We loop forward until we reach +
        while (lastDigitIndex < line.Length && line[lastDigitIndex] != '+')
        {
            char currentChar = line[lastDigitIndex];

            fullTransactionDetailsBuilder.Append(currentChar);
            // Append everything, including the Code value
            lastDigitIndex++;
        }
        //Making string that contains details and the codeData combined
        string fullTransactionDetails = fullTransactionDetailsBuilder.ToString();
        //Making a string that contains only the transaction detail without the codeData. We take the fullTransaction and cut the codeData from the beggining of the string.
        string transactionDetails = fullTransactionDetails.Substring(codeData.Length + 1).TrimStart();
        //Note that we already have CodeData as string

        ////////////////////////////
        //Start parsing finance data.
        ////////////////////////////
        lastDigitIndex++;
        //Tells if this transaction is a Plus or a Minus.
        bool isPlusOperation = true;
        string operationValue = "";
        string balance = "";

        // Extract the first 16 characters (The operation value)
        StringBuilder dataBuilder = new StringBuilder();
        int charsRead = 0;
        while (charsRead < 16 && lastDigitIndex < line.Length)
        {
            char currentChar = line[lastDigitIndex];
            dataBuilder.Append(currentChar);
            lastDigitIndex++;
            charsRead++;
        }
        operationValue = dataBuilder.ToString().Trim();


        // Read the separator "4" or whitespace
        //4 digit means substrack (Minus operation), whitespace means addon (Plus operation)
        if (lastDigitIndex < line.Length && (line[lastDigitIndex] == '4' || char.IsWhiteSpace(line[lastDigitIndex])))
        {
            isPlusOperation = (line[lastDigitIndex] == '4') ? true : false;
            lastDigitIndex++;
        }
        dataBuilder.Clear(); // Clear the StringBuilder for reuse

        // Extract the remaining 18 characters after the separator
        while (charsRead < 34 && lastDigitIndex < line.Length)
        {
            char currentChar = line[lastDigitIndex];
            dataBuilder.Append(currentChar);
            lastDigitIndex++;
            charsRead++;
        }
        balance = dataBuilder.ToString().Trim();


        ///////////////////////
        ///    GET DATES
        ///////////////////////
        DateOnly valueDate;
        DateOnly proccessingDate;
        DateOnly date;
        
        //Get Value date
        if (DateOnly.TryParseExact(line.Substring(112, 8).Trim(), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out valueDate))
        {
            //OK
            //Console.WriteLine("Parsed date: " + valueDate.ToString("yyyy-MM-dd")); // Output in a different format if needed
        }
        else
        {
            Console.WriteLine("Invalid date format (Value date)");
        }
        //Get processing date
        if (DateOnly.TryParseExact(line.Substring(120, 8).Trim(), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out proccessingDate))
        {
            //OK
            //Console.WriteLine("Parsed date: " + proccessingDate.ToString("yyyy-MM-dd")); // Output in a different format if needed
        }
        else
        {
            Console.WriteLine("Invalid date format (Processing date)");
        }
        //Get Date (Not used in most of the files, if not used its 20000 etc)
        if (DateOnly.TryParseExact(line.Substring(128, 8).Trim(), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
        {
            //OK
            //Console.WriteLine("Parsed date: " + date.ToString("yyyy-MM-dd")); // Output in a different format if needed
        }
        else
        {
            Console.WriteLine("Invalid date format (date4)");
        }

        transaction.refernce = referanceData.Trim();
        transaction.fullTransactionDetails = Program.ConvertUnicode(fullTransactionDetails).Trim();
        transaction.transactionDetails = transactionDetails.Trim();
        transaction.operationValue = long.Parse(operationValue);
        transaction.balance = long.Parse(balance);
        transaction.isPlusOperation = isPlusOperation;
        transaction.transactionCode = codeData.Trim();
        transaction.ValueDate = valueDate;
        transaction.ProccessingDate = proccessingDate;
        transaction.Date = date;

        return transaction;
    }
}