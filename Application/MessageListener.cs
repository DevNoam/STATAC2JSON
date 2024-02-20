///////////////////////////////
// Message Listener 2024 DevNoam.
//
//This class Listenes for new message that comes into the messages folder.
///////////////////////////////
using Newtonsoft.Json;
using System.Text;

public class MessageListener
{
    //Create a Queue for new Messages
    Queue<FileSystemEventArgs> STATACQueue = new Queue<FileSystemEventArgs>();

    //PAYEXT, will be used in future release, not sure if it will be locally stored or based on web API.
    //Queue<FileSystemEventArgs> PAYEXTQueue = new Queue<FileSystemEventArgs>();

    //Time to wait before starting message parsing when the server detects a new message.
    public float timeToWaitBeforeProcessing = 3f;
    private float currentTimePassed = 0;
    public void StartListen()
    {
        Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\MSGS");
        //Get current app directory (Can be anything we want to listen for new messages into)
        string directoryPath = System.IO.Directory.GetCurrentDirectory() + @"\MSGS";
        
        ///////////////////////////////
        //RESERVED: Find messages that haven't been procecesed (If files have been added while the server is off, they will never be proccessed.)
        //////////////////////////////

        //Start the Waiter loop (Like a Update function of an GameEngine)
        Thread updateThread = new Thread(Waiter);
        updateThread.Start();        


        // Create instance of fileWatcher based on the directoryPath
        FileSystemWatcher watcher = new FileSystemWatcher(directoryPath);

        //Subscribe to file events in the explorer:
        watcher.Created += OnFileCreated;
        watcher.Renamed += OnFileRenamed;
        //watcher.Changed += OnFileChanged;     
        //watcher.Deleted += OnFileDeleted;


        // Set the option to include subdirectories, Can be useful if will be used for sub accounts.
        //Currently its set to true since we dont have specific static folder for new messages.
        watcher.IncludeSubdirectories = true; 
        watcher.EnableRaisingEvents = true;

        //Print data that the server has been started.
        Console.WriteLine("File watch process started for path: " + directoryPath);
        Console.WriteLine("-----------------------------------");

        //Display status message in the console, prints data in Queue and Q to quit
        Timer statusUpdateTimer = new Timer(_ => DisplayStatusBar($"In Queue: {STATACQueue.Count}; Press Q to stop the process"), null, 0, 500);
        
        
        // Wait for the user to press 'q' to exit the program
        while (Console.ReadKey().KeyChar != 'q') { }
        if (STATACQueue.Count > 0)
        { 
            Console.WriteLine("Finishing message Queue and stopping server.");
            while(STATACQueue.Count > 0) { }
        }

        // Dispose the watcher when done
        statusUpdateTimer.Dispose();
        watcher.Dispose();
        Console.WriteLine("Server stopped.");
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        string fileType = Path.GetExtension(e.FullPath);
        string subfolderName = Path.GetFileName(Path.GetDirectoryName(e.FullPath));
        string fileName = Path.GetFileName(e.FullPath);
        if (fileType.ToUpper() != ".MSQ")
            return;
        currentTimePassed = 0;
        Console.WriteLine("Detected MSQ file");
        //Add to queue
        STATACQueue.Enqueue(e);
    }

    private void OnFileRenamed(object sender, FileSystemEventArgs e)
    {
        string fileType = Path.GetExtension(e.FullPath);
        // Call your function or logic here
        if (fileType.ToUpper() != ".MSQ")
            return;
        currentTimePassed = 0;
        Console.WriteLine($"New {e.ChangeType} MSQ found: {e.FullPath}");
        //Add to queue
        STATACQueue.Enqueue(e);
    }

    private async void Waiter()
    {
        //Wait for messages
        //Start procecessing messages only after the number of messages in the queue are stable for a few seconds.
        while (true)
        {
            // Your update code here
            if (STATACQueue.Count > 0)
            {
                Console.WriteLine("time: " + currentTimePassed);
                if (currentTimePassed >= timeToWaitBeforeProcessing)
                {
                    Console.WriteLine($"Starting process of {STATACQueue.Count} messages.");
                    Console.WriteLine("-----------------------------------");
                    int messagesCount = 0; //Amount of messages prosscessed
                    while (STATACQueue.Count > 0)
                    {
                        FileSystemEventArgs message = STATACQueue.Dequeue();
                        Console.WriteLine($"working on message");
                        ProcessSTATAC(message);
                        messagesCount++;
                    }
                    currentTimePassed = 0;
                    Console.WriteLine($"Operation completed with {messagesCount} messages.");
                    Console.WriteLine("-----------------------------------");
                }
                currentTimePassed++;
            }
            // Introduce a delay to control the update rate
            Thread.Sleep(1000); // Sleep for 1 second (adjust as needed)
        }
    }

    private bool ProcessSTATAC(FileSystemEventArgs file)
    {
        MessageUnpacker unpacker = new MessageUnpacker();
        Console.WriteLine($"New Message detected for {Path.GetFileName(Path.GetDirectoryName(file.FullPath))}: ");
        Console.WriteLine($"File: {Path.GetFileName(file.FullPath)}");

        Console.WriteLine("Starting parsing..");
        //wait
        STATAC message = unpacker.ParseData(file.FullPath);
        if (message != null)
        {
            //TEMPORARY CREATE TEMP FILE FOR MONITORING.
            Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(file.FullPath), "ParsedData"));

            string newFilePath = Path.Combine(Path.GetDirectoryName(file.FullPath), "ParsedData", Path.GetFileNameWithoutExtension(file.FullPath) + "_ParsedData.json");

            using (StreamWriter writer = new StreamWriter(newFilePath, false, Encoding.UTF8))
            {
                writer.Write(JsonConvert.SerializeObject(message, Formatting.Indented));
            }

            Console.WriteLine("Starting Uploading process..");



            //Upload data to DB
            bool uploadStatus = DBManager.PushTransaction(message);
            Console.WriteLine("Finished.");
        }
        else
        {
            Console.WriteLine("Message cancelled: " + file.FullPath);
            return false;
        }

        if (message.format == "STATAC")
        {
            // Move the file to temp folder
            Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(file.FullPath), "TMP"));
            try
            {
                File.Move(file.FullPath, Path.Combine(Path.GetDirectoryName(file.FullPath), "TMP", Path.GetFileNameWithoutExtension(file.FullPath) + ".tmp"));

            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case UnauthorizedAccessException:
                        Console.WriteLine("Error: No permissions to access the specified folder.");
                        break;
                    default:
                        Console.WriteLine($"Error moving the file: {ex.Message}");
                        break;
                }
            }

        }
        Console.WriteLine("-----------------------------------");
        return true;
    }

    void DisplayStatusBar(string status)
    {
        int originalTop = Console.CursorTop;
        int originalLeft = Console.CursorLeft;

        Console.SetCursorPosition(0, Console.WindowHeight - 1); // Set cursor to the last line
        Console.Write(new string(' ', Console.WindowWidth)); // Clear the last line

        Console.SetCursorPosition(0, Console.WindowHeight - 1);
        Console.Write(status);

        Console.SetCursorPosition(originalLeft, originalTop); // Restore original cursor position
    }
}
