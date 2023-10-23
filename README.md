# AutoPC Demo
This is an example project to show how I often times use my [AutoPC](https://github.com/kenny2892/AutoPC) project for automation.

# Demo's Goal
In this demo, the goal is to get the details of each table entry that can be found on the main display page of the demo website.

To achieve this goal, the demo exporter will use AutoPC to do the following:
1. Find which monitor currently has the website open.
2. Navigate to the data page.
   <p align="center">
     <img src="https://i.gyazo.com/a315ef4d0caddc676c606d29626684cf.png" width="600">
   </p>
3. Search for all data with 'j' in their ID.
4. Open the Details page for each of these entries. To find the entries on screen, the program will:
   - Scroll to the bottom of the page.
   - Move the mouse to the center of the screen.
   - Using the fact that the hovered over row will change color, find the left side edge of the table.
   - Loop from the bottom of the screen to the top while checking the color of the pixel that is 30px to the left of the edge.
     - Use the fact that each row is the same size (41px tall) and that the hyperlink will always be 13px up to speed the search up.
   - Keep track of each row found on screen, and then open them all.
5. Export the data via copying with the clipboard.
   <p align="center">
     <img src="https://i.gyazo.com/2a07680f6a536e4daedf76c179c4b7c5.png" width="600">
   </p>
6. Parse the data and save the item to a list.
7. Go back to the data page and repeat steps 3 - 6 until all entries have been exported.

After all that, I would normally export the data to a csv file for use in Excel. But since this is a demo, the exporter instead compares it's exported data to the data found in the SQLite database.

If you would like to run this yourself, you will need to run the AutoPcDemoWebsite project (if using Visual Studio, run it without debugging so that you can launch the exporter as well), and then run the DemoExporter project.
