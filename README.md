# E-Commerce AI Assistant API

> **Estado:** En progreso

API moderna para e-commerce construida con .NET 9, integrando capacidades de Inteligencia Artificial a través de Azure OpenAI y Microsoft Semantic Kernel, siguiendo una arquitectura escalable y patrones modernos de desarrollo.

## ✨ Características Principales

*   **Integración con IA:** Utiliza Azure OpenAI y Microsoft Semantic Kernel para ofrecer funcionalidades avanzadas como asistencia en compras, recomendaciones personalizadas, etc.
*   **Arquitectura Limpia (Clean Architecture):** Estructura organizada para mantenibilidad y escalabilidad.
*   **Patrón CQRS con MediatR:** Separación clara de comandos y consultas para una mejor gestión de la lógica de negocio.
*   **Gestión de Productos:** Incluye almacenamiento de imágenes/archivos en Azure Blob Storage.
*   **Acceso a Datos Eficiente:** Usa Dapper para consultas SQL de alto rendimiento y el Patrón Repository para abstracción.
*   **Monitoreo:** Implementa Health Checks para supervisar el estado de la API y sus dependencias (como SQL Server).
*   **Mapeo Avanzado:** Utiliza AutoMapper para simplificar el mapeo entre objetos.
*   **Configuración Segura:** Preparado para usar variables de entorno (ej. `.env`) para gestionar secretos.
*   **Endpoints Modulares:** Organización clara de los endpoints de la API.

## 💻 Tecnologías Utilizadas

*   .NET 9.0
*   ASP.NET Core
*   Microsoft Semantic Kernel
*   Azure OpenAI
*   Dapper
*   MediatR (CQRS)
*   Azure Blob Storage
*   SQL Server
*   Health Checks (AspNetCore.HealthChecks.SqlServer)
*   AutoMapper

## 🔑 Dependencias Clave (Versiones de ejemplo)

*   Microsoft.SemanticKernel 1.46.0
*   Azure.Storage.Blobs 12.24.0
*   Dapper 2.1.66
*   MediatR 12.5.0
*   AutoMapper 14.0.0
*   Microsoft.Data.SqlClient 6.0.1
*   AspNetCore.HealthChecks.SqlServer 9.0.0

## 🏗️ Arquitectura

*   Clean Architecture
*   CQRS Pattern
*   Repository Pattern
*   Dependency Injection
*   Modular Endpoints

## ☁️ Infraestructura

*   Azure OpenAI Service
*   Azure Blob Storage
*   SQL Server Database
*   (Implícito: Hosting en Vercel/Azure App Service u otro proveedor)

## 🚀 Cómo Empezar

Sigue estos pasos para configurar y ejecutar el proyecto localmente.

### Prerrequisitos

*   .NET 9 SDK o superior
*   SQL Server (o SQL Server Express/Developer Edition)
*   Una cuenta de Azure con acceso a:
    *   Azure OpenAI Service
    *   Azure Blob Storage
*   Un editor de código como Visual Studio Code o Visual Studio

### Instalación

1.  Clona el repositorio:
    ```bash
    git clone https://github.com/lemuayala/ecomerce-api-iabot.git
    cd ecomerce-api-iabot
    ```
2.  Configura tus secretos/variables de entorno. Crea un archivo `.env` en la raíz del proyecto API (o usa User Secrets en Visual Studio) con las siguientes variables (ajusta los nombres según tu implementación real):
    ```dotenv
    SQL_CONNECTION_STRING="TuCadenaDeConexionSQLServer"
    AZURE_BLOB_CONNECTION_STRING="TuCadenaDeConexionBlobStorage"
    AZURE_OPENAI_ENDPOINT="TuEndpointDeAzureOpenAI"
    AZURE_OPENAI_API_KEY="TuApiKeyDeAzureOpenAI"
    AZURE_OPENAI_DEPLOYMENT_NAME="TuNombreDeDespliegueOpenAI"
    # Agrega otras variables que tu aplicación necesite
    ```
3.  Restaura las dependencias de .NET:
    ```bash
    dotnet restore
    ```
4.  Aplica las migraciones de la base de datos (si usas Entity Framework Core - *ajustar si no es el caso*):
    ```bash
    # Asegúrate de estar en el directorio del proyecto que contiene el DbContext si usas EF
    # dotnet ef database update
    ```
    *(Si no usas EF Core, necesitarás ejecutar scripts SQL manualmente para crear la estructura de la base de datos).*

5.  Ejecuta la aplicación:
    ```bash
    dotnet run --launch-profile https
    # O presiona F5 en Visual Studio / usa el botón de Run en VS Code
    ```

La API debería estar ahora ejecutándose localmente (generalmente en `https://localhost:xxxx` y `http://localhost:yyyy`). Revisa la configuración de `launchSettings.json`.

## 🎮 Uso / Endpoints API

Una vez que la API esté en ejecución, puedes interactuar con ella usando herramientas como Postman, Insomnia o Swagger (si está configurado).

*(Documenta aquí los endpoints principales, por ejemplo:*
*   `GET /api/products` - Obtiene la lista de productos.
*   `POST /api/products` - Crea un nuevo producto (requiere autenticación/autorización si está implementado).
*   `POST /api/assistant/chat` - Interactúa con el asistente de IA.
*   `GET /health` - Verifica el estado de la API (endpoint de Health Checks).
*   ...)*

## 🤝 Contribuciones

Las contribuciones son bienvenidas. Si deseas contribuir, por favor:

1.  Haz un Fork del repositorio.
2.  Crea una nueva rama (`git checkout -b feature/nueva-funcionalidad`).
3.  Realiza tus cambios y haz commit (`git commit -m 'Añade nueva funcionalidad'`).
4.  Haz push a la rama (`git push origin feature/nueva-funcionalidad`).
5.  Abre un Pull Request.

## 📄 Licencia

Este proyecto está bajo la Licencia MIT. Consulta el archivo LICENSE para más detalles.
