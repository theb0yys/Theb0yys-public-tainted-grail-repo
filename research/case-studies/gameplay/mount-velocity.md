# Native Mount Velocity Tuning

Use the mount's own velocity getters when the goal is a simple movement-speed profile.

## Working implementation lineage

Avalon Mounts 0.1.3 proved the native running and turning getter paths live with a `1.25` multiplier. The same DLL/profile was user-smoke-tested through mount, movement, transitions, save/load and quit/relaunch on the observed stack.

## Native owners

- `VMount.RunningVelocity`
- `VMount.TurningVelocity`

## Working patch shape

Use postfixes on the property getters:

~~~text
native mount computes velocity
→ getter returns native value
→ mod multiplies returned value
→ native mount movement consumes the adjusted value
~~~

A multiplier of `1.0` is vanilla.

## Why this is preferable

The speed mod does not need to:

- drive the mount transform;
- replace mount input;
- call mount state methods;
- alter save data;
- invent a second movement controller.

It adjusts the values the native mount controller already consumes.

## Scope rule

Treat running and turning as separate settings. Keep deeper mount systems—recall, armour, stamina, animation, ownership—outside this simple speed pattern.
