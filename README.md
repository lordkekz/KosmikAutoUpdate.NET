# KosmikAutoUpdate.NET
.NET Library for KosmikAutoUpdate.

## Motivation
My motivation for this library was to be able to deploy new versions of my cross-platform Avalonia apps with a packaged runtime but without needing to re-download the (fairly large) runtime files with every new version.

## Library Design Overview
The Library consists of two components: an application library ("library") to be deployed as part of your main application ("app"), and a standalone patcher executable ("patcher") which is used to apply the file changes.
### Challenges
- We can't overwrite the application files while it is running.
- The patcher must be portable and platform-independent.
- The patcher must have an appropriate storage footprint compared to the main application. Packaging a cross-platform UI framework like AvaloniaUI is thus not feasible.
### How it works
1. When the app starts, you call the library to check for updates.
2. You can then decide to which version you want to update.
3. You tell the library to download the updated files to a temporary directory and generate a "patch manifest" describing which files to update and delete. (You can monitor observe the progess of the download and show it to the user.)
4. Once everything is prepared, the library will start the patcher and your app needs to exit.
5. Once the app has exited, the patcher can apply the update.
6. When the update is complete, the patcher starts the app again.