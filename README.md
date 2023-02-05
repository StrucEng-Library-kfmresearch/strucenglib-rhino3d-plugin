[![Continuous Integration](https://github.com/StrucEng-Library-kfmresearch/strucenglib-rhino3d-plugin/actions/workflows/distrib_sh_build.yml/badge.svg?branch=master)](https://github.com/StrucEng-Library-kfmresearch/strucenglib-rhino3d-plugin/actions/workflows/distrib_sh_build.yml)
[![CodeQL](https://github.com/StrucEng-Library-kfmresearch/strucenglib-rhino3d-plugin/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/StrucEng-Library-kfmresearch/strucenglib-rhino3d-plugin/actions/workflows/codeql-analysis.yml)
[![Total alerts](https://img.shields.io/lgtm/alerts/g/StrucEng-Library-kfmresearch/strucenglib-rhino3d-plugin.svg?logo=lgtm&logoWidth=18)](https://lgtm.com/projects/g/StrucEng-Library-kfmresearch/strucenglib-rhino3d-plugin/alerts/)
[![Latest tagged version](https://img.shields.io/github/v/tag/StrucEng-Library-kfmresearch/strucenglib-rhino3d-plugin.svg)](https://github.com/StrucEng-Library-kfmresearch/strucenglib-rhino3d-plugin/blob/master/CHANGELOG)


# StrucEngLib Plugin
Rhinoceros 3D Plugin For StrucEng Library. The StrucEng Library includes mechanical models, safety concepts, GUI's, load generator, etc. for the structural analysis of reinforced concrete and masonry. This plugin integrates functionality of [Compas Finite element analysis (FEA 1)](https://compas.dev/compas_fea/latest/index.html) into [Rhinoceros 3D](https://www.rhino3d.com/de/).

  
<p align="left">
    <img src="https://user-images.githubusercontent.com/2311941/183869882-6bfad852-f495-4ffc-91e1-8e0277bd8fd5.png" alt="strucenglib" width="700"/>
</p>

### Installation
https://strucenglib.ethz.ch/strucenglib_plugin/home/

### Released Versions
Consider [CHANGELOG](./CHANGELOG) for a summary of changes in the recent versions. 

### Build
The build setup is specified and reproducible with a linux based Vagrant image.
Please consider [./tools/vagrant](./tools/vagrant) for detailed build instructions. 
```
$ vagrant up
$ ./distrib_vagrant.sh help
distrib.sh: ./distrib.sh {update_version|version|build|package|deploy_test|deploy|distrib|distrib_test}
commands: 
  update_version <version>.....: updates version
  version......................: list version
  build........................: build dotnet solution
  test.........................: build dotnet solution, run tests
  package......................: builds solution, creates yak package format
  deploy_test..................: deploys the yak package found to test store
  deploy.......................: deploys the yak package found store
  distrib......................: builds, packages, deploys package to store
  distrib_test.................: builds, packages, deploys package to test store
```
### Files
```
./StrucEngLib/...........: .net C# Rhino Plugin
./StrucEngLibTest/.......: Tests
./tools/.................: Development tools
./tools/vagrant/.........: Vagrant documentation and scripts
./tools/distrib/.........: Build and distribution tools
./tools/distrib/assets...: Asset files for distribution, manifest
./tools/test/............: Unit test runner
```
