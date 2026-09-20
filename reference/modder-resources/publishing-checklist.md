# Publishing Checklist

Use this before committing external content to the public repository and again before packaging a public mod release.

This checklist is a redistribution backstop. It does not replace build, runtime, persistence, compatibility or release testing.

## Before an asset enters git

- [ ] The creator/publisher and original source are known.
- [ ] The exact licence/terms and version are recorded.
- [ ] The terms cover the intended modification.
- [ ] The terms cover the intended form of redistribution.
- [ ] Required attribution/licence/NOTICE files are prepared.
- [ ] Share-alike or source-code obligations are understood.
- [ ] The asset is not an extracted Tainted Grail/game asset.
- [ ] The asset is not a Unity/Fab/other marketplace source file whose terms prohibit raw redistribution.
- [ ] Any trademark, identifiable-person, voice, privacy or publicity issues have been considered.
- [ ] Provenance is recorded using [Asset provenance](asset-provenance.md).

If any item is unresolved, do not commit the asset.

## Before committing source/code

- [ ] No game DLLs or Unity DLLs.
- [ ] No generated interop assemblies.
- [ ] No executables/archives/compiled plug-ins unless the repository process explicitly calls for them.
- [ ] No bulk decompiled game source.
- [ ] Third-party code licences are recorded and notices preserved.
- [ ] Local paths, usernames, tokens, API keys and private diagnostics are absent.
- [ ] Example code is source-only and public-safe.

## Before packaging a mod

- [ ] Build output comes from the intended source revision.
- [ ] Only required runtime files are staged.
- [ ] Marketplace assets are present only in forms the applicable licence allows.
- [ ] Required attribution and licence notices ship with the release.
- [ ] No accidental source packs, Unity packages, PSD/BLEND/FBX source files or vendor archives are included unless redistribution is explicitly permitted and intended.
- [ ] No game binaries/assets or private diagnostics are included.
- [ ] Configuration defaults do not contain local machine paths or secrets.

## Before calling it “tested”

Label each claim separately:

- [ ] Build validated.
- [ ] Loader validated.
- [ ] Runtime proven.
- [ ] Persistence proven, if persistence is claimed.
- [ ] Compatibility tested for every build/runtime actually claimed.
- [ ] Release package validated from the packaged artifact, not only the development tree.

See [Public Evidence Standard](../../sources/evidence-standard.md).

## Final archive inspection

Open the final archive from a clean staging directory and inspect the file list manually.

A safe release should contain only files you intentionally chose to distribute.

Do not infer “public safe” from a successful build or a successful game launch.
