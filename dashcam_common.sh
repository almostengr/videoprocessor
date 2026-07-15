#!/bin/bash

# Get the directory where the current script resides
SCRIPT_DIR=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" &> /dev/null && pwd)

# Source the file using the absolute path
source "$SCRIPT_DIR/common.sh"

ctaDuration=12
subscribeBoxColor="green"
bgBoxColor="green"
textColor="white"
channelBrandText="KENNY RAM DASH CAM"

selectCallToAction() {
    case $dayOfWeek in
        0)
        subscribeBoxText="ONE WAY TO SUPPORT THE CHANNEL - PLEASE SUBSCRIBE"
        ;;

        1)
        subscribeBoxText="NEXT EXIT: SUBSCRIBE"
        ;;

        2)
        subscribeBoxText="MERGE INTO THE COMMUNITY - SUBSCRIBE NOW!"
        ;;

        4)
        subscribeBoxText="STOP AND SUBSCRIBE"
        ;;

        *)
        subscribeBoxText="HELP THE CHANNEL GROW BY SUBSCRIBING NOW!"
        ;;
    esac
}
