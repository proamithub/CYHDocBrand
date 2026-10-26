$ErrorActionPreference = "Stop"

$connStr = "Data Source=208.91.198.196; Initial Catalog=CYHDocBrand; User Id=CYHDocBrandSA; Password=CYHDocBrand@PassAK; pooling=true;Max Pool Size=200;MultipleActiveResultSets=True;TrustServerCertificate=True"

Write-Host "Connecting to SQL Server: 208.91.198.196 (Database: CYHDocBrand)..." -ForegroundColor Cyan
$con = New-Object System.Data.SqlClient.SqlConnection($connStr)
$con.Open()
Write-Host "Connected successfully!" -ForegroundColor Green

# 1. Read Full_Database_Setup_CYH.sql
$sqlFilePath = Join-Path $PSScriptRoot "Full_Database_Setup_CYH.sql"
if (-not (Test-Path $sqlFilePath)) {
    $sqlFilePath = "D:\MyAppAi\CYHDocBrand\Scripts\Full_Database_Setup_CYH.sql"
}

$rawSql = [IO.File]::ReadAllText($sqlFilePath, [Text.Encoding]::UTF8)

# Split by GO lines
$batches = [System.Text.RegularExpressions.Regex]::Split($rawSql, "^\s*GO\s*$", [System.Text.RegularExpressions.RegexOptions]::Multiline -bor [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)

$batchIndex = 0
$executedCount = 0

foreach ($batch in $batches) {
    $trimmed = $batch.Trim()
    if ([string]::IsNullOrWhiteSpace($trimmed)) {
        continue
    }

    # Skip CREATE DATABASE or USE database statements
    if ($trimmed -match "^\s*CREATE\s+DATABASE" -or $trimmed -match "^\s*IF\s+NOT\s+EXISTS\s*\(\s*SELECT\s+name\s+FROM\s+sys\.databases" -or $trimmed -match "^\s*USE\s+\[") {
        Write-Host "Skipping database creation/switch batch: $($trimmed.Substring(0, [Math]::Min(50, $trimmed.Length)))..." -ForegroundColor Yellow
        continue
    }

    $batchIndex++
    $summary = $trimmed.Substring(0, [Math]::Min(70, $trimmed.Length)).Replace("`r", " ").Replace("`n", " ")

    try {
        $cmd = $con.CreateCommand()
        $cmd.CommandText = $trimmed
        $cmd.CommandTimeout = 60
        $cmd.ExecuteNonQuery() | Out-Null
        $cmd.Dispose()
        Write-Host "[$batchIndex] Applied: $summary..." -ForegroundColor Green
        $executedCount++
    } catch {
        Write-Host "[$batchIndex] Error in batch: $summary" -ForegroundColor Red
        Write-Host "Details: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`nTotal schema batches executed: $executedCount" -ForegroundColor Cyan

# 2. Seed Default Admin & Doctor accounts via ManageDoctor_CYH
Write-Host "`nSeeding Default Admin and Doctor accounts..." -ForegroundColor Cyan
try {
    $cmdSeed = $con.CreateCommand()
    $cmdSeed.CommandText = "ManageDoctor_CYH"
    $cmdSeed.CommandType = [System.Data.CommandType]::StoredProcedure
    $cmdSeed.Parameters.AddWithValue("@Flag", "SEED_INITIAL_DATA") | Out-Null
    $cmdSeed.ExecuteNonQuery() | Out-Null
    $cmdSeed.Dispose()
    Write-Host "Default Admin & Doctor seeded successfully!" -ForegroundColor Green
} catch {
    Write-Host "Seed accounts notice: $($_.Exception.Message)" -ForegroundColor Yellow
}

# 3. Seed Default Credit Types
Write-Host "`nSeeding Default Credit Types..." -ForegroundColor Cyan
try {
    $cmdCheckType = $con.CreateCommand()
    $cmdCheckType.CommandText = "SELECT COUNT(1) FROM [dbo].[CreditTypes_CYH]"
    $typeCount = [Convert]::ToInt32($cmdCheckType.ExecuteScalar())
    $cmdCheckType.Dispose()

    if ($typeCount -eq 0) {
        # Type 1: Template Publish
        $cmdT1 = $con.CreateCommand()
        $cmdT1.CommandText = "ManageCredits_CYH"
        $cmdT1.CommandType = [System.Data.CommandType]::StoredProcedure
        $cmdT1.Parameters.AddWithValue("@Flag", "SAVE_TYPE") | Out-Null
        $cmdT1.Parameters.AddWithValue("@Name", "Template Photo Publish") | Out-Null
        $cmdT1.Parameters.AddWithValue("@Code", "TEMPLATE_PUBLISH") | Out-Null
        $cmdT1.Parameters.AddWithValue("@ServiceType", "Free") | Out-Null
        $cmdT1.Parameters.AddWithValue("@IsPaid", $false) | Out-Null
        $cmdT1.Parameters.AddWithValue("@IsActive", $true) | Out-Null
        $cmdT1.Parameters.AddWithValue("@Description", "Standard healthcare template customization and publish") | Out-Null
        $cmdT1.Parameters.AddWithValue("@CreditCost", 1) | Out-Null
        $cmdT1.ExecuteScalar() | Out-Null
        $cmdT1.Dispose()

        # Type 2: AI Banner
        $cmdT2 = $con.CreateCommand()
        $cmdT2.CommandText = "ManageCredits_CYH"
        $cmdT2.CommandType = [System.Data.CommandType]::StoredProcedure
        $cmdT2.Parameters.AddWithValue("@Flag", "SAVE_TYPE") | Out-Null
        $cmdT2.Parameters.AddWithValue("@Name", "AI Banner Creation & Publish") | Out-Null
        $cmdT2.Parameters.AddWithValue("@Code", "AI_BANNER_PUBLISH") | Out-Null
        $cmdT2.Parameters.AddWithValue("@ServiceType", "Paid") | Out-Null
        $cmdT2.Parameters.AddWithValue("@IsPaid", $true) | Out-Null
        $cmdT2.Parameters.AddWithValue("@IsActive", $true) | Out-Null
        $cmdT2.Parameters.AddWithValue("@Description", "AI generated prompt healthcare banner customization and publish") | Out-Null
        $cmdT2.Parameters.AddWithValue("@CreditCost", 1) | Out-Null
        $cmdT2.ExecuteScalar() | Out-Null
        $cmdT2.Dispose()

        Write-Host "Default Credit Types seeded successfully!" -ForegroundColor Green
    } else {
        Write-Host "Credit Types already exist ($typeCount types)." -ForegroundColor Yellow
    }

    # Initialize doctor credit rows at 00
    $cmdDocs = $con.CreateCommand()
    $cmdDocs.CommandText = "SELECT Id FROM [dbo].[DoctorProfiles]"
    $rDocs = $cmdDocs.ExecuteReader()
    $docIds = New-Object System.Collections.Generic.List[int]
    while ($rDocs.Read()) {
        $docIds.Add($rDocs.GetInt32(0))
    }
    $rDocs.Close()
    $cmdDocs.Dispose()

    foreach ($docId in $docIds) {
        $cmdInit = $con.CreateCommand()
        $cmdInit.CommandText = "ManageCredits_CYH"
        $cmdInit.CommandType = [System.Data.CommandType]::StoredProcedure
        $cmdInit.Parameters.AddWithValue("@Flag", "INIT_DOCTOR_CREDITS") | Out-Null
        $cmdInit.Parameters.AddWithValue("@DoctorId", $docId) | Out-Null
        $cmdInit.ExecuteNonQuery() | Out-Null
        $cmdInit.Dispose()
    }
    Write-Host "Doctor credit accounts verified/initialized ($($docIds.Count) doctors)." -ForegroundColor Green
} catch {
    Write-Host "Credit system seed notice: $($_.Exception.Message)" -ForegroundColor Yellow
}

# 4. Verification Check
Write-Host "`n================ VERIFICATION REPORT ================" -ForegroundColor Cyan
$cmdTables = $con.CreateCommand()
$cmdTables.CommandText = "SELECT name FROM sys.tables ORDER BY name"
$rTables = $cmdTables.ExecuteReader()
Write-Host "Tables in CYHDocBrand:" -ForegroundColor Yellow
while ($rTables.Read()) {
    Write-Host "  [Table] $($rTables[0])" -ForegroundColor White
}
$rTables.Close()
$cmdTables.Dispose()

$cmdProcs = $con.CreateCommand()
$cmdProcs.CommandText = "SELECT name FROM sys.procedures WHERE is_ms_shipped = 0 ORDER BY name"
$rProcs = $cmdProcs.ExecuteReader()
Write-Host "`nProcedures in CYHDocBrand:" -ForegroundColor Yellow
while ($rProcs.Read()) {
    Write-Host "  [Proc]  $($rProcs[0])" -ForegroundColor White
}
$rProcs.Close()
$cmdProcs.Dispose()

$cmdUsers = $con.CreateCommand()
$cmdUsers.CommandText = "SELECT Email, FullName, Role, IsActive FROM AspNetUsers"
$rUsers = $cmdUsers.ExecuteReader()
Write-Host "`nUsers in CYHDocBrand:" -ForegroundColor Yellow
while ($rUsers.Read()) {
    Write-Host "  [User]  $($rUsers[0]) | $($rUsers[1]) | Role: $($rUsers[2]) | Active: $($rUsers[3])" -ForegroundColor White
}
$rUsers.Close()
$cmdUsers.Dispose()

$con.Close()
Write-Host "`n================ SYNC FINISHED ================" -ForegroundColor Green
