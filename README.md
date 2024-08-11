# SpecSharer - An application to extract, store and share Specflows Bindings

Spechsharer is an application that uses Microsoft.CodeAnalysis.CSharp to extract bindings from a chosen file and either display them in the console, store them in another chosen local file or programmatically store them in a shared Github Repository using the Octokit library.

## Usage

Please create a Github Personal Access Token and store it in the file "\Resources\PatStore.txt", sitting relative to your SpecSharer.exe file.

The application can be run from console along with the following commands:
- h|help: Use alone or with one other command to get help for that command.
- p|path: The path to the file or files that you want extracted. If local this should be a full path to a single file. If on Github this should be a relative path to either a file or directory.
- t|target: The path to the file where extracted bindings will be stored. If the file does not exist one will be created. If local this should be a full path to a single file. If on Github this should be a relative path to a file. If this is not included a summary of results will be printed to console but nothing will be stored.
- g|github: Command to store in or retrieve from with Github. Do not include if a local operation is desired.
- r|retrieve: Command to retrieve from Github instead of storing. Does nothing if g|github is not set.