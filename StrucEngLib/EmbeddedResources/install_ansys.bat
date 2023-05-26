@echo off
echo This installs ansys version of strucenglib

@echo on
set conda="%1"
set cenv=strucenglib3

call %conda% activate %cenv%

Rem uninstall
call python -m compas_rhino.uninstall -v 7.0
call python -m compas_rhino.uninstall -v 7.0 -p compas
call python -m compas_rhino.uninstall -v 7.0 -p compas_fea
call python -m compas_rhino.uninstall -v 7.0 -p strucenglib_snippets
call python -m compas_rhino.uninstall -v 7.0 -p Sandwichmodel
call python -m compas_rhino.uninstall -v 7.0 -p Printerfunctions

call pip uninstall -y compas_fea
call pip uninstall -y strucenglib_snippets
call pip uninstall -y strucenglib_connect

Rem install
call pip install compas
call pip install https://github.com/StrucEng-Library-kfmresearch/strucenglib-connect/archive/master.zip#subdirectory=strucenglib_connect
call pip install https://github.com/StrucEng-Library-kfmresearch/strucenglib-snippets/archive/ansys.zip
call pip install https://github.com/StrucEng-Library-kfmresearch/compas_fea/archive/ansys.zip

Rem link
call python -m compas_rhino.install -v 7.0
call python -m compas_rhino.install -v 7.0 -p compas
call python -m compas_rhino.install -v 7.0 -p compas_fea
call python -m compas_rhino.install -v 7.0 -p strucenglib_connect
call python -m compas_rhino.install -v 7.0 -p strucenglib

@echo off
echo 
echo If something broke, try to delete conda environment %cenv% and retry
echo Installation finished. Press any key to exit . . .
pause>nul
