#!/bin/sh

docker run -e "TZ=Europe/Berlin" -v /data/share/apps/githuboverview:/myapp --rm sgr/githubsummary
