FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["BookDiary/BookDiary.csproj", "BookDiary/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Infraestructure/Infraestructure.csproj", "Infraestructure/"]
COPY ["Persistence/Persistence.csproj", "Persistence/"]

RUN dotnet restore "BookDiary/BookDiary.csproj"

COPY . .
WORKDIR "/src/BookDiary"
RUN dotnet publish "BookDiary.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BookDiary.dll"]