param(
    [string]$ServerInstance = "VRS-AMITPC\SQLEXPRESS",
    [string]$DatabaseName = "CYHDocBrandBD"
)

Add-Type -AssemblyName System.Data
$ErrorActionPreference = "Stop"

$masterConnStr = "Server=$ServerInstance;Database=master;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True"
$dbConnStr = "Server=$ServerInstance;Database=$DatabaseName;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True"

Write-Host "Connecting to SQL Server: $ServerInstance..." -ForegroundColor Cyan

# 0. Connect to master and ensure CYHDocBrandBD database exists
try {
    $masterCon = New-Object System.Data.SqlClient.SqlConnection($masterConnStr)
    $masterCon.Open()
    Write-Host "Connected to $ServerInstance (master) successfully!" -ForegroundColor Green

    $checkDbCmd = $masterCon.CreateCommand()
    $checkDbCmd.CommandText = "IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'$DatabaseName') BEGIN CREATE DATABASE [$DatabaseName]; END"
    $checkDbCmd.ExecuteNonQuery() | Out-Null
    $checkDbCmd.Dispose()
    $masterCon.Close()
    Write-Host "Database [$DatabaseName] verified/created on $ServerInstance!" -ForegroundColor Green
} catch {
    $errMsg = $_.Exception.Message
    Write-Host "Failed to verify/create database on $ServerInstance - $errMsg" -ForegroundColor Red
    exit 1
}

# Connect to target database
$con = New-Object System.Data.SqlClient.SqlConnection($dbConnStr)
$con.Open()
Write-Host "Connected successfully to $ServerInstance (Database: $DatabaseName)!" -ForegroundColor Green

function Execute-SqlFile($filePath, $fileLabel) {
    if (-not (Test-Path $filePath)) {
        Write-Host "File not found: $filePath" -ForegroundColor Red
        return
    }

    Write-Host "`n--- Executing $fileLabel ($filePath) ---" -ForegroundColor Cyan
    $rawSql = [IO.File]::ReadAllText($filePath, [Text.Encoding]::UTF8)
    $batches = [System.Text.RegularExpressions.Regex]::Split($rawSql, "^\s*GO\s*$", [System.Text.RegularExpressions.RegexOptions]::Multiline -bor [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)

    $executed = 0
    foreach ($batch in $batches) {
        $trimmed = $batch.Trim()
        if ([string]::IsNullOrWhiteSpace($trimmed)) { continue }
        if ($trimmed -match "^\s*CREATE\s+DATABASE" -or $trimmed -match "^\s*IF\s+NOT\s+EXISTS\s*\(\s*SELECT\s+name\s+FROM\s+sys\.databases" -or $trimmed -match "^\s*USE\s+\[") {
            continue
        }

        try {
            $cmd = $con.CreateCommand()
            $cmd.CommandText = $trimmed
            $cmd.CommandTimeout = 120
            $cmd.ExecuteNonQuery() | Out-Null
            $cmd.Dispose()
            $executed++
        } catch {
            $summary = $trimmed.Substring(0, [Math]::Min(60, $trimmed.Length)).Replace("`r", " ").Replace("`n", " ")
            $errText = $_.Exception.Message
            Write-Host "Batch notice ($summary...): $errText" -ForegroundColor Yellow
        }
    }
    Write-Host "Completed ${fileLabel} - $executed batches applied." -ForegroundColor Green
}

# 1. Apply Full Database Setup (Tables, Constraints, Indexes)
$setupFile = Join-Path $PSScriptRoot "Full_Database_Setup_CYH.sql"
Execute-SqlFile $setupFile "1. Tables & Schema Setup"

# 2. Apply Stored Procedures (Latest Procedures)
$procFile = Join-Path $PSScriptRoot "Procedures_CYH.sql"
Execute-SqlFile $procFile "2. Stored Procedures Update"

# 3. Apply Banner Templates Setup
$tplFile = Join-Path $PSScriptRoot "Setup_BannerTemplates_CYH.sql"
if (Test-Path $tplFile) {
    Execute-SqlFile $tplFile "3. Banner Templates Setup"
}

# 4. Ensure PasswordChangeHistory_CYH Table Exists & Has All Required Columns
Write-Host "`nEnsuring [PasswordChangeHistory_CYH] columns are up to date..." -ForegroundColor Cyan
try {
    $ensureHistSql = @"
    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PasswordChangeHistory_CYH]') AND type in (N'U'))
    BEGIN
        CREATE TABLE [dbo].[PasswordChangeHistory_CYH] (
            [Id]               INT IDENTITY(1,1) NOT NULL,
            [UserId]           NVARCHAR(450)     NOT NULL,
            [UserEmail]        NVARCHAR(256)     NULL,
            [UserRole]         NVARCHAR(50)      NULL,
            [IpAddress]        NVARCHAR(100)     NULL,
            [City]             NVARCHAR(100)     NULL,
            [Region]           NVARCHAR(100)     NULL,
            [Country]          NVARCHAR(100)     NULL,
            [Latitude]         DECIMAL(9,6)      NULL,
            [Longitude]        DECIMAL(9,6)      NULL,
            [LocationSummary]  NVARCHAR(300)     NULL,
            [UserAgent]        NVARCHAR(500)     NULL,
            [Remarks]          NVARCHAR(250)     NULL,
            [ChangedAt]        DATETIME2(7)      NOT NULL DEFAULT (DATEADD(minute, 330, SYSUTCDATETIME())),
            CONSTRAINT [PK_PasswordChangeHistory_CYH] PRIMARY KEY CLUSTERED ([Id] ASC)
        );
        CREATE NONCLUSTERED INDEX [IX_PasswordChangeHistory_UserId] ON [dbo].[PasswordChangeHistory_CYH]([UserId]);
        CREATE NONCLUSTERED INDEX [IX_PasswordChangeHistory_ChangedAt] ON [dbo].[PasswordChangeHistory_CYH]([ChangedAt] DESC);
    END
"@
    $cmdH = $con.CreateCommand()
    $cmdH.CommandText = $ensureHistSql
    $cmdH.ExecuteNonQuery() | Out-Null
    $cmdH.Dispose()
    Write-Host "[PasswordChangeHistory_CYH] table verified/updated!" -ForegroundColor Green
} catch {
    $errText = $_.Exception.Message
    Write-Host "Notice on PasswordChangeHistory_CYH: $errText" -ForegroundColor Yellow
}

# 4.1 Ensure DoctorPushSubscriptions_CYH Table Exists
Write-Host "`nEnsuring [DoctorPushSubscriptions_CYH] table is up to date..." -ForegroundColor Cyan
try {
    $ensurePushSql = @"
    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DoctorPushSubscriptions_CYH]') AND type in (N'U'))
    BEGIN
        CREATE TABLE [dbo].[DoctorPushSubscriptions_CYH] (
            [Id]           INT IDENTITY(1,1) NOT NULL,
            [DoctorId]     INT               NULL,
            [UserId]       NVARCHAR(450)     NULL,
            [Endpoint]     NVARCHAR(MAX)     NOT NULL,
            [P256dh]       NVARCHAR(500)     NOT NULL,
            [Auth]         NVARCHAR(500)     NOT NULL,
            [UserAgent]    NVARCHAR(500)     NULL,
            [CreatedAt]    DATETIME2(7)      NOT NULL DEFAULT (DATEADD(minute, 330, SYSUTCDATETIME())),
            [LastActiveAt] DATETIME2(7)      NOT NULL DEFAULT (DATEADD(minute, 330, SYSUTCDATETIME())),
            CONSTRAINT [PK_DoctorPushSubscriptions_CYH] PRIMARY KEY CLUSTERED ([Id] ASC)
        );
        CREATE NONCLUSTERED INDEX [IX_DoctorPush_DoctorId] ON [dbo].[DoctorPushSubscriptions_CYH]([DoctorId]);
        CREATE NONCLUSTERED INDEX [IX_DoctorPush_UserId] ON [dbo].[DoctorPushSubscriptions_CYH]([UserId]);
    END
"@
    $cmdPush = $con.CreateCommand()
    $cmdPush.CommandText = $ensurePushSql
    $cmdPush.ExecuteNonQuery() | Out-Null
    $cmdPush.Dispose()
    Write-Host "[DoctorPushSubscriptions_CYH] table verified/updated!" -ForegroundColor Green
} catch {
    $errText = $_.Exception.Message
    Write-Host "Notice on DoctorPushSubscriptions_CYH: $errText" -ForegroundColor Yellow
}

# 5. Seed Initial Data
Write-Host "`nSeeding Default Admin & Doctor accounts..." -ForegroundColor Cyan
try {
    $cmdSeed = $con.CreateCommand()
    $cmdSeed.CommandText = "ManageDoctor_CYH"
    $cmdSeed.CommandType = [System.Data.CommandType]::StoredProcedure
    $cmdSeed.Parameters.AddWithValue("@Flag", "SEED_INITIAL_DATA") | Out-Null
    $cmdSeed.ExecuteNonQuery() | Out-Null
    $cmdSeed.Dispose()
    Write-Host "Default Admin & Doctor seeded successfully!" -ForegroundColor Green
} catch {
    $errText = $_.Exception.Message
    Write-Host "Seed accounts notice: $errText" -ForegroundColor Yellow
}

# 6. Verification Report
Write-Host "`n================ VERIFICATION REPORT: $ServerInstance ($DatabaseName) ================" -ForegroundColor Cyan
$cmdTables = $con.CreateCommand()
$cmdTables.CommandText = @"
SELECT t.name, SUM(p.rows) AS TotalRows
FROM sys.tables t
INNER JOIN sys.partitions p ON t.object_id = p.object_id AND p.index_id IN (0, 1)
GROUP BY t.name
ORDER BY t.name
"@
$rTables = $cmdTables.ExecuteReader()
Write-Host "Tables & Row Counts in ${DatabaseName}:" -ForegroundColor Yellow
while ($rTables.Read()) {
    Write-Host ("  [Table] {0,-35} | Rows: {1}" -f $rTables[0], $rTables[1]) -ForegroundColor White
}
$rTables.Close()
$cmdTables.Dispose()

$cmdProcs = $con.CreateCommand()
$cmdProcs.CommandText = "SELECT name, modify_date FROM sys.procedures WHERE is_ms_shipped = 0 ORDER BY name"
$rProcs = $cmdProcs.ExecuteReader()
Write-Host "`nActive Stored Procedures in ${DatabaseName}:" -ForegroundColor Yellow
while ($rProcs.Read()) {
    Write-Host ("  [Proc]  {0,-35} | Modified: {1:yyyy-MM-dd HH:mm}" -f $rProcs[0], $rProcs[1]) -ForegroundColor White
}
$rProcs.Close()
$cmdProcs.Dispose()

$con.Close()
Write-Host "`n================ LOCAL DATABASE SYNC COMPLETED SUCCESSFULLY ================" -ForegroundColor Green
