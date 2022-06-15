# Tools

This document describes the setup tools to build, package and deploy strucenglib
on linux. All dependencies are captured in the provided vagrant box. Install vagrant as described in /tools/vagrant.

## Distrib Tool
```sh
distrib.sh: ./distrib.sh {update_version|version|build|package|deploy_test|deploy|distrib|distrib_test}
commands: 
  update_version <version>.....: updates version
  version......................: list version
  build........................: build dotnet solution
  package......................: builds solution, creates yak package format
  deploy_test..................: deploys the yak package found to test store
  deploy.......................: deploys the yak package found store
  distrib......................: builds, packages, deploys package to store
  distrib_test.................: builds, packages, deploys package to test store
```

## Build
```sh
# boot vagrant and login
cd /project/root/dir
vagrant up
vagrant ssh

# cd into distrib tools
cd /vagrant/tools/distrib

# build the dotnet solution
./distrib.sh build

# Package yak format
./distrib.sh package
```
The built Rhino plugin can susequently be found in `/StrucEngLib/bin/Debug/net48/`.
The yak package can be found in `/distrib/build`

## Publish to Rhino Store
Log into yak with wine yak.exe login. Your mcneel account must be connected with
strucenglib.

```sh
vagrant ssh
cd /vagrant/tools/distrib
wine yak.exe login
```

Publish to test store
```sh
vagrant ssh
cd /vagrant/tools/distrib
./distrib package
./distrib.sh distrib_test
```

Publish to store
```sh
vagrant ssh
cd /vagrant/tools/distrib
./distrib package
./distrib.sh distrib
```

## Test Builds

In order to install builds deployed with `distrib.sh distrib_test|deploy_test`, the test store in Rhino must be enabled. In Rhino, access Options/Tools/Advanced/ and add the value `https://test.yak.rhino3d.com` to the property `` (use semicolon as separator).
  

## Links
- In case wine has issues publishing the yak file, update dotnet binary as described here
https://web.archive.org/save/https://gist.github.com/abertschi/464c5143f0290572711bb909b610208d
- Yak Cmd line tool: https://developer.rhino3d.com/guides/yak/yak-cli-reference/
- https://developer.rhino3d.com/guides/yak/pushing-a-package-to-the-server/
- https://developer.rhino3d.com/guides/yak/the-anatomy-of-a-package/
