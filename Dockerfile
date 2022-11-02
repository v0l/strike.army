# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

#install npm
#RUN curl -fsSL https://deb.nodesource.com/setup_16.x | bash -
#RUN apt-get install -y nodejs

#run yarn install
#COPY spa/package.json spa/yarn.lock spa/
#RUN cd spa && npx yarn install

# Copy everything else and build
COPY . .
RUN rm -rf appsettings.*.json
RUN dotnet publish -c Release -o out strike.army.csproj

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "strike.army.dll"]