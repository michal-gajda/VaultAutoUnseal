# VaultAutoUnseal

```powershell
git init
dotnet new gitignore
dotnet new sln --name VaultAutoUnseal
dotnet new worker --framework net10.0 --use-program-main --output src/Worker --name VaultAutoUnseal.Worker
dotnet add src/Worker package Microsoft.Extensions.Hosting.WindowsServices
```
