﻿FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
 
FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ./Avia.csproj .
RUN ls
RUN dotnet restore "Avia.csproj"
COPY . .
RUN dotnet build "Avia.csproj" -c Release -o /app
 
FROM build AS publish
RUN dotnet publish "Avia.csproj" -c Release -o /app
 
FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Avia.dll"]