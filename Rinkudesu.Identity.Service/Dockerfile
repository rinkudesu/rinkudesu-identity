FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "Rinkudesu.Identity.Service/Rinkudesu.Identity.Service.csproj"
WORKDIR "/src/Rinkudesu.Identity.Service"
RUN dotnet build "Rinkudesu.Identity.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Rinkudesu.Identity.Service.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
EXPOSE 80
WORKDIR /app
COPY --from=publish /app/publish .
HEALTHCHECK --interval=20s --start-period=5s --retries=3 CMD curl --fail http://localhost/health || exit 1
ENTRYPOINT ["dotnet", "Rinkudesu.Identity.Service.dll"]
