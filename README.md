# Random Unzip

Unzip random files from Zip archives.

## Why

The image datasets online often contains much more files than needed. This tool helps creating small datasets without creating millions of files on disk.

## Usage

To take 600 files from the celeba dataset:

`dotnet RandomUnzip.dll -i img_align_celeba.zip -o R:\dataset\celeba\ -t 600`

Without `-t N` it unzips all files.

You can specify filename encoding with `-e Shift-JIS` if needed. Default is UTF-8.

## Build

Cross-platform. Use [Dotnet Core 2.1](https://dotnet.microsoft.com/download) to build and run.