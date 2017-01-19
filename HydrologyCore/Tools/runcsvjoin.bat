for /f %%i in ('dir ..\bin\debug  /b/a:d/od/t:c') do set LAST=%%i
csvjoin.exe ..\bin\debug\%LAST% t.csv