FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["ClearCut.sln", "./"]
COPY ["src/ClearCut.Web/ClearCut.Web.csproj", "src/ClearCut.Web/"]
RUN dotnet restore "src/ClearCut.Web/ClearCut.Web.csproj"
COPY . .
RUN dotnet publish "src/ClearCut.Web/ClearCut.Web.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "ClearCut.Web.dll"]
