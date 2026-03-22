# NotionFlow Database Setup Script
# Este script configura y aplica las migraciones de base de datos

Write-Host "╔════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  NotionFlow Database Setup - PostgreSQL Migration     ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Validar que PostgreSQL está disponible
Write-Host "🔍 Verificando conectividad a PostgreSQL..." -ForegroundColor Yellow

try {
    $pg_test = pg_isready 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ PostgreSQL está disponible" -ForegroundColor Green
    } else {
        Write-Host "❌ PostgreSQL no está accesible" -ForegroundColor Red
        Write-Host ""
        Write-Host "💡 Soluciones:" -ForegroundColor Yellow
        Write-Host "   1. Instala PostgreSQL: https://www.postgresql.org/download/windows/"
        Write-Host "   2. O usa Docker: docker run --name notionflow-postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=notionflow -p 5432:5432 -d postgres:latest"
        exit 1
    }
} catch {
    Write-Host "⚠️  No se pudo verificar PostgreSQL. Continuando..." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "📦 Aplicando migraciones a la base de datos..." -ForegroundColor Yellow

# Cambiar al directorio del API
cd "D:\workspace\notionFlow\NotionFlow.Api"

# Aplicar las migraciones
Write-Host ""
Write-Host "▶️  Ejecutando: dotnet ef database update" -ForegroundColor Cyan
Write-Host ""

dotnet ef database update

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "╔════════════════════════════════════════════════════════╗" -ForegroundColor Green
    Write-Host "║  ✅ ¡Migraciones Aplicadas Exitosamente!             ║" -ForegroundColor Green
    Write-Host "╚════════════════════════════════════════════════════════╝" -ForegroundColor Green
    Write-Host ""
    Write-Host "📋 Tablas creadas:" -ForegroundColor Green
    Write-Host "   • AspNetUsers (Identity Users)"
    Write-Host "   • AspNetRoles (Identity Roles)"
    Write-Host "   • Courses"
    Write-Host "   • CourseStudents"
    Write-Host "   • Evaluations"
    Write-Host "   • Grades"
    Write-Host "   • Contents"
    Write-Host ""
    Write-Host "🚀 Próximos pasos:" -ForegroundColor Green
    Write-Host "   1. Compilar el frontend: dotnet build (en NotionFlow.App)"
    Write-Host "   2. Ejecutar la aplicación"
    Write-Host ""
} else {
    Write-Host ""
    Write-Host "❌ Error al aplicar las migraciones" -ForegroundColor Red
    Write-Host ""
    Write-Host "💡 Verifica:" -ForegroundColor Yellow
    Write-Host "   1. PostgreSQL está ejecutándose"
    Write-Host "   2. La base de datos 'notionflow' existe"
    Write-Host "   3. Las credenciales en appsettings.json son correctas"
    exit 1
}
