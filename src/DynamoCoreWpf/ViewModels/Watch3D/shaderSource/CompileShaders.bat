@echo off
call "%DXSDK_DIR%Utilities\bin\dx_setenv.cmd"
pushd %~dp0

REM this is a comment 
REM https://docs.microsoft.com/en-us/windows/win32/direct3dtools/dx-graphics-tools-fxc-syntax
REM - /Vi - verbose include info
REM - /0d - disable optimziations
REM -/Zi Enable debugging information.
REM /T fx_5_0 - shader model 5
REM -/I ./../ include
REM -  /Fo  output binary path
REM file to compile Default.fx

fxc /Vi /Od /Zi /T vs_4_0 /I ./helix_shader_defs/COMMON /Fo ..\compiledShaders\vsDynamoMesh vsDynamoMesh.hlsl
fxc /Vi /Od /Zi /T ps_4_0 /I ./helix_shader_defs/COMMON /Fo ..\compiledShaders\psDynamoMesh psDynamoMesh.hlsl
popd
pause