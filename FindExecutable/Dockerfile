# https://hub.docker.com/_/microsoft-dotnet-runtime/
FROM mcr.microsoft.com/dotnet/runtime:6.0
RUN apt-get update && apt-get install -y git
COPY --chmod=444 *.* /myapp/
WORKDIR /myapp
ENTRYPOINT ["dotnet", "FindExecutable.dll"]
