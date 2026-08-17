XV2 SAVE EDITOR — RELEASE CANDIDATE 13
=====================================

Made with love by: Demyliciouss
With help from: Gliscors

QUICK START
-----------
1. Extract the entire ZIP into a normal folder.
2. Run START XV2 SAVE EDITOR.cmd. Do not run it from inside the ZIP.
   The included private .NET runtime means users do not need to install .NET.
3. Drag a supported save onto the editor, or use Open Save.
4. Make your edits and review the change summary / save-health warnings.
5. Press Save Changes. The editor creates a safety backup before writing.

SUPPORTED SAVE CONTAINERS
-------------------------
- PC / Steam: DBXV2.sav
- Xbox: supported decrypted .bin containers
- PlayStation: verified decrypted and encrypted SDATA/.DAT variants

PLATFORM NOTES
--------------
Steam:
- Put DBXV2.sav back in the correct Steam userdata save folder after editing.
- Use Link Steam ID before saving/exporting when moving a save between accounts.
- Steam ID lookup: https://steamid.io/

PlayStation:
- Some outputs still require re-encryption and profile resigning with a compatible
  external service/tool (for example HTOS) before the console accepts them.

Xbox:
- Rename the edited output back to the original Xbox save name when required.

SAFETY
------
- Automatic backups are created before writes, not on every UI change.
- Backups are filtered into Documents\\XV2 Save Editor Backups by platform and
  source save, so PC, Xbox, and PlayStation backups stay separate.
- Backup Recovery can compare or restore backups and selectively restore CaCs.
- Diagnostics and pre-save validation only repair structures that were verified.
- Keep your original save somewhere separate until the edited save works in-game.

HELP AND CONTACT
----------------
Use the ? button inside the editor for instructions, settings, recent saves,
version information, and clickable support links.

Discord: demyliciouss
Discord: gliscors
https://discord.com/invite/desurui
https://discord.gg/rrpvUequwX

KNOWN LIMITATIONS
-----------------
- Switch containers are not supported by this release.
- Console profile resigning is outside the editor.
- Unknown offsets/flags are intentionally left unsupported rather than guessed.
