function Clear-Project {
  Write-Host "`n***** Clear project" -ForegroundColor Green

  $folders = ("bin", "obj", "TESTRESULTS", "PUBLISH", "DIST")
  $files = ("STYLECOP.CACHE")

  Get-ChildItem -Include $folders -Recurse -force -directory | Remove-Item -Force -Recurse
  Get-ChildItem -Include $files -Recurse -force | Remove-Item -Force -Recurse
}

function Get-FileVersion {
  $date = Get-Date
  $year = $Date.Year - 2000
  $dayOfYear = $Date.DayOfYear
  $secFromMidnight = [math]::Round(($date.Hour * 24 * 60 + $date.Minute * 60 + $date.Second) / 2, 0)
  return "$year.$dayOfYear.$secFromMidnight"
}

function Update-NewFileVersion {
  Param([string] $filePath)

  Write-Host "`n***** Set new version" -ForegroundColor Green

  $content = Get-Content -path $filePath

  $result = [regex]::matches($content, '\[assembly: AssemblyFileVersion\(\"((\d*)\.(\d*)\.(\d*)\.(\d*))\"\)\]')
  $oldVersion = $result[0].Groups[1]
  $numbers = Get-FileVersion
  $newVersion = $result[0].Groups[2].Value + "." + $numbers

  $content -Replace $oldVersion, $newVersion | Out-File $filePath -Encoding utf8

  return $newVersion
}

function Build-Program {
  Param([string] $folder, [string]$arhitecture)

  Write-Host "`n***** Building program" -ForegroundColor Green

  Push-Location $folder
    msbuild /p:Configuration=Release /p:Platform=$arhitecture /nologo /v:m | Foreach-Object{ Write-Host $_ }
    $code = $LASTEXITCODE
  Pop-Location

  if (!($code -eq 0)) { exit }
}

function Sign-Program {
  Param([string] $fileName)

  Write-Host "`n***** Sign program" -ForegroundColor Green
  Invoke-Expression "signtool sign /v /sha1 4cfb93fd81458d4450ff461473b91422848e8758 /t http://time.certum.pl $fileName"

  if (!($LASTEXITCODE -eq 0)) { exit }
}


function Create-Distribution {
  Write-Host "`n***** Create distribution" -ForegroundColor Green

  Remove-Item 'dist' -Recurse
  New-Item -Name "dist" -ItemType "directory"
  Push-Location "dist"

  New-Item -Name "MNetESlogGui" -ItemType "directory"
  Push-Location "MNetESlogGui"

  xcopy ..\..\MNetESlogGui\bin\Release . /e /i /y /s
  Remove-Item *.pdb

  Pop-Location
  Pop-Location
}

function Create-ZIP {
  Param([string] $fileName)

  Write-Host "`n***** Create ZIP" -ForegroundColor Green

  $compress = @{
    Path = "dist\MNetESlogGui\*"
    CompressionLevel = "Optimal"
    DestinationPath = "dist\$fileName"
  }
  Compress-Archive @compress
}

function Update-Git {
  Param([string] $newVersion, [string] $versionInfoFile)

  Write-Host "`n***** Updating GIT" -ForegroundColor Green
  $versionTag = "PUBLISH-$newVersion"

  git add $versionInfoFile
  git commit -m "$versionTag"
  git tag -a $versionTag -m "verzija $versionTag"
  git push origin master
}

#
# ***********************************************************************************
#

$buildmode = $args[0]
if ($args.Length -lt 1) {
  return "podaj parameter build / publish / clean"
}

Write-Host "`nbuildmode = $buildmode" -ForegroundColor Red

Clear-Project
if ($buildmode -eq 'clean') { exit }

$newVersion = Update-NewFileVersion 'VersionInfo.cs'

Build-Program 'MNetESlogService' 'AnyCPU'
Build-Program 'MNetESlogGui' 'AnyCPU'
if ($buildmode -eq 'build') {
  git checkout -- 'VersionInfo.cs'
  exit
}

Create-Distribution

Sign-Program "dist\MNetESlogGui\MNetESlogGui.exe"
Sign-Program "dist\MNetESlogGui\MNetESlogService.dll"

$zipName = "MNetESlogGui_" + ($newVersion -replace '\.', '_') + ".zip"
Create-ZIP $zipName

Update-Git $newVersion 'VersionInfo.cs'

Write-Host "End" -ForegroundColor Green


