@ECHO OFF
SET Configuration=Debug
SET Platform=x64
SET PROJECT_ROOT=%CD%
REM SET TestConsole=C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe
REM SET TestConsole=C:\Program Files (x86)\Microsoft Visual Studio\2017\Comunity\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe
REM SET TestConsole=C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe
SET TestConsole=C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe
SET TestSettings=%PROJECT_ROOT%\Tests\DbgEngTest\WinDbgCs_%Platform%.testsettings
SET DbgEngTest=%PROJECT_ROOT%\bin\\Debug\net461\CsDebugScript.Tests.dll

rem "%TestConsole%" /Settings:"%TestSettings%" /inIsolation /Platform:%Platform% "%DbgEngTest%"
"%TestConsole%" /inIsolation /Platform:%Platform% "%DbgEngTest%" /Tests:CsDebugScript.Tests.DebugControlTest.GoBreakContinuosTestDepth,CsDebugScript.Tests.DebugControlTest.BreakpointBreakAndContinue,CsDebugScript.Tests.DebugControlTest.BreakpointSanityTest,CsDebugScript.Tests.DebugControlTest.GoBreakContinousVariablesChange,CsDebugScript.Tests.DebugControlTest.BreakpointWithBreakAfterHit
rem "%TestConsole%" /inIsolation /Platform:%Platform% "%DbgEngTest%" /Tests:CsDebugScript.Tests.DebugControlTest.BreakpointBreakAndContinue
rem "%TestConsole%" /inIsolation /Platform:%Platform% "%DbgEngTest%" /TestCaseFilter:"Name~DebugControlTest"
rem "%TestConsole%" /Settings:"%TestSettings%" /inIsolation /Platform:%Platform% "%DbgEngTest%" /TestCaseFilter:"TestCategory=CLR"
