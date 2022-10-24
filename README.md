# Commander
## Setup
### Debugging
Set command line argument "-webroot" in Debug options
### Preparing IE webview
Add DWORD value in with RegEdit.exe:

Key:
`Computer\HKEY_CURRENT_USER\Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION`
Value:
`Commander.exe  REG_DWORD  11000`
