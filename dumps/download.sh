#!/bin/bash
dumps_version="dumps_3"
ARTIFACTS_USER='cidownload'
ARTIFACTS_PASSWORD='AP6JaG9ToerxBc7gWP5LcU1CNpb'
ARTIFACTS_URL="https://sharpdebug.jfrog.io/sharpdebug/api/storage/generic-local/$dumps_version/"

command -v curl >/dev/null 2>&1 || { echo "Please install 'curl'." >&2; exit 1; }
command -v unzip >/dev/null 2>&1 || { echo "Please install 'unzip'." >&2; exit 1; }

files=$(curl -u$ARTIFACTS_USER:$ARTIFACTS_PASSWORD $ARTIFACTS_URL 2>/dev/null | grep -Po '(?<="uri" : "/)[^"]*')
for file in $files ; do
    url="https://sharpdebug.jfrog.io/sharpdebug/generic-local/$dumps_version/$file"
    echo $url '-->' $file
    curl -u$ARTIFACTS_USER:$ARTIFACTS_PASSWORD $url --output $file 2>/dev/null
    extract_path=$(pwd)
    if grep -q clr "$file"; then
        subfolder="${file%.*}"
        extract_path="$extract_path/$subfolder"
    fi
    unzip -qo $file -d $extract_path
    rm $file
done
