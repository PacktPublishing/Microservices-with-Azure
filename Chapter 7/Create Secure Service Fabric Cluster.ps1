$subscriptionId = '296f81dd-c155-4800-8b0f-5df96fb45f5b'
$password = 'P@ssw0rd123'
$clusterDNSName = 'microserviceswithazure.southcentralus.cloudapp.azure.com'
$resourceGroupName = 'microserviceswithazure-rg'
$resourcesLocaltion = 'southcentral us'
$keyVaultName = 'microserviceskeyvault'
$keyVaultKeyName = 'clustercertificate'

 Create a self-signed certificate
$certificatePassword = ConvertTo-SecureString -String $password -AsPlainText -Force
New-SelfSignedCertificate -CertStoreLocation Cert:\CurrentUser\My -DnsName $clusterDNSName -Provider 'Microsoft Enhanced Cryptographic Provider v1.0' | Export-PfxCertificate -FilePath ([Environment]::GetFolderPath('Desktop')+'/ClusterCertificate.pfx') -Password $certificatePassword

Login-AzureRmAccount
Get-AzureRmSubscription
Set-AzureRmContext -SubscriptionId $subscriptionId

 Create Resource Group
New-AzureRmResourceGroup -Name $resourceGroupName -Location $resourcesLocaltion

 Create Key Vault Instance
New-AzureRmKeyVault -VaultName $keyVaultName -ResourceGroupName $resourceGroupName -Location $resourcesLocaltion -EnabledForDeployment

Upload certificate to Key Vault
$cer = Add-AzureKeyVaultKey -VaultName $keyVaultName -Name $keyVaultKeyName -KeyFilePath ([Environment]::GetFolderPath('Desktop')+'/ClusterCertificate.pfx') -KeyFilePassword $certificatePassword

Validate certificate upload
$cer.KeyId

 Set Secret in Key Vault
$bytes = [System.IO.File]::ReadAllBytes(([Environment]::GetFolderPath('Desktop')+'/ClusterCertificate.pfx'))
$base64 = [System.Convert]::ToBase64String($bytes)

$jsonBlob = @{
   data = $base64
   dataType = 'pfx'
   password = $password
   } | ConvertTo-Json

$contentbytes = [System.Text.Encoding]::UTF8.GetBytes($jsonBlob)
$content = [System.Convert]::ToBase64String($contentbytes)

$secretValue = ConvertTo-SecureString -String $content -AsPlainText -Force
Set-AzureKeyVaultSecret -VaultName $keyVaultName -Name $keyVaultKeyName -SecretValue $secretValue

 Output Thumbprint
$clusterCertificate = new-object System.Security.Cryptography.X509Certificates.X509Certificate2 ([Environment]::GetFolderPath('Desktop')+'/ClusterCertificate.pfx'), $certificatePassword
$clusterCertificate.Thumbprint

 Connect to Service Fabric cluster.
Connect-ServiceFabricCluster -ConnectionEndpoint ($clusterDNSName + ':19000') `
          -KeepAliveIntervalInSec 10 `
          -X509Credential -ServerCertThumbprint [cluster certificate thumbprint] `
          -FindType FindByThumbprint -FindValue [client certificate thumbprint] `
          -StoreLocation CurrentUser -StoreName My