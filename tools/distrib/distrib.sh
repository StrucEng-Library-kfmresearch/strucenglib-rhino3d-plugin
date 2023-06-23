#!/bin/bash
set -euo pipefail

#
# Build script to build dotnet project, and create release artifacts
# See ensure_binaries for binary dependencies
#

script_dir=$( dirname "$(readlink -f "$0")" )
# script_dir="$( cd -- "$( dirname -- "${BASH_SOURCE[0]:-$0}"; )" &> /dev/null && pwd 2> /dev/null; )";
proj_root="$script_dir/../.."
yak_bin="pwsh -c $script_dir/./yak.exe"
yak_login="mono $script_dir/./yak.exe"
env_file=$script_dir/distrib.env
build_dir=$script_dir/build
asset_dir=$script_dir/assets

ensure_binaries_build() {
    ensure_binary xmlstarlet
    ensure_binary dotnet
}

ensure_binaries_deploy() {
    ensure_binary pwsh
}

ensure_binaries() {
    ensure_binaries_build
    ensure_binaries_deploy
}

source_environment() {
    if [ -f "$env_file" ]; then
        echo "Sourcing environment file $env_file"
        source "$env_file"
    else
        echo "Environment file does not exist: $env_file"
        echo "Make sure YAK_TOKEN is set"
    fi

    if [[ -z "${YAK_TOKEN}" ]]; then
        echo "YAK_TOKEN not set, deployment will likely fail"
    else
        echo "YAK_TOKEN is set."
    fi
}


ensure_binary() {
    local cmd="$1"
    if ! command -v "$cmd" &> /dev/null
    then
        echo "$cmd could not be found"
        exit 1
    fi
}

login() {
    echo "copy paste the URL into browser and copy <__token__> after login"
    echo "After login, browser URL will be: http://localhost:5123/mcneel#state=<state>&token=<__token__>"
    echo "export YAK_TOKEN=<__token__> in distrib.env file"
    $yak_login login
}

version() {
    local f1="$proj_root/StrucEngLib/StrucEngLib.csproj"

    xmlstarlet sel -t -m "//Version[1]"  -v . -n $f1
}

update_version() {
    local version=${1:-}
    if [ -z "$version" ]
    then
        echo "No argument set. $0 <version>"
        exit 1
    fi

    local f1="$proj_root/StrucEngLib/src/StrucEngLibPlugin.cs"
    local f2="$proj_root/StrucEngLib/StrucEngLib.csproj"

    echo "updating version to $version ..."
    sed -i "s/Version =.*/Version = \"$version\";/" "$f1"
    cat "$f1" | grep Version
    xmlstarlet ed --inplace -u "//Version[1]" -v "$version" "$f2"
    cat "$f2" | grep Version
}

build() {
    # Builds .net project
    echo "build..."
    dotnet build "$proj_root"
}

create_package_dir() {
    echo "create_package_dir..."

    local version=${1:-}
    if [ -z "$version" ]
    then
        echo "No argument set. $0 <version>"
        exit 1
    fi

    local bin="$proj_root/StrucEngLib/bin/Debug/net48"
    local out=$build_dir
    local assets=$asset_dir

    # Copy relevant bits
    rm -rf "$out"
    mkdir -p "$out"
    cp -r "$assets/"* $out
    cp "$proj_root/LICENSE" "$out"
    cp "$proj_root/CHANGELOG.md" "$out"
    cp "$proj_root/README.md" "$out"
    cp -rf "$bin/"* "$out" 2>/dev/null

    sed -i "s/__VERSION__/$version/g" "$out/manifest.yml"

    ls -al "$out"
    cat "$out/manifest.yml"
}

package() {
    echo "package..."

    build
    local v=$(version)
    create_package_dir "$v"

    local _cd=$(pwd)
    cd $build_dir
    $yak_bin build
    
    printf '\033[0m'  # Reset color
    ls -al 
    cat "manifest.yml"
    cd "$_cd"
}

deploy() {
    echo "deploy..."

    local _cd=$(pwd)
    cd $build_dir

    local yak_out=$(ls | grep yak)

    source_environment

    set -x
    $yak_bin push $yak_out
    set +x

    cd "$_cd"
}

deploy_test() {
    echo "deploy to test repo..."

    local test_repo="--source=https://test.yak.rhino3d.com"
    local _cd=$(pwd)
    cd $build_dir

    local yak_out=$(ls | grep yak)

    source_environment

    set -x
    $yak_bin push $test_repo $yak_out
    set +x

    cd "$_cd"
}


distrib_test() {
    echo "distrib_test..."

    local v=$(version)
    local interactive=${1:-yes}
    echo "using version: $v"
    
    build
    unit_test
    create_package_dir "$v"
    package
    if [ "$interactive" == "yes" ]
    then
        read -p "Press enter to deploy"
    fi
    source_environment
    deploy_test
}

distrib() {
    echo "distrib..."
    local v=$(version)
    local interactive=${1:-yes}
    echo "using version: $v"
    
    build
    unit_test
    create_package_dir "$v"
    package

    if [[ "$interactive" == "yes" ]]
    then
        read -p "Press enter to deploy"
    fi

    source_environment
    deploy
}

unit_test() {
    echo "Unit testing..."
    local test_bin=$proj_root/tools/test/run_tests.sh

    $test_bin
}

ci_build() {
    local datetag=$(date +"%Y-%m-%dT%H-%M-%S%z")
    local v="-develop-$datetag"
    local version=$(version | cut -d'-' -f1)
    local interactive="yes"

    version="${version}${v}"

    update_version "$version"
    build
    create_package_dir "$version"
    package

    if [ "$interactive" == "yes" ]
    then
        read -p "Press enter to deploy"
    fi

    deploy_test
}

help() {
    echo "distrib.sh: $0 {login|update_version|version|build|package|deploy_test|deploy|distrib|distrib_test}" >&2
    echo "commands: " >&2
    echo "  login........................: login with yak to obtain token" >&2
    echo "  update_version <version>.....: updates version" >&2
    echo "  version......................: list version" >&2
    echo "  build........................: build dotnet solution" >&2
    echo "  test.........................: build dotnet solution, run tests" >&2
    echo "  package......................: builds solution, creates yak package format" >&2
    echo "  deploy_test..................: deploys the yak package found to test store" >&2
    echo "  deploy.......................: deploys the yak package found store" >&2
    echo "  distrib......................: builds, packages, deploys package to store" >&2
    echo "  distrib_test.................: builds, packages, deploys package to test store" >&2

    echo ""
}

command=${1:-}
case "$command" in
    login)
        ensure_binaries_deploy
        login
        ;;
    version)
        ensure_binaries_build
        version
        ;;
    update_version)
        ensure_binaries_build
        version=${2:-}
        if [ -z "$version" ]
        then
            echo "No argument set. $0 $1 <version>"
            exit 1
        fi
        update_version "$version"
        ;;
    build)
        ensure_binaries_build
        build
        ;;
    test)
        ensure_binaries_build
        unit_test
        ;;
    deploy_test)
        ensure_binaries_deploy
        deploy_test
        ;;
    deploy)
        ensure_binaries_deploy
        deploy
        ;;
    package)
        ensure_binaries_build
        package
        ;;
    ci_build)
        ensure_binaries_deploy
        ci_build
        ;;
    distrib)
        ensure_binaries_deploy
        interactive=${2:-yes}
        echo "run '$0 $1 no' to disable interative mode"
        distrib "$interactive"
        ;;
    distrib_test)
        ensure_binaries_deploy
        interactive=${2:-yes}
        echo "run '$0 $1 no' to disable interative mode"
        distrib_test "$interactive"
        ;;
    create_package_dir)
        ensure_binaries_build
        version=${2:-}
        if [ -z "$version" ]
        then
            echo "No argument set. $0 $1 <version>"
            exit 1
        fi
        create_package_dir "$version"
        ;;
    *)
        help
        exit 2
esac
