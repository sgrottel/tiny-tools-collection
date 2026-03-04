FROM alpine:3.19

# Install ghostscript and poppler-utils (for pdfinfo)
RUN apk add --no-cache ghostscript poppler-utils

WORKDIR /pdfs

# Default command prints versions so you know it works
CMD ["sh", "-c", "gs --version && pdfinfo -v"]
