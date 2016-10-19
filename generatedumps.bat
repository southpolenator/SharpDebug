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

pushd
%~dp0\bin\Debug\Tests\ExceptionDumper.exe -a %~dp0\dumps\NativeDumpTest.gcc.exe -d %~dp0\dumps\NativeDumpTest.gcc.mdmp
del %~dp0\dumps\libgcc_s_dw2-1.dll
del %~dp0\dumps\libstdc++-6.dll
popd
