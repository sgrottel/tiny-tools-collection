#
# Simple docker file to run the Summary crawler on RPi mini computer
#
# Test on Windows:
#   docker build -t sgr/githubsummary .
#   docker run --rm -v ".:/myapp" -it sgr/githubsummary
#

# Use the official Alpine Linux image as the base image
FROM alpine:latest

VOLUME /myapp

# Install dependencies
RUN apk update && \
    apk add --no-cache \
    curl \
    tar \
    icu-libs \
    less \
    git

# Install PowerShell
RUN curl -L https://github.com/PowerShell/PowerShell/releases/download/v7.2.0/powershell-7.2.0-linux-alpine-x64.tar.gz -o /tmp/powershell.tar.gz && \
    mkdir -p /opt/microsoft/powershell/7 && \
    tar zxf /tmp/powershell.tar.gz -C /opt/microsoft/powershell/7 && \
    rm /tmp/powershell.tar.gz && \
    ln -s /opt/microsoft/powershell/7/pwsh /usr/bin/pwsh

# Install GitHub CLI
RUN curl -fsSL https://github.com/cli/cli/releases/download/v2.0.0/gh_2.0.0_linux_amd64.tar.gz -o /tmp/gh.tar.gz && \
    tar -xzf /tmp/gh.tar.gz -C /tmp && \
    mv /tmp/gh_2.0.0_linux_amd64/bin/gh /usr/local/bin/gh && \
    rm -rf /tmp/gh*

# Set PowerShell as the default shell
CMD ["pwsh"]
