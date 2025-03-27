This editor-only tool was developed to protect Unity IL2CPP builds using several uncommon techniques found in AAA games (e.g., Genshin Impact).
These techniques include layout-randomized metadata encryption, export modification, and initialization pattern obfuscation.

The entire process is automated, and the asset does not contain any demo scenes.
By importing Mfuscator and initiating the build, the protection measures are applied.
To examine the result, try to dump the build with any popular Unity IL2CPP dumping tool (or dumper).

[!] If you use other build postprocessing scripts, you can configure the callback order in the "Window/MFS Settings" window to avoid any conflicts.

FREQUENTLY ASKED QUESTIONS:

1. I get a "The current system user does not have full access" error when building.
- To fix the error, you need to either run Unity as an administrator (not recommended) or grant
the current system user read and write access to the folder and subfolders where Unity Editor is installed.
The default path for Windows is "C:\Program Files\Unity\Hub".

2. I can't update the package. I get a "Cannot Delete" error.
- Close Unity Editor to release the process, navigate to the path where your
project is located, and delete the "Mfuscator" folder. Then you can open Unity
again and import the new version.

3. A third-party antivirus interferes with the game process or generates false positives.
- To avoid false positives, your ".exe"/".dll" PE files must be signed with a valid
digital signature (see https://www.digicert.com/faq/code-signing-trust/what-is-code-signing).
Note that anti-viruses usually have whitelisting services, and if for some reason you still have false positives with a valid signature, you should
send an email to the anti-virus whitelisting service to have them manually whitelist your game.

[!] If MFS doesn't compile but there are no errors in the console after the build, make sure you have automatic
cleanup disabled ("Clear on Build", "Clear on Recompile").

If you encounter any issues, please feel free to contact help@mew.icu, and we will try to help you as soon as we can.
https://mew.icu/
