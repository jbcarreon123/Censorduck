# Censorduck
A program that detects swears.
> Note that this uses **Windows Speech Recognition**, a system that is depectated on Windows 11.
> This program is just for ***** and giggles, so probably won't update this to use Voice Access.

## Setting it up
1. Enable "Online Speech Recognition" on Windows' Settings.
   - [Windows 10](https://www.tenforums.com/tutorials/118136-enable-disable-online-speech-recognition-windows-10-a.html) | [Windows 11](https://www.elevenforum.com/t/enable-or-disable-online-speech-recognition-in-windows-11.7552/)
2. Run the program and wait it to generate a file on the directory you put the program
3. Put discord webhooks on the file (which is `cdConfig.yml`)
   ![image](https://github.com/jbcarreon123/Censorduck/assets/86447165/5719a4c8-5f89-40cd-a599-dd122cd0a4e6)
4. Change your default **input** audio device to the appropriate one
   - [Windows 10](https://www.tenforums.com/tutorials/111310-change-default-sound-input-device-windows-10-a.html) | [Windows 11](https://www.elevenforum.com/t/change-default-sound-input-device-in-windows-11.1864/)
6. Save the file, and run the program.

## Compiling Censorduck
### Prerequisites
- a Windows 10 or 11 machine
- .NET 6.0 SDK
### Steps
1. Run a command prompt or PowerShell window, and `cd` to the directory of the source code
2. Type `dotnet build`
3. The files will be generated on `[dir]\bin\debug\net6.0-windows...\win-x64\`
