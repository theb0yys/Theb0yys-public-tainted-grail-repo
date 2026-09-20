# Contextual Music

A self-contained FMOD Core example with no bundled music assets.

To make the project immediately buildable/copyable, it generates two tiny WAV tones in memory:

- 220 Hz for open-world context;
- 330 Hz for interior context.

The important mechanism is the same one used by a real music mod:

~~~text
read game context
→ choose one mod-owned lane
→ FMOD Core createSound
→ playSound on FoA master channel group
→ crossfade owned voices
→ duck only owned music during combat
→ stop/release every owned channel/sound on teardown
~~~

Replace BuildToneWav with your real WAV/resource loader when adapting the example.

This project intentionally does not suppress native music. Native-music suppression is a separate compatibility decision.

## Build

~~~powershell
dotnet build .\ContextualMusic.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Guide: [Build a contextual music mod](../../../../guides/tasks/audio/build-a-contextual-music-mod.md)
