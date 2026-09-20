# Authenticated TGE Live Handshake

A bounded installed-game probe validated the production local transport inside the FoA process.

It exercised:

- expected host identity/version/API/session;
- service discovery;
- identical-request replay handling;
- stale-session rejection;
- unknown-service rejection;
- wrong-key rejection;
- healthy query after negative controls;
- listener PID ownership;
- rollback/removal of temporary proof files.

The only registered service in that proof was the read-only identity service.

## Lesson

A successful authenticated transport handshake proves connectivity/session/authentication—not gameplay provider correctness or general SDK readiness.
