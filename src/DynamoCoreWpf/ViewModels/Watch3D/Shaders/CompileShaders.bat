@echo off
call "%DXSDK_DIR%Utilities\bin\dx_setenv.cmd"
pushd %~dp0
fxc /T fx_5_0 /I ./../ /Fo ..\Resources\_dynamo.bfx Default.fx
popd
pause