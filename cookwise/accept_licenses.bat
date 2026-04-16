@echo off
set JAVA_HOME=C:\Program Files\Java\jdk-25
set ANDROID_HOME=C:\Users\lenovo\AppData\Local\Android\Sdk
set PATH=%JAVA_HOME%\bin;%PATH%
(echo y & echo y & echo y & echo y & echo y & echo y & echo y & echo y & echo y & echo y) | "%ANDROID_HOME%\cmdline-tools\latest\bin\sdkmanager.bat" --sdk_root="%ANDROID_HOME%" --licenses