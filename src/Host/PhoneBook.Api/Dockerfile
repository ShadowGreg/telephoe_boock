FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["telephoe boock.sln", "."]
COPY ["src/Host/PhoneBook.Api/PhoneBook.Api.csproj", "src/Host/PhoneBook.Api/"]
COPY ["src/Modules/Contacts/PhoneBook.Contacts.Application/PhoneBook.Contacts.Application.csproj", "src/Modules/Contacts/PhoneBook.Contacts.Application/"]
COPY ["src/Modules/Contacts/PhoneBook.Contacts.Domain/PhoneBook.Contacts.Domain.csproj", "src/Modules/Contacts/PhoneBook.Contacts.Domain/"]
COPY ["src/Modules/Contacts/PhoneBook.Contacts.Infrastructure/PhoneBook.Contacts.Infrastructure.csproj", "src/Modules/Contacts/PhoneBook.Contacts.Infrastructure/"]

RUN dotnet restore "src/Host/PhoneBook.Api/PhoneBook.Api.csproj"

COPY . .
RUN dotnet publish "src/Host/PhoneBook.Api/PhoneBook.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS run
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "PhoneBook.Api.dll"]
