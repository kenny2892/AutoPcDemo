using AutoPC;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Monitor = AutoPC.Monitor;

namespace DemoExporter
{
    public class Program
    {

        private static bool Pause { get; set; } = false;

        [STAThread]
        public static void Main(string[] args)
        {
            Console.Title = "Demo Exporter";
            Console.WriteLine(Console.Title);

            try
            {
                Activate();
            }

            catch(Exception e)
            {
                Console.WriteLine(e);
            }

            Console.WriteLine("Press any key to close...");
            Console.ReadKey();
        }

        private static void Activate()
        {
            Console.WriteLine("This is an example of how I would typically use mu AutoPC project");
            Console.WriteLine("The goal of this example is to do the following steps:\n" +
                "1. Find which monitor currently has the website open.\n" +
                "2. Navigate to the data page.\n" +
                "3. Search for all data with 'j' in their ID.\n" +
                "4. Open the Details page for each of these entries.\n" +
                "\tTo find the entries on screen, the program will:\n" +
                "\t4a. Scroll to the bottom of the page.\n" +
                "\t4b. Move the mouse to the center of the screen.\n" +
                "\t4c. Using the fact that the hovered over row will change color, find the left side edge of the table.\n" +
                "\t4d. Loop from the bottom of the screen to the top while checking the color of the pixel that is 30px to the left of the edge.\n" +
                "\t\t- Use the fact that each row is the same size (41px tall) and that the hyperlink will always be 13px up to speed the search up.\n" +
                "\t4e. Keep track of each row found on screen, and then open them all.\n" +
                "5. Export the data via copying with the clipboard.\n" +
                "6. Parse the data and save the item to a list.\n" +
                "7. Go back to the data page and repeat steps 3 - 6 until all entries have been exported.");
            Console.WriteLine("The program will also regularly check for mouse movement caused by the user. It will pause if it detects any.");

            Console.Write("Press ENTER to start...");
            Console.ReadLine();

            Monitor toUse = null;
            while(toUse is null)
            {
                toUse = FindMonitor();

                if(toUse is null)
                {
                    Console.WriteLine("Unable to find the website's logo. Please ensure that the website is visible on screen.");
                    Console.Write("Press ENTER to try again...");
                    Console.ReadLine();
                }
            }

            // Activate Mouse Movement Detection
            Mouse.StartMonitorMovement(() => Pause = true);

            // Perform setup and navigation
            var logoCoords = FindLogoCoordinates(toUse);
            OpenDataPage(toUse);
            SearchForData("j", logoCoords);
            ScrollToBottom(toUse, logoCoords);
            int tableLeftEdge = FindTableLeftEdge(toUse);

            // Now begin scanning the screen for rows to click
            List<Data> exportedData = new List<Data>();
            while(true)
            {
                var rows = FindRowsOnScreen(toUse, tableLeftEdge);

                foreach(var rowCoords in rows)
                {
                    var result = ExportRow(toUse, logoCoords, rowCoords);

                    if(exportedData.All(data => data.ID != result.ID))
                    {
                        Console.WriteLine("Exported #" + result.ID);
                        exportedData.Add(result);
                    }

                    else
                    {
                        Console.WriteLine("Duplicate of #" + result.ID);
                    }

                    // Close the tab (this works on Firefox and Chrome)
                    Keyboard.Type("w", ctrl: true);
                    Thread.Sleep(200);
                }

                // Scroll Up
                var screenshotBeforeScroll = toUse.GetScreenshotPicture();
                Thread.Sleep(200);
                Mouse.ScrollUp(Mouse.ScrollWheelAmount * 9); // For this webpage, 9 scroll ups will result in the bottom entry being a repeat, but we jsut have the program skip dupes
                Thread.Sleep(1000);

                // We could also check for the "Data Logo.png" to show up, but in this example, I'm just going to check for screen changes after the scroll
                var screenshotAfterScroll = toUse.GetScreenshotPicture();
                if(screenshotBeforeScroll.CompareTo(screenshotAfterScroll))
                {
                    Console.WriteLine("Reached the top.");
                    break;
                }
            }

            Mouse.StopMonitorMovement();

            // Normally, this is where I would export the data. But since this is a demo, I'll instead compare the results with the actual databast to see if it is all correct
            //SaveToFile(exportedData);
            Console.WriteLine(CompareResults(exportedData) ? "All data matches!" : "Mismatched data");

            Console.WriteLine("Completed");
        }

        private static Monitor FindMonitor()
        {
            Monitor toUse = null;

            for(int i = 0; i < Screen.AllScreens.Length; i++)
            {
                var toCheck = new Monitor(i);
                if(toCheck.IsPictureOnScreen(true, true, "Website Logo.png", colorSearchBuffer: 50))
                {
                    toUse = toCheck;
                    break;
                }
            }

            return toUse;
        }

        private static Coordinate FindLogoCoordinates(Monitor toUse)
        {
            Coordinate logoCoords = new Coordinate();
            while(logoCoords.IsEmpty)
            {
                logoCoords = toUse.FindPicture(true, true, "Website Logo.png", colorSearchBuffer: 50);

                if(logoCoords.IsEmpty)
                {
                    Console.WriteLine("Could not find the website logo. Please ensure that the website is visible on screen.");
                    Console.Write("Press ENTER to try again...");
                    Console.ReadLine();
                    Mouse.UpdateLastKnownCoords();
                    Pause = false;
                }
            }

            return logoCoords;
        }

        private static void OpenDataPage(Monitor toUse)
        {
            // Check if the page is already open
            if(toUse.IsPictureOnScreen(true, true, "Data Logo.png", colorSearchBuffer: 50))
            {
                return;
            }

            // Find the header and click it
            Coordinate headerCoords = new Coordinate();
            while(headerCoords.IsEmpty)
            {
                headerCoords = toUse.FindPicture(true, true, "Data Header.png", colorSearchBuffer: 50);

                if(headerCoords.IsEmpty)
                {
                    Console.WriteLine("Could not find the data header. Please ensure that the headers are visible on screen.");
                    Console.Write("Press ENTER to try again...");
                    Console.ReadLine();
                    Mouse.UpdateLastKnownCoords();
                    Pause = false;
                }
            }

            CheckForPauseRequest();
            Mouse.MoveAndLeftClick(headerCoords.X + 5, headerCoords.Y + 5);

            // Wait a max of 5 seconds for the page to load before asking the user
            int maxWaitCount = 10;
            int waitCount = 0;
            while(!toUse.IsPictureOnScreen(true, true, "Data Logo.png", colorSearchBuffer: 50))
            {
                Thread.Sleep(500);
                waitCount++;

                if(waitCount >= maxWaitCount)
                {
                    Console.WriteLine("Could not confirm that the data page has loaded.");
                    Console.Write("Please open it manually and then press ENTER...");
                    Console.ReadLine();
                    Mouse.UpdateLastKnownCoords();
                    Pause = false;
                }
            }
        }

        private static void SearchForData(string searchTerm, Coordinate logoCoords)
        {
            // Click below the logo
            CheckForPauseRequest();
            Mouse.MoveAndLeftClick(logoCoords.X, logoCoords.Y + 100);

            // Tab over to the Search Bar
            Keyboard.Type(SpecialKeys.TAB);
            Keyboard.Type(SpecialKeys.TAB);

            // Type the search term
            Keyboard.Type(searchTerm);
            Keyboard.Type(SpecialKeys.ENTER);

            // Wait a sec for it to process
            Thread.Sleep(1000);
        }

        private static void ScrollToBottom(Monitor toUse, Coordinate logoCoords)
        {
            // Click the window into focus
            CheckForPauseRequest();
            Mouse.MoveAndLeftClick(logoCoords);

            // Scroll Down
            while(true)
            {
                // Technically, this could have a false positive due to the taskbar's clock,
                // But that would only cause 1 false positive, so I just left it as is.
                // To avoid such a thing, you could get a sub image of the screenshot that is only showing the contents of the webpage and use that for the comparison
                var screenshotBeforeScroll = toUse.GetScreenshotPicture();
                Mouse.ScrollDownBig();
                Thread.Sleep(200);
                var screenshotAfterScroll = toUse.GetScreenshotPicture();

                if(screenshotBeforeScroll.CompareTo(screenshotAfterScroll))
                {
                    break;
                }
            }
        }

        private static int FindTableLeftEdge(Monitor toUse)
        {
            CheckForPauseRequest();

            // Move the mouse to the center
            Mouse.Move(toUse.ConvertToGlobalCoords(toUse.Width / 2, toUse.Height / 2));

            // Scan the screenshot for the left bound
            // We know that the background is all white, so just check if the pixel changes to a different color
            int leftBound = -1;

            while(leftBound < 0)
            {
                var screenshot = toUse.GetScreenshotPicture();
                for(int i = 0; i < toUse.Width; i++)
                {
                    if(!PictureEditor.CompareColors(Color.White, screenshot.GetColor(i, toUse.Height / 2)))
                    {
                        leftBound = i;
                        break;
                    }
                }

                if(leftBound < 0)
                {
                    Console.WriteLine("Could not find the table bounds. Please ensure that the table is visible on screen.");
                    Console.Write("Press ENTER to try again...");
                    Console.ReadLine();

                    Mouse.UpdateLastKnownCoords();
                    Pause = false;
                }
            }

            return leftBound;
        }

        private static List<Coordinate> FindRowsOnScreen(Monitor toUse, int tableLeftEdge)
        {
            List<Coordinate> rowCoords = new List<Coordinate>();
            var screenshot = toUse.GetScreenshotPicture();

            var rowEdgeColor = Color.FromArgb(222, 226, 230);
            var topOfTableColor = Color.FromArgb(33, 37, 41);
            var hyperlinkColor = Color.FromArgb(13, 110, 253);

            for(int y = toUse.Height - 1; y >= 41; y--)
            {
                // Check if this coordinate is the lower edge of a row, if the upper edge is also visible, and if 12 up is the hyperlink underline
                if(MatchingColors(screenshot.GetColor(tableLeftEdge + 30, y), rowEdgeColor) && 
                    MatchingColors(screenshot.GetColor(tableLeftEdge + 30, y - 41), rowEdgeColor, topOfTableColor) &&
                    MatchingColors(screenshot.GetColor(tableLeftEdge + 30, y - 12), hyperlinkColor))
                {
                    // The center of the Hyperlink will be 30 to the right and 16 up
                    rowCoords.Add(toUse.ConvertToGlobalCoords(tableLeftEdge + 30, y - 16));
                    y -= 40; // The loop will also minus 1
                }
            }

            return rowCoords;
        }

        private static bool MatchingColors(Color toCheck, params Color[] colorsToMatch)
        {
            foreach(var toMatch in colorsToMatch)
            {
                if(PictureEditor.CompareColors(toCheck, toMatch))
                {
                    return true;
                }
            }

            return false;
        }

        private static Data ExportRow(Monitor toUse, Coordinate logoCoords, Coordinate rowCoords)
        {
            CheckForPauseRequest();

            Mouse.MoveAndLeftClick(rowCoords);
            Thread.Sleep(500);

            // Wait while the page opens
            while(!toUse.IsPictureOnScreen(true, true, "Details Logo.png", colorSearchBuffer: 50))
            {
                CheckForPauseRequest();
                Thread.Sleep(1000);
            }

            string clipboardText = "";
            while(String.IsNullOrEmpty(clipboardText))
            {
                try
                {
                    CheckForPauseRequest();
                    Mouse.MoveAndLeftClick(logoCoords.X, logoCoords.Y + 100);
                    Keyboard.Type("a", ctrl: true);
                    Keyboard.Type("c", ctrl: true);

                    clipboardText = Clipboard.GetText();
                }

                catch(Exception)
                {
                    Console.WriteLine("Clipboard error. Trying again in 2 seconds.");
                    Thread.Sleep(2000);
                }
            }

            return ParseExportText(clipboardText);
        }

        private static Data ParseExportText(string clipboardText)
        {
            // Note: I am not using the TestData class from the demo website's project because, in a real case, I wouldn't have access to that and would need to make my own.
            // This example project is pretty simple to parse due to the data being layed out in a vertical list
            // Here is an example of what we will be parsing:
            /*
            AutoPcDemo

                Home
                Test Data
                Privacy

            Details
            TestData

            ID
                0KFO0S3A34IE76S3MFVC 
            CategoryOne
                Cat 3 
            CategoryTwo
                Inner Cat 2 
            Date
                11/13/2017 12:00:00 AM 
            AmountDisplay
                $17.42 
            Enabled
                FALSE 

            Edit | Back to List
            © 2023 - AutoPcDemo - Privacy
             */

            var lines = clipboardText.Split("\n").Select(line => line.Trim()).ToList();
            Data result = new Data();
            result.ID = lines[lines.IndexOf("ID") + 1];
            result.CategoryOne = lines[lines.IndexOf("CategoryOne") + 1];
            result.CategoryTwo = lines[lines.IndexOf("CategoryTwo") + 1];
            result.Date = DateTime.TryParse(lines[lines.IndexOf("Date") + 1], out DateTime convertedDate) ? convertedDate : DateTime.Parse("1/1/1990");
            result.Amount = lines[lines.IndexOf("AmountDisplay") + 1];
            result.Enabled = lines[lines.IndexOf("Enabled") + 1] == "TRUE";

            return result;
        }

        private static void SaveToFile(List<Data> exportedData)
        {
            var lines = ParseDataToCsvLines(exportedData);

            // Save export to file
            string savePath = "";
            while(String.IsNullOrEmpty(savePath))
            {
                Console.WriteLine("When the prompt appears, save the file...");
                Thread.Sleep(2000);

                savePath = Selection.SelectSaveFile("Save the Data Export", "CSV files (*.csv)|*.csv");

                if(String.IsNullOrEmpty(savePath))
                {
                    Console.WriteLine("Invalid input. Would you like to cancel?");
                    Console.Write("( Y / N ): ");
                    string input = Console.ReadLine().ToLower();

                    if(input == "y" || input == "yes")
                    {
                        break;
                    }
                }
            }

            if(!String.IsNullOrEmpty(savePath))
            {
                while(Selection.IsFileOpen(savePath))
                {
                    Console.Write("File is currently open. Please close it and then press ENTER...");
                    Console.ReadLine();
                }
            }

            File.WriteAllLines(savePath, lines, Encoding.UTF8);
        }

        private static List<string> ParseDataToCsvLines(List<Data> exportedData)
        {
            List<string> lines = new List<string>() { "ID,Category 1,Category 2,Date,Amount,Is Enabled" };
            foreach(var data in exportedData)
            {
                lines.Add(String.Join(",", data.ID, data.CategoryOne, data.CategoryTwo, data.Amount, data.Enabled));
            }

            return lines;
        }

        private static bool CompareResults(List<Data> exportedData)
        {
            using var db = new TestingDataContext();

            var dataFromDatabase = db.TestDatas.Where(fromDatabase => fromDatabase.ID.ToLower().Contains("j")).ToList();

            if(dataFromDatabase.Count != exportedData.Count)
            {
                Console.WriteLine("Mismatched Data Count");
                return false;
            }

            else if(!dataFromDatabase.All(fromDatabase => exportedData.Count(exported => exported.ID == fromDatabase.ID) == 1))
            {
                Console.WriteLine("Mismatched Data ID Count");
                return false;
            }

            bool noMismatches = true;

            foreach(var fromDatabase in dataFromDatabase)
            {
                var exported = exportedData.First(exported => exported.ID == fromDatabase.ID);
                var allMatch = fromDatabase.CategoryOne == exported.CategoryOne && fromDatabase.CategoryTwo == exported.CategoryTwo &&
                    fromDatabase.Date == exported.Date && fromDatabase.AmountDisplay == exported.Amount && fromDatabase.Enabled == exported.Enabled;

                if(!allMatch)
                {
                    Console.WriteLine($"\tID: {exported.ID} Category 1: {exported.CategoryOne} Category 2: {exported.CategoryTwo} Date: {exported.Date.ToString("G")} Amount: {exported.Amount} Enabled: {exported.Enabled}");
                    noMismatches = false;
                }
            }

            return noMismatches;
        }

        private static void CheckForPauseRequest()
        {
            if(Pause)
            {
                Console.WriteLine("Mouse movement detected.");
                Console.Write("Press ENTER to resume...");
                Console.ReadLine();

                Mouse.UpdateLastKnownCoords();
                Pause = false;
            }
        }
    }
}