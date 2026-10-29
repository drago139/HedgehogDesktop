param([string]$OutputPath='App\HedgehogDesktop.exe')
$ErrorActionPreference='Stop'
Set-Location $PSScriptRoot
$compiler='C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
New-Item -ItemType Directory -Force App,evidence,evidence\icon | Out-Null
& $compiler /nologo /target:exe /out:evidence\HitAreaTests.exe /reference:System.Drawing.dll src\PetHitArea.cs src\HitAreaTests.cs
if($LASTEXITCODE -ne 0) {exit $LASTEXITCODE}
& '.\evidence\HitAreaTests.exe'
if($LASTEXITCODE -ne 0) {exit $LASTEXITCODE}
& $compiler /nologo /target:exe /out:evidence\ThrowTests.exe /reference:System.Drawing.dll src\ThrowMotion.cs src\ThrowTests.cs
if($LASTEXITCODE -ne 0) {exit $LASTEXITCODE}
& '.\evidence\ThrowTests.exe'
if($LASTEXITCODE -ne 0) {exit $LASTEXITCODE}
& $compiler /nologo /target:exe /out:evidence\PetRosterTests.exe src\PetRoster.cs src\PetRosterTests.cs
if($LASTEXITCODE -ne 0) {exit $LASTEXITCODE}
& '.\evidence\PetRosterTests.exe'
if($LASTEXITCODE -ne 0) {exit $LASTEXITCODE}
& $compiler /nologo /target:exe /out:evidence\MotionTests.exe /reference:System.Drawing.dll src\Walker.cs src\MotionTests.cs
if($LASTEXITCODE -ne 0) {exit $LASTEXITCODE}
& '.\evidence\MotionTests.exe'
if($LASTEXITCODE -ne 0) {exit $LASTEXITCODE}
& $compiler /nologo /target:exe /out:evidence\AnimationTests.exe src\FrameClock.cs src\AnimationTests.cs
if($LASTEXITCODE -ne 0) {exit $LASTEXITCODE}
& '.\evidence\AnimationTests.exe'
if($LASTEXITCODE -ne 0) {exit $LASTEXITCODE}
& $compiler /nologo /target:exe /out:evidence\AssetTests.exe /reference:System.Drawing.dll src\AssetTests.cs
if($LASTEXITCODE -ne 0) {exit $LASTEXITCODE}
& '.\evidence\AssetTests.exe' 'assets\walk' 'assets\hedgehog.ico'
if($LASTEXITCODE -ne 0) {exit $LASTEXITCODE}
& $compiler /nologo /target:exe /out:evidence\CreateIcon.exe /reference:System.Drawing.dll src\CreateIcon.cs
if($LASTEXITCODE -ne 0) {exit $LASTEXITCODE}
& '.\evidence\CreateIcon.exe' assets\walk\frame-001.png assets\hedgehog.ico evidence\icon\preview.png
if($LASTEXITCODE -ne 0) {exit $LASTEXITCODE}
Compress-Archive -Path 'assets\throw\*.png' -DestinationPath 'assets\throw-frames.zip' -Force
Compress-Archive -Path 'assets\walk\*.png' -DestinationPath 'assets\hedgehog-frames.zip' -Force
& $compiler /nologo /target:exe /out:evidence\RenderCacheTests.exe /reference:System.Drawing.dll /reference:System.IO.Compression.dll /reference:System.IO.Compression.FileSystem.dll /resource:assets\hedgehog-frames.zip,HedgehogFrames /resource:assets\throw-frames.zip,ThrowFrames src\Native.cs src\PetRenderCache.cs src\PetFrames.cs src\PetHitArea.cs src\RenderCacheTests.cs
if($LASTEXITCODE -ne 0) {exit $LASTEXITCODE}
& '.\evidence\RenderCacheTests.exe'
if($LASTEXITCODE -ne 0) {exit $LASTEXITCODE}
& $compiler /nologo /optimize+ /target:winexe /platform:anycpu ('/out:'+ $OutputPath) /win32icon:assets\hedgehog.ico /reference:System.Drawing.dll /reference:System.Windows.Forms.dll /reference:System.IO.Compression.dll /reference:System.IO.Compression.FileSystem.dll /reference:System.Xml.Linq.dll /resource:assets\hedgehog-frames.zip,HedgehogFrames /resource:assets\throw-frames.zip,ThrowFrames src\Program.cs src\PetApplication.cs src\PetRoster.cs src\PetFrames.cs src\Native.cs src\PetRenderCache.cs src\Walker.cs src\PetHitArea.cs src\ThrowMotion.cs src\FrameClock.cs src\PetForm.cs
exit $LASTEXITCODE
