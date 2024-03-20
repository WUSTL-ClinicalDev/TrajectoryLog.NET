# TrajectoryLog.NET (TrajectoryLog "dot" NET) 
# Summary
## API Methods
 - **EnableDebug**: Writes additional log files through the console to assist in debugging the application when loading and interpreting Trajectory Logs.
 - **DisableDebug**: Turns off the Debug messages from the API.
 - **LoadLog**: Reads the file presented to the method and inteprets it to a *TrajectoryLogInfo* class object.
 - **ToCSV**: Export Trajectory Log to CSV file.
 - **BuildFluence**: Generates a 1mm resolution fluence image based on MLC positions. Inputs for MLCString include *Expected* or *Actual*.
 - **PublishPDF**: Generates a simple PDF report of a Trajectory Log file.

# Description
Prior to using the TrajectoryLog.NET API, clone the code from this repository and compile using the same .NET Framework version as your ESAPI version (if intended to be used alongside ESAPI). 
Add the using directive for TrajectoryLog.NET
```csharp
using TrajectoryLog.NET;
```
**Note: Nuget packages specific to ESAPI versions for TrajectoryLog.NET to be available soon**

## LoadLog
This method shows how to interpret the Trajectory Log.
```csharp
OpenFileDialog ofd = new OpenFileDialog();
ofd.Filter = "Trajectory Log File (*.bin)|*.bin";
TrajectoryAPI.EnableDebug();
TrajectorySpecifications.TrajectoryLogInfo localLog = null;
if(ofd.ShowDialog() == true)
{
  localLog = TrajectoryAPI.LoadLog(ofd.FileName);
}
```
## BuildFluence
This method returns a matrix with pixel values relative to the amount of MU delivered to open leaf segments 
```csharp
double[,] actualFluence = TrajectoryAPI.BuildFluence(localLog, "Actual");
double[,] expectedFluence = TrajectoryAPI.BuildFluence(localLog,"Expected");
```
**Items of Note**
 - The fluence resolution is 1mm.
 - The fluence resolution does not currently consider the position of any collimating jaws. *Fluence shows leaf openings behind jaws*
 - Rounding of positions could lead to a single vertical line where the leaf meeting point is (even if behind the jaws).

## ToCSV
This method will export a CSV to a user specified location.
```csharp
Console.WriteLine("Do you want to write .csv? (y/n)");
if (Console.ReadLine().Trim().Equals("y",StringComparison.OrdinalIgnoreCase))
{
  TrajectoryAPI.ToCSV(localLog);
}
```
The CSV file export will open a *SaveFileDialog* for the user to select their own save location for CSV. 
![CSV Example](https://github.com/WUSTL-ClinicalDev/TrajectoryLog.NET/blob/master/TrajectoryLog.NET/img/SampleCSV.PNG)

## PublishPDF
```csharp
TrajectoryAPI.PublishPDF(localLog);
```
![PDF Example](https://github.com/WUSTL-ClinicalDev/TrajectoryLog.NET/blob/master/TrajectoryLog.NET/img/SampleReport.PNG)

*For any questions or issues, please note them through Github features or send to matthew.schmidt@wustl.edu.*

