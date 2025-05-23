# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY Vjezba.sln ./
COPY Vjezba.DAL/Vjezba.DAL.csproj Vjezba.DAL/
COPY Vjezba.Model/Vjezba.Model.csproj Vjezba.Model/
COPY Vjezba.Web/Vjezba.Web.csproj Vjezba.Web/

# Restore dependencies
RUN dotnet restore

# Copy the rest of the source code
COPY . .

# Build the app
RUN dotnet publish Vjezba.Web/Vjezba.Web.csproj -c Release -o /app/publish

# Use the ASP.NET runtime image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Expose the port your app runs on
EXPOSE 7010

# Run the app
ENTRYPOINT ["dotnet", "Vjezba.Web.dll"]

