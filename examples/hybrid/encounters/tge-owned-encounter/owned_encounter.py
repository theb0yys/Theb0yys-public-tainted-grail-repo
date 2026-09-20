from __future__ import annotations

import argparse
import json
from pathlib import Path
import sys
import time


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Spawn and remove one exact TGE-owned native encounter actor."
    )
    parser.add_argument("--sdk-tools", required=True, type=Path)
    parser.add_argument("--port", required=True, type=int)
    parser.add_argument(
        "--template",
        choices=("wyrdspirit", "outlaw-1h"),
        default="wyrdspirit",
    )
    parser.add_argument("--observe-seconds", type=float, default=3.0)
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

    plan = preview(client, (args.template,))
    request = spawn_request(client, plan)

    created = state(client.invoke(request))
    encounter_id = created["encounterId"]

    appeared = wait_for_state(client, encounter_id, "appeared", 20.0)
    handles = actor_handles(appeared)

    time.sleep(max(0.0, min(args.observe_seconds, 10.0)))

    query(client, encounter_id, remove=True)
    removed = wait_for_state(client, encounter_id, "removed", 20.0)

    if actor_handles(removed) != handles:
        raise ProtocolError("Removed actor handles differ from the appeared actors.")
    if removed["cleanupComplete"] != "true":
        raise ProtocolError("Encounter removal did not confirm cleanup.")

    print(json.dumps({
        "status": "PASSED",
        "encounterId": encounter_id,
        "template": args.template,
        "ownedHandles": handles,
        "appeared": True,
        "cleanupComplete": True,
    }, indent=2))
    return 0


def actor_handles(snapshot: dict) -> list[str]:
    count = int(snapshot["trackedCount"])
    return [snapshot[f"actor.{index}.id"] for index in range(count)]


def wait_for_state(client, encounter_id: str, wanted: str, timeout: float) -> dict:
    deadline = time.monotonic() + timeout
    last = None
    while time.monotonic() < deadline:
        current = query(client, encounter_id)
        if current != last:
            print(json.dumps({
                "encounterId": encounter_id,
                "state": current["state"],
                "trackedCount": current["trackedCount"],
                "cleanupComplete": current["cleanupComplete"],
            }))
            last = current

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

    raise TimeoutError("Timed out waiting for encounter state: " + wanted)


if __name__ == "__main__":
    raise SystemExit(main())
