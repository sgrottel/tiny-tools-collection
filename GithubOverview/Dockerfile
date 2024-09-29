#
# Simple docker file to run the Summary crawler on RPi mini computer
#
# Test on Windows:
#   docker build -t sgr/githubsummary .
#   docker run --rm -v ".:/myapp" -t sgr/githubsummary
#
# Deploy on Pi:
#  sudo docker build -t sgr/githubsummary .
#  sudo docker run --rm -v ".:/myapp" -t sgr/githubsummary
#

FROM alpine:latest

VOLUME /myapp

RUN apk add powershell github-cli

CMD ["pwsh","-File","/myapp/SummaryCrawler.ps1","-WorkingDirectory","/myapp","-NoLogo","-NonInteractive","-NoProfile"]
