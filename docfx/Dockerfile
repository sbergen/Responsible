FROM unityci/editor:ubuntu-2022.3.29f1-base-3
ARG DEBIAN_FRONTEND=noninteractive
RUN apt-get -q update \
    && apt-get -q install -y --no-install-recommends --allow-downgrades \
    dotnet-sdk-8.0
RUN dotnet tool install docfx --version 2.76.0 --tool-path /bin
