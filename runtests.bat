@ECHO OFF
SET Configuration=Debug
SET Platform=x64
SET PROJECT_ROOT=%CD%
SET TestConsole=C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe
SET TestSettings=%PROJECT_ROOT%\Tests\DbgEngTest\WinDbgCs_%Platform%.testsettings
SET DbgEngTest=%PROJECT_ROOT%\bin\%CONFIGURATION%\Tests\DbgEngTest.dll

"%TestConsole%" /Settings:"%TestSettings%" /inIsolation /Platform:%Platform% "%DbgEngTest%"
