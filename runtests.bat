@ECHO OFF
SET CONFIGURATION=Debug
SET PROJECT_ROOT=%CD%
SET NUGET_PACKAGES=%userprofile%\.nuget\packages
SET DEAFULT_TARGET_FRAMEWORK=net461
SET DEAFULT_TARGET_NETSTANDARD=netstandard2.0
SET DEAFULT_TARGET_NETCOREAPP=netcoreapp2.0
SET OPENCOVER=%NUGET_PACKAGES%\OpenCover\4.6.519\tools\OpenCover.Console.exe
SET XUNIT_TOOLS=%NUGET_PACKAGES%\xunit.runner.console\2.3.1\tools\net452
SET XUNIT_TOOLS_CORE=%NUGET_PACKAGES%\xunit.runner.console\2.3.1\tools\netcoreapp2.0
SET NATIVE_TARGET_DIR=%PROJECT_ROOT%\bin\%CONFIGURATION%\%DEAFULT_TARGET_NETCOREAPP%
SET NATIVE_TESTS_DLL=%NATIVE_TARGET_DIR%\SharpDebug.Tests.Native.dll

REM cd %NATIVE_TARGET_DIR%
REM dotnet %XUNIT_TOOLS_CORE%\xunit.console.dll %NATIVE_TESTS_DLL% -noshadow -parallel assemblies -trait x64=true
REM cd %PROJECT_ROOT%

"%OPENCOVER%" -oldstyle -register:user -target:"C:\Program Files\dotnet\dotnet.exe" -targetargs:"%XUNIT_TOOLS_CORE%\xunit.console.dll %NATIVE_TESTS_DLL% -noshadow -parallel assemblies -trait x64=true -appveyor" -filter:"+[SharpDebug*]*" -excludebyattribute:*.ExcludeFromCodeCoverage* -hideskipped:All -output:.\code_coverage_x64.Native.Core.xml -targetdir:%NATIVE_TARGET_DIR%
