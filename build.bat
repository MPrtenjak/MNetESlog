@setlocal

@cd MNetESlogGui
@msbuild /t:build /p:Configuration=Release /nologo /v:m
@if %errorlevel% neq 0 exit /b %errorlevel%
@cd ..