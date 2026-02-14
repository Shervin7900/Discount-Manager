@echo off
SET CONFIG=Release
SET OUTPUT_DIR=.\packages

echo Cleaning existing packages...
if exist %OUTPUT_DIR% rd /s /q %OUTPUT_DIR%
mkdir %OUTPUT_DIR%

echo Building and packing Shared projects...
dotnet pack src\Shared\DiscountManager.Shared.SharedKernel\DiscountManager.Shared.SharedKernel.csproj --configuration %CONFIG% --output %OUTPUT_DIR%

echo Building and packing Modules...
for /d %%D in (src\Modules\*) do (
    for %%F in ("%%D\*.csproj") do (
        echo Processing %%F...
        dotnet pack "%%F" --configuration %CONFIG% --output %OUTPUT_DIR%
    )
)

echo Done! Packages are available in %OUTPUT_DIR%
pause
