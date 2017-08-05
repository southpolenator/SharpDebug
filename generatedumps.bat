@ECHO OFF

SET gccpath=C:\MingW\bin
SET gccpath64=C:\mingw-w64\mingw64\bin
SET clangpath=C:\Program Files\LLVM\bin
SET wk10path=C:\Program Files (x86)\Windows Kits\10
SET wk10version=10.0.10586.0
SET vs2013=C:\Program Files (x86)\Microsoft Visual Studio 12.0
SET vs2015=C:\Program Files (x86)\Microsoft Visual Studio 14.0

SET wk10Include=%wk10path%\Include\%wk10version%
SET wk10Lib=%wk10path%\Lib\%wk10version%

REM
REM Compile and generate dump using MingW GCC compiler
REM
pushd %gccpath64%
%gccpath64%\g++ "%~dp0\Tests\DumpApps\NativeDumpTest\NativeDumpTest.cpp" -o "%~dp0\dumps\NativeDumpTest.x64.gcc.exe" -std=c++11 -g
xcopy /D /Y %gccpath64%\libgcc_s_seh-1.dll "%~dp0\dumps\"
xcopy /D /Y %gccpath64%\libstdc++-6.dll "%~dp0\dumps\"
popd

pushd "%~dp0\bin\Debug\Tests"
"%~dp0\bin\Debug\Tests\ExceptionDumper.exe" -a "%~dp0\dumps\NativeDumpTest.x64.gcc.exe" -d "%~dp0\dumps\NativeDumpTest.x64.gcc.mdmp"
del "%~dp0\dumps\libgcc_s_seh-1.dll"
del "%~dp0\dumps\libstdc++-6.dll"
popd

REM
REM Compile and generate dump using MingW GCC compiler
REM
pushd "%~dp0\dumps"
rem "%clangpath%\clang++.exe" "%~dp0\Tests\DumpApps\NativeDumpTest\NativeDumpTest.cpp" -o "%~dp0\dumps\NativeDumpTest.x64.clang.exe" -std=c++14 -g
rem del "%~dp0\dumps\NativeDumpTest.x64.clang.ilk"
rem "%clangpath%\clang++.exe" "%~dp0\Tests\DumpApps\NativeDumpTest\NativeDumpTest.cpp" -c -o "%~dp0\dumps\NativeDumpTest.x64.clang.obj" -std=c++14 -gdwarf -O0
rem "%clangpath%\lld-link" -debug "%~dp0\dumps\NativeDumpTest.x64.clang.obj"
"%clangpath%\clang-cl.exe" /I"%wk10Include%\shared" /I"%wk10Include%\um" /I"%vs2015%\VC\include" "%~dp0\Tests\DumpApps\NativeDumpTest\NativeDumpTest.cpp" "/Fe%~dp0\dumps\NativeDumpTest.x64.clang.exe" "/Fd:%~dp0\dumps\NativeDumpTest.x64.clang.pdb" /EHsc /Zi /link /LIBPATH:"%vs2015%\VC\lib" /LIBPATH:"%wk10Lib%\um\x86"
del "%~dp0\dumps\NativeDumpTest.x64.clang.ilk"
popd

pushd "%~dp0\bin\Debug\Tests"
"%~dp0\bin\Debug\Tests\ExceptionDumper.exe" -a "%~dp0\dumps\NativeDumpTest.x64.clang.exe" -d "%~dp0\dumps\NativeDumpTest.x64.clang.mdmp"
popd

REM
REM Compile and generate dump using MingW GCC compiler
REM
pushd %gccpath%
%gccpath%\g++ "%~dp0\Tests\DumpApps\NativeDumpTest\NativeDumpTest.cpp" -o "%~dp0\dumps\NativeDumpTest.gcc.exe" -std=c++11 -g
xcopy /D /Y %gccpath%\libgcc_s_dw2-1.dll "%~dp0\dumps\"
xcopy /D /Y %gccpath%\libstdc++-6.dll "%~dp0\dumps\"
popd

pushd "%~dp0\bin\Debug\Tests"
"%~dp0\bin\Debug\Tests\ExceptionDumper32.exe" -a "%~dp0\dumps\NativeDumpTest.gcc.exe" -d "%~dp0\dumps\NativeDumpTest.gcc.mdmp"
del "%~dp0\dumps\libgcc_s_dw2-1.dll"
del "%~dp0\dumps\libstdc++-6.dll"
popd

REM
REM Compile and generate dump using Visual Studio 2013 compiler
REM
pushd "%~dp0\dumps"
"%vs2013%\VC\bin\cl.exe" /I"%wk10Include%\shared" /I"%wk10Include%\um" /I"%vs2013%\VC\include" "%~dp0\Tests\DumpApps\NativeDumpTest\NativeDumpTest.cpp" "/Fe:%~dp0\dumps\NativeDumpTest.VS2013.exe" "/Fd:%~dp0\dumps\NativeDumpTest.VS2013.pdb" /EHsc /Zi /link /LIBPATH:"%vs2013%\VC\lib" /LIBPATH:"%wk10Lib%\um\x86"
del "%~dp0\dumps\NativeDumpTest.obj"
del "%~dp0\dumps\NativeDumpTest.VS2013.ilk"
popd

pushd "%~dp0\bin\Debug\Tests"
"%~dp0\bin\Debug\Tests\ExceptionDumper32.exe" -a "%~dp0\dumps\NativeDumpTest.VS2013.exe" -d "%~dp0\dumps\NativeDumpTest.VS2013.mdmp"
popd

:End
