FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Tesseract.slnx ./
COPY src/Tesseract.Application/Tesseract.Application.csproj src/Tesseract.Application/
COPY src/Tesseract.Domain/Tesseract.Domain.csproj src/Tesseract.Domain/
COPY src/Tesseract.Infrastructure/Tesseract.Infrastructure.csproj src/Tesseract.Infrastructure/
COPY src/Tesseract.Web/Tesseract.Web.csproj src/Tesseract.Web/

RUN dotnet restore src/Tesseract.Web/Tesseract.Web.csproj

COPY . .
RUN dotnet publish src/Tesseract.Web/Tesseract.Web.csproj --configuration Release --output /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Tesseract.Web.dll"]
