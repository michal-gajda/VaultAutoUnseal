# VaultAutoUnseal

```powershell
git init
dotnet new gitignore
dotnet new sln --name VaultAutoUnseal
dotnet new worker --framework net10.0 --use-program-main --output src/Worker --name VaultAutoUnseal.Worker
dotnet add src/Worker package Microsoft.Extensions.Hosting.WindowsServices
```

```powershell
 dotnet publish src/Worker/VaultAutoUnseal.Worker.csproj --configuration Release --output C:\Workspaces\Tools\VaultAutoUnseal
```

```powershell
sc.exe create "VaultAutoUnlock" binPath= "C:\Workspaces\Tools\VaultAutoUnseal\VaultAutoUnseal.Worker.exe" DisplayName= "Vault Auto Unlock Service" start= auto depend= "Vault/Consul"
```
