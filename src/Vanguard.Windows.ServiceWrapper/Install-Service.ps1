param(
	[Parameter(Mandatory=$true)][string]$Name,
	[Parameter(Mandatory=$true)][string]$DisplayName,
	[Parameter(Mandatory=$true)][string]$Configuration,
	[string]$Description = ""
)

$service = Get-Service -Name $Name -ErrorAction SilentlyContinue
if ($service.Length -gt 0) {
	Write-Error "Service $Name is already installed"
	exit
}

$binaryPath = "$($PWD.Path)\Vanguard.Windows.ServiceWrapper.exe /Environment:$Configuration"

New-Service -Name $Name -DisplayName $DisplayName -Description $Description -StartupType Auto -BinaryPathName $binaryPath
# TODO: Set service account