#SingleInstance Force

; Set the initial value for %v%
v := 95

; Set the directory path for the number images
ImageDirectory := "%APPDATA%\XaviFortes\Roblox\images\"

; Expand the environment variable in the image directory path
; ImageDirectory := ExpandEnvVars(ImageDirectory)

; Create the image directory if it doesn't exist
if !FileExist(ImageDirectory)
    RunWait, cmd /c mkdir "%ImageDirectory%"

Loop
{
    ; Activate the Roblox Beta Player window
    WinActivate, RobloxBetaPlayer.exe
    
    ; Resize the window to 800x600
    WinMove, RobloxBetaPlayer.exe, , , 0, 0, 800, 600
    
    ; Wait for the window to become active
    WinWaitActive, RobloxBetaPlayer.exe
    
    ; Search for images with numbers one, two, or three in the specified area
    ImageSearch, FoundX, FoundY, 65, 525, 750, 585, *%ImageDirectory%1.png|*%ImageDirectory%2.png|*%ImageDirectory%3.png|*%ImageDirectory%4.png|*%ImageDirectory%5.png|*%ImageDirectory%6.png|*%ImageDirectory%7.png|*%ImageDirectory%8.png
    
    ; If an image is found, drag it to the specified location and increment %v%
    if ErrorLevel = 0
    {
        MouseClickDrag, left, %FoundX%, %FoundY%, %v%, 555, 0
        v += 70
    }
    else
    {
        ; Exit the loop if no more images are found
        break
    }
}

MsgBox, All images have been processed.

; Function to expand environment variables in a string
ExpandEnvVars(Str) {
    return RegExReplace(Str, "%([^%]+)%", "$1")
}
