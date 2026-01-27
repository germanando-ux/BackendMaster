# <summary>
# Etapa de base: Define el entorno de ejecución ligero para .NET 10.
# </summary>
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# <summary>
# Etapa de construcción: Utiliza el SDK completo para compilar la solución.
# Se copian los archivos .csproj primero para aprovechar la caché de capas de Docker.
# </summary>
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copia de archivos de proyecto para restauración de Nugets
COPY ["Store.Api/Store.Api.csproj", "Store.Api/"]
COPY ["Store.Data/Store.Data.csproj", "Store.Data/"]
COPY ["Store.Domain/Store.Domain.csproj", "Store.Domain/"]

# <method>
# Ejecuta la restauración de dependencias de forma aislada.
# </method>
RUN dotnet restore "Store.Api/Store.Api.csproj"

# Copia el resto del código fuente de todas las carpetas
COPY . .
WORKDIR "/src/Store.Api"

# <method>
# Compila el proyecto en modo Release.
# </method>
RUN dotnet build "Store.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# <summary>
# Etapa de publicación: Genera los binarios finales optimizados.
# </summary>
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Store.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# <summary>
# Etapa final: Copia los binarios a la imagen base ligera y define el punto de entrada.
# </summary>
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Store.Api.dll"]