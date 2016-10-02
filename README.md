# ITweakMyBuild
Change global build parameters on the fly

During development even highly recommended tasks like e.g. static code analysis can be 
sometimes annoying, since they can slow down the build significantly, and might fail
the build due to false positives just because the code is already compilable, but 
implementation is not yet complete.

In such cases you may want to temporarily turn of these tasks, finish your implementation,
and then turn on the task again, to verify that your code succeeds the static analysis.

While you could solve this using several solution configurations, like "Debug", "DebugNoCA", etc..,
using this approach has some odds:

- since you have to maintain all these configurations in every project, you will end up in the maintenence hell soon.
- also switching between configurations in Visual Studio is a time consuming task, if you have more than a few projects.

ITweakMyBuild gives you another level of control by directly interacting with MSBuild. 
It let's you locally override any property, without the need to switch the build configuration or edit project settings.

### Usage

Show the configuration page by clicking the entry in the Tools menu:

![screenshot](https://github.com/tom-englert/ITweakMyBuild/blob/master/menu.png)

Configure any property override, and activate or deactivate it on the fly:

![screenshot](https://github.com/tom-englert/ITweakMyBuild/blob/master/toolwindow.png)

Build you project. Whenever any property override is active, a red, flashing marker 
appears the title bar of Visual Studio while the project builds, to remind you that this 
build is tweaked, and you should do a final build without tweaks before commiting any work.


