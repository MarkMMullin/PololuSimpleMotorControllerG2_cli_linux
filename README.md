# PololuSimpleMotorControllerG2_cli_linux
Rework of the Pololu Simple Motor Controller CLI to build on mac and linux using JetBrains Rider with no fuss

Build order - Usbwrapper_linux,SmcG2_linux,SmcGCCmd_linux

SmcG2Cmd_linux is the command line utility you want, not those other droids.

This is not original work, it's pretty much a line for line copy of the Pololu USB project here -- https://github.com/pololu/pololu-usb-sdk.  All I did was get rid of the stuff I didn't want and clean up the directory and framework references where Windows centric stuff had snuck in.

If Pololu forks this and goes forward with it, this project will die.  I only did it because I had to