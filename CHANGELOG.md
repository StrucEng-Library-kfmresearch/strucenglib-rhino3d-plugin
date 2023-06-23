## v0.0.21, 23-06-23
- Fix all links to new Github Organisation and Website
- Dependency installer now remembers Anaconda Home Directory
- Introduce Python Developer Mode to link a git repository into Rhino 3D virtual environment.
  - https://strucenglib.ethz.ch/strucenglib_plugin/developer/python_developer_mode/
- Settings view has links to changelog and dependency installer

## v0.0.20, 23-05-26
- remove --user prefix when pip installing dependencies in dependency installer
- use compas version from strucenglib github in dependency installer

## v0.0.19, 23-04-20
- update dependency installer to download ansys and abaqus specific python dependencies
  - compas_fea:
     - ansys version: https://github.com/StrucEng-Library-kfmresearch/compas_fea/tree/ansys
    - abaqus version: https://github.com/StrucEng-Library-kfmresearch/compas_fea/tree/abaqus
  - strucenglib_snippets:
    - ansys version: https://github.com/StrucEng-Library-kfmresearch/strucenglib-snippets/tree/ansys
    - abaqus version: https://github.com/StrucEng-Library-kfmresearch/strucenglib-snippets/tree/abaqus

- change code generation: use sandwich model import: strucenglib.sandwichmodel

![install_abaqus](https://user-images.githubusercontent.com/2311941/233221581-224190c0-2fcb-4f0a-b5b7-49f253d263ca.PNG)

## v0.0.14
- change remote computation endpoint
- add UI textfield to set remote endpoint (Settings tab/ Remote Server)

## v0.0.13
- introduce strucenglib connect to execute FEA on remote host
- Add checkbox to either execute locally or remote
- run in background is off by default

## v0.0.12, 2022-09-05
- Introduce collapsible panels in linfe and sandwich model
- change of terminology: we refer to a set from now on as a 'constraint'
- no longer require a button 'mouse select' to select a layer in active rhino document.
- introduce 'settings' panel for Plugin info and further settings
- internal: introduce more fine grained namespaces for UI code

## v0.0.11
- introduce background executor to run python script without freezing rhino (checkbox to run in background)

## v0.0.10
- fix: scroll scrollbar of parent if mousewheel event in child, scroll was otherwise blocked.
- LinFE/SM: No analysis output if a step contains a set, also not within a load, (compas otherwise generates an error)
- always normalize rhino layer font size to fix layers with too large font sizes from compas analysis output.
- bugfix: linfe update step ui if layer/ load is removed 
- change UI text fields to simple text fields without suggestion completion
- rhino select layers which are currently focused in strucenglib
- show dialog while Rhino executes code and is frozen
- show ok/cancel buttons in validation dialog to either continue/cancel code gen/exec.
- validation: info if load contains sets and elements as this may lead to compas error.

## v0.0.9
- LinFe: Introduce spring forces, section forces, shell forces, 
  section moments, shell moments for analysis output
- Sandwichmodel: Validation with auto correct if missing analysis output in linfe.
- Show more meaningful error message if python code throws an error
- Add button to show python code in Rhino's EditPythonCode panel (code is copied to clipboard)  

## v0.0.8
- Update Rhino Menu bar on new StrucEngLib update

## v0.0.6
- Bugfix in code emit: displacement id not reused in step order
- Add 'test import' functionality in dependency installer
- Linfe: Preselect local coordinates on select
- update menu bar on new version update
 
## v0.0.5
- Introduce dependency installer to install compas, sandwichmodel, compas-fea
- Command: StrucEngLibInstallDependencies
- Fix missing step_order in python output

## v0.0.4
- Add sandwich validation: warning if no steps assigned
- Add logo
- Add Commands: StrucEngLibAbout, StrucEngLibHelp, StrucEngLibChangelog

## v0.0.3
- Introduce this changelog
- Introduce sandwich model
- Generate code for sandwich model for multiple steps
- Show visualization in sandwich model with sample images
- Add option to set file name of generated python file
- Add visual cues to show currently edited element
- show button to inspect code on 'generate number' feature
