from __future__ import annotations

import argparse
import json
from pathlib import Path
import sys
import time


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Create one temporary native Wyrdspirit through the reviewed TGE ownership boundary, then clean it up."
    )
    parser.add_argument("--sdk-tools", required=True, type=Path)
    parser.add_argument("--port", required=True, type=int)
    parser.add_argument("--visible-seconds", type=float, default=5.0)
    args = parser.parse_args()

    sys.path.insert(0, str(args.sdk_tools.resolve(strict=True)))

    from tge_sdk_client import ProtocolError
    from tge_sdk_transport import TgeSdkClient, key_from_environment
    from tge_encounters import preview, spawn_request, state, query

    client = TgeSdkClient(
        args.port,
        key_from_environment(),
        expected_host_id="kane.tgfoa.tainted-grail-extender",
        expected_host_version="0.1.0",
        timeout=8,
    )
    client.connect()

    plan = preview(client, ("wyrdspirit",))
    request = spawn_request(client, plan)
    first = state(client.invoke(request))
    encounter_id = first["encounterId"]

    appeared = wait(client, encounter_id, "appeared", 20.0)
    handle = appeared["actor.0.id"]
    native_id = appeared["actor.0.nativeId"]

    print(json.dumps({
        "phase": "appeared",
        "encounterId": encounter_id,
        "ownedHandle": handle,
        "nativeIdentityObserved": bool(native_id),
    }))

    time.sleep(max(0.0, min(args.visible_seconds, 15.0)))

    query(client, encounter_id, remove=True)
    removed = wait(client, encounter_id, "removed", 20.0)

    if removed["actor.0.id"] != handle or removed["actor.0.state"] != "removed":
        raise ProtocolError("Exact owned actor was not confirmed removed.")

    print(json.dumps({
        "status": "PASSED",
        "encounterId": encounter_id,
        "ownedHandle": handle,
        "cleanupComplete": removed["cleanupComplete"] == "true",
    }, indent=2))
    return 0


def wait(client, encounter_id: str, wanted: str, timeout: float) -> dict:
    deadline = time.monotonic() + timeout
    while time.monotonic() < deadline:
        current = query(client, encounter_id)
        if current["state"] == wanted:
            return current
        if current["state"] in {"failed", "cleanup_failed"}:
            raise RuntimeError(
                "Encounter failed: "
                + current["failure"]
                + "/"
                + current["cleanupFailure"]
            )
        time.sleep(0.25)
    raise TimeoutError("Timed out waiting for " + wanted)


if __name__ == "__main__":
    raise SystemExit(main())
