FROM unityci/editor:ubuntu-2019.4.14f1-linux-il2cpp-0.10.0 as builder
RUN apt-get -q update \
    && apt-get -q install -y --no-install-recommends --allow-downgrades \
    unzip
RUN mkdir -p /docfx \
    && curl https://github.com/dotnet/docfx/releases/download/v2.56.7/docfx.zip -L --output /docfx/docfx.zip \
    && unzip /docfx/docfx.zip -d /docfx/ \
    && rm /docfx/docfx.zip

FROM unityci/editor:ubuntu-2019.4.14f1-linux-il2cpp-0.10.0
ARG DEBIAN_FRONTEND=noninteractive
RUN apt-get -q update \
    && apt-get -q install -y --no-install-recommends --allow-downgrades \
    mono-devel
COPY --from=builder /docfx /docfx
RUN printf '#!/bin/bash\nmono /docfx/docfx.exe $@' > /usr/local/bin/docfx \
    && chmod a+x /usr/local/bin/docfx
