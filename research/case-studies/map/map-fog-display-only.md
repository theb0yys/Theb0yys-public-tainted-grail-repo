# Map Fog: Display Without Rewriting Discovery Memory

No Map Fog deliberately selected the map display path while leaving native `MapMemory.visitedPixels` untouched.

That keeps:

- map memory/save ownership native;
- quest/discovery state native;
- fast-travel ownership native;

while changing only what the map screen reveals.

## Lesson

A visual override can be substantially safer when it does not rewrite the underlying progression/discovery truth.
