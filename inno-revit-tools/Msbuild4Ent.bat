C:
cd C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin

set SOURCE_DIR="D:\Git\revit\inno-revit-tools"
set TARGET="D:\Update_Inno"

MSBuild.exe %SOURCE_DIR%\inno-revit-tools\inno-revit-tools.csproj /p:Platform=x64 /p:Configuration=Release /p:OutputPath=bin\Upgrade 
Pause
MSBuild.exe %SOURCE_DIR%\ReinforcementModeling\ReinforcementModeling.csproj /p:Platform=x64 /p:Configuration=Release /p:OutputPath=bin\Upgrade
Pause
MSBuild.exe %SOURCE_DIR%\CommonTools\CommonTools.csproj /p:Platform=x64 /p:Configuration=Release /p:OutputPath=bin\Upgrade
Pause
MSBuild.exe %SOURCE_DIR%\CustomActions\CustomActions.csproj /p:Platform=x64 /p:Configuration=Release /p:OutputPath=bin\Upgrade
Pause
MSBuild.exe %SOURCE_DIR%\CommonUtils\CommonUtils.csproj /p:Platform=x64 /p:Configuration=Release /p:OutputPath=bin\Upgrade
Pause
D:
cd %SOURCE_DIR%\WixInstaller
start /wait candle.exe -dLongAssyVersion=1.2.3.0 -dShortAssyVersion=1.2.3 -d"DevEnvDir=C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\IDE\\" -dSolutionDir=%SOURCE_DIR%\ -dSolutionExt=.sln -dSolutionFileName=inno-revit-tools.sln -dSolutionName=inno-revit-tools -dSolutionPath=%SOURCE_DIR%\inno-revit-tools.sln -dConfiguration=Upgrade -dOutDir=bin\Upgrade\MsiForUpgrade -dPlatform=x86 -dProjectDir=%SOURCE_DIR%\WixInstaller\ -dProjectExt=.wixproj -dProjectFileName=WixInstaller.wixproj -dProjectName=WixInstaller -dProjectPath=%SOURCE_DIR%\WixInstaller\WixInstaller.wixproj -dTargetDir=%SOURCE_DIR%\WixInstaller\bin\Upgrade\MsiForUpgrade -dTargetExt=.msi -dTargetFileName=Setup.msi -dTargetName=Setup -dTargetPath=%SOURCE_DIR%\WixInstaller\bin\Upgrade\MsiForUpgrade\Setup.msi -out obj\Release\ -arch x86 -ext "C:\Program Files (x86)\WiX Toolset v3.11\bin\\WixIIsExtension.dll" -ext ..\Lib\WixUIExtension.dll InvalidInfoDlg.wxs UserRegDlg.wxs Product.wxs

start /wait Light.exe -out %SOURCE_DIR%\WixInstaller\bin\Upgrade\MsiForUpgrade\Setup.msi -pdbout %SOURCE_DIR%\WixInstaller\bin\Upgrade\MsiForUpgrade\Setup.wixpdb -cultures:null -dBuild=Release -ext "C:\Program Files (x86)\WiX Toolset v3.11\bin\\WixIIsExtension.dll" -ext ..\Lib\WixUIExtension.dll -contentsfile obj\Release\WixInstaller.wixproj.BindContentsFileListnull.txt -outputsfile obj\Release\WixInstaller.wixproj.BindOutputsFileListnull.txt -builtoutputsfile obj\Release\WixInstaller.wixproj.BindBuiltOutputsFileListnull.txt -wixprojectfile %SOURCE_DIR%\WixInstaller\WixInstaller.wixproj obj\Release\InvalidInfoDlg.wixobj obj\Release\UserRegDlg.wixobj obj\Release\Product.wixobj

cd %SOURCE_DIR%\WixUpgrade
start /wait candle.exe -dConfiguration=Upgrade Patch.wxs
start /wait light.exe patch.wixobj -out patch.wixmsp

if not exist %TARGET% mkdir %TARGET%
if not exist %TARGET%\Diff\ mkdir %TARGET%\Diff
if not exist %TARGET%\Upgrade\patch_Setup\ mkdir %TARGET%\Upgrade\patch_Setup
D:
cd %SOURCE_DIR%\WixInstaller
start /wait torch.exe -p -xi "\\192.168.1.43\patch_Setup\production\Setup.wixpdb" %SOURCE_DIR%\WixInstaller\bin\Upgrade\MsiForUpgrade\Setup.wixpdb -out %TARGET%\Diff\diff.wixmst

start /wait pyro.exe ..\WixUpgrade\patch.wixmsp -out %TARGET%\Upgrade\patch_Setup\patch.msp -t RTM %TARGET%\Diff\diff.wixmst
