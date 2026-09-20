# TGE Owned Encounters

The encounter service is an advanced example of safe shared execution.

It uses:

- reviewed allowlisted templates;
- preview/fingerprint before execution;
- native placement verification;
- one owned actor handle per result;
- `MarkedNotSaved` for temporary actors;
- exact owned `Location` cleanup;
- asynchronous lifecycle status;
- no broad “delete actor by native ID” authority.

Use the public [runtime actor mechanics](../../mechanics/encounters/native-runtime-spawn.md) to understand the native lifecycle.

The SDK/service layer adds **ownership, request, preview, handle and cleanup contracts** around that lifecycle.
