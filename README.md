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

## NSSM

```powershell
winget install --id NSSM.NSSM --scope machine
```

## Consul

```powershell
nssm install Consul C:\Workspaces\Tools\Consul\consul.exe "agent -config-dir=C:\Workspaces\Tools\Consul\config"
```

```text
datacenter = "dc1"
node_name  = "z440"
data_dir   = "C:/Workspaces/Tools/Consul/data"
log_level  = "INFO"

server = true
bootstrap_expect = 1

bind_addr      = "192.168.1.58"
advertise_addr = "192.168.1.58"
client_addr    = "0.0.0.0"

ports {
  http = 8500
  dns  = 8600
}

ui_config {
  enabled = true
}
```

## Vault

```powershell
nssm install Vault C:\Workspaces\Tools\Vault\vault.exe "server -config=C:\Workspaces\Tools\Vault\config\vault.hcl"
```

```text
ui = true

disable_mlock = true

storage "consul" {
  address = "127.0.0.1:8500"
  path    = "vault/"
}

listener "tcp" {
  address     = "0.0.0.0:8200"
  tls_disable = 1
}

api_addr     = "http://192.168.1.58:8200"
cluster_addr = "http://192.168.1.58:8201"
```

```powershell
[Environment]::SetEnvironmentVariable("VAULT_ADDR", "http://127.0.0.1:8200", "Machine")
```
