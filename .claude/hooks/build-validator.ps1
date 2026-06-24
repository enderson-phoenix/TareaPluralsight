# build-validator.ps1
# Hook: PostToolUse — Build automatico tras editar un archivo .cs
#
# PROPOSITO:
#   Detectar errores de compilacion inmediatamente despues de que Claude edite
#   cualquier archivo C#, antes de que se acumulen multiples archivos rotos.
#   Actua como primer nivel del quality gate automatico de CalSystem.
#
# CUANDO SE EJECUTA:
#   Evento PostToolUse con matcher "Edit|Write".
#   Se filtra internamente: solo actua si el archivo editado termina en .cs
#
# SALIDA:
#   [PASS] Build OK                    -> compilacion exitosa, sin errores
#   [WARN] Build OK con N warning(s)   -> compila pero hay advertencias
#   [FAIL] N error(s) de compilacion   -> errores, muestra las primeras lineas
#
# BLOQUEANTE: No. Solo informa, no cancela ninguna accion de Claude.
#
# VARIABLES DE ENTORNO:
#   $env:CLAUDE_TOOL_INPUT -> JSON con input de la herramienta que disparo el hook
#                             Para Edit/Write: { "file_path": "ruta/al/archivo" }

try {
    # Leer el path del archivo que Claude acaba de editar
    $toolInput = $env:CLAUDE_TOOL_INPUT | ConvertFrom-Json -ErrorAction Stop
    $filePath = $toolInput.file_path

    # Solo proceder si es un archivo C#
    if ($filePath -notmatch '\.cs$') { exit 0 }

    $fileName = [IO.Path]::GetFileName($filePath)
    Write-Host ''
    Write-Host "-- change-validator [Op1]: $fileName editado -- verificando compilacion..."

    # Ejecutar build silencioso sin incrementales para forzar recompilacion
    $buildOutput = dotnet build --no-incremental -v q 2>&1

    # Contar errores y warnings en la salida
    $errorCount   = ($buildOutput | Select-String ' error ').Count
    $warningCount = ($buildOutput | Select-String ' warning ').Count

    if ($errorCount -gt 0) {
        # Mostrar conteo y primeras lineas de error para diagnostico rapido
        Write-Host "`[FAIL`] $errorCount error(s) de compilacion. Ejecuta /validate para detalles."
        $buildOutput | Select-String ' error ' | Select-Object -First 2 | ForEach-Object {
            Write-Host "  $($_.Line)"
        }
    }
    elseif ($warningCount -gt 0) {
        # Compila pero hay advertencias
        Write-Host "`[WARN`] Build OK con $warningCount warning(s)."
    }
    else {
        # Compilacion limpia
        Write-Host "`[PASS`] Build OK"
    }
}
catch {
    # No interrumpir el flujo de Claude si el hook falla inesperadamente
    exit 0
}
