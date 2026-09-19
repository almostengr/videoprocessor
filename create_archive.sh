#!/bin/bash

# #############################################################################
#
# create archive from each of the files in a specified directory
#
# #############################################################################

if [ -z "$1" || "$1" == "" ]; then   
    echo "Directory not provided"
fi 

if [ ! -d "$1" ]; then
    echo "Directory does not exist"
fi

cd "$1"

for DIRECTORY in */
do
    [[ -d "$DIRECTORY" ]] || continue

    echo "Archiving ${DIRECTORY}"

    ARCHIVE="${DIRECTORY%/}.tar.xz"
    echo "Archive file name ${ARCHIVE}"

    tar -cJf "$ARCHIVE" "$DIRECTORY"

    echo "Done archiving ${DIRECTORY}"
    
    EXIT_CODE=$?

    if [[ $EXIT_CODE -gt 0 ]]; then
        echo "Error occurred while archiving. Exit code ${EXIT_CODE}"
        continue
    fi

    echo "Removing ${DIRECTORY}"

    rm -r "${DIRECTORY}"

    echo "Done removing ${DIRECTORY}"
done
