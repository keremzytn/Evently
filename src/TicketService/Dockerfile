FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Shared/Shared.csproj", "Shared/"]
COPY ["TicketService/TicketService.csproj", "TicketService/"]
RUN dotnet restore "TicketService/TicketService.csproj"
COPY . .
WORKDIR "/src/TicketService"
RUN dotnet build "TicketService.csproj" -c Release -o /app/build

FROM build AS publish
WORKDIR "/src/TicketService"
RUN dotnet publish "TicketService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TicketService.dll"]

