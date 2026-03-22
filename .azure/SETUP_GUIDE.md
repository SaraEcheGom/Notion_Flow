# 🚀 GUÍA DE CONFIGURACIÓN FINAL - NotionFlow

## Estado Actual
- ✅ Backend API: Traducido y compilado exitosamente
- ✅ Migración de BD: Creada (`EnglishTranslation`)
- ⏳ Aplicar migraciones: Requiere PostgreSQL
- ⏳ Frontend: Compilación pendiente
- ⏳ XAML Views: Traducción pendiente

---

## 📋 REQUISITOS PREVIOS

### Opción 1: PostgreSQL Local (Recomendado para Desarrollo)

**Windows:**
1. Descargar desde: https://www.postgresql.org/download/windows/
2. Instalar con usuario: `postgres` contraseña: `postgres`
3. Puerto: `5432`
4. Crear base de datos: `notionflow`

**macOS (usando Homebrew):**
```bash
brew install postgresql@15
brew services start postgresql@15
createdb notionflow
```

**Linux (Ubuntu/Debian):**
```bash
sudo apt-get install postgresql postgresql-contrib
sudo -u postgres psql
CREATE DATABASE notionflow;
\q
```

### Opción 2: Docker (Recomendado para Producción)

```powershell
# PowerShell (Windows)
docker run --name notionflow-postgres `
  -e POSTGRES_PASSWORD=postgres `
  -e POSTGRES_DB=notionflow `
  -p 5432:5432 `
  -d postgres:latest

# Bash/Linux/Mac
docker run --name notionflow-postgres \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=notionflow \
  -p 5432:5432 \
  -d postgres:latest
```

---

## 🔧 PASO 1: PREPARAR POSTGRESQL

### Verificar Conectividad

```powershell
# PowerShell - Verificar que PostgreSQL está disponible
pg_isready

# Resultado esperado:
# accepting connections
```

### Crear Base de Datos (si no existe)

```bash
# Conectar a PostgreSQL
psql -U postgres

# En el prompt de PostgreSQL:
CREATE DATABASE notionflow;
\dt  # Listar tablas (vacío al inicio)
\q  # Salir
```

---

## 💾 PASO 2: APLICAR MIGRACIONES

### Ejecutar Migraciones

```powershell
# 1. Navegar a la carpeta del API
cd D:\workspace\notionFlow\NotionFlow.Api

# 2. Aplicar las migraciones
dotnet ef database update

# Resultado esperado:
# Build succeeded.
# Done. Applying migration 'EnglishTranslation'.
# The database was successfully updated.
```

### Verificar Tablas Creadas

```bash
# Conectar a la BD
psql -U postgres -d notionflow

# Listar tablas
\dt

# Resultado esperado (tablas):
# aspnetusers
# aspnetroles
# aspnetrolesclams
# courses
# coursestudents
# evaluations
# grades
# contents

# Salir
\q
```

---

## 🏗️ PASO 3: COMPILAR FRONTEND

### Limpiar y Compilar

```powershell
# 1. Limpiar build previos
cd D:\workspace\notionFlow\NotionFlow.App
dotnet clean

# 2. Restaurar dependencias
dotnet restore

# 3. Compilar
dotnet build

# Resultado esperado:
# Build succeeded.
```

---

## ✨ PASO 4: VERIFICAR COMPILACIÓN

### Checklists de Validación

**Backend API:**
```powershell
cd D:\workspace\notionFlow\NotionFlow.Api
dotnet build

# ✅ Debe completar sin errores
```

**Frontend App:**
```powershell
cd D:\workspace\notionFlow\NotionFlow.App
dotnet build

# ✅ Debe completar sin errores
```

**Base de Datos:**
```bash
psql -U postgres -d notionflow
SELECT * FROM "AspNetUsers";  # Tabla vacía pero existe
\q
```

---

## 🚀 PASO 5: EJECUTAR LA APLICACIÓN

### Opción A: Ejecutar Solo Backend

```powershell
cd D:\workspace\notionFlow\NotionFlow.Api
dotnet run

# Resultado esperado:
# info: Microsoft.Hosting.Lifetime[14]
#       Now listening on: https://localhost:7046
#       Now listening on: http://localhost:5220
```

El API estará disponible en:
- HTTP: `http://localhost:5220`
- HTTPS: `https://localhost:7046`

### Opción B: Ejecutar Backend + Frontend

**Terminal 1 - Backend:**
```powershell
cd D:\workspace\notionFlow\NotionFlow.Api
dotnet run
```

**Terminal 2 - Frontend:**
```powershell
cd D:\workspace\notionFlow\NotionFlow.App
dotnet maui run
```

---

## 🧪 PASO 6: PRUEBAS INICIALES

### Verificar Conectividad API

```bash
# Verificar que el API está disponible
curl -X GET http://localhost:5220/api/courses

# Resultado esperado:
# [] (array vacío, sin base de datos)
```

### Verificar BD

```bash
psql -U postgres -d notionflow

# Ver estructura
\d courses
\d evaluations
\d contents
\d grades

\q
```

---

## ⚙️ CONFIGURACIÓN AVANZADA

### Variables de Entorno

Editar `appsettings.json` (NotionFlow.Api):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=notionflow;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Key": "NotionFlowSuperSecretKey2024XYZ123456",
    "Issuer": "NotionFlow.Api",
    "Audience": "NotionFlow.App"
  }
}
```

### Cambiar Contraseña PostgreSQL

Si necesitas usar una contraseña diferente:

1. Crear usuario nuevo (opcional):
   ```bash
   psql -U postgres
   CREATE USER notionflow_user WITH PASSWORD 'tu_contraseña';
   GRANT ALL PRIVILEGES ON DATABASE notionflow TO notionflow_user;
   \q
   ```

2. Actualizar `appsettings.json`:
   ```json
   "DefaultConnection": "Host=localhost;Database=notionflow;Username=notionflow_user;Password=tu_contraseña"
   ```

---

## 🐛 TROUBLESHOOTING

### Error: "password authentication failed for user"

**Solución:**
1. Verificar usuario/contraseña en `appsettings.json`
2. Verificar que PostgreSQL está ejecutándose
3. Resetear contraseña: `psql -U postgres ALTER USER postgres PASSWORD 'postgres';`

### Error: "database does not exist"

**Solución:**
```bash
psql -U postgres
CREATE DATABASE notionflow;
\q
```

### Error: "port 5432 already in use"

**Solución:**
```bash
# Windows - Encontrar proceso usando puerto 5432
netstat -ano | findstr :5432

# Linux/Mac
lsof -i :5432

# O usar puerto diferente en PostgreSQL
```

### Error de Compilación en Frontend

**Solución:**
```powershell
# Limpiar completamente
dotnet clean --configuration Debug --configuration Release
rm -r bin
rm -r obj

# Restaurar y compilar
dotnet restore
dotnet build
```

---

## 📚 DOCUMENTOS DE REFERENCIA

- **Estado de Migración**: [DEPLOYMENT_STATUS.md](./DEPLOYMENT_STATUS.md)
- **Resumen de Traducción**: [TRANSLATION_SUMMARY.md](./TRANSLATION_SUMMARY.md)
- **Script Automático**: [setup-database.ps1](./setup-database.ps1)

---

## 📞 COMANDOS ÚTILES

### EF Core - Migrations

```bash
# Listar migraciones
dotnet ef migrations list

# Crear nueva migración
dotnet ef migrations add [NombreMigracion]

# Deshacer última migración (sin aplicar a BD)
dotnet ef migrations remove

# Deshacer última migración (reverting en BD)
dotnet ef database update [MigracionAnterior]

# Ver SQL de migración
dotnet ef migrations script
```

### PostgreSQL - Utilidades

```bash
# Conectar a BD específica
psql -U postgres -d notionflow

# Dentro de psql:
\dt                     # Listar tablas
\d [tablename]          # Ver estructura de tabla
SELECT * FROM [table];  # Consultar datos
\c [dbname]             # Cambiar BD
\l                      # Listar bases de datos
\q                      # Salir
```

### .NET CLI

```bash
# Limpiar
dotnet clean

# Restaurar paquetes
dotnet restore

# Compilar
dotnet build

# Ejecutar
dotnet run

# Publicar
dotnet publish -c Release
```

---

## ✅ CHECKLIST FINAL

- [ ] PostgreSQL instalado y ejecutándose
- [ ] Base de datos `notionflow` creada
- [ ] Migraciones aplicadas (`dotnet ef database update`)
- [ ] Backend compila sin errores (`dotnet build`)
- [ ] Frontend compila sin errores (`dotnet build`)
- [ ] API responde en `http://localhost:5220`
- [ ] BD contiene tablas: courses, evaluations, contents, grades
- [ ] Usuario puede registrarse
- [ ] Usuario puede hacer login
- [ ] Usuario puede ver cursos

---

## 🎉 ¡LISTO!

Una vez completados todos los pasos, tu aplicación NotionFlow está:
- ✅ Completamente traducida al inglés
- ✅ Compilable en backend y frontend
- ✅ Conectada a base de datos PostgreSQL
- ✅ Lista para desarrollo y deployment

**Para iniciar desarrollo:**
```powershell
# Terminal 1
cd D:\workspace\notionFlow\NotionFlow.Api
dotnet run

# Terminal 2
cd D:\workspace\notionFlow\NotionFlow.App
dotnet maui run
```

**Happy coding! 🚀**

