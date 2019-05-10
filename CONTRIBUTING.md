# Contribution guidelines

:heart: Welcome, and thanks for considering our project :heart:

These are mostly guidelines, not rules. Use your best judgment, and feel free to propose changes to this document in a pull request.

## Communication

All communication is currently held in the [GitHub issues](https://github.com/nwheels-io/NuLink/issues). Before filing an issue, please first check existing issues, including closed ones.

Please note we have a [Code of Conduct](#Code-of-conduct), please follow it in all your interactions with the project.

## Questions and suggestions

To ask a question or file a suggestion, just open an issue describing it. 

In the future, we plan to move questions to StackOverflow.com.

## Reporting a bug

First verify that you are using the latest version of the tool.

To report a bug, file an issue that contains:
- Steps to reproduce
- Expected result
- Actual result
- NuLink command line and output
- Output of `dotnet --info` command

## Contributing code

### Setting up environment

You will need:

- .NET Core SDK 2.1+ (not tested on 3.0)
- C# IDE of your choice (Rider, VSCode, Visual Studio, etc).

To build the cloned repo:

```
$ cd source
$ dotnet restore
$ dotnet build
```

### Testing

At the moment, we have acceptance tests which actually install the tool from NuGet and apply it to pre-created projects and packages. Acceptance tests run as part of continuous integration on Linux and Windows VMs (see next section). 

Unit tests are currently missing (the alpha version is a successful proof of concept). Next step is extracting some abstractions to make it possible to write unit tests. We will use NUnit framework.

### Continuous integration

All changes in code and documentation are merged into `master` through pull requests. 

This project runs [continuous integration on AppVeyor](https://ci.appveyor.com/project/felix-b/nulink). There are two builds, both scripted using YAML files:

- `NuLink`: CI build triggered on every push to a PR branch and every merge into `master`, scripted by [appveyor.yml](appveyor.yml). 
  - The build does: compile and package the tool; push package to CI feed; install the tool from CI feed; run acceptance tests. 
  - For cross-platform testing, the build runs on both Linux and Windows VMs.
  - Commits containing only changes to documentation .md files don't trigger the build

- `NuLink-Release`: creates release version of the tool package and pushes it to **nuget.org**. It is scripted by [release.appveyor.yml](release.appveyor.yml). This build is triggered manually by maintainers.

### Submitting changes

Code and documentation changes should be submitted in a pull request.

If you want to submit a relatively small change that doesn't impact the concept, just submit a PR.

If it's a big and fundamental change or enhancement, please first file an issue and discuss it with us.

### Coding conventions

We follow standard C#/.NET rules for naming and casing, with emphasis on these points:

- Tabs: use spaces and indent size of 4

- Control flow statements (`if`, `while`, etc) must always have brackets, even when their body is a single line

- Object/collection initializers and anonymous delegates: the opening bracket stays on the same line (K&R style).

## Code of Conduct

### Our Pledge

In the interest of fostering an open and welcoming environment, we as contributors and maintainers pledge to making participation in our project and our community a harassment-free experience for everyone, regardless of age, body size, disability, ethnicity, gender identity and expression, level of experience, nationality, personal appearance, race, religion, or sexual identity and orientation.

### Our Standards

Examples of behavior that contributes to creating a positive environment include:

* Using welcoming and inclusive language
* Being respectful of differing viewpoints and experiences
* Gracefully accepting constructive criticism
* Focusing on what is best for the community
* Showing empathy towards other community members

Examples of unacceptable behavior by participants include:

* The use of sexualized language or imagery and unwelcome sexual attention or advances
* Trolling, insulting/derogatory comments, and personal or political attacks
* Public or private harassment
* Publishing others' private information, such as a physical or electronic address, without explicit permission
* Other conduct which could reasonably be considered inappropriate in a professional setting

### Our Responsibilities

Project maintainers are responsible for clarifying the standards of acceptable behavior and are expected to take appropriate and fair corrective action in response to any instances of unacceptable behavior.

Project maintainers have the right and responsibility to remove, edit, or reject comments, commits, code, wiki edits, issues, and other contributions that are not aligned to this Code of Conduct, or to ban temporarily or permanently any contributor for other behaviors that they deem inappropriate, threatening, offensive, or harmful.

### Scope

This Code of Conduct applies both within project spaces and in public spaces when an individual is representing the project or its community. Examples of representing a project or community include using an official project e-mail address, posting via an official social media account, or acting as an appointed representative at an online or offline event. Representation of a project may be further defined and clarified by project maintainers.

### Enforcement

Instances of abusive, harassing, or otherwise unacceptable behavior may be reported by contacting the project team at [team@nwheels.io](mailto:team@nwheels.io). The project team will review and investigate all complaints, and will respond in a way that it deems appropriate to the circumstances. The project team is obligated to maintain confidentiality with regard to the reporter of an incident. Further details of specific enforcement policies may be posted separately.

Project maintainers who do not follow or enforce the Code of Conduct in good faith may face temporary or permanent repercussions as determined by other members of the project's leadership.

### Attribution

This Code of Conduct is adapted from the [Contributor Covenant][homepage], version 1.4, available at [http://contributor-covenant.org/version/1/4][version]

[homepage]: http://contributor-covenant.org
[version]: http://contributor-covenant.org/version/1/4/
