# example-c-sharp
Example of a unit cell expressed as C#

## Setup (Windows)
* `dotnet` CLI comes pre-installed as part of Windows 11 Professional

## Setup (Mac)
* Download .NET from https://dotnet.microsoft.com/en-us/download which installs
    * .NET SDK 8.0.302
    * .NET Runtime 8.0.6
    * ASP.NET Core Runtime 8.0.6
* Download and install .NET 6.0 Runtime for Mac

## Build (Windows or Mac)
* in a terminal, run
    ```
    cd /path/to/example-c-sharp
    dotnet build LatticeRobotCS.sln
    ```

## Run (Windows or Mac)
* in a terminal, run
    ```
    cd /path/to/example-c-sharp
    dotnet run --project LatticeRobotCS/LatticeRobotCS.csproj
    ```
* use a viewer such as https://3dviewer.net/ to see the result
