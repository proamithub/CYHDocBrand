param(
    [string]$LiveServer = "208.91.198.196",
    [string]$LiveDb = "CYHDocBrand",
    [string]$LiveUser = "CYHDocBrandSA",
    [string]$LivePass = "CYHDocBrand@PassAK",
    [string]$LocalServer = ".",
    [string]$LocalDb = "CYHDocBrandBD"
)

Add-Type -AssemblyName System.Data

$ErrorActionPreference = "Stop"

$liveConnStr = "Data Source=$LiveServer; Initial Catalog=$LiveDb; User Id=$LiveUser; Password=$LivePass; MultipleActiveResultSets=True; TrustServerCertificate=True"
$localConnStr = "Server=$LocalServer; Database=$LocalDb; Integrated Security=True; MultipleActiveResultSets=True; TrustServerCertificate=True"

Write-Host "Connecting to Live: $LiveServer / $LiveDb..." -ForegroundColor Cyan
$liveConn = New-Object System.Data.SqlClient.SqlConnection($liveConnStr)
$liveConn.Open()
Write-Host "Live Connected successfully!" -ForegroundColor Green

Write-Host "Connecting to Local: $LocalServer / $LocalDb..." -ForegroundColor Cyan
$localConn = New-Object System.Data.SqlClient.SqlConnection($localConnStr)
$localConn.Open()
Write-Host "Local Connected successfully!" -ForegroundColor Green

# 1. Disable all constraints on Local
Write-Host "`nDisabling foreign key constraints on Local..." -ForegroundColor Yellow
$disableCmd = $localConn.CreateCommand()
$disableCmd.CommandText = "EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';"
$disableCmd.ExecuteNonQuery() | Out-Null
$disableCmd.Dispose()

# 2. Get list of user tables from Live
$getTablesCmd = $liveConn.CreateCommand()
$getTablesCmd.CommandText = @"
SELECT t.name 
FROM sys.tables t
WHERE t.is_ms_shipped = 0
ORDER BY t.name
"@
$reader = $getTablesCmd.ExecuteReader()
$tables = @()
while ($reader.Read()) {
    $tables += $reader.GetString(0)
}
$reader.Close()
$getTablesCmd.Dispose()

Write-Host "Found $($tables.Count) user tables to sync." -ForegroundColor Cyan

foreach ($table in $tables) {
    Write-Host "`nSyncing table: [$table]..." -ForegroundColor White

    # Check if table exists in local
    $checkCmd = $localConn.CreateCommand()
    $checkCmd.CommandText = "SELECT COUNT(*) FROM sys.tables WHERE name = '$table'"
    $exists = [int]$checkCmd.ExecuteScalar()
    $checkCmd.Dispose()

    if ($exists -eq 0) {
        Write-Host "  Table [$table] does not exist in local! Skipping." -ForegroundColor Yellow
        continue
    }

    # Check if table has identity column
    $idCmd = $localConn.CreateCommand()
    $idCmd.CommandText = "SELECT COUNT(*) FROM sys.identity_columns WHERE object_id = OBJECT_ID('$table')"
    $hasIdentity = [int]$idCmd.ExecuteScalar() -gt 0
    $idCmd.Dispose()

    # Clear local table
    $delCmd = $localConn.CreateCommand()
    $delCmd.CommandText = "DELETE FROM [$table];"
    $delCmd.ExecuteNonQuery() | Out-Null
    $delCmd.Dispose()

    # Read from live
    $selectCmd = $liveConn.CreateCommand()
    $selectCmd.CommandText = "SELECT * FROM [$table]"
    $liveReader = $selectCmd.ExecuteReader()

    # Get column schema from live reader
    $schemaTable = $liveReader.GetSchemaTable()
    
    # Bulk copy to local
    $bulkOptions = [System.Data.SqlClient.SqlBulkCopyOptions]::KeepIdentity -bor [System.Data.SqlClient.SqlBulkCopyOptions]::KeepNulls
    $bulkCopy = New-Object System.Data.SqlClient.SqlBulkCopy($localConn, $bulkOptions, $null)
    $bulkCopy.DestinationTableName = "[$table]"
    $bulkCopy.BulkCopyTimeout = 120

    # Map only columns that exist in both tables
    $localColsCmd = $localConn.CreateCommand()
    $localColsCmd.CommandText = "SELECT c.name FROM sys.columns c WHERE c.object_id = OBJECT_ID('$table')"
    $localColsReader = $localColsCmd.ExecuteReader()
    $localCols = @{}
    while ($localColsReader.Read()) {
        $localCols[$localColsReader.GetString(0).ToLower()] = $localColsReader.GetString(0)
    }
    $localColsReader.Close()
    $localColsCmd.Dispose()

    foreach ($row in $schemaTable.Rows) {
        $colName = $row["ColumnName"]
        if ($localCols.ContainsKey($colName.ToLower())) {
            $bulkCopy.ColumnMappings.Add($colName, $colName) | Out-Null
        }
    }

    $bulkCopy.WriteToServer($liveReader)
    $liveReader.Close()
    $selectCmd.Dispose()
    $bulkCopy.Close()

    # Verify count
    $cntCmd = $localConn.CreateCommand()
    $cntCmd.CommandText = "SELECT COUNT(*) FROM [$table]"
    $localCount = [int]$cntCmd.ExecuteScalar()
    $cntCmd.Dispose()

    Write-Host "  Successfully synced [$table] -> $localCount rows in local." -ForegroundColor Green
}

# 3. Re-enable constraints on Local
Write-Host "`nRe-enabling foreign key constraints on Local..." -ForegroundColor Yellow
$enableCmd = $localConn.CreateCommand()
$enableCmd.CommandText = "EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL';"
try {
    $enableCmd.ExecuteNonQuery() | Out-Null
    Write-Host "All constraints re-enabled successfully!" -ForegroundColor Green
} catch {
    Write-Host "Warning re-enabling some constraints: $($_.Exception.Message)" -ForegroundColor Yellow
}
$enableCmd.Dispose()

# 4. Final Comparison Report
Write-Host "`n================ DATA SYNC COMPARISON REPORT ================" -ForegroundColor Cyan
Write-Host ("{0,-35} | {1,10} | {2,10}" -f "Table Name", "Live Rows", "Local Rows") -ForegroundColor Yellow
Write-Host ("-" * 62) -ForegroundColor Yellow

foreach ($table in $tables) {
    $liveCntCmd = $liveConn.CreateCommand()
    $liveCntCmd.CommandText = "SELECT COUNT(*) FROM [$table]"
    $lCnt = [int]$liveCntCmd.ExecuteScalar()
    $liveCntCmd.Dispose()

    $locCntCmd = $localConn.CreateCommand()
    $locCntCmd.CommandText = "IF OBJECT_ID('$table') IS NOT NULL SELECT COUNT(*) FROM [$table] ELSE SELECT -1"
    $locCnt = [int]$locCntCmd.ExecuteScalar()
    $locCntCmd.Dispose()

    $status = if ($lCnt -eq $locCnt) { "OK" } else { "MISMATCH" }
    $color = if ($lCnt -eq $locCnt) { "Green" } else { "Red" }

    Write-Host ("{0,-35} | {1,10} | {2,10}  [{3}]" -f $table, $lCnt, $locCnt, $status) -ForegroundColor $color
}

$liveConn.Close()
$localConn.Close()

Write-Host "`n================ LIVE TO LOCAL SYNC COMPLETED SUCCESSFULLY ================" -ForegroundColor Green
