rem LOL. My first .bat file in 20 years
rem Usage:
rem Attempts to compile all .hlsl files in current directory and recursive lower directories
rem must be passed path too either the compilers named DXC.exe or FXC.exe 
rem Compiles as shader model 5 (hard coded)
rem entry point of Vertex Shader must be function VS
rem entry point of Pixel Shader must be function FS (for fragment shader...)

rem WARNING: SHARPDX FAILS ON MY SHADERS WITH DXC. USE FXC UNTIL PROBLEM IDENTIFIED (SEMANTICS?)

@echo off
rem Don't want to set any global / environment variables
setlocal
cls
echo HLSL Shader Compiler Batch Script
echo ---------------------------------
echo.

rem The below tests for a blank/non-existent first argument
rem the ~ removes any quotes
if "%~1"=="" (
	echo Please provide the filepath for either FXC.exe or DXC.exe compilers as the first argument for the .bat file
	goto end
	) 

rem Remove the path and get the filename and extension

set fullpath=%~1
set filename=%~nx1

if "%filename%"=="" (
	echo Provided path "%~1" did not include filename...
	goto end
	)

rem echo Filename to use: "%filename%"

rem Check that the filename is either FXC.exe or DXC.exe

set compiler="NONE"

if "%filename%"=="fxc.exe" (
	set compiler="FXC"
	) else (
		if "%filename%"=="dxc.exe" (
			set compiler="DXC"
			) else (
				echo Provided compiler file was not either FXC.exe or DXC.exe
				goto end
			)
	)

rem Check if executable for compiler even exists

if not exist %fullpath% (
	echo Provided file "%fullpath%" does not exist
	goto end
	)

rem Here we atleast have a compiler. Let's delete all existing .hlsl.bytes files
del *.hlsl.bytes

set fragmentCount=0
for /r %%f in (*Fragment*.hlsl) do set /a fragmentCount+=1
set vertexCount=0
for /r %%v in (*Vertex*.hlsl) do set /a vertexCount+=1

echo Directory (and sub directory) search found %vertexCount% potential vertex shader files and %fragmentCount% potential fragment shader files
echo.

if not %vertexCount%==0 (
	echo Vertex Shader Files:
	echo ---------------------
	for %%v in (*Vertex*.hlsl) do echo %%v
	)

echo.

if not %fragmentCount%==0 (
	echo Fragment Shader Files:
	echo ---------------------
	for %%f in (*Fragment*.hlsl) do echo %%f
	)

echo.

if not %vertexCount%==0 (
	echo Attempting to compile Vertex Shaders...
	for %%v in (*Vertex*.hlsl) do (
		echo Compiling: %%v
		if "%compiler"=="FXC" (
				%fullpath% /T vs_5_0 /E main /Fo %%v.bytes %%v
			) else (
				%fullpath% -T vs_5_0 -E main /Fo %%v.bytes %%v
			)
		)
	)

echo.

if not %fragmentCount%==0 (
	echo Attempting to compile Fragment Shaders...
	for %%f in (*Fragment*.hlsl) do (
			echo Compiling: %%f
			if "%compiler"=="FXC" (
				%fullpath% /T ps_5_0 /E main /Fo %%f.bytes %%f
			) else (
				%fullpath% -T ps_5_0 -E main /Fo %%f.bytes %%f
			)
		)
	)

echo.
echo Complete...

goto skipfail
:end 
	echo Check input and try again...
:skipfail
	echo Exiting...
endlocal