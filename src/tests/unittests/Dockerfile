# /Dockerfile.integration
FROM microsoft/dotnet:2.0.0-sdk as builder
COPY . /app
WORKDIR /app/src/tests/unittests
CMD ["dotnet", "test"]