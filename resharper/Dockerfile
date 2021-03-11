FROM unityci/editor:ubuntu-2019.4.14f1-linux-il2cpp-0.10.0 as builder
RUN apt-get -q update \
    && apt-get -q install -y --no-install-recommends --allow-downgrades \
    unzip
RUN mkdir -p /resharper \
    && curl https://download.jetbrains.com/resharper/dotUltimate.2020.3.3/JetBrains.ReSharper.CommandLineTools.2020.3.3.zip -L --output /resharper/resharper.zip \
    && curl https://github.com/JetBrains/resharper-unity/releases/download/net203-rtm-2020.3.3/jetbrains.unity.2020.3.3.149.nupkg -L --output /resharper/jetbrains.unity.2020.3.3.149.nupkg \
    && unzip /resharper/resharper.zip -d /resharper/ \
    && rm /resharper/resharper.zip

FROM unityci/editor:ubuntu-2019.4.14f1-linux-il2cpp-0.10.0
ARG DEBIAN_FRONTEND=noninteractive
RUN wget https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb \
    && dpkg -i packages-microsoft-prod.deb \
    && rm packages-microsoft-prod.deb \
    && apt-get -q update \
    && apt-get install -y --no-install-recommends apt-transport-https \
    && apt-get -q update \
    && apt-get -q install -y --no-install-recommends --allow-downgrades dotnet-sdk-3.1
COPY --from=builder /resharper /resharper
RUN printf '#!/bin/bash\n/resharper/inspectcode.sh $@' > /usr/local/bin/inspectcode \
    && chmod a+x /usr/local/bin/inspectcode
