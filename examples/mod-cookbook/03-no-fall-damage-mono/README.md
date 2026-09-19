# 03 — No Fall Damage Example

**Category:** damage  
**Source-path evidence:** SOURCE_BUILD_EVIDENCED  
**This rewritten public example:** NOT_RUN

This patches the real FoA FallDamageUtil.DealFallDamage path and changes only damage aimed at Hero.Current.

It does not skip the whole method. Instead it changes the damage argument to zero and lets the original method continue.

That is an important pattern when the original method may still own side effects.

## Build

~~~powershell
dotnet build .\NoFallDamageExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

## Test

Use a disposable save and compare a fall that normally causes health loss.

Also confirm ordinary combat damage is unchanged.

The underlying source path built successfully, but the inspected evidence did not yet contain the required live fall behavior check.
