FROM jlesage/baseimage-gui:ubuntu-24.04-v4

USER root

ARG GITHUB_ORG="13xforever"

ARG GITHUB_REPO="ps3-disc-dumper"

ARG ASSETNAME_ZIP="ps3-disc-dumper_linux_v4.4.2.zip"

ARG ASSETNAME="ps3-disc-dumper"

ARG ASSET_VERSION="v4.4.2"

WORKDIR /app

RUN apt-get update

RUN apt-get install -y curl

RUN apt-get install unzip

RUN apt-get install -y libicu-dev

RUN apt-get install -y dbus

RUN apt-get install -y dotnet-sdk-10.0

RUN apt-get install -y aspnetcore-runtime-10.0

RUN apt-get install -y libice6

RUN apt-get install -y libsm6


RUN curl -L --fail --output /app/${ASSETNAME_ZIP} https://github.com/${GITHUB_ORG}/${GITHUB_REPO}/releases/download/${ASSET_VERSION}/${ASSETNAME_ZIP}


RUN unzip ${ASSETNAME_ZIP}

RUN chmod +x ${ASSETNAME_ZIP}

WORKDIR /app

RUN chmod 777 /app

ENV DOTNET_BUNDLE_EXTRACT_BASE_DIR=/app/.bundle_extract

ENV DRIVE=/dev/sr0

RUN mkdir -p /var/run/dbus

RUN chmod +x /var/run/dbus

#RUN dbus-daemon --config-file=/usr/share/dbus-1/system.conf --print-address

  

#RUN chmod 666 /dev/sr0

  

#Doing DBUS Stuff

#RUN getent group messagebus || addgroup -S messagebus && \

# getent passwd messagebus || adduser -S -G messagebus -H -D messagebus

  

#RUN mkdir -p /var/run/dbus && \

# chown messagebus:messagebus /var/run/dbus

  

RUN mkdir -p /mnt/cdrom

  

#groupadd --gid 19 cdrom

#RUN mount -t udf -o ro /dev/sr0 /mnt/cdrom

  
  

# Copy the start script.

COPY startapp.sh /startapp.sh

  

RUN chmod +x /startapp.sh

  

RUN set-cont-env APP_NAME "ps3-disc-dumper"

  

ENV SUB_GROUP_IDS=19

  

ARG DUMPER_OUTPUT_PATH=/data/output

ARG DUMPER_OUTPUT_IRD_PATH=/data/ird/output

  

ENV DUMPER_OUTPUT_PATH=${DUMPER_OUTPUT_PATH}

ENV DUMPER_OUTPUT_IRD_PATH=${DUMPER_OUTPUT_IRD_PATH}

  

# Run first

# docker buildx build -t ps3disc:1.0.0 .

  

# Run second

# docker run --rm -p 5800:5800 -p 5900:5900 ps3disc:1.0.0