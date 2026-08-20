# Nome do container e configurações
$ContainerName = "memoana-postgres"
$PortHost = "5432"
$PortContainer = "5432"
$DbName = "MemoAnaDb"
$DbUser = "m3m04n4"
$DbPassword = "M3m04N4@D!3"
$VolumeName = "memoana-postgres-data"
$ImageName = "postgres:17" 

$volumeExists = docker volume ls --q -f "name=^${VolumeName}$"
if (-not $volumeExists) {
    Write-Host "Criando volume de dados: $VolumeName..." -ForegroundColor Cyan
    docker volume create $VolumeName | Out-Null
}

$containerExists = docker ps -a -q -f "name=^${ContainerName}$"

if ($containerExists) {
    $status = docker inspect --format='{{.State.Running}}' $ContainerName
    
    if ($status -eq "true") {
        Write-Host "O container '$ContainerName' já está rodando." -ForegroundColor Green
    } else {
        Write-Host "Iniciando container '$ContainerName' existente..." -ForegroundColor Yellow
        docker start $ContainerName | Out-Null
        Write-Host "Container iniciado com sucesso!" -ForegroundColor Green
    }
} else {
    Write-Host "Criando e iniciando novo container '$ContainerName'..." -ForegroundColor Cyan
    
    docker run -d `
        --name $ContainerName `
        -p "${PortHost}:${PortContainer}" `
        -e POSTGRES_DB=$DbName `
        -e POSTGRES_USER=$DbUser `
        -e POSTGRES_PASSWORD=$DbPassword `
        -v "${VolumeName}:/var/lib/postgresql/data" `
        --restart unless-stopped `
        $ImageName | Out-Null

    Write-Host "Container '$ContainerName' criado e rodando na porta $PortHost!" -ForegroundColor Green
}

# 3. Exibe o estado e logs rápidos
Write-Host ""
Write-Host "Status do Container:" -ForegroundColor Green
docker ps -f "name=^${ContainerName}$"