@echo off
if !%1==! goto End

if not exist %1 md %1
if errorlevel 1 goto Error
del /S /Q %1\*.*

echo Deploying Assemblies...
copy /y HFM.exe %1
copy /y HFM.exe.config %1
copy /Y HFM.Helpers.dll %1
copy /Y HFM.Instances.dll %1
copy /Y HFM.Instrumentation.dll %1
copy /Y HFM.Preferences.dll %1
copy /Y HFM.Proteins.dll %1
copy /Y HTMLparser.dll %1

echo Copying Support Files and Folders...
copy /Y GPLv2.TXT %1
copy /Y "HTMLparser License.txt" %1

if not exist %1\CSS md %1\CSS
if errorlevel 1 goto Error
xcopy /Y CSS %1\CSS

if not exist %1\XML md %1\XML
if errorlevel 1 goto Error
xcopy /Y XML %1\XML

if not exist %1\XSL md %1\XSL
if errorlevel 1 goto Error
xcopy /Y XSL %1\XSL

echo Finished Cleanly.
goto End

:Error
echo An Error Occured While Creating a Deploy Folder.

:End
echo on
