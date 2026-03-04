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

gs -sDEVICE=pdfwrite \
   -o /pdfs/output.pdf \
   -dDEVICEWIDTHPOINTS=595 -dDEVICEHEIGHTPOINTS=842 -dFIXEDMEDIA -dPDFFitPage -dAutoRotatePages=/None -dUseCropBox=false -dUseTrimBox=false -dUseArtBox=false -dUseBleedBox=false -dUseMediaBox \
   /pdfs/file1.pdf /pdfs/file2.pdf /pdfs/file3.pdf

```

example:
```
docker run --rm -v "C:\Downloads:/pdfs" pdf-tools gs -sDEVICE=pdfwrite -o /pdfs/output.pdf -dDEVICEWIDTHPOINTS=595 -dDEVICEHEIGHTPOINTS=842 -dFIXEDMEDIA -dPDFFitPage -dAutoRotatePages=/None -dUseCropBox=false -dUseTrimBox=false -dUseArtBox=false -dUseBleedBox=false -dUseMediaBox -dUseUserUnit=false "/pdfs/Adeptus Mechanicus – Maniple Verask-Alpha 2023-06-20.pdf" "/pdfs/Aeldari Combat Patrol Kygharil's Protectors.pdf" "/pdfs/Tyranids - TYRANID ASSAULT BROOD - eng_17-09_warhammer40000_combat_patrol_tyranid_assault_brood-fctbfrf7xg-qh5wxkrjzo.pdf"
```

check:
```
docker run --rm \
  -v "$(pwd)":/pdfs \
  pdf-tools \
  pdfinfo /pdfs/output.pdf | grep "Page size"
```

example:
```
gci *.pdf | % { $dn = "/pdfs/" + $_.Name; Write-Host $_.Name -Foreground Cyan -Background Black; docker run --rm -v "C:\Downloads:/pdfs" pdf-tools pdfinfo $dn; Write-Host }
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
