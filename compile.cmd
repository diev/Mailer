@echo off
if "%1"=="" echo Usage: %~nx0 SourceFile.cs && goto :eof

rem Windows 7+
set csc=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\csc.exe
if exist %csc% goto compile

rem Windows 2003
set csc=%SystemRoot%\Microsoft.NET\Framework\v3.5\csc.exe
if exist %csc% goto compile

rem Windows XP
set csc=%SystemRoot%\Microsoft.NET\Framework\v2.0.50727\csc.exe
if exist %csc% goto compile

echo No Microsoft .NET ?!!
goto :eof

:compile
%csc% %1
goto :eof
