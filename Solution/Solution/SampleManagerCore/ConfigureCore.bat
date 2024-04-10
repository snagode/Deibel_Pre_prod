@echo off
set CWD=%~dp0

set NewStr="C:\Thermo\SampleManager\Server\VGSM\Exe\"
set OldStr="SAMPLEMANAGERPATH"

set _FilePath=SampleManagerObjectMode\
set _FileName=SampleManagerObjectMode.csproj.config
set _OutFileName=SampleManagerObjectMode.csproj.user

set _FilePath2=SampleManagerTasks\
set _FileName2=SampleManagerTasks.csproj.config
set _OutFileName2=SampleManagerTasks.csproj.user

set _FilePath3=SampleManagerWebApiTasks\
set _FileName3=SampleManagerWebApiTasks.csproj.config
set _OutFileName3=SampleManagerWebApiTasks.csproj.user

echo Configuring Solution for Build
echo ==============================
echo Please set the path to the Instance Executable Directory

set /P NewStr="SampleManager Path [%NewStr%]:"

SETLOCAL
SETLOCAL ENABLEDELAYEDEXPANSION

rmdir "SampleManagerObjectMode\bin" /s /q
del   "SampleManagerObjectMode\SampleManagerObjectModel.csproj.user" /q
del   "SampleManagerObjectMode\SampleManagerObjectModel.csproj.vspscc" /q
rmdir "SampleManagerTasks\bin" /s /q
del   "SampleManagerTasks\SampleManagerTasks.csproj.user" /q
del   "SampleManagerTasks\SampleManagerTasks.csproj.vspscc" /q
rmdir "SampleManagerWebApiTasks\bin" /s /q
del   "SampleManagerWebApiTasks\SampleManagerWebApiTasks.csproj.user" /q
del   "SampleManagerWebApiTasks\SampleManagerWebApiTasks.csproj.vspscc" /q

if exist "%_FilePath%%_OutFileName%" (
    del "%_FilePath%%_OutFileName%" >nul 2>&1
    )

Call ConfigureSubstitute.bat %OldStr% %NewStr% %_FilePath%%_FileName% > %_FilePath%%_OutFileName%

if exist "%_FilePath2%%_OutFileName2%" (
    del "%_FilePath2%%_OutFileName2%" >nul 2>&1
    )

Call ConfigureSubstitute.bat %OldStr% %NewStr% %_FilePath2%%_FileName2% > %_FilePath2%%_OutFileName2%

if exist "%_FilePath3%%_OutFileName3%" (
    del "%_FilePath3%%_OutFileName3%" >nul 2>&1
    )

Call ConfigureSubstitute.bat %OldStr% %NewStr% %_FilePath3%%_FileName3% > %_FilePath3%%_OutFileName3%

:: Pause script for approx. 5 seconds...
echo Done
PING 127.0.0.1 -n 6 > NUL 2>&1
exit /b