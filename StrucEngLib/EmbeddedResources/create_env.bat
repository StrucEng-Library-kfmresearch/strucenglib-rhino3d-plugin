@echo off
echo This installs compas and dependencies for StrucEngLib. It may take a while.

@echo on
set conda="%1"
set cenv=strucenglib3

call %conda% create -n %cenv% -c conda-forge python=3.9 compas --yes
call %conda% activate %cenv%


@echo off
echo
echo Environment created. Install packages next. Press any key to exit . . .
pause>nul