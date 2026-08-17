# Verified QQ Bang save layout

The QQ Bang inventory follows the nine normal 0x1000-byte inventory sections.

- Decrypted-data offset: `0xA410` (`42000`)
- Record count: `512`
- Record size: `8` bytes
- `+0..+2`: six packed 4-bit stats, low nibble first. Stored `0..10` means `-5..+5`.
- `+3`: preserved unknown byte
- `+4`: type (`9` for QQ Bang)
- `+5`: quantity (`byte`)
- `+6/+7`: preserved inventory fields
- `+1`: Ki (`sbyte`)
- `+2`: Stamina (`sbyte`)
- `+3`: Basic Attack (`sbyte`)
- `+4`: Strike Supers (`sbyte`)
- `+5`: Ki Blast Supers (`sbyte`)

The writer changes only offsets `+0` through `+5` and `+7`. Copy/paste transfers
only the six stat bytes. Revert restores the selected record's exact original eight
bytes. Empty records are not offered for editing or insertion.
