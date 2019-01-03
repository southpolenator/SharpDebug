## Building using Visual Studio
- Open solution file located in repository root directory with Visual Studio.
- Start __Build solution__ to get things rolling
- If you want to build Visual Studio extension, you need to __Build solution__ one more time (this step is needed only the first time, so that Visual Studio binplaces all dependencies correctly).

## Building using `dotnet` command
- Open console and change directory to repository root
- Type `dotnet build`

Note that you will probably get errors about building WinDbg extension, but you can ignore those if you don't need it. If you want to build WinDbg extension, you need to build using Visual Studio.

## Running tests
Before running tests, you need to download all dumps. Execute `dumps\download.ps1` script to fetch them from JFrog server.

With Visual Studio, you can use __Test Explorer__ to find and execute tests.

Enter `Tests\CsDebugScript.Tests` directory. Run `dotnet test` command.
