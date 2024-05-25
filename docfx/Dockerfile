FROM unityci/editor:ubuntu-2022.3.29f1-base-3 as builder
RUN apt-get -q update \
    && apt-get -q install -y --no-install-recommends --allow-downgrades \
    unzip
RUN mkdir -p /docfx \
    && curl https://github.com/dotnet/docfx/releases/download/v2.56.7/docfx.zip -L --output /docfx/docfx.zip \
    && unzip /docfx/docfx.zip -d /docfx/ \
    && rm /docfx/docfx.zip

FROM unityci/editor:ubuntu-2022.3.29f1-base-3
ARG DEBIAN_FRONTEND=noninteractive
RUN apt-get -q update \
    && apt-get install -y --no-install-recommends gnupg ca-certificates \
    && apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF \
    && echo "deb https://download.mono-project.com/repo/ubuntu stable-bionic main" | tee /etc/apt/sources.list.d/mono-official-stable.list \
    && apt-get -q update \
    && apt-get -q install -y --no-install-recommends --allow-downgrades \
    mono-runtime \
    mono-devel \
    msbuild
COPY --from=builder /docfx /docfx
RUN printf '#!/bin/bash\nmono /docfx/docfx.exe $@' > /usr/local/bin/docfx \
    && chmod a+x /usr/local/bin/docfx
