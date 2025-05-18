# ITEQ2

A C# WPF based Application to load, view, edit and save IT-Equipment related files. Built for Wood PLC

---

## How to Run

### Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or newer

---

## Dependencies

This project uses the following NuGet packages:

- [CsvHelper](https://joshclose.github.io/CsvHelper/) – for reading/writing CSV files easily

Install via NuGet Package Manager Console:

```bash
Install-Package CsvHelper




SETUP:

Clone the repository:

 - bash: git clone https://github.com/yourname/ITEQ2-GridViewPresets.git

Launch the program, then:

 - Click File → Open Files

 - Navigate to the Temp\ folder inside the cloned repo

 - Select and load both:

	- fucReportExampleData.csv

	- ITEQExampleData.csv

Save the working document:

 - Use File → Save

Set up AppData folder:

 - Navigate to %AppData%\ITEQ2\Data\

 - Copy fucReportExampleData.csv from the Temp\ folder into this directory

 - You should also see workingDoc.csv here (created during Save)

You're done!

 - Restart the application

 - It will now load the saved files automatically on launch