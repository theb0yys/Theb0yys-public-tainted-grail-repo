from __future__ import annotations

import argparse
import json
from pathlib import Path
import secrets
import sys


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Connect to an already-running TGE host and exercise bounded identity/discovery controls."
    )
    parser.add_argument("--sdk-tools", required=True, type=Path)
    parser.add_argument("--port", required=True, type=int)
    args = parser.parse_args()

    sdk_tools = args.sdk_tools.resolve(strict=True)
    sys.path.insert(0, str(sdk_tools))

    from tge_sdk_client import ProtocolError
    from tge_sdk_transport import TgeSdkClient, TransportError, key_from_environment

    key = key_from_environment()
    client = TgeSdkClient(
        args.port,
        key,
        expected_host_id="kane.tgfoa.tainted-grail-extender",
        expected_host_version="0.1.0",
        timeout=8,
    )

    identity = client.connect()
    if not identity.succeeded or identity.values.get("apiVersion") != "0.1":
        raise ProtocolError("Authenticated identity response did not match the expected API.")

    request = client.create_request("tge.core.identity", "0.1", "services")
    services = client.invoke(request)
    if not services.succeeded:
        raise ProtocolError("Service discovery failed: " + services.code)

    replay = client.invoke(request)
    if replay != services:
        raise ProtocolError("Identical-request replay did not return the same response.")

    unknown = client.invoke(
        client.create_request("community.example.unknown-service", "0.1", "noop")
    )
    if unknown.code != "service_not_found":
        raise ProtocolError("Unknown-service negative control did not fail as expected.")

    wrong_key_denied = False
    try:
        wrong = TgeSdkClient(
            args.port,
            secrets.token_bytes(32),
            expected_host_id="kane.tgfoa.tainted-grail-extender",
            expected_host_version="0.1.0",
            timeout=8,
        )
        wrong.connect()
    except (ProtocolError, TransportError):
        wrong_key_denied = True

    if not wrong_key_denied:
        raise ProtocolError("Wrong-key negative control unexpectedly authenticated.")

    healthy = client.invoke(
        client.create_request("tge.core.identity", "0.1", "services")
    )
    if not healthy.succeeded:
        raise ProtocolError("Host did not remain healthy after negative controls.")

    print(json.dumps({
        "status": "PASSED",
        "sessionId": identity.session_id,
        "serviceCount": services.values.get("total"),
        "sameRequestReplay": True,
        "unknownServiceRejected": True,
        "wrongKeyRejected": True,
        "healthyAfterNegativeControls": True,
        "keyRecorded": False,
    }, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
