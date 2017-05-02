# ITweakMyBuild  ![Badge](https://tom-englert.visualstudio.com/_apis/public/build/definitions/75bf84d2-d359-404a-a712-07c9f693f635/10/badge)
Change global build parameters on the fly

During development even highly recommended tasks like e.g. static code analysis can be 
sometimes annoying, since they can slow down the build significantly, and might fail
the build due to false positives just because the code is already compilable, but 
implementation is not yet complete.

In such cases you may want to temporarily turn of these tasks, finish your implementation,
and finally turn on the task again, to verify that your code succeeds the static analysis.

While you could solve this using several solution configurations, like "Debug", "DebugNoCA", etc..,
using this approach has some disadvantages:

- since you have to maintain all these configurations in every project, you will end up in the maintenence hell soon.
- also switching between configurations in Visual Studio is a time consuming task, if you have more than a few projects.

ITweakMyBuild gives you another level of control by directly interacting with MSBuild. 
It lets you locally override any property, without the need to switch the build configuration or edit project settings.

### Download & Installation

Download the stable release from the [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=TomEnglert.ITweakMyBuild)

Download the latest build from the [Open VsixGallery](http://vsixgallery.com/extension/ITweakMyBuild.tom-englert.de.92edc38f-1498-4327-8078-6ff3cec52067/)

This will install the Visual Studio Extension. 
If the shared MSBuild targets are not yet installed, the tool will prompt you to elevate to install them.

If you think this tool is helpful, support the project by writing a short [review](https://marketplace.visualstudio.com/items?itemName=TomEnglert.ITweakMyBuild#review-details)


### Usage

Show the configuration page by clicking the entry in the Tools menu:

![screenshot](Assets/menu.png)

Configure any property override, and activate or deactivate it on the fly:

![screenshot](Assets/toolwindow.png)

Build you project. Whenever any property override is active, a red, flashing marker 
appears in the title bar of Visual Studio while the project builds, to remind you that this 
build is tweaked, and you should do a final build without tweaks before commiting any work.

![screenshot](Assets/buildindicator.png)

### Limitations

With this technique you will override any property after your project has been evaluated by MSBuild.
This works fine for properties that are evaluated in build tasks. 
However conditions in nodes of your project, e.g. in the `<PropertyGroup>` nodes, are evaluated before, so 
changing e.g. the `Configuration` property won't have any effect.

### Risks

Since it's possible to tweak any property, be careful what you are changing. 
However the worst that can happen is a broken build with no or garbage output, and a rebuild with no tweaks will fix it.

### Side Effects

Visual Studio also uses MSBuild to parse the projects. So if you e.g. tweak `RunCodeAnalysis` to false 
before you open the solution, it will also appear as disabled in all projects property pages even thoug it is enabled 
in the individual project settings; disabling the tweak again while the project is loaded won't be reflected on the property page, 
too, since Visual Studio does not expect properties to change during runtime, and still displays the wrong value.

### Scope

The scope of tweaks is global to the current user, so if you have started serveral instances of Visual Studio, 
all instances will see the same tweaks, and even if you start a build from the command line, the tweaks will be applied.
Also tweaks are persistent, so they stay until you explicitly turn them off.



