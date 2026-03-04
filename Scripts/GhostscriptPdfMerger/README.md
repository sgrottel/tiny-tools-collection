# Docker + Ghostscript PDF Merger

Use Ghostscript, hosted in a docker, to merge multiple PDF files into one PDF.

## Build Docker

docker build -t pdf-tools .


## Run Docker

build:
```
docker build -t pdf-tools .
```

run:
```
docker run --rm \
  -v "$(pwd)":/pdfs \
  pdf-tools \
  gs -sDEVICE=pdfwrite \
     -o /pdfs/output.pdf \
     -sPAPERSIZE=a4 \
     -dFIXEDMEDIA \
     -dPDFFitPage \
     /pdfs/file1.pdf /pdfs/file2.pdf /pdfs/file3.pdf
```

check:
```
docker run --rm \
  -v "$(pwd)":/pdfs \
  pdf-tools \
  pdfinfo /pdfs/output.pdf | grep "Page size"
```

### Old info:

Let’s say your PDFs are in:

`C:\Users\Sebastian\Documents\pdfs`

Docker on Windows needs paths in Unix format. So convert it to:

`/c/Users/Sebastian/Documents/pdfs`

Run:

```
docker run -it --rm -v /c/Users/Sebastian/Documents/pdfs:/pdfs ghostscript-pdf
```

## Merge PDFs

```
gs -dBATCH -dNOPAUSE -q -sDEVICE=pdfwrite -sOutputFile=merged.pdf file1.pdf file2.pdf file3.pdf
```
