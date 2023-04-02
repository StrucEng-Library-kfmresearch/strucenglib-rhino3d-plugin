## Reproducible Build

### Linux

The development build of this project is captured in a vagrant linux box. For a
reproducible setup on linux, install the following binaries:

- vagrant https://www.vagrantup.com/
- virtualbox https://www.virtualbox.org/.


All development files for vagrant can be found at [/tools/vagrant/](https://github.com/StrucEng-Library-kfmresearch/strucenglib-rhino3d-plugin/edit/master/tools/vagrant/).

 
Upon installation, run the following commands to build the project.

``` sh
# boot box
vagrant up

# build solution
/tools/distrib/distrib_vagrant.sh help
/tools/distrib/distrib_vagrant.sh build
```

This setup will cross compile strucenglib on linux for windows.  
The official build system for this project is Linux.
