FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Lib9c.ActionEvaluator/Lib9c.ActionEvaluator.csproj", "Lib9c.ActionEvaluator/"]
RUN dotnet restore "Lib9c.ActionEvaluator/Lib9c.ActionEvaluator.csproj"
COPY . .
WORKDIR "/src/Lib9c.ActionEvaluator"
RUN dotnet build "Lib9c.ActionEvaluator.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Lib9c.ActionEvaluator.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Lib9c.ActionEvaluator.dll"]
