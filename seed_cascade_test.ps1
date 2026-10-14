$baseUrl = "http://localhost:5001/api"

# 1. Login
Write-Host "=== Logging in ===" -ForegroundColor Cyan
$loginBody = '{"username":"admin2","password":"TestUser123"}'
$loginResult = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
$token = $loginResult.token
Write-Host "Logged in OK"

$authHeaders = @{
    "Content-Type"  = "application/json"
    "Authorization" = "Bearer $token"
}

# 2. Get existing list IDs
Write-Host "`n=== Getting existing lists ===" -ForegroundColor Cyan
$lists = Invoke-RestMethod -Uri "$baseUrl/dynamiclists" -Headers $authHeaders
$paisListId = ($lists | Where-Object { $_.name -eq "Pais" }).id
$estadoListId = ($lists | Where-Object { $_.name -eq "Estado" }).id
$cidadeListId = ($lists | Where-Object { $_.name -eq "Cidade" }).id
Write-Host "Pais:   $paisListId"
Write-Host "Estado: $estadoListId"  
Write-Host "Cidade: $cidadeListId"

# 3. Check existing data
Write-Host "`n=== Checking existing data ===" -ForegroundColor Cyan
$paisData = Invoke-RestMethod -Uri "$baseUrl/tabledata/list/$paisListId" -Headers $authHeaders
Write-Host "Pais records: $($paisData.Count)"

if ($paisData.Count -eq 0) {
    # Add countries
    Write-Host "`n=== Adding countries ===" -ForegroundColor Cyan
    $brasilBody = @{listId = $paisListId; values = @{nome = "Brasil" } } | ConvertTo-Json
    $brasil = Invoke-RestMethod -Uri "$baseUrl/tabledata" -Method Post -Body $brasilBody -Headers $authHeaders
    $brasilId = $brasil.id
    Write-Host "Brasil: $brasilId"

    $argBody = @{listId = $paisListId; values = @{nome = "Argentina" } } | ConvertTo-Json
    $argentina = Invoke-RestMethod -Uri "$baseUrl/tabledata" -Method Post -Body $argBody -Headers $authHeaders
    $argentinaId = $argentina.id
    Write-Host "Argentina: $argentinaId"
}
else {
    Write-Host "Countries already exist, getting IDs"
    $brasilId = ($paisData | Where-Object { $_.values.nome -eq "Brasil" }).id
    $argentinaId = ($paisData | Where-Object { $_.values.nome -eq "Argentina" }).id
    Write-Host "Brasil: $brasilId"
    Write-Host "Argentina: $argentinaId"
}

# 4. Add states
$estadoData = Invoke-RestMethod -Uri "$baseUrl/tabledata/list/$estadoListId" -Headers $authHeaders
Write-Host "`nEstado records: $($estadoData.Count)"

if ($estadoData.Count -eq 0) {
    Write-Host "=== Adding states ===" -ForegroundColor Cyan
    $spBody = @{listId = $estadoListId; values = @{nome = "Sao Paulo"; pais_id = "$brasilId" } } | ConvertTo-Json
    $sp = Invoke-RestMethod -Uri "$baseUrl/tabledata" -Method Post -Body $spBody -Headers $authHeaders
    $spId = $sp.id
    Write-Host "Sao Paulo: $spId"

    $rjBody = @{listId = $estadoListId; values = @{nome = "Rio de Janeiro"; pais_id = "$brasilId" } } | ConvertTo-Json
    $rj = Invoke-RestMethod -Uri "$baseUrl/tabledata" -Method Post -Body $rjBody -Headers $authHeaders
    $rjId = $rj.id
    Write-Host "Rio de Janeiro: $rjId"

    $baBody = @{listId = $estadoListId; values = @{nome = "Buenos Aires"; pais_id = "$argentinaId" } } | ConvertTo-Json
    $ba = Invoke-RestMethod -Uri "$baseUrl/tabledata" -Method Post -Body $baBody -Headers $authHeaders
    $baId = $ba.id
    Write-Host "Buenos Aires: $baId"
}
else {
    Write-Host "States already exist"
    $spId = ($estadoData | Where-Object { $_.values.nome -eq "Sao Paulo" }).id
    $rjId = ($estadoData | Where-Object { $_.values.nome -eq "Rio de Janeiro" }).id
    $baId = ($estadoData | Where-Object { $_.values.nome -eq "Buenos Aires" }).id
    Write-Host "SP: $spId, RJ: $rjId, BA: $baId"
}

# 5. Add cities
$cidadeData = Invoke-RestMethod -Uri "$baseUrl/tabledata/list/$cidadeListId" -Headers $authHeaders
Write-Host "`nCidade records: $($cidadeData.Count)"

if ($cidadeData.Count -eq 0) {
    Write-Host "=== Adding cities ===" -ForegroundColor Cyan
    $cities = @(
        @{nome = "Sao Paulo Capital"; estado_id = "$spId" },
        @{nome = "Campinas"; estado_id = "$spId" },
        @{nome = "Santos"; estado_id = "$spId" },
        @{nome = "Rio Capital"; estado_id = "$rjId" },
        @{nome = "Niteroi"; estado_id = "$rjId" },
        @{nome = "La Plata"; estado_id = "$baId" },
        @{nome = "Mar del Plata"; estado_id = "$baId" }
    )
    foreach ($city in $cities) {
        $cityBody = @{listId = $cidadeListId; values = $city } | ConvertTo-Json
        Invoke-RestMethod -Uri "$baseUrl/tabledata" -Method Post -Body $cityBody -Headers $authHeaders | Out-Null
        Write-Host "  Added: $($city.nome)"
    }
}
else {
    Write-Host "Cities already exist"
}

# 6. Verify form and update fields with correct list IDs
Write-Host "`n=== Checking form ===" -ForegroundColor Cyan
$forms = Invoke-RestMethod -Uri "$baseUrl/dynamicforms" -Headers $authHeaders
$form = $forms | Where-Object { $_.name -eq "Cadastro de estabelecimento" }
if ($form) {
    $formId = $form.id
    Write-Host "Form: $formId"
    Write-Host "Fields: $($form.fields.Count)"
    foreach ($f in $form.fields | Sort-Object order) {
        Write-Host "  $($f.order): $($f.label) [selectSourceListId=$($f.selectSourceListId), parent=$($f.parentFormFieldId), cascade=$($f.cascadeFilterField), valueField=$($f.selectValueField), displayField=$($f.selectDisplayField)]"
    }
    Write-Host "`nForm URL: http://localhost:5003/forms/$formId/view"
}
else {
    Write-Host "Form not found!"
}

Write-Host "`n=== DATA SUMMARY ===" -ForegroundColor Green
$paisData2 = Invoke-RestMethod -Uri "$baseUrl/tabledata/list/$paisListId" -Headers $authHeaders
$estadoData2 = Invoke-RestMethod -Uri "$baseUrl/tabledata/list/$estadoListId" -Headers $authHeaders
$cidadeData2 = Invoke-RestMethod -Uri "$baseUrl/tabledata/list/$cidadeListId" -Headers $authHeaders
Write-Host "Pais ($paisListId): $($paisData2.Count) records"
Write-Host "Estado ($estadoListId): $($estadoData2.Count) records"
Write-Host "Cidade ($cidadeListId): $($cidadeData2.Count) records"
