@echo off
setlocal
set JAVA_HOME=C:\Program Files\Java\jdk-24.0.2
set ANDROID_HOME=C:\Users\lenovo\AppData\Local\Android\Sdk
set PATH=%JAVA_HOME%\bin;%PATH%

echo y | "C:\Users\lenovo\AppData\Local\Android\Sdk\cmdline-tools\latest\bin\sdkmanager.bat" --sdk_root="C:\Users\lenovo\AppData\Local\Android\Sdk" "platform-tools" "platforms;android-35" "build-tools;35.0.0"