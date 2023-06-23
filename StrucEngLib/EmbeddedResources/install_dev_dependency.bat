@echo off
rem pass <conda exec> <depdenceny name> <dependency path>


set conda="%1"
set dep="%2"
set dep_path="%3"
set cenv=strucenglib3

echo ""
echo "==== installing %dep% - %dep_path%"
echo ""

@echo on
call %conda% activate %cenv%


call python -m compas_rhino.uninstall -v 7.0 -p %dep%
call pip install -e %dep_path%
call python -m compas_rhino.install -v 7.0
call python -m compas_rhino.install -v 7.0 -p %dep%
@echo off
echo 
echo Done for %conda%, %dep%, %dep_path%, %cenv%
pause>nul
