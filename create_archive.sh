#!/bin/bash

for DIRECTORY in */
do
    [[ -d "$DIRECTORY" ]] || continue

    echo "Archiving ${DIRECTORY}"

    ARCHIVE="${DIRECTORY%/}.tar.xz"
    echo "Archive file name ${ARCHIVE}"

    tar -cJf "$ARCHIVE" "$DIRECTORY"

    EXITCODE=$?

    if [[ $EXITCODE -eq 0 ]]; then
        echo "Removing ${DIRECTORY}"

        rm -r "${DIRECTORY}"

        echo "Done removing ${DIRECTORY}"
    else 
        echo "Error occurred while archiving. Exit code ${EXITCODE}"
    fi

    echo "Done archiving ${DIRECTORY}"
done
