@ECHO OFF

SET gccpath=C:\MingW\bin

REM
REM Compile and generate dump using MingW GCC compiler
REM
pushd %gccpath%
%gccpath%\g++ %~dp0\Tests\DumpApps\NativeDumpTest\NativeDumpTest.cpp -o %~dp0\dumps\NativeDumpTest.gcc.exe -std=c++11 -g
%gccpath%\cv2pdb -C %~dp0\dumps\NativeDumpTest.gcc.exe
xcopy /D /Y %gccpath%\libgcc_s_dw2-1.dll %~dp0\dumps\
xcopy /D /Y %gccpath%\libstdc++-6.dll %~dp0\dumps\
popd

pushd %~dp0\bin\Debug\Tests
%~dp0\bin\Debug\Tests\ExceptionDumper32.exe -a %~dp0\dumps\NativeDumpTest.gcc.exe -d %~dp0\dumps\NativeDumpTest.gcc.mdmp
del %~dp0\dumps\libgcc_s_dw2-1.dll
del %~dp0\dumps\libstdc++-6.dll
popd

REM
REM Compile and generate dump using Visual Studio 2013 compiler
REM
pushd %~dp0\dumps
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\VC\bin\cl.exe" /I"C:\Program Files (x86)\Windows Kits\10\Include\10.0.10586.0\shared" /I"C:\Program Files (x86)\Windows Kits\10\Include\10.0.10586.0\um" /I"C:\Program Files (x86)\Microsoft Visual Studio 12.0\VC\include" %~dp0\Tests\DumpApps\NativeDumpTest\NativeDumpTest.cpp /Fe:%~dp0\dumps\NativeDumpTest.VS2013.exe /Fd:%~dp0\dumps\NativeDumpTest.VS2013.pdb /EHsc /Zi /link /LIBPATH:"C:\Program Files (x86)\Microsoft Visual Studio 12.0\VC\lib" /LIBPATH:"C:\Program Files (x86)\Windows Kits\10\Lib\10.0.10586.0\um\x86"
del %~dp0\dumps\NativeDumpTest.obj
del %~dp0\dumps\NativeDumpTest.VS2013.ilk
popd

pushd %~dp0\bin\Debug\Tests
%~dp0\bin\Debug\Tests\ExceptionDumper32.exe -a %~dp0\dumps\NativeDumpTest.VS2013.exe -d %~dp0\dumps\NativeDumpTest.VS2013.mdmp
popd
