#
# Simple docker file to run the Summary crawler on RPi mini computer
#
# Test on Windows:
#   docker build -t sgr/githubsummary .
#   docker run -e "TZ=Europe/Berlin" -v ".:/myapp" --rm sgr/githubsummary
#
# Deploy on Pi:
#  sudo docker build -t sgr/githubsummary .
#  sudo docker run -e "TZ=Europe/Berlin" -v /data/share/apps/githuboverview:/myapp --rm sgr/githubsummary
#

FROM alpine:latest

VOLUME /myapp

RUN apk add powershell github-cli

CMD ["/myapp/SummaryCrawler.sh"]
