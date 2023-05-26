# Unit Tests

Runs nunit-console runner on linux. dotnet test fails on linux with missing runner binary which is why we manually run nunit-console runner.

All development files for test can be found at [/tools/test/](https://github.com/StrucEng-Library-kfmresearch/strucenglib-rhino3d-plugin/edit/master/tools/test/).

In vagrant, run 

```
/vagrant/tools/test/run_tests.sh
```
