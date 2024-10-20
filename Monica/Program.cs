using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Colorful;
using Console = Colorful.Console;
using System.Drawing;

class MonicaApp
{
    const string AsciiArt = @"
███    ███  ██████  ███    ██ ██  ██████  █████  
████  ████ ██    ██ ████   ██ ██ ██      ██   ██ 
██ ████ ██ ██    ██ ██ ██  ██ ██ ██      ███████ 
██  ██  ██ ██    ██ ██  ██ ██ ██      ██   ██  
██      ██  ██████  ██   ████ ██  ██████ ██   ██ 

Notice: Monica will be updated with a fiddler / tencer type of proxy server that redirects chosen urls to a new url 

Repo: https://github.com/MatchPort/Monica

To start, create a monica file using option 2!
";
    static Dictionary<string, string> redirectMap = new Dictionary<string, string>();

    static void Main()
    {
        Console.Title = "Monica | API / URL Exchanger | github.com/MatchPort/Monica";
        Console.WriteLine(AsciiArt, Color.PaleVioletRed);

        bool exitProgram = false;

        while (!exitProgram)
        {
            ShowMainMenu();
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    ExchangeUrls();
                    break;
                case "2":
                    CreateMonicaFile();
                    break;
                case "3":
                    exitProgram = true;
                    Console.WriteLine("Exiting the application. Goodbye!", Color.LightGreen);
                    break;
                default:
                    Console.WriteLine("Invalid selection, please try again.", Color.Red);
                    break;
            }
        }
    }

    static void ShowMainMenu()
    {
        Console.WriteLine("--- Main Menu ---", Color.LightYellow);
        Console.WriteLine("1. Exchange URLs");
        Console.WriteLine("2. Create Monica File");
        Console.WriteLine("3. Exit");
        Console.Write("Select an option: ", Color.LightYellow);
    }

    static void ExchangeUrls()
    {
        Console.WriteLine("\n--- Exchange URLs ---", Color.LightYellow);

        string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string[] monicaFiles = Directory.GetFiles(exeDirectory, "*.monica");

        if (monicaFiles.Length == 0)
        {
            Console.WriteLine("No .monica files found in the directory!", Color.Red);
            return;
        }

        Console.WriteLine("Select a .monica file to process:\n", Color.LightYellow);
        for (int i = 0; i < monicaFiles.Length; i++)
        {
            Console.WriteLine($"{i + 1}: {Path.GetFileName(monicaFiles[i])}", Color.White);
        }

        int selectedFileIndex = -1;
        while (selectedFileIndex < 0 || selectedFileIndex >= monicaFiles.Length)
        {
            Console.Write("\nEnter the number of the file you want to use: ", Color.LightYellow);
            string input = Console.ReadLine();

            if (int.TryParse(input, out selectedFileIndex) && selectedFileIndex >= 1 && selectedFileIndex <= monicaFiles.Length)
            {
                selectedFileIndex--;
            }
            else
            {
                Console.WriteLine("Invalid selection. Please try again.", Color.Red);
                selectedFileIndex = -1;
            }
        }

        string selectedFilePath = monicaFiles[selectedFileIndex];
        List<string> urls = File.ReadAllLines(selectedFilePath).Where(line => !string.IsNullOrWhiteSpace(line)).ToList();

        Console.WriteLine("\nMonica file loaded:", Color.Green);
        urls.ForEach(url => Console.WriteLine(url, Color.White));

        Console.Write("\nEnter the redirect main domain: ", Color.LightYellow);
        string redirectDomain = Console.ReadLine();

        string logFolder = "logs";
        if (!Directory.Exists(logFolder))
        {
            Directory.CreateDirectory(logFolder);
        }

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string logFileName = Path.Combine(logFolder, $"monica_log_{timestamp}.txt");

        using (StreamWriter logFile = new StreamWriter(logFileName, true))
        {
            logFile.WriteLine("// START OF MONICA LOG //");

            foreach (var url in urls)
            {
                string validUrl = ValidateAndFixUrl(url);

                if (validUrl == null)
                {
                    Console.WriteLine($"Invalid URL: {url}", Color.Red);
                    continue;
                }

                Uri uri = new Uri(validUrl);
                string newUrl = $"{redirectDomain}{uri.PathAndQuery}";

                logFile.WriteLine(newUrl);
            }

            logFile.WriteLine("\nExchange Log:");

            Console.WriteLine("\nRedirecting URLs...\n", Color.LightGreen);
            foreach (var url in urls)
            {
                string validUrl = ValidateAndFixUrl(url);

                if (validUrl == null)
                {
                    Console.WriteLine($"Invalid URL: {url}", Color.Red);
                    continue;
                }

                Uri uri = new Uri(validUrl);
                string newUrl = $"{redirectDomain}{uri.PathAndQuery}";

                logFile.WriteLine($"{url} → {newUrl}");
                redirectMap[url] = newUrl;

                Console.WriteLine($"{url} ➔ {newUrl}", Color.Magenta);
            }

            logFile.WriteLine("// END OF LOG //");
        }


        Console.Clear();
        Console.WriteLine($"███    ███  ██████  ███    ██ ██  ██████  █████  \r\n████  ████ ██    ██ ████   ██ ██ ██      ██   ██ \r\n██ ████ ██ ██    ██ ██ ██  ██ ██ ██      ███████ \r\n██  ██  ██ ██    ██ ██  ██ ██ ██      ██   ██  \r\n██      ██  ██████  ██   ████ ██  ██████ ██   ██ \r\n                    \nLog file created: {logFileName}", Color.LightGreen);
    }

    static void CreateMonicaFile()
    {
        Console.WriteLine("\n--- Create Monica File ---", Color.LightYellow);

        List<string> urls = new List<string>();

        while (true)
        {
            Console.Write("Enter a URL (or type 'finished' to complete): ", Color.LightYellow);
            string url = Console.ReadLine();

            if (url.ToLower() == "finished")
            {
                break;
            }

            if (!string.IsNullOrWhiteSpace(url))
            {
                urls.Add(url);
            }
            else
            {
                Console.WriteLine("Invalid URL, please try again.", Color.Red);
            }
        }

        if (urls.Count == 0)
        {
            Console.WriteLine("No URLs entered. Returning to main menu.", Color.Red);
            return;
        }

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string monicaFileName = $"monica_{timestamp}.monica";

        Console.Clear();
        File.WriteAllLines(monicaFileName, urls);
        Console.WriteLine($"███    ███  ██████  ███    ██ ██  ██████  █████  \r\n████  ████ ██    ██ ████   ██ ██ ██      ██   ██ \r\n██ ████ ██ ██    ██ ██ ██  ██ ██ ██      ███████ \r\n██  ██  ██ ██    ██ ██  ██ ██ ██      ██   ██  \r\n██      ██  ██████  ██   ████ ██  ██████ ██   ██ \r\n                    \nMonica file created: {monicaFileName}", Color.LightGreen);
    }

    static string ValidateAndFixUrl(string url)
    {
        if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
        {
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                url = "http://" + url;
            }

            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                return null;
            }
        }

        return url;
    }
}
