# Dockerfile
FROM debian:bullseye

RUN apt-get update && \
    apt-get install -y ghostscript && \
    apt-get clean

WORKDIR /pdfs

ENTRYPOINT [ "bash" ]
