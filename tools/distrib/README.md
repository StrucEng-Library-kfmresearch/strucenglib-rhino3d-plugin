# Tools

This document describes the setup tools to build, package and deploy strucenglib
on linux. All dependencies are captured in the provided vagrant box. Install vagrant as described in /tools/vagrant.
The script distrib.sh captures common deployment commands with dotnet and yak.exe


## Distrib Tool
```sh
distrib.sh: ./distrib.sh {update_version|version|build|package|deploy_test|deploy|distrib|distrib_test}
commands: 
  update_version <version>.....: updates version
  version......................: list version
  build........................: build dotnet solution
  package......................: builds solution, creates yak package format
  deploy_test..................: deploys the yak package to test store
  deploy.......................: deploys the yak package to store
  distrib......................: builds, packages, deploys package to store
  distrib_test.................: builds, packages, deploys package to test store
```

Ensure that environment variable `YAK_TOKEN` is set. Set token in ./distrib.env.

``` sh
export YAK_TOKEN=...
```

The yak_token can be obtained with yak.exe login.  
- On Windows: %AppData%\Roaming\McNeel\yak.yml
- On Linux: mono/pwsh yak.exe login (needs xserver for browser login)
  - Copy paste url to browser, upon success extract token query parameter from browser url 

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
./distrib.sh test

# Package yak format
./distrib.sh package
```
The built Rhino plugin can susequently be found in `/StrucEngLib/bin/Debug/net48/`.
The yak package can be found in `./build`

## Obtain Access Token
Log into yak with yak.exe login. Your mcneel account must be connected with
strucenglib.

```sh
vagrant ssh
cd /vagrant/tools/distrib
mono yak.exe login
```
Copy token obtained from login into distrib.env or export YAK_TOKEN into the environment.

```
# from yak.exe login
http://localhost:5123/mcneel#state=<state>&token=<copy this token>
export YAK_TOKEN=<token>
```


### Publish to test store
```sh
vagrant ssh
cd /vagrant/tools/distrib
./distrib package
./distrib.sh distrib_test
```

### Publish to store
```sh
vagrant ssh
cd /vagrant/tools/distrib
./distrib package
./distrib.sh distrib
```

## Workflow to Release new Version

```sh
# Precondition: YAK_TOKEN obtained
vagrant up                                              # 1. Boot Vagrant
./distrib_vagrant.sh version                            # 2. Verify correct version set
./distrib_vagrant.sh upate_version "<version string>"   # 2.1 Update version if neeeded
emacs CHANGELOG                                         # 2.5 Update release notes 
./distrib_vagrant.sh deploy                             # 3. Build/Test/Package/Deploy

# Post Release task
git tag "<version-string>"                              # Tag version
git push --tags                                         # Push tags

# Switch to develop branch and prepare new development
git checkout develop
git rebase master
./distrib_vagrant.sh update_version "<version string +1>" 
git commit -am "Ready for development "<version string +1>""
```
Upon success, the new version "<version string>" is available in Rhino Package Manager.

## Test Builds

In order to install builds deployed with `distrib.sh distrib_test|deploy_test`, the test store in Rhino must be enabled. In Rhino, access Options/Tools/Advanced/ and add the value `https://test.yak.rhino3d.com` to the property `Rhino.Options.PackageManager.Sources` (use semicolon as separator).
  
## Deployment on Windows
https://developer.rhino3d.com/guides/yak/pushing-a-package-to-the-server/

## Links
- In case wine has issues publishing the yak file, update dotnet binary as described here
https://web.archive.org/save/https://gist.github.com/abertschi/464c5143f0290572711bb909b610208d
- Yak Cmd line tool: https://developer.rhino3d.com/guides/yak/yak-cli-reference/
- https://developer.rhino3d.com/guides/yak/pushing-a-package-to-the-server/
- https://developer.rhino3d.com/guides/yak/the-anatomy-of-a-package/

