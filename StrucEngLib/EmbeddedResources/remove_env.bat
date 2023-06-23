@echo off
@echo on
set conda="%1"
set cenv=strucenglib3

%conda% remove -n %cenv% --all
