# Docker + Ghostscript PDF Merger

Use Ghostscript, hosted in a docker, to merge multiple PDF files into one PDF.

## Build Docker

docker build -t ghostscript-pdf .

## Run Docker

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
