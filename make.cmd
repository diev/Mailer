@echo off
set fx=%SystemRoot%\Microsoft.NET\Framework

rem make specific edition
goto v40
goto v35
goto v20

rem make the best
:v40
rem Windows 7+
set net=%fx%\v4.0.30319
if exist %net%\csc.exe goto make

:v35
rem Windows 2003
set %net%=%fx%\v3.5
if exist %net%\csc.exe goto make

:v20
rem Windows XP
set net=%fx%\v2.0.50727
if exist %net%\csc.exe goto make

echo No Microsoft .NET ?!!
goto :eof

:make
echo using %net%
set path=%net%;%path%
rem MSBuild
csc /out:Mailer.exe /recurse:*.cs
goto :eof
